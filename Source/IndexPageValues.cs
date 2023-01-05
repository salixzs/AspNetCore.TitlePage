using System.Diagnostics;

namespace Salix.AspNetCore.TitlePage;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class IndexPageValues
{
    /// <summary>
    /// Create new Index page values class with default values.
    /// </summary>
    public IndexPageValues()
    {
    }

    /// <summary>
    /// Create new Index page values class.
    /// </summary>
    /// <param name="apiName">Name of the API.</param>
    public IndexPageValues(string apiName) => this.ApiName = apiName;

    /// <summary>
    /// Short name of the API.
    /// </summary>
    public string ApiName { get; internal set; } = "API";

    /// <summary>
    /// Populate with description of API.
    /// </summary>
    public string Description { get; internal set; } = "API is successfully launched.";

    /// <summary>
    /// Populate with version number for API.
    /// </summary>
    public string Version { get; internal set; } = "Undetermined";

    /// <summary>
    /// Specify Date and Time when API was built.
    /// Defaults to DateTime.Min = hides built time from page.
    /// </summary>
    public DateTime BuiltTime { get; internal set; } = DateTime.MinValue;

    /// <summary>
    /// Environment name, where API is hosted.
    /// Use IWebHostEnvironment.Environment name to populate this automatically.
    /// </summary>
    public string HostingEnvironment { get; internal set; } = "Undetermined";

    /// <summary>
    /// Populate with text, describing which mode API was built (DEBUG, PRODUCTION).
    /// </summary>
    public string BuildMode { get; internal set; } = "Undetermined";

    /// <summary>
    /// Buttons to be added to Index page, where key = Button title and Value = URL to open.
    /// </summary>
    public Dictionary<string, string> LinkButtons { get; internal set; } = new Dictionary<string, string>();

    /// <summary>
    /// Name of the file (txt, html) to be included in index page, located at root.
    /// </summary>
    public string? IncludeFileName { get; internal set; }

    /// <summary>
    /// Key-value pairs of configuration values to be shown on index page.
    /// </summary>
    public Dictionary<string, string>? Configurations { get; internal set; }

    /// <summary>
    /// Displays object main properties in Debug screen. (Only for development purposes).
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{this.ApiName} v{this.Version})";
}

internal static class IndexPageValuesFluentExtensions
{
    /// <summary>
    /// Sets the name of API.
    /// </summary>
    /// <param name="idxPageValues">The index page options.</param>
    /// <param name="apiName">The short name of API (like "ProductApi").</param>
    public static IndexPageValues SetName(this IndexPageValues idxPageValues, string apiName)
    {
        idxPageValues.ApiName = string.IsNullOrEmpty(apiName) ? "API" : apiName;
        return idxPageValues;
    }

    /// <summary>
    /// Sets the description for API Index page options.
    /// </summary>
    /// <param name="idxPageValues">The index page values.</param>
    /// <param name="description">The short description of API.</param>
    public static IndexPageValues SetDescription(this IndexPageValues idxPageValues, string description)
    {
        idxPageValues.Description = string.IsNullOrEmpty(description) ? "API is successfully launched." : description;
        return idxPageValues;
    }

    /// <summary>
    /// Adds the version (number) to API Index page options.
    /// </summary>
    /// <param name="idxPageValues">The index page values.</param>
    /// <param name="version">Full version (like "v1", "1.0", "3.12.3").</param>
    public static IndexPageValues SetVersion(this IndexPageValues idxPageValues, string version)
    {
        idxPageValues.Version = string.IsNullOrEmpty(version) ? "Undetermined" : version;
        return idxPageValues;
    }

    /// <summary>
    /// Sets the API build date and time.
    /// </summary>
    /// <param name="idxPageValues">The index page values.</param>
    /// <param name="builtTime">Date and Time when API was built. When not specified = defaults to DateTime.Min = hides built time from page.</param>
    public static IndexPageValues SetBuildTime(this IndexPageValues idxPageValues, DateTime builtTime)
    {
        idxPageValues.BuiltTime = builtTime;
        return idxPageValues;
    }

