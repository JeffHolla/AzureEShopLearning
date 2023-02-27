using Microsoft.Extensions.Configuration;

namespace Microsoft.eShopWeb.ApplicationCore.Clients.FunctionClients;

public class DeliveryOrderProcessor : AzureFunctionClient
{
    private const string FunctionName = "DeliveryOrderProcessor";

    public DeliveryOrderProcessor(IConfiguration configuration)
        : base(configuration, FunctionName) { }
}
