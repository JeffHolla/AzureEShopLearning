using Microsoft.Extensions.Configuration;

namespace Microsoft.eShopWeb.ApplicationCore.Clients.FunctionClients;

public class OrderItemReserver : AzureFunctionClient
{
    private const string FunctionName = "OrderItemReserver";

    public OrderItemReserver(IConfiguration configuration)
        : base(configuration, FunctionName) { }
}
