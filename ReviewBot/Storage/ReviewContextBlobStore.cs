#region using

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Review.Core.DataModel;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Storage
{
    public class ReviewContextBlobStore : IReviewContextStore
    {
        private readonly CloudStorageAccount _cloudStorageAccount;

        public ReviewContextBlobStore(IConfiguration configuration)
        {
            _cloudStorageAccount = CloudStorageAccount.Parse(configuration["AzureBlobStorageConnectionString"]);
        }

        public async Task<ReviewContext> GetContext(string contextid)
        {
            var container = await GetBlobContainer();

            // Create test blob - default strategy is last writer wins - so UploadText will overwrite existing blob if present
            var blockBlob = container.GetBlockBlobReference(contextid);
            if (!await blockBlob.ExistsAsync())
            {
                var reviewContext = new ReviewContext { Id = contextid };
                using (var ms = reviewContext.ToMemoryStream())
                {
                    await blockBlob.UploadFromStreamAsync(ms);
                }

                return reviewContext;
            }

            using (var blobStream = await blockBlob.OpenReadAsync())
            {
                var reviewContext = blobStream.Deserialize<ReviewContext>();
                reviewContext.ETag = blockBlob.Properties.ETag;
                return reviewContext;
            }
        }

        public async Task SaveContext(ReviewContext context)
        {
            if (context.ETag == null) throw new ArgumentException("Cannot save context without ETag.");

            var container = await GetBlobContainer();
            var blockBlob = container.GetBlockBlobReference(context.Id);

            using (var ms = context.ToMemoryStream())
            {
                await blockBlob.UploadFromStreamAsync(
                    ms,
                    AccessCondition.GenerateIfMatchCondition(context.ETag),
                    new BlobRequestOptions(),
                    new OperationContext());
            }
        }

        private async Task<CloudBlobContainer> GetBlobContainer()
        {
            var blobClient = _cloudStorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("reviewcontexts");
            await container.CreateIfNotExistsAsync();
            return container;
        }
    }
}