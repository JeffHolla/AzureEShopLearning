using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EShopOrdersFunction.Helpers.EnvironmentHelpers;
using EShopOrdersFunction.Helpers.HttpHelpers;
using EShopOrdersFunction.Helpers.ResourceConnections;
using EShopOrdersFunction.Helpers.ResourceRelatedHelpers.CosmosDb;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace EShopOrdersFunction.DeliveryOrderProcessor
{
    public static class DeliveryOrderProcessorToCosmosDb
    {
        private const string ContainerPartitionKey = "/id";
        private const string DatabaseName = "ToDoItems";
        private const string ContainerName = "TestContainer";

        [FunctionName("DeliveryOrderProcessorToCosmosDb")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(
            databaseName: DatabaseName,
            containerName: ContainerName,
            CreateIfNotExists = true,
            Connection = "CosmosDBConnection",
            PartitionKey = ContainerPartitionKey)] IAsyncCollector<dynamic> document,
            ILogger logger)
        {
            EnvironmentHelpers.LogEnvironmentVariables(logger);

            var jsonBody = await req.GetRequestBody(logger);
            var objToCosmos = CosmosDbHelpers.NormalizeJsonStringForCosmosDb(jsonBody, logger, out var id);

            // The partition key must be present in the JSON string
            logger.LogDebug($"Adding a task to upload a file.");
            await document.AddAsync(objToCosmos);
            
            string responseMessage = $"Added a task to upload a file to Azure Cosmos Db [{DatabaseName}] " +
                                        $"to the container [{ContainerName}] " +
                                        $"with id [{id}] " +
                                        $"and partition key [{ContainerPartitionKey}].";

            logger.LogInformation(responseMessage);
            return new OkObjectResult(responseMessage);
        }
    }
}
