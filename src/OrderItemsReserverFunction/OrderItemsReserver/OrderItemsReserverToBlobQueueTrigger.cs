using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EShopOrdersFunction.Helpers.ResourceConnections;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace EShopOrdersFunction.OrderItemsReserver
{
    public static class OrderItemsReserverToBlobQueueTrigger
    {
        [Disable]
        [FunctionName("OrderItemsReserverToBlobQueueTrigger")]
        public static async Task Run(
            [ServiceBusTrigger("ordersqueue", Connection = "ServiceBusConnection")] string myQueueItem,
            ILogger log)
        {
            var blobName = $"{Guid.NewGuid()}.json";
            var blobClient = await BlobConnections.GetBlobClient(blobName);

            log.LogInformation($"Queue item - [{myQueueItem}]");

            var dataBytes = Encoding.UTF8.GetBytes(myQueueItem);
            using var stream = new MemoryStream(dataBytes);
            await blobClient.UploadAsync(stream);

            string responseMessage = $"Queue item was uploaded to the blob container [{blobClient.BlobContainerName}] with name {blobName}.";
            log.LogInformation(responseMessage);
        }

    }
}
