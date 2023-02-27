using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShopOrdersFunction.Helpers.ResourceRelatedHelpers.CosmosDb
{
    public static class CosmosDbHelpers
    {
        public static JObject NormalizeJsonStringForCosmosDb(string jsonBody, ILogger log, out string id)
        {
            var jsonObj = JObject.Parse(jsonBody);

            if (!jsonObj.ContainsKey("Id") && !jsonObj.ContainsKey("id"))
            {
                var errorMessage = "Json doesn't contains 'Id' or 'id' property!";
                log.LogCritical(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            id = jsonObj.ContainsKey("Id")
                        ? jsonObj["Id"].Value<string>()
                        : jsonObj["id"].Value<string>();

            // We MUST add an id property to the CreateItemAsync method (if it doesn't exist or doesn't look like 'id') and id should be a string
            if (jsonObj.ContainsKey("Id"))
            {
                jsonObj.Add("id", id.ToString());
            }
            else
            {
                jsonObj["id"] = id.ToString();
            }

            return jsonObj;
        }
    }
}
