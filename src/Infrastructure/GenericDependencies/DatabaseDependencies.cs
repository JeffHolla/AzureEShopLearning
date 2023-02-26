using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.eShopWeb.Infrastructure.GenericDependencies;

public static class DatabaseDependencies
{
    const string CatalogConnection = "CatalogConnection";
    const string IdentityConnection = "IdentityConnection";

    public static void ConfigureDatabasesServices(IConfiguration configuration, IServiceCollection services)
    {
        var useOnlyInMemoryDatabase = false;
        if (configuration["UseOnlyInMemoryDatabase"] != null)
        {
            useOnlyInMemoryDatabase = bool.Parse(configuration["UseOnlyInMemoryDatabase"]);
        }

        if (useOnlyInMemoryDatabase)
        {
            services.AddDbContext<CatalogContext>(c =>
               c.UseInMemoryDatabase("Catalog"));

            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseInMemoryDatabase("Identity"));
        }
        else
        {
            var catalogConStr = configuration.GetConnectionString(CatalogConnection);
            Console.WriteLine("CatalogConnection");
            Console.WriteLine(catalogConStr);

            var identityConStr = configuration.GetConnectionString(IdentityConnection);
            Console.WriteLine("IdentityConnection");
            Console.WriteLine(identityConStr);

            // use real database
            // Requires LocalDB which can be installed with SQL Server Express 2016 if uses local
            // https://www.microsoft.com/en-us/download/details.aspx?id=54284
            services.AddDbContext<CatalogContext>(c =>
                c.UseSqlServer(catalogConStr));

            // Add Identity DbContext
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlServer(identityConStr));
        }
    }
}
