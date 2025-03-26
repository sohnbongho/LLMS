//using LangChain.Schema;
//using LangChain.Providers;
//using System.Net.Http.Json;
//using System.Text.Json;

//public class OllamaChatModel : BaseChatModel
//{
//    private readonly string _model;

//    public OllamaChatModel(string model)
//    {
//        _model = model;
//    }

//    public override string ModelType => _model;
//    public override string LlmType => "ollama";

//    public override async Task<LlmResult> GenerateAsync(
//        List<Message> messages,
//        Dictionary<string, object>? kwargs = null,
//        CancellationToken cancellationToken = default)
//    {
//        using var client = new HttpClient();

//        var payload = new
//        {
//            model = _model,
//            stream = false,
//            messages = messages.Select(m => new
//            {
//                role = m.Role.ToString().ToLower(),
//                content = m.Content
//            }).ToArray()
//        };

//        var response = await client.PostAsJsonAsync("http://localhost:11434/api/chat", payload, cancellationToken);
//        response.EnsureSuccessStatusCode();

//        var json = await response.Content.ReadAsStringAsync(cancellationToken);
//        using var jsonDoc = JsonDocument.Parse(json);
//        var content = jsonDoc.RootElement.GetProperty("message").GetProperty("content").GetString() ?? "";

//        return new LlmResult
//        {
//            Generations = new List<Generation>
//            {
//                new Generation { Text = content }
//            }
//        };
//    }
//}
