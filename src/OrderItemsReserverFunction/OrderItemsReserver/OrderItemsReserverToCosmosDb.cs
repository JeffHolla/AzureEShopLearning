using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;
using Newtonsoft.Json.Linq;
using EShopOrdersFunction.Helpers.ResourceConnections;
using EShopOrdersFunction.Helpers.HttpHelpers;
using EShopOrdersFunction.Helpers.EnvironmentHelpers;
using EShopOrdersFunction.Helpers.ResourceRelatedHelpers.CosmosDb;

namespace EShopOrdersFunction.OrderItemsReserver
{
    public static class OrderItemsReserverToCosmosDb
    {
        private const string ContainerPartitionKey = "/id";
        [Disable]
        [FunctionName("OrderItemsReserverToCosmosDb")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            EnvironmentHelpers.LogEnvironmentVariables(logger);

            var jsonBody = await req.GetRequestBody(logger);
            var jsonObj = CosmosDbHelpers.NormalizeJsonStringForCosmosDb(jsonBody, logger, out var id);

            var container = await CosmosDBConnections.GetContainer(ContainerPartitionKey, logger);

            // The partition key must be present in the JSON string
            logger.LogDebug($"Trying to create item.");
            await container.CreateItemAsync(jsonObj, new PartitionKey(id));

            string responseMessage = $"Body of the request was uploaded to the Azure Cosmos Db [{container.Database.Id}] " +
                                        $"to the container [{container.Id}] " +
                                        $"with id [{id}] " +
                                        $"and partition key [{ContainerPartitionKey}].";

            logger.LogInformation(responseMessage);
            return new OkObjectResult(responseMessage);
        }
    }
}
