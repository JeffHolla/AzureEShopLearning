using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShopOrdersFunction.Helpers.ResourceConnections
{
    public static class BlobConnections
    {
        public static async Task<BlobClient> GetBlobClient(string blobName)
        {
            string storageConnectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");

            if (string.IsNullOrEmpty(storageConnectionString) || string.IsNullOrEmpty(containerName))
            {
                throw new Exception("Blob Storage Connection String or Container Name not found. " +
                                    $"Obtained values: '{nameof(storageConnectionString)}' - '{storageConnectionString}'," +
                                                    $" '{nameof(containerName)}' - '{containerName}'");
            }

            var blobService = new BlobServiceClient(storageConnectionString);

            var blobContainerClient = blobService.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();

            return blobContainerClient.GetBlobClient(blobName);
        }
    }
}
