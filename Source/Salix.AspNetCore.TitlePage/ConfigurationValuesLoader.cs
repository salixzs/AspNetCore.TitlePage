using Microsoft.Extensions.Configuration;

namespace Salix.AspNetCore.TitlePage;

/// <inheritdoc/>
public class ConfigurationValuesLoader : IConfigurationValuesLoader
{
    private readonly IConfigurationRoot _configuration;

    /// <inheritdoc cref="IConfigurationValuesLoader"/>
    public ConfigurationValuesLoader(IConfiguration configuration) => _configuration = (IConfigurationRoot)configuration;

    public Dictionary<string, string?> GetConfigurationValues(HashSet<string>? whitelistFilter = null)
    {
        var selectedConfigurations = new Dictionary<string, string?>();
        RecurseChildren(_configuration.GetChildren(), "", selectedConfigurations, whitelistFilter);
        return selectedConfigurations;
    }

    private void RecurseChildren(IEnumerable<IConfigurationSection> children, string parentKey, Dictionary<string, string?> selectedConfigurations, HashSet<string>? whitelistFilter)
    {
        foreach (var child in children)
        {
            string totalKey = BuildTotalKey(parentKey, child.Key);
            (string? value, var provider) = GetValueAndProvider(_configuration, child.Path);

            if (provider != null && (whitelistFilter?.Any(k => totalKey.StartsWith(k, StringComparison.OrdinalIgnoreCase)) != false))
            {
                AddConfiguration(selectedConfigurations, totalKey, value, provider);
            }
            else
            {
                RecurseChildren(child.GetChildren(), totalKey, selectedConfigurations, whitelistFilter);
            }
        }
    }

    private string BuildTotalKey(string parentKey, string childKey)
    {
        if (childKey.IsInteger())
        {
            return string.IsNullOrEmpty(parentKey) ? childKey : $"{parentKey}[{childKey}]";
        }

        if (!string.IsNullOrEmpty(parentKey))
        {
            return parentKey.EndsWith(']') ? $"{parentKey}.{childKey}" : $"{parentKey}/{childKey}";
        }

        return childKey;
    }

    private void AddConfiguration(Dictionary<string, string?> selectedConfigurations, string totalKey, string? value, IConfigurationProvider provider)
    {
        string providerType = provider.GetType().Name;
        string providerSource = providerType switch
        {
            "FileConfigurationProvider" => $" ({((FileConfigurationProvider)provider).Source.Path})",
            "EnvironmentVariablesConfigurationProvider" => " (ENV)",
            "CommandLineConfigurationProvider" => " (CMD)",
            "KeyPerFileConfigurationProvider" => " (KeyFile)",
            "AzureAppConfigurationProvider" => " (Azure AppCfg)",
            "AzureKeyVaultConfigurationProvider" => " (KeyVault)",
            _ => " (SYS/MEM)"
        };

        selectedConfigurations.Add($"{totalKey}{providerSource}", value);
    }

    private static (string? Value, IConfigurationProvider? Provider) GetValueAndProvider(
        IConfigurationRoot root,
        string key)
    {
        foreach (var provider in root.Providers.Reverse())
        {
            if (provider.TryGet(key, out string? value))
            {
                return (value, provider);
            }
        }

        return (null, null);
    }
}
