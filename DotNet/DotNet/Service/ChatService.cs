using DocumentFormat.OpenXml.Packaging;
using DotNetLLM.DTO;
using DotNetLLM.Repo;
using log4net;
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

    public async Task<ChatResponse> Chat(ChatRequest request)
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
                model = "llama3.2:1b",
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
        catch(Exception ex)
        {
            _logger.Error("fail chat. ", ex);
            return new ChatResponse
            {
                Content = string.Empty,
            };
        }

        
    }
}

