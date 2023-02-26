using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Identity;

namespace Microsoft.eShopWeb.Infrastructure.GenericDependencies;

public static class AzureDependencies
{
    public static void AddGenericAzureServices(IServiceCollection services, IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureAppConfiguration((context, config) =>
        {
            var builtConfig = config.Build();
            Console.WriteLine("Debug views - ");
            Console.WriteLine(builtConfig.GetDebugView());
            Console.WriteLine("----------------------");

            //// Use VaultName from the configuration to create the full vault URI.
            var vaultName = builtConfig["VaultName"];
            var vaultUri = new Uri($"https://{vaultName}.vault.azure.net/");

            //// Load all secrets from the vault into configuration. This will automatically
            //// authenticate to the vault using a managed identity. If a managed identity
            //// is not available, it will check if Visual Studio and/or the Azure CLI are
            //// installed locally and see if they are configured with credentials that can
            //// access the vault.
            config.AddAzureKeyVault(vaultUri, new DefaultAzureCredential());
        });

        services.AddApplicationInsightsTelemetry();
    }
}
