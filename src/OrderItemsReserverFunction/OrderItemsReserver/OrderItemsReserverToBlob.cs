using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EShopOrdersFunction.Helpers.ResourceConnections;

namespace EShopOrdersFunction.OrderItemsReserver
{
    public static class OrderItemsReserverToBlobHttpTrigger
    {
        [Disable]
        [FunctionName("OrderItemsReserverToBlobHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest request,
            ILogger log)
        {
            var blobName = $"{DateTime.UtcNow:u} {Guid.NewGuid()} .json";
            var blobClient = await BlobConnections.GetBlobClient(blobName);

            string jsonBody = await request.ReadAsStringAsync();
            log.LogInformation("Json body was readed.");
            log.LogInformation($"Body - [{jsonBody}]");

            if (string.IsNullOrWhiteSpace(jsonBody))
            {
                var errorMessage = "Body data wasn't provided!";
                log.LogCritical(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            await blobClient.UploadAsync(request.Body);

            string responseMessage = $"Body of the request was uploaded to the blob container [{blobClient.BlobContainerName}] with name {blobName}.";

            log.LogInformation(responseMessage);
            return new OkObjectResult(responseMessage);
        }
    }
}
