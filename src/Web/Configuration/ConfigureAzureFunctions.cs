using Microsoft.eShopWeb.ApplicationCore.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.eShopWeb.Web.Configuration;

public static class ConfigureAzureFunctions
{
    public const string EnvironmentVariable = "ASPNETCORE_ENVIRONMENT";
    public const string FuncUrl = "OrderItemReserverFunctionSettings_Url";
    public const string FuncKey = "OrderItemReserverFunctionSettings_Key";
    public const string FunctionJsonSection = "OrderItemReserverFunctionSettings";

    public static IServiceCollection AddAzureFunctions(this IServiceCollection services, IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable(EnvironmentVariable);
        switch (env)
        {
            case "Production":
                var orderItemReserverSettings = new OrderItemReserverFunctionSettings
                {
                    Url = Environment.GetEnvironmentVariable(FuncUrl),
                    Key = Environment.GetEnvironmentVariable(FuncKey)
                };
                services.AddSingleton<OrderItemReserverFunctionSettings>(orderItemReserverSettings);
                break;

            case "Development":
                var section = configuration.GetRequiredSection(FunctionJsonSection);
                services.Configure<OrderItemReserverFunctionSettings>(configuration.GetRequiredSection(FunctionJsonSection));
                services.AddSingleton(section.Get<OrderItemReserverFunctionSettings>());
                break;
        }

        return services;
    }
}
