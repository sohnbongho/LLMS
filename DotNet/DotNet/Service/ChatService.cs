using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DotNetLLM.DTO;
using DotNetLLM.Repo;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers.Ollama;
using log4net;
using Ollama;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

namespace DotNetLLM.Service;

public class ChatService
{
    private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private readonly HttpClient _httpClient;
    private readonly IChatRepo _repo;

    public ChatService(IHttpClientFactory httpClientFactory, IChatRepo repo)
    {
        _httpClient = httpClientFactory.CreateClient();
        _repo = repo;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>    
    public async Task<ChatResponse> Chat_Test1(ChatRequest request)
    {
        var question = request.Question;
        string docText = "";

        // tax.docx 고정 경로 (예: 프로젝트 루트의 Files 폴더)
        string docPath = Path.Combine(Directory.GetCurrentDirectory(), "tax.docx");

        if (!System.IO.File.Exists(docPath))
            throw new FileNotFoundException("문서 파일이 존재하지 않습니다.", docPath);

        try
        {
            using (var wordDoc = WordprocessingDocument.Open(docPath, false))
            {
                docText = wordDoc.MainDocumentPart?.Document?.Body?.InnerText ?? "";
            }

            var payload = new
            {
                model = "deepseek-r1:1.5b",
                stream = false,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant that answers based on the document." },
                    new { role = "user", content = $"문서 내용:\n{docText}\n\n질문: {question}" }
                }
            };

            var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/chat", payload);
            var json = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(json);
            var answer = jsonDoc.RootElement.GetProperty("message").GetProperty("content").GetString();

            return new ChatResponse
            {
                Content = answer ?? string.Empty,
            };
        }
        catch (Exception ex)
        {
            _logger.Error("fail chat. ", ex);
            return new ChatResponse
            {
                Content = string.Empty,
            };
        }
    }
    public async Task<ChatResponse> Chat(ChatRequest request)
    {
        try
        {
            var provider = new OllamaProvider();
            var embeddingModel = new OllamaEmbeddingModel(provider, id: "all-minilm");
            var llm = new OllamaChatModel(provider, id: "llama3.2:1b");

            var vectorDatabase = new SqLiteVectorDatabase(dataSource: "vectors.db");

            var vectorCollection = await vectorDatabase.AddDocumentsFromAsync<DocxDocumentLoader>(
                embeddingModel, // Used to convert text to embeddings
                dimensions: 384, // Should be 384 for all-minilm
                dataSource: DataSource.FromPath("tax.docx"),
                collectionName: "harry_docx",
                textSplitter: null,
                behavior: AddDocumentsToDatabaseBehavior.JustReturnCollectionIfCollectionIsAlreadyExists);


            var question = request.Question;
            var similarDocuments = await vectorCollection.GetSimilarDocuments(embeddingModel, question, amount: 5);
            // Use similar documents and LLM to answer the question
            var answer = await llm.GenerateAsync(
$"""
Use the following pieces of context to answer the question at the end.
If the answer is not in context then just say that you don't know, and don't try to make up an answer.
Answer with the final result only. Do not explain. Keep it extremely short.

{similarDocuments.AsString()}

Question: {question}
Answer:
""");

            Console.WriteLine($"LLM answer: {answer}");

            return new ChatResponse
            {
                Content = answer[0]?.ToString() ?? string.Empty,

            };

        }
        catch(Exception ex)
        {
            Console.WriteLine("error", ex);
            return new ChatResponse
            {
                Content = string.Empty,

            };
        }
        
    }
    static string ExtractTextFromDocx(string filePath)
    {
        try
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
            {
                Body body = doc.MainDocumentPart.Document.Body ?? null;

                if (body == null)
                    return string.Empty;

                // 문단별로 텍스트 추출 후 결합
                return string.Join("\n",
                    body.Descendants<Paragraph>()
                        .Select(p => p.InnerText));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DOCX 파일 처리 중 오류 발생: {ex.Message}");
            return string.Empty;
        }
    }
}

