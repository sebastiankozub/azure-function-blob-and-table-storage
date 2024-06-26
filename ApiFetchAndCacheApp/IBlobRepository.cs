﻿using ApiFetchAndCacheApp.Options;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace ApiFetchAndCacheApp
{
    public interface IBlobRepository
    {
        Task<Stream> GetAsync(string path);
        Task<Response<BlobContentInfo>> CreateAsync(string path, Stream blobValue);
        Task<Response<BlobContentInfo>> UpdateAsync(string path, Stream blobValue);
        Task<Response<BlobContentInfo>> CreateOrUpdateAsync(string path, Stream blobValue);
        Task<Response> DeleteAsync(string path);
    }


    public class BlobRepository : IBlobRepository
    {
        private readonly ILogger<BlobRepository> _logger;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly PayloadStorageOptions _payloadStorageOptions;

        public BlobRepository(ILoggerFactory loggerFactory, IAzureClientFactory<BlobServiceClient> blobClientFactory, PayloadStorageOptions payloadStorageOptions)
        {
            _logger = loggerFactory.CreateLogger<BlobRepository>();

            _blobContainerClient = blobClientFactory.CreateClient("ApiFetchAndCache").GetBlobContainerClient(payloadStorageOptions.Container);
            _blobContainerClient.CreateIfNotExists();

            _payloadStorageOptions = payloadStorageOptions;
        }

        public async Task<Stream> GetAsync(string path)
        {
            var blobClient = _blobContainerClient.GetBlobClient(path);
            var blobResponse = await blobClient.OpenReadAsync();//.DownloadContentAsync();   // use openreadasync() return stream for efficiency

            return blobResponse; //.Value.Content;
        }

        public Task<Response<BlobContentInfo>> CreateAsync(string path, Stream blobValue) // upload stream for efficiency
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(path);
            return blobClient.UploadAsync(blobValue);
        }

        public Task<Response<BlobContentInfo>> CreateOrUpdateAsync(string path, Stream blobValue)
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(path);
            return blobClient.UploadAsync(blobValue, overwrite: true);
        }

        public Task<Response> DeleteAsync(string path)
        {
            var blobClient = _blobContainerClient.GetBlobClient(path);
            return blobClient.DeleteAsync();
        }

        public Task<Response<BlobContentInfo>> UpdateAsync(string path, Stream blobValue)
        {
            BlobClient blobClient = _blobContainerClient.GetBlobClient(path);
            return blobClient.UploadAsync(blobValue, overwrite: true);
        }
    }
}
