using System.Configuration;
using System.Net.Mime;
using Ardalis.ListStartupServices;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using BlazorAdmin;
using BlazorAdmin.Services;
using Blazored.LocalStorage;
using BlazorShared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.ApplicationCore.Clients.FunctionClients;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.GenericDependencies;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web;
using Microsoft.eShopWeb.Web.Configuration;
using Microsoft.eShopWeb.Web.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddConsole();

        AddAzureServices(builder.Services, builder.Configuration, builder.Host);

        DatabaseDependencies.ConfigureDatabasesServices(builder.Configuration, builder.Services);

        builder.Services.AddCoreServices(builder.Configuration);
        builder.Services.AddWebServices(builder.Configuration);

        AddAzureServices(builder.Services, builder.Configuration, builder.Host);

        AddWebAndDatabaseServices(builder.Services, builder.Configuration);

        await RunApp(builder);
    }

    private static async Task RunApp(WebApplicationBuilder builder)
    {
        // App request pipeline
        var app = builder.Build();

        app.Logger.LogInformation("App created...");

        app.Logger.LogInformation("Seeding Database...");

        using (var scope = app.Services.CreateScope())
        {
            var scopedProvider = scope.ServiceProvider;
            try
            {
                var catalogContext = scopedProvider.GetRequiredService<CatalogContext>();
                await CatalogContextSeed.SeedAsync(catalogContext, app.Logger);

                var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var identityContext = scopedProvider.GetRequiredService<AppIdentityDbContext>();
                await AppIdentityDbContextSeed.SeedAsync(identityContext, userManager, roleManager);
            }
            catch (Exception ex)
            {
                app.Logger.LogError(ex, "An error occurred seeding the DB.");
            }
        }

        var catalogBaseUrl = builder.Configuration.GetValue(typeof(string), "CatalogBaseUrl") as string;
        if (!string.IsNullOrEmpty(catalogBaseUrl))
        {
            app.Use((context, next) =>
            {
                context.Request.PathBase = new PathString(catalogBaseUrl);
                return next();
            });
        }

        app.UseHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    var result = new
                    {
                        status = report.Status.ToString(),
                        errors = report.Entries.Select(e => new
                        {
                            key = e.Key,
                            value = Enum.GetName(typeof(HealthStatus), e.Value.Status)
                        })
                    }.ToJson();
                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(result);
                }
            });
        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
        {
            app.Logger.LogInformation("Adding Development middleware...");
            app.UseDeveloperExceptionPage();
            app.UseShowAllServicesMiddleware();
            app.UseMigrationsEndPoint();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.Logger.LogInformation("Adding non-Development middleware...");
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseCookiePolicy();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute("default", "{controller:slugify=Home}/{action:slugify=Index}/{id?}");
            endpoints.MapRazorPages();
            endpoints.MapHealthChecks("home_page_health_check", new HealthCheckOptions { Predicate = check => check.Tags.Contains("homePageHealthCheck") });
            endpoints.MapHealthChecks("api_health_check", new HealthCheckOptions { Predicate = check => check.Tags.Contains("apiHealthCheck") });
            //endpoints.MapBlazorHub("/admin");
            endpoints.MapFallbackToFile("index.html");
        });

        app.Logger.LogInformation("LAUNCHING");
        app.Run();
    }

    private static void AddAzureServices(IServiceCollection services, IConfiguration configuration, IHostBuilder hostBuilder)
    {
        AzureDependencies.AddGenericAzureServices(services, hostBuilder);

        // Or does it makes sense to use Scoped services?
        services.AddSingleton<OrderItemReserver>(new OrderItemReserver(configuration));
        services.AddSingleton<DeliveryOrderProcessor>(new DeliveryOrderProcessor(configuration));

        //services.AddSingleton<ServiceBusClient> // We can create ServiceBusClient for injection using same way
        services.AddSingleton<ServiceBusSender>(options =>
        {
            var config = options.GetService<IConfiguration>();

            var serviceBusOptions = config.GetRequiredSection("ServiceBusOptions");
            var serviceBusConnectionStr = serviceBusOptions.GetRequiredSection("ConnectionString").Value;
            var queueName = serviceBusOptions.GetRequiredSection("QueueName").Value;

            var serviceBusClient = new ServiceBusClient(serviceBusConnectionStr);

            return serviceBusClient.CreateSender(queueName);
        });
    }

    private static void AddWebAndDatabaseServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCookieSettings();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

        services.AddIdentity<ApplicationUser, IdentityRole>()
                   .AddDefaultUI()
                   .AddEntityFrameworkStores<AppIdentityDbContext>()
                                   .AddDefaultTokenProviders();

        services.AddScoped<ITokenClaimsService, IdentityTokenClaimService>();

        // Add memory cache services
        services.AddMemoryCache();
        services.AddRouting(options =>
        {
            // Replace the type and the name used to refer to it with your own
            // IOutboundParameterTransformer implementation
            options.ConstraintMap["slugify"] = typeof(SlugifyParameterTransformer);
        });

        services.AddMvc(options =>
        {
            options.Conventions.Add(new RouteTokenTransformerConvention(
                     new SlugifyParameterTransformer()));

        });
        services.AddControllersWithViews();
        services.AddRazorPages(options =>
        {
            options.Conventions.AuthorizePage("/Basket/Checkout");
        });
        services.AddHttpContextAccessor();
        services
            .AddHealthChecks()
            .AddCheck<ApiHealthCheck>("api_health_check", tags: new[] { "apiHealthCheck" })
            .AddCheck<HomePageHealthCheck>("home_page_health_check", tags: new[] { "homePageHealthCheck" });

        services.Configure<ServiceConfig>(config =>
        {
            config.Services = new List<ServiceDescriptor>(services);
            config.Path = "/allservices";
        });

        // blazor configuration
        var configSection = configuration.GetRequiredSection(BaseUrlConfiguration.CONFIG_NAME);
        services.Configure<BaseUrlConfiguration>(configSection);
        var baseUrlConfig = configSection.Get<BaseUrlConfiguration>();

        // Blazor Admin Required Services for Prerendering
        services.AddScoped(s => new HttpClient
        {
            BaseAddress = new Uri(baseUrlConfig.WebBase)
        });

        // add blazor services
        services.AddBlazoredLocalStorage();
        services.AddServerSideBlazor();
        services.AddScoped<ToastService>();
        services.AddScoped<HttpService>();
        services.AddBlazorServices();

        services.AddDatabaseDeveloperPageExceptionFilter();
    }
}
