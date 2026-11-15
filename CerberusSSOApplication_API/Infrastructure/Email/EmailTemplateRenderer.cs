using System.Reflection;
using System.Text.Encodings.Web;

namespace Infrastructure.Email;

public class EmailTemplateRenderer
{
    public static string Render(string templateName, IReadOnlyDictionary<string, string> payload)
    {
        var resourceName = $"Infrastructure.Email.Templates.{templateName}.html";

        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            throw new InvalidOperationException($"Email template '{templateName}' not found.");
        }

        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();

        foreach (var (key, value) in payload)
        {
            html = html.Replace("{{" + key + "}}", HtmlEncoder.Default.Encode(value));
        }

        return html;
    }
}
