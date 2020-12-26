#region using

using System;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Review.Core.DataModel;
using ReviewBot.Utility;

#endregion

namespace ReviewBot.Storage
{
    public class ReviewContextBlobStore : IReviewContextStore
    {
        private readonly BlobServiceClient _blobServiceClient;

        public ReviewContextBlobStore(IConfiguration configuration)
        {
            _blobServiceClient = new BlobServiceClient(configuration["AzureBlobStorageConnectionString"]);
        }

        public async Task<ReviewContext> GetContext(string contextId)
        {
            var container = await GetBlobContainer();

            // Create test blob - default strategy is last writer wins - so UploadText will overwrite existing blob if present
            var blockBlob = container.GetBlobClient(contextId);
            if (!await blockBlob.ExistsAsync())
            {
                var reviewContext = new ReviewContext { Id = contextId, ETag = "" };
                await using var ms = reviewContext.ToMemoryStream();
                await blockBlob.UploadAsync(ms);

                return reviewContext;
            }

            var blobProperties = await blockBlob.GetPropertiesAsync();

            await using var blobStream = await blockBlob.OpenReadAsync();
            {
                var reviewContext = blobStream.Deserialize<ReviewContext>();
                reviewContext.ETag = blobProperties.Value.ETag.ToString();
                return reviewContext;
            }
        }

        public async Task SaveContext(ReviewContext context)
        {
            if (context.ETag == null) throw new ArgumentException("Cannot save context without ETag.");

            var container = await GetBlobContainer();
            var blockBlob = container.GetBlobClient(context.Id);

            await using var ms = context.ToMemoryStream();
            await blockBlob.UploadAsync(
                ms, new BlobUploadOptions { Conditions = new BlobRequestConditions { IfMatch = new ETag(context.ETag)} });
        }

        private async Task<BlobContainerClient> GetBlobContainer()
        {
            var blobClient = _blobServiceClient.GetBlobContainerClient("reviewcontexts");
            await blobClient.CreateIfNotExistsAsync();
            return blobClient;
        }
    }
}