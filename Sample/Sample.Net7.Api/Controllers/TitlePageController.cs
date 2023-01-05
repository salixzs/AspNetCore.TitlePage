using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Salix.AspNetCore.TitlePage;

namespace Sample.Net7.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TitlePageController : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IConfigurationValuesLoader _configLoader;
    private readonly ILogger<WeatherForecastController> _logger;

    public TitlePageController(
        IWebHostEnvironment hostingEnvironment,
        IConfigurationValuesLoader configLoader,
        ILogger<WeatherForecastController> logger)
    {
        _hostingEnvironment = hostingEnvironment;
        _configLoader = configLoader;
        _logger = logger;
    }

    [HttpGet("/")]
    public ContentResult DisplayTitlePage()
    {
        // Load filtered configuration items from entire configuration based on given whitelist filter
        var configurationItems =
            _configLoader.GetConfigurationValues(new HashSet<string>
            {
                    "AllowedHosts", "contentRoot", "Logging/LogLevel", "LogicConfiguration", "DatabaseConnection"
            });

        // #if !DEBUG <--- Do that only when running not in DEBUG mode
        var obfuscatedConfig = ObfuscateConfigurationValues(configurationItems);
        // #endif

        var apiAssembly = Assembly.GetAssembly(typeof(TitlePageController));
        var apiAssemblyVersion = apiAssembly!.GetName().Version ?? new Version(1, 0, 0, 0);
        var indexPage = new IndexPage("Sample API")
            .SetDescription("Demonstrating capabilities of Salix.FrontPage NuGet package.")
            .SetHostingEnvironment(_hostingEnvironment.EnvironmentName)
            .SetVersion(apiAssemblyVersion.ToString())        // Reads version from Assembly info
            .SetBuildTime(GetBuildTime(                       // Setting build time calculated from Assemly auto-incrementing approach
                new DateTime(2023, 1, 5, 17, 0, 0),              // Latest version "start" date
                apiAssemblyVersion.Build,
                apiAssemblyVersion.Revision))
            .AddLinkButton("Health/Test", "/health/status")   // Possibility to add URLs as buttons to some additiona functionality
            .AddLinkButton("Swagger", "/swagger")             // Like going to Swagger UI page
            .AddLinkButton("Hangfire", "/hangfire")           // Or hangfire management page
            .SetConfigurationValues(obfuscatedConfig)         // Populates shown configuration values
            .IncludeContentFile("build_data.html");           // Speficy external content file to add to title page (normally should be generated during build)

        // "Hacking" to understand what mode API is getting compiled.
#if DEBUG
        indexPage.SetBuildMode("#DEBUG (Should not be in production!)");
#else
            indexPage.SetBuildMode("Release");
#endif
        return new ContentResult
        {
            ContentType = "text/html",
            StatusCode = (int)HttpStatusCode.OK,
            Content = indexPage.GetContents(),
        };
    }

    private static DateTime GetBuildTime(DateTime versionStartDate, int daysSinceStart, int minutesSinceMidnight) =>
        versionStartDate.AddDays(daysSinceStart).Date.AddMinutes(minutesSinceMidnight);

    /// <summary>
    /// Obfuscates the configuration values for safe-ish showing on index page.
    /// </summary>
    /// <param name="configurationItems">The loaded original configuration items.</param>
    /// <returns>Same list of configuration values, where selected are obfuscated.</returns>
    private static Dictionary<string, string> ObfuscateConfigurationValues(Dictionary<string, string> configurationItems)
    {
        var obfuscated = new Dictionary<string, string>();
        foreach (var original in configurationItems)
        {
            if (original.Key.StartsWith("LogicConfiguration/SomeIp", StringComparison.OrdinalIgnoreCase)
                || original.Key.StartsWith("LogicConfiguration/SomeEmail", StringComparison.OrdinalIgnoreCase)
                || original.Key.StartsWith("LogicConfiguration/SomeArray[1].Id", StringComparison.OrdinalIgnoreCase))
            {
                obfuscated.Add(original.Key, original.Value.HideValuePartially());
                continue;
            }

            if (original.Key.StartsWith("DatabaseConnection", StringComparison.OrdinalIgnoreCase))
            {
                obfuscated.Add(original.Key, original.Value.ObfuscateSqlConnectionString(true));
                continue;
            }

            obfuscated.Add(original.Key, original.Value);
        }
        return obfuscated;
    }
}
