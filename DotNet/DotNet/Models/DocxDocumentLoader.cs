using DocumentFormat.OpenXml.Packaging;
using LangChain.DocumentLoaders;

public class DocxDocumentLoader : IDocumentLoader
{
    public async Task<IReadOnlyCollection<Document>> LoadAsync(DataSource source, DocumentLoaderSettings? settings = null, CancellationToken cancellationToken = default)
    {
        var filePath = source.Value?.ToString();

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("DOCX file not found", filePath);

        string text = await Task.Run(() =>
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            return doc.MainDocumentPart?.Document?.InnerText ?? "";
        });

        return new List<Document>
        {
            new Document(text, new Dictionary<string, object>
            {
                { "source", filePath }
            })
        };
    }
}
