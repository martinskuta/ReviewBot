#region copyright

// Copyright 2007 - 2025 Innoveo AG, Zurich/Switzerland
// All rights reserved. Use is subject to license terms.

#endregion

#region using

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Review.Core.DataModel;

#endregion

namespace ReviewBot.Storage;

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
        var blobClient = container.GetBlobClient(contextId);
        if (!await blobClient.ExistsAsync())
        {
            var newReviewContext = new ReviewContext { Id = contextId, ETag = "" };
            await using var writeStream = await blobClient.OpenWriteAsync(false);
            await JsonSerializer.SerializeAsync(writeStream, newReviewContext);

            return newReviewContext;
        }

        var blobProperties = await blobClient.GetPropertiesAsync();

        await using var blobStream = await blobClient.OpenReadAsync();
        var existingReviewContext = await JsonSerializer.DeserializeAsync<ReviewContext>(blobStream);
        existingReviewContext.ETag = blobProperties.Value.ETag.ToString();
        return existingReviewContext;
    }

    public async Task SaveContext(ReviewContext context)
    {
        if (context.ETag == null) throw new ArgumentException("Cannot save context without ETag.");

        var container = await GetBlobContainer();
        var blobClient = container.GetBlobClient(context.Id);

        await using var writeStream = await blobClient.OpenWriteAsync(
                                          true,
                                          new BlobOpenWriteOptions { OpenConditions = new BlobRequestConditions { IfMatch = new ETag(context.ETag) } });
        await JsonSerializer.SerializeAsync(writeStream, context);
    }

    private async Task<BlobContainerClient> GetBlobContainer()
    {
        var blobClient = _blobServiceClient.GetBlobContainerClient("reviewcontexts");
        await blobClient.CreateIfNotExistsAsync();
        return blobClient;
    }
}