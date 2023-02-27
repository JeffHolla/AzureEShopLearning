using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShopOrdersFunction.Helpers.EnvironmentHelpers
{
    public static class EnvironmentHelpers
    {
        public static void LogEnvironmentVariables(ILogger logger)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("-----------------");
            builder.AppendLine("Environment variables -> ");
            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                builder.AppendLine($"{item.Key}: {item.Value}");
            }
            builder.AppendLine("-----------------");

            logger.LogInformation(builder.ToString());
        }
    }
}
