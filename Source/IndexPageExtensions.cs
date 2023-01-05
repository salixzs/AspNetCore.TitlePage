using System.Globalization;
using System.Reflection;

namespace Salix.AspNetCore.TitlePage;

/// <summary>
/// Extension methods to set up Index page options.
/// </summary>
public static class IndexPageExtensions
{
    /// <summary>
    /// Sets the name of API.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="apiName">The short name of API (like "ProductApi").</param>
    public static IndexPage SetName(this IndexPage idxPage, string apiName)
    {
        idxPage.IndexPageOptions.SetName(apiName);
        return idxPage;
    }

    /// <summary>
    /// Sets the description for API Index page options.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="description">The short description of API.</param>
    public static IndexPage SetDescription(this IndexPage idxPage, string description)
    {
        idxPage.IndexPageOptions.SetDescription(description);
        return idxPage;
    }

    /// <summary>
    /// Sets the name of Hosting environment. Use IWebHostEnvironment.Environment name to populate this automatically.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="hostingEnvironmentName">Hosting environment (like "Development", "Test", "Production").</param>
    public static IndexPage SetHostingEnvironment(this IndexPage idxPage, string hostingEnvironmentName)
    {
        idxPage.IndexPageOptions.SetHostingEnvironment(hostingEnvironmentName);
        return idxPage;
    }

    /// <summary>
    /// Build mode for API.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="buildMode">Build mode - Debug, Staging, Production.</param>
    public static IndexPage SetBuildMode(this IndexPage idxPage, string buildMode)
    {
        idxPage.IndexPageOptions.SetBuildMode(buildMode);
        return idxPage;
    }

    /// <summary>
    /// Adds the link button in Index page for additional relevant links.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="buttonTitle">Title of the button.</param>
    /// <param name="linkUrl">The link URL to go to when button is clicked.</param>
    /// <param name="shouldHide">Button is hidden, if true (default - false).</param>
    public static IndexPage AddLinkButton(this IndexPage idxPage, string buttonTitle, string linkUrl, bool shouldHide = false)
    {
        if (!shouldHide)
        {
            idxPage.IndexPageOptions.AddLinkButton(buttonTitle, linkUrl);
        }

        return idxPage;
    }

    /// <summary>
    /// Set the relative address of the Health/Test page for API.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="healthPage">Relative URL of Health Page.</param>
    [Obsolete("Use AddLinkButton(string title, string Url) method to add custom link buttons.", false)]
    public static IndexPage SetHealthPageUrl(this IndexPage idxPage, string healthPage)
    {
        idxPage.IndexPageOptions.SetHealthPageUrl(healthPage);
        return idxPage;
    }

    /// <summary>
    /// Set the relative address of the Swagger UI endpoint.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="swaggerUi">Relative URL of Swagger UI page.</param>
    [Obsolete("Use AddLinkButton(string title, string Url) method to add custom link buttons.", false)]
    public static IndexPage SetSwaggerUrl(this IndexPage idxPage, string swaggerUi)
    {
        idxPage.IndexPageOptions.SetSwaggerUrl(swaggerUi);
        return idxPage;
    }

    /// <summary>
    /// Retrieves Version number from assembly (usually controlled by AssemblyInfo.cs file) and sets as version number for index page.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="assembly">Assembly, for which to get version number.</param>
    /// <param name="partsToReturn">How many parts/numbers to return as version (AssemblyInfo can contain 4 by default or custom).</param>
    public static IndexPage SetVersionFromAssembly(this IndexPage idxPage, Assembly assembly, int partsToReturn = 2)
    {
        string[] versionParts = assembly.GetName().Version.ToString().Split('.');
        string version = versionParts.Length switch
        {
            0 => "Not determined",
            > 3 when partsToReturn == 4 => $"{versionParts[0]}.{versionParts[1]}.{versionParts[2]}.{versionParts[3]}",
            > 2 when partsToReturn >= 3 => $"{versionParts[0]}.{versionParts[1]}.{versionParts[2]}",
            > 1 when partsToReturn >= 2 => $"{versionParts[0]}.{versionParts[1]}",
            _ => versionParts[0]
        };

        idxPage.IndexPageOptions.SetVersion(version);
        return idxPage;
    }

