using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace HotelVoC.Core.Services;

public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _model = configuration["Ollama:Model"] ?? "llama3.2";
        var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<(string sentiment, string topic)> AnalyzeFeedback(string feedbackText)
    {
        var prompt = "Analyze this customer feedback and return ONLY a JSON object. " +
             "No explanation, no markdown, just raw JSON. " +
             "Return exactly this format: " +
             "{\"sentiment\": \"Positive|Negative|Neutral\", \"topic\": \"one short topic name\"} " +
             "Feedback: " + feedbackText;
        var requestBody = new
        {
            model = _model,
            prompt = prompt,
            stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/generate", content);
        var responseString = await response.Content.ReadAsStringAsync();

        var ollamaResponse = JsonSerializer.Deserialize<JsonElement>(responseString);
        var responseText = ollamaResponse.GetProperty("response").GetString() ?? "";

        // Extract JSON from response
        var start = responseText.IndexOf('{');
        var end = responseText.LastIndexOf('}');

        if (start == -1 || end == -1)
            return ("Neutral", "General");

        var jsonPart = responseText.Substring(start, end - start + 1);
        var parsed = JsonSerializer.Deserialize<JsonElement>(jsonPart);

        var sentiment = parsed.GetProperty("sentiment").GetString() ?? "Neutral";
        var topic = parsed.GetProperty("topic").GetString() ?? "General";

        return (sentiment, topic);
    }

    public async Task<string> GenerateDailySummary(string feedbackSummary)
    {
        var prompt = $"""
            You are a business analyst. Based on these customer feedbacks from today,
            write a 2-sentence summary for the manager.
            Be direct and highlight the most critical issue.
            Return ONLY the summary text, nothing else.
            
            Feedbacks: {feedbackSummary}
            """;

        var requestBody = new
        {
            model = _model,
            prompt = prompt,
            stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/generate", content);
        var responseString = await response.Content.ReadAsStringAsync();

        var ollamaResponse = JsonSerializer.Deserialize<JsonElement>(responseString);
        return ollamaResponse.GetProperty("response").GetString() ?? "";
    }
}