using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microsoft.eShopWeb.ApplicationCore.Clients.FunctionClients;
public abstract class AzureFunctionClient
{
    protected readonly HttpClient _httpClient;

    private Uri FunctionUrl { get; set; }
    private string BaseUrl { get; set; }
    private string Key { get; set; }

    public AzureFunctionClient(IConfiguration configuration, string functionName)
    {
        _httpClient = new HttpClient();

        BaseUrl = configuration.GetSection($"{functionName}:{nameof(BaseUrl)}").Value;
        Key = configuration.GetSection($"{functionName}:{nameof(Key)}").Value;
        FunctionUrl = new Uri($"{BaseUrl}?code={Key}");
    }

    public async Task PostAsJsonAsync<T>(T objectToSend)
    {
        await _httpClient.PostAsJsonAsync(FunctionUrl, objectToSend);
    }
}
