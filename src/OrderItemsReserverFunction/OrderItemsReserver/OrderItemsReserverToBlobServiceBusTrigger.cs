using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EShopOrdersFunction.Helpers.ResourceConnections;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace EShopOrdersFunction.OrderItemsReserver
{
    public static class OrderItemsReserverToBlobServiceBusTrigger
    {
        private const string ServiceBusQueueName = "OrderItemsReserverQueue";
        private const string ContainerName = "order-item-reservations";

        [FunctionName("OrderItemsReserverToBlobServiceBusTrigger")]
        public static async Task Run(
            [ServiceBusTrigger(ServiceBusQueueName, Connection = "ServiceBusConnection")] string myQueueItem,
            ILogger log)
        {
            var blobName = $"{Guid.NewGuid()}.json";
            var blobClient = await BlobConnections.GetBlobClient(blobName, ContainerName);

            log.LogInformation($"Queue item - [{myQueueItem}]");

            var dataBytes = Encoding.UTF8.GetBytes(myQueueItem);
            try
            {
                using var stream = new MemoryStream(dataBytes);
                await blobClient.UploadAsync(stream);
            }
            catch (Exception ex)
            {
                // Sending post email
                await new HttpClient().PostAsJsonAsync("https://prod-75.eastus.logic.azure.com:443/workflows/14b723afa0594e6a9ebddc0184aab723/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=m8MRUYtsjirrL_l-9wbgE7uyxqXUtlkE82y7-IgCXzA", ex.Message);
            }

            string responseMessage = $"Queue item was uploaded to the blob container [{blobClient.BlobContainerName}] with name {blobName}.";
            log.LogInformation(responseMessage);
        }

    }
}
