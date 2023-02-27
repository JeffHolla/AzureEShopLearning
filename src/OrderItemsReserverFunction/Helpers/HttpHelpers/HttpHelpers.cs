using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EShopOrdersFunction.Helpers.HttpHelpers
{
    public static class HttpHelpers
    {
        public static async Task<string> GetRequestBody(this HttpRequest request, ILogger log)
        {
            string jsonBody = await request.ReadAsStringAsync(); // uses utf-8 for default encoding
            log.LogInformation("Json body was readed.");
            log.LogInformation($"Body - [{jsonBody}]");

            if (string.IsNullOrWhiteSpace(jsonBody))
            {
                var errorMessage = "Body data wasn't provided!";
                log.LogCritical(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            return jsonBody;
        }
    }
}
