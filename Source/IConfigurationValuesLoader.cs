namespace Salix.AspNetCore.TitlePage;

/// <summary>
/// External logic to load customized set of configuration values to be shown on Index page for API.
/// </summary>
public interface IConfigurationValuesLoader
{
    /// <summary>
    /// Gets the selected configuration values to be shown on Index page for API.
    /// </summary>
    /// <param name="whitelistFilter">The filter of whitelisted configuration keys (keys, that "starts-with" given string).</param>
    public Dictionary<string, string?> GetConfigurationValues(HashSet<string>? whitelistFilter = null);
}