    /// <summary>
    /// Sets the name of Hosting environment. Use IWebHostEnvironment.Environment name to populate this automatically.
    /// </summary>
    /// <param name="idxPageValues">The index page values.</param>
    /// <param name="hostingEnvironmentName">Full version (like "v1", "1.0", "3.12.3").</param>
    public static IndexPageValues SetHostingEnvironment(this IndexPageValues idxPageValues, string hostingEnvironmentName)
    {
        idxPageValues.HostingEnvironment = string.IsNullOrEmpty(hostingEnvironmentName) ? "Undetermined" : hostingEnvironmentName;
        return idxPageValues;
    }

    /// <summary>
    /// Sets the name of build mode, describing in which mode API was built (DEBUG, PRODUCTION).
    /// </summary>
    /// <param name="idxPageValues">The index page values.</param>
    /// <param name="buildMode">Short description of build mode.</param>
    public static IndexPageValues SetBuildMode(this IndexPageValues idxPageValues, string buildMode)
    {
        idxPageValues.BuildMode = string.IsNullOrEmpty(buildMode) ? "Undetermined" : buildMode;
        return idxPageValues;
    }

    /// <summary>
    /// Add link button title and Url to index page link buttons collection.
    /// </summary>
    /// <param name="idxPageValues">The index page values.</param>
    /// <param name="buttonTitle">The Title text to place on button.</param>
    /// <param name="linkUrl">The link URL to open when button is clicked.</param>
    public static IndexPageValues AddLinkButton(this IndexPageValues idxPageValues, string buttonTitle, string linkUrl)
    {
        idxPageValues.LinkButtons.Add(buttonTitle, linkUrl);
        return idxPageValues;
    }

    /// <summary>
    /// Relative address of the Health/Test page for API.
    /// </summary>
    /// <param name="idxPageValues">The index page values.</param>
    /// <param name="healthPage">Relative URL of Health Page.</param>
    [Obsolete("Use AddLinkButton(string title, string Url) method to add custom link buttons.", false)]
    public static IndexPageValues SetHealthPageUrl(this IndexPageValues idxPageValues, string healthPage)
    {
        idxPageValues.LinkButtons.Add("Health/Test", healthPage);
        return idxPageValues;
    }

    /// <summary>
    /// When using and configuring Swagger, populate with address, where it publishes its page.
    /// When not specified - will not display link to it.
    /// </summary>
    /// <param name="idxPageValues">The index page values.</param>
    /// <param name="swaggerUrl">Relative URL of the Swagger UI page.</param>
    [Obsolete("Use AddLinkButton(string title, string Url) method to add custom link buttons.", false)]
    public static IndexPageValues SetSwaggerUrl(this IndexPageValues idxPageValues, string swaggerUrl)
    {
        idxPageValues.LinkButtons.Add("Swagger", swaggerUrl);
        return idxPageValues;
    }

    /// <summary>
    /// Allows to include contents file in page (some data, readme, latest git log).
    /// File should be in root of API application.
    /// </summary>
    /// <param name="idxPageValues">The index page values.</param>
    /// <param name="fileName">Name of the text/HTML file to include in page.</param>
    public static IndexPageValues IncludeContentFile(this IndexPageValues idxPageValues, string fileName)
    {
        idxPageValues.IncludeFileName = fileName;
        return idxPageValues;
    }

    /// <summary>
    /// Assigns Key-Value pair dictionary of configuration values to show on index page.
    /// </summary>
    /// <param name="idxPageValues">The index page values.</param>
    /// <param name="configurationValues">Key-Value pairs of configuration values.</param>
    public static IndexPageValues SetConfigurationValues(this IndexPageValues idxPageValues, Dictionary<string, string> configurationValues)
    {
        idxPageValues.Configurations = configurationValues;
        return idxPageValues;
    }
}
