namespace InnSales.Services;

public class DocumentGenerationService : IDocumentGenerationService
{
    public string GenerateDocument(string template, object data)
    {

        if (string.IsNullOrWhiteSpace(template) || data == null)
            return template;

        foreach (var prop in data.GetType().GetProperties())
        {
            string key = $"{{{{{prop.Name}}}}}";
            string value = prop.GetValue(data)?.ToString() ?? "";
            template = template.Replace(key, value);
        }

        return $"Document generated: {template}";
    }
}



