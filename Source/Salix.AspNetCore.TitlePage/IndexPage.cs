using System.Text;
using System.Text.RegularExpressions;

namespace Salix.AspNetCore.TitlePage;

/// <summary>
/// Class to compose landing/index page for API.
/// </summary>
public class IndexPage
{
    private static readonly Regex BodyContentRegex = new(@"(?s)(?<=<body>).+(?=<\/body>)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

    internal IndexPageValues IndexPageOptions { get; } = new IndexPageValues();

    /// <summary>
    /// Index page with default options/values.
    /// </summary>
    public IndexPage() { }

    /// <summary>
    /// Index page with default options/values and specified name.
    /// </summary>
    /// <param name="apiName">Name of the API</param>
    public IndexPage(string apiName) => this.IndexPageOptions.SetName(apiName);

    /// <summary>
    /// Retrieves Index/Landing page as string, containing ready-made HTML.
    /// </summary>
    public string GetContents()
    {
        // {IncludeFile} = <div class="column"></div> or string empty
        var indexHtml = new StringBuilder(PageHtml.index);
        indexHtml = indexHtml
            .Replace("{ApiName}", this.IndexPageOptions.ApiName)
            .Replace("{Description}", this.IndexPageOptions.Description)
            .Replace("{Version}", this.IndexPageOptions.Version)
            .Replace("{Environment}", this.IndexPageOptions.HostingEnvironment)
            .Replace("{Mode}", this.IndexPageOptions.BuildMode);

        indexHtml = this.IndexPageOptions.BuiltTime == DateTime.MinValue
            ? indexHtml.Replace("{Built}", "---")
            : indexHtml.Replace("{Built}", this.IndexPageOptions.BuiltTime.ToHumanDateString());

        if (this.IndexPageOptions.LinkButtons.Count > 0)
        {
            var buttons = new StringBuilder("<hr/>");
            buttons.AppendLine("<p style=\"margin-top:2em;\">");
            foreach (var button in this.IndexPageOptions.LinkButtons)
            {
                buttons.Append("<a class=\"button\" href=\"").Append(button.Value).Append("\">").Append(button.Key).AppendLine("</a>");
            }

            indexHtml = indexHtml.Replace("{Buttons}", buttons.ToString());
        }
        else
        {
            indexHtml = indexHtml.Replace("{Buttons}", string.Empty);
        }

        indexHtml = string.IsNullOrEmpty(this.IndexPageOptions.IncludeFileName)
            ? indexHtml
                .Replace("{OneColumnStyle}", "min-width:100%;")
                .Replace("{IncludeFile}", string.Empty)
            : indexHtml
                .Replace("{OneColumnStyle}", "padding-right: 2rem;")
                .Replace("{IncludeFile}", LoadFileContents(this.IndexPageOptions.IncludeFileName ?? "Missing file"));

        indexHtml = this.IndexPageOptions.Configurations?.Count > 0
            ? indexHtml.Replace("{ConfigValues}", this.GenerateConfigurationsTable())
            : indexHtml.Replace("{ConfigValues}", "Configuration values are hidden for security purposes.");

        return indexHtml.ToString();
    }

    private static string LoadFileContents(string includeFileName)
    {
        if (!File.Exists(includeFileName))
        {
            return $"<div class=\"column\"><h1>Included contents</h1><p>Contents file {includeFileName} not found!</p></div>";
        }

        if (new FileInfo(includeFileName).Length > 51200)
        {
            return $"<div class=\"column\"><h1>Included contents</h1><p>Contents file {includeFileName} is too big!</p></div>";
        }

        string contents = File.ReadAllText(includeFileName, Encoding.UTF8);
        if (Path.GetExtension(includeFileName).StartsWith(".HTM", StringComparison.OrdinalIgnoreCase))
        {
            if (contents.Contains("<body>"))
            {
                contents = BodyContentRegex.Match(contents).Value;
            }

            return $"<div class=\"column\">{contents}</div>";
        }

        return $"<div class=\"column\"><h1>Included contents</h1><pre>{contents}</pre></div>";
    }

    private string GenerateConfigurationsTable()
    {
        var builder = new StringBuilder();
        builder.AppendLine("<table>")
            .AppendLine("<thead>")
            .AppendLine("<tr><th>Key</th><th>Value</th></tr>")
            .AppendLine("</thead>")
            .AppendLine("<tbody>");
#pragma warning disable CS8602 // Dereference of a possibly null reference - called when checked
        foreach (var cfg in this.IndexPageOptions.Configurations)
        {
            builder.Append("<tr><td>").Append(cfg.Key).Append("</td><td>").Append(cfg.Value).AppendLine("</td></tr>");
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        builder.AppendLine("</tbody>")
            .AppendLine("</table>");

        return builder.ToString();
    }
}
