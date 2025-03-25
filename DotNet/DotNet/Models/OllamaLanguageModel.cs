//using LangChain.Base;
//using LangChain.Providers;
//using LangChain.Schema;
//using System.Net.Http.Json;
//using System.Text.Json;

//namespace DotNetLLM.Models;

//public class OllamaLanguageModel : BaseLanguageModel
//{
//    private readonly string _model;

//    public OllamaLanguageModel(string model)
//    {
//        _model = model;
//    }

//    public override string LlmType { get; set; } = "ollama";
//    public override string ModelType { get; set; } = "local";

//    // 정확한 시그니처로 수정
//    public override Task<LlmResult> GeneratePrompt(
//        BasePromptValue[] promptValues,
//        IReadOnlyCollection<string>? stopWords = null,
//        Dictionary<string, object>? kwargs = null,
//        CancellationToken cancellationToken = default)
//    {
//        var prompt = string.Join("\n", promptValues.Select(p => p.ToString()));

//        var result = new LlmResult
//        {
//            Generations = new List<Generation>
//            {
//                new Generation
//                {
//                    Text = prompt
//                }
//            }
//        };

//        return Task.FromResult(result);
//    }

//    public override async Task<LlmResult> GenerateAsync(
//        string prompt,
//        List<Message>? messages = null,
//        IReadOnlyCollection<string>? stopWords = null,
//        Dictionary<string, object>? kwargs = null,
//        CancellationToken cancellationToken = default)
//    {
//        using var client = new HttpClient();

//        var payload = new
//        {
//            model = _model,
//            stream = false,
//            messages = new[]
//            {
//                new { role = "system", content = "You are a helpful assistant." },
//                new { role = "user", content = prompt }
//            }
//        };

//        var response = await client.PostAsJsonAsync("http://localhost:11434/api/chat", payload, cancellationToken);
//        response.EnsureSuccessStatusCode();

//        var json = await response.Content.ReadAsStringAsync(cancellationToken);
//        using var jsonDoc = JsonDocument.Parse(json);

//        var content = jsonDoc.RootElement.GetProperty("message").GetProperty("content").GetString() ?? "";

//        var result = new LlmResult
//        {
//            Generations = new List<Generation>
//            {
//                new Generation
//                {
//                    Text = content
//                }
//            }
//        };

//        return result;
//    }
//}
