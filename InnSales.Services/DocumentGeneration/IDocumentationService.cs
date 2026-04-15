namespace InnSales.Services;

public interface IDocumentGenerationService
{
    string GenerateDocument(string template, object data);
}