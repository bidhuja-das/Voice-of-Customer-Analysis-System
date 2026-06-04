using HotelVoC.Core.Data;
using HotelVoC.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelVoC.Core.Services;

public class InsightService
{
    private readonly AppDbContext _context;
    private readonly OllamaService _ollamaService;

    public InsightService(AppDbContext context, OllamaService ollamaService)
    {
        _context = context;
        _ollamaService = ollamaService;
    }

    public async Task GenerateInsights()
    {
        // Get all analyzed feedbacks with topics
        var feedbacks = await _context.Feedbacks
            .Include(f => f.Topic)
            .Include(f => f.SentimentResult)
            .Where(f => f.IsAnalyzed && f.Topic != null)
            .ToListAsync();

        // Group by topic
        var grouped = feedbacks
            .GroupBy(f => f.Topic!.Name)
            .ToList();

        foreach (var group in grouped)
        {
            var topicName = group.Key;
            var count = group.Count();
            var negativeCount = group.Count(f => f.SentimentResult?.Label == "Negative");

            // Determine urgency
            // Determine urgency — adjusted for smaller datasets
            // Determine urgency using both count AND percentage
            var urgency = "Low";
            var negativePercent = count > 0 ? (double)negativeCount / count * 100 : 0;

            if (negativeCount >= 3 && negativePercent >= 60)
                urgency = "Critical";
            else if (negativeCount >= 2 && negativePercent >= 50)
                urgency = "High";
            else if (negativeCount >= 1 && negativePercent >= 30)
                urgency = "Medium";
            else
                urgency = "Low";

            // Generate AI summary for this topic
            var texts = string.Join(". ", group.Select(f => f.RawText).Take(5));
            var summary = await _ollamaService.GenerateDailySummary(texts);

            // Check if insight already exists for this topic
            var existing = await _context.Insights
                .FirstOrDefaultAsync(i => i.TopicName == topicName);

            if (existing != null)
            {
                // Update existing
                existing.Summary = summary;
                existing.UrgencyLevel = urgency;
                existing.FeedbackCount = count;
                existing.Title = $"{topicName} — {urgency} Priority";
            }
            else
            {
                // Create new
                _context.Insights.Add(new Insight
                {
                    Title = $"{topicName} — {urgency} Priority",
                    Summary = summary,
                    TopicName = topicName,
                    UrgencyLevel = urgency,
                    FeedbackCount = count
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<Insight>> GetAll()
    {
        return await _context.Insights
            .OrderByDescending(i => i.FeedbackCount)
            .ToListAsync();
    }
}