using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EShopOrdersFunction.Helpers.ResourceConnections
{
    public static class CosmosDBConnections
    {
        public static async Task<Container> GetContainer(string containerPartitionKey, ILogger logger)
        {
            var connectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
            var cosmosDatabaseId = Environment.GetEnvironmentVariable("CosmosDatabaseId");
            var containerName = Environment.GetEnvironmentVariable("ContainerName");

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
            {
                throw new Exception("Storage connection string or container name not found. " +
                                    $"Obtained values: '{nameof(connectionString)}' - '{connectionString}'," +
                                                    $" '{nameof(cosmosDatabaseId)}' - '{cosmosDatabaseId}'," +
                                                    $" '{nameof(containerName)}' - '{containerName}'");
            }

            var client = new CosmosClient(connectionString);
            var database = (await client.CreateDatabaseIfNotExistsAsync(cosmosDatabaseId)).Database;
            var simpleContainer = await database.CreateContainerIfNotExistsAsync(containerName, containerPartitionKey);

            logger.LogInformation("Container was retrieved.");

            return simpleContainer.Container;
        }
    }
}
