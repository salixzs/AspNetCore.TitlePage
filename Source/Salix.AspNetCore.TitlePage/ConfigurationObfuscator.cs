using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;

namespace Salix.AspNetCore.TitlePage;

/// <summary>
/// Extensions to string values for obfuscation
/// </summary>
public static partial class ConfigurationObfuscator
{
#if NET7_0_OR_GREATER
    [GeneratedRegex(@"(?<key>[^=;,]+)=(?<val>[^;,]+(,\d+)?)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SqlConnectionStringRegex();

    [GeneratedRegex(@"\A(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\z", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex IpAddressDetermineRegex();

    [GeneratedRegex(@"\b(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex IpAddressParseRegex();
#endif

    /// <summary>
    /// Hides sensitive data in SQL connection string. For local server (localhost) does not hide anything.
    /// </summary>
    /// <param name="sqlConnectionString">SQL Connection string.</param>
    /// <param name="partially">If true - will hide server, database and UserId (if present) partially - only password is hidden completely.</param>
    /// <returns>Obfuscated connection string with hidden sensitive parts.</returns>
    public static string ObfuscateSqlConnectionString(this string sqlConnectionString, bool partially = false)
    {
#if NET7_0_OR_GREATER
        var parts = SqlConnectionStringRegex().Matches(sqlConnectionString);
#else
        var sqlConnectionStringRegex = new Regex(@"(?<key>[^=;,]+)=(?<val>[^;,]+(,\d+)?)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeout: TimeSpan.FromMilliseconds(1000));
        var parts = sqlConnectionStringRegex.Matches(sqlConnectionString);
#endif
        var obfuscatedResult = new StringBuilder();
        bool isLocalServer = false;
        foreach (var part in parts.Cast<Match>())
        {
            string key = part.Groups["key"].Value.Trim();
            string value = part.Groups["val"].Value.Trim();

            switch (key.ToUpperInvariant())
            {
                case "DATA SOURCE":
                case "SERVER":
                case "ADDRESS":
                case "ADDR":
                case "NETWORK ADDRESS":
                    if (value.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase)
                        || value.StartsWith(".\\SQLExpress", StringComparison.OrdinalIgnoreCase)
                        || value.Contains("LOCALHOST", StringComparison.CurrentCultureIgnoreCase))
                    {
                        isLocalServer = true;
                        obfuscatedResult
                            .Append("Server=")
                            .Append(value)
                            .Append(';');
                        break;
                    }

                    if (!partially)
                    {
                        obfuscatedResult.Append("Server=[hidden];");
                        break;
                    }

                    // address,port
                    string port = string.Empty;
                    if (value.Contains(','))
                    {
                        string[] split = value.Split(',');
                        if (split.Length == 2)
                        {
                            obfuscatedResult
                                .Append("Server=")
                                .Append(HideValuePartially(split[0]))
                                .Append(',')
                                .Append(split[1])
                                .Append(';');
                            break;
                        }
                    }

                    // server\instance
                    if (value.Contains('\\'))
                    {
                        string[] split = value.Split('\\');
                        if (split.Length == 2)
                        {
                            obfuscatedResult
                                .Append("Server=")
                                .Append(HideValuePartially(split[0]))
                                .Append('\\')
                                .Append(HideValuePartially(split[1]))
                                .Append(';');
                            break;
                        }
                    }

                    obfuscatedResult
                        .Append("Server=")
                        .Append(HideValuePartially(value))
                        .Append(';');
                    break;
                case "INITIAL CATALOG":
                case "DATABASE":
                    if (isLocalServer)
                    {
                        obfuscatedResult
                            .Append("Database=")
                            .Append(value)
                            .Append(';');
                        break;
                    }

                    if (!partially)
                    {
                        obfuscatedResult.Append("Database=[hidden];");
                        break;
                    }

                    obfuscatedResult
                        .Append("Database=")
                        .Append(HideValuePartially(value))
                        .Append(';');
                    break;
                case "USER ID":
                case "UID":
                    if (isLocalServer)
                    {
                        obfuscatedResult
                            .Append("User Id=")
                            .Append(value)
                            .Append(';');
                        break;
                    }

                    if (!partially)
                    {
                        obfuscatedResult.Append("User Id=[hidden];");
                        break;
                    }

                    obfuscatedResult
                        .Append("User Id=")
                        .Append(HideValuePartially(value))
                        .Append(';');
                    break;
                case "PASSWORD":
                case "PWD":
                    if (isLocalServer)
                    {
                        obfuscatedResult
                            .Append("Password=")
                            .Append(value)
                            .Append(';');
                        break;
                    }

                    obfuscatedResult.Append("Password=[hidden];");
                    break;
                default:
                    obfuscatedResult
                        .Append(key)
                        .Append('=')
                        .Append(value)
                        .Append(';');
                    break;
            }
        }

        return obfuscatedResult.ToString();
    }

    /// <summary>
    /// Hides the string value partially, replacing middle part of string (bit more tha half of it) with asterisks (*).
    /// Example: "SomeServer" = "So******er"
    /// </summary>
    /// <param name="initialValue">The initial string value to obfuscate.</param>
    public static string HideValuePartially(this string initialValue)
    {
        if (initialValue.Length < 6)
        {
            return "[hidden]";
        }

        // email address
        if (initialValue.Split('@').Length - 1 == 1 && initialValue.Contains('.'))
        {
            try
            {
                var email = new MailAddress(initialValue);
                string[] hostParts = email.Host.Split('.');
                string hostName = string.Join(".", hostParts, 0, hostParts.Length - 1);
                if (new HashSet<string> { "OUTLOOK", "YANDEX", "HOTMAIL", "ICLOUD", "GMAIL" }.Contains(hostName.ToUpperInvariant()))
                {
                    hostName = "***";
                }
                string topDomain = hostParts[^1];
                return $"{HideValuePartially(email.User)}@{HideValuePartially(hostName)}.{topDomain}";
            }
            catch
            {
                // It is not an e-mail
            }
        }

        // IP address
#if NET7_0_OR_GREATER
        var ipAddressMatches = IpAddressDetermineRegex().Match(initialValue);
        var ipAddressExists = ipAddressMatches != Match.Empty;
#else
        var ipAddressRegex = new Regex(@"\A(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\z", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeout: TimeSpan.FromMilliseconds(1000));
        var ipAddressMatches = ipAddressRegex.Matches(initialValue);
        var ipAddressExists = ipAddressMatches.Count > 0;
#endif
        if (ipAddressExists)
        {
#if NET7_0_OR_GREATER
            var ipParts = IpAddressParseRegex().Matches(initialValue);
#else
            var ipPartsRegex = new Regex(@"\b(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeout: TimeSpan.FromMilliseconds(1000));
            var ipParts = ipPartsRegex.Matches(initialValue);
#endif
            if (ipParts[0].Groups.Count == 5) // whole + 4 parts
            {
                string obfuscatedIp = ipParts[0].Groups[1].Value.Length switch
                {
                    3 => string.Concat(ipParts[0].Groups[1].Value.AsSpan(0, 2), "*"),
                    2 => string.Concat(ipParts[0].Groups[1].Value.AsSpan(0, 1), "*"),
                    _ => "*"
                };
                obfuscatedIp += ".*.*.";
                obfuscatedIp += ipParts[0].Groups[4].Value.Length switch
                {
                    3 => string.Concat("*", ipParts[0].Groups[4].Value.AsSpan(1, 2)),
                    2 => string.Concat("*", ipParts[0].Groups[4].Value.AsSpan(1, 1)),
                    _ => "*"
                };

                return obfuscatedIp;
            }
        }

        // Text middle part is replaced with * ("SomeServer" = "So******er")
        int replaceablePartLength = (initialValue.Length / 2) + 1;
        int lastThirdLength = ((initialValue.Length - replaceablePartLength) / 2) + ((initialValue.Length - replaceablePartLength) % 2);
        if (lastThirdLength > 3)
        {
            lastThirdLength = 3;
        }

        int firstThirdLength = initialValue.Length - replaceablePartLength - lastThirdLength;
        if (firstThirdLength > 3)
        {
            firstThirdLength = 3;
        }

        replaceablePartLength = initialValue.Length - lastThirdLength - firstThirdLength;
        return string.Concat(initialValue.AsSpan(0, firstThirdLength), new string('*', 5), initialValue.AsSpan(firstThirdLength + replaceablePartLength, lastThirdLength));
    }
}