    /// <summary>
    /// Sets the version of API code.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="version">Full version (like "v1", "1.0", "3.12.3").</param>
    public static IndexPage SetVersion(this IndexPage idxPage, string version)
    {
        idxPage.IndexPageOptions.SetVersion(version);
        return idxPage;
    }

    /// <summary>
    /// When non-deterministic version number in AssemblyInfo.cs is used
    /// in format "X.x.*" (X = major version number, x = minor version number, * to fill current datetime as numbers by compiler)
    /// this method can extract this datetime back as typed.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="assembly">Assembly with non-deterministic version number (with *) set in AssemblyInfo.cs</param>
    /// <returns>
    /// Extracted Date and Time or DateTime.Min if something not right.
    /// </returns>
    public static IndexPage SetBuildTimeFromAssembly(this IndexPage idxPage, Assembly assembly)
    {
        string[] version = assembly.GetName().Version.ToString().Split('.');

        if (version.Length != 4)
        {
            idxPage.IndexPageOptions.SetBuildTime(DateTime.MinValue);
        }

        if (!int.TryParse(version[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int build))
        {
            idxPage.IndexPageOptions.SetBuildTime(DateTime.MinValue);
        }

        if (!int.TryParse(version[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int revision))
        {
            idxPage.IndexPageOptions.SetBuildTime(DateTime.MinValue);
        }

        var extractedBuildTime = new DateTime(2000, 1, 1) + new TimeSpan(build, 0, 0, 0) + TimeSpan.FromSeconds(revision * 2);
        if (extractedBuildTime < new DateTime(2020, 1, 1))
        {
            idxPage.IndexPageOptions.SetBuildTime(DateTime.MinValue);
        }
        else
        {
            idxPage.IndexPageOptions.SetBuildTime(new DateTime(2000, 1, 1) + new TimeSpan(build, 0, 0, 0) + TimeSpan.FromSeconds(revision * 2));
        }

        return idxPage;
    }

    /// <summary>
    /// Sets the API build date and time.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="buildTime">Date and Time when API was built. When not specified = defaults to DateTime.Min = hides built time from page.</param>
    public static IndexPage SetBuildTime(this IndexPage idxPage, DateTime buildTime)
    {
        idxPage.IndexPageOptions.SetBuildTime(buildTime);
        return idxPage;
    }

    /// <summary>
    /// Allows to include contents file in page (some data, readme, latest git log).
    /// File should be in root of API application.
    /// </summary>
    /// <param name="idxPage">The index page instance.</param>
    /// <param name="includeFileName">Name of the text/HTML file to include in page.</param>
    public static IndexPage IncludeContentFile(this IndexPage idxPage, string includeFileName)
    {
        idxPage.IndexPageOptions.IncludeContentFile(includeFileName);
        return idxPage;
    }

    /// <summary>
    /// Appends configuration values to page.
    /// </summary>
    /// <param name="idxPage">Page object.</param>
    /// <param name="configurations">Collections of configuration items to add.</param>
    /// <param name="shouldHide">When true (default - false) - will not add given items to page. Used for dynamic control (Hosting.IsDevelopment).</param>
    /// <returns></returns>
    public static IndexPage SetConfigurationValues(this IndexPage idxPage, Dictionary<string, string> configurations, bool shouldHide = false)
    {
        if (!shouldHide)
        {
            idxPage.IndexPageOptions.SetConfigurationValues(configurations);
        }

        return idxPage;
    }

    /// <summary>
    /// Determines whether the specified string to check is integer. Also handles empty/null strings accordingly.
    /// </summary>
    /// <param name="stringToCheck">The string to check.</param>
    /// <returns>
    /// <c>true</c> if the specified string to check is integer; otherwise (incl. empty/null), <c>false</c>.
    /// </returns>
    public static bool IsInteger(this string stringToCheck) => !string.IsNullOrWhiteSpace(stringToCheck) && stringToCheck.Trim().All(char.IsNumber);
}
