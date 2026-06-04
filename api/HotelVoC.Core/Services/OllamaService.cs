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
    var prompt = "Analyze this customer feedback. " +
                 "Return ONLY a JSON object with no explanation and no markdown. " +
                 "Format must be exactly: {\"sentiment\": \"Positive\", \"topic\": \"Delivery Speed\"} " +
                 "Sentiment must be one of: Positive, Negative, Neutral. " +
                 "Topic must be 1-3 words only. " +
                 "Feedback: " + feedbackText;

    var requestBody = new
    {
        model = _model,
        prompt = prompt,
        stream = false
    };

    var json = JsonSerializer.Serialize(requestBody);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    try
    {
        var response = await _httpClient.PostAsync("/api/generate", content);
        var responseString = await response.Content.ReadAsStringAsync();

        var ollamaResponse = JsonSerializer.Deserialize<JsonElement>(responseString);
        var responseText = ollamaResponse.GetProperty("response").GetString() ?? "";

        // Try to extract JSON
        var start = responseText.IndexOf('{');
        var end = responseText.LastIndexOf('}');

        if (start >= 0 && end > start)
        {
            var jsonPart = responseText.Substring(start, end - start + 1);
            try
            {
                var parsed = JsonSerializer.Deserialize<JsonElement>(jsonPart);

                var sentiment = parsed.GetProperty("sentiment").GetString() ?? "Neutral";
                var topic = parsed.GetProperty("topic").GetString() ?? "General";

                // Normalize sentiment
                sentiment = sentiment.Trim();
                if (!new[] { "Positive", "Negative", "Neutral" }.Contains(sentiment))
                    sentiment = "Neutral";

                // Normalize topic
                topic = topic.Trim();
                if (topic.Length > 50) topic = topic.Substring(0, 50);

                return (sentiment, topic);
            }
            catch
            {
                // JSON found but couldn't parse — use fallback
                return GetFallbackSentiment(feedbackText);
            }
        }

        // No JSON found — use keyword fallback
        return GetFallbackSentiment(feedbackText);
    }
    catch
    {
        return ("Neutral", "General");
    }
}

private (string sentiment, string topic) GetFallbackSentiment(string text)
{
    var lower = text.ToLower();

    // Simple keyword sentiment
    var negativeWords = new[] { "late", "damaged", "wrong", "broken", "terrible", "awful", "refund", "missing", "fraud", "scam", "rude", "slow", "never", "worst", "horrible", "disappointing" };
    var positiveWords = new[] { "great", "excellent", "perfect", "amazing", "love", "fast", "good", "best", "wonderful", "fantastic", "happy", "satisfied", "recommend" };

    var negScore = negativeWords.Count(w => lower.Contains(w));
    var posScore = positiveWords.Count(w => lower.Contains(w));

    var sentiment = negScore > posScore ? "Negative" : posScore > negScore ? "Positive" : "Neutral";

    // Simple keyword topic
    var topic = "General Feedback";
    if (lower.Contains("deliver") || lower.Contains("ship") || lower.Contains("late") || lower.Contains("arriv")) topic = "Delivery Speed";
    else if (lower.Contains("refund") || lower.Contains("return") || lower.Contains("money")) topic = "Refund Process";
    else if (lower.Contains("quality") || lower.Contains("broken") || lower.Contains("damaged")) topic = "Product Quality";
    else if (lower.Contains("support") || lower.Contains("service") || lower.Contains("rude") || lower.Contains("help")) topic = "Customer Support";
    else if (lower.Contains("app") || lower.Contains("website") || lower.Contains("crash")) topic = "App Experience";
    else if (lower.Contains("price") || lower.Contains("cost") || lower.Contains("charge")) topic = "Pricing";
    else if (lower.Contains("package") || lower.Contains("box") || lower.Contains("wrap")) topic = "Packaging";

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