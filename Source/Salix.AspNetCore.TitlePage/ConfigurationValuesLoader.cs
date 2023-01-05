using Microsoft.Extensions.Configuration;

namespace Salix.AspNetCore.TitlePage;

/// <inheritdoc/>
public class ConfigurationValuesLoader : IConfigurationValuesLoader
{
    private readonly IConfigurationRoot _configuration;

    /// <inheritdoc cref="IConfigurationValuesLoader"/>
    public ConfigurationValuesLoader(IConfiguration configuration) => _configuration = (IConfigurationRoot)configuration;

    /// <inheritdoc/>
    public Dictionary<string, string?> GetConfigurationValues(HashSet<string>? whitelistFilter = null)
    {
        var selectedConfigurations = new Dictionary<string, string?>();
        void RecurseChildren(IEnumerable<IConfigurationSection> children, string parentKey)
        {
            foreach (var child in children)
            {
                string totalKey;
                if (child.Key.IsInteger())
                {
                    totalKey = string.IsNullOrEmpty(parentKey) ? child.Key : $"{parentKey}[{child.Key}]";
                }
                else if (!string.IsNullOrEmpty(parentKey) && parentKey.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                {
                    totalKey = string.IsNullOrEmpty(parentKey) ? child.Key : $"{parentKey}.{child.Key}";
                }
                else
                {
                    totalKey = string.IsNullOrEmpty(parentKey) ? child.Key : $"{parentKey}/{child.Key}";
                }

                (string? value, var provider) = GetValueAndProvider(_configuration, child.Path);
                if (provider != null && (whitelistFilter == null || (whitelistFilter?.Any(k => totalKey.StartsWith(k, StringComparison.OrdinalIgnoreCase)) == true)))
                {
                    if (provider.GetType().BaseType?.Name == "FileConfigurationProvider")
                    {
                        selectedConfigurations.Add($"{totalKey} ({((FileConfigurationProvider)provider).Source.Path})", value);
                        continue;
                    }

                    switch (provider.GetType().Name)
                    {
                        case "EnvironmentVariablesConfigurationProvider":
                            selectedConfigurations.Add($"{totalKey} (ENV)", value);
                            continue;
                        case "CommandLineConfigurationProvider":
                            selectedConfigurations.Add($"{totalKey} (CMD)", value);
                            continue;
                        case "KeyPerFileConfigurationProvider":
                            selectedConfigurations.Add($"{totalKey} (KeyFile)", value);
                            continue;
                        case "AzureAppConfigurationProvider":
                            selectedConfigurations.Add($"{totalKey} (Azure AppCfg)", value);
                            continue;
                        case "AzureKeyVaultConfigurationProvider":
                            selectedConfigurations.Add($"{totalKey} (KeyVault)", value);
                            continue;
                        default:
                            selectedConfigurations.Add($"{totalKey} (SYS/MEM)", value);
                            break;
                    }
                }
                else
                {
                    RecurseChildren(child.GetChildren(), totalKey);
                }
            }
        }

        RecurseChildren(_configuration.GetChildren(), "");
        return selectedConfigurations;
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
