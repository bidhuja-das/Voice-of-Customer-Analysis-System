using HotelVoC.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelVoC.Core.Services;

public class AnalyticsService
{
    private readonly AppDbContext _context;

    public AnalyticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> GetSentimentSummary()
    {
        var results = await _context.SentimentResults.ToListAsync();
        var total = results.Count;

        return new
        {
            total = total,
            positive = results.Count(r => r.Label == "Positive"),
            negative = results.Count(r => r.Label == "Negative"),
            neutral = results.Count(r => r.Label == "Neutral"),
            positivePercent = total > 0 ? Math.Round((double)results.Count(r => r.Label == "Positive") / total * 100, 1) : 0,
            negativePercent = total > 0 ? Math.Round((double)results.Count(r => r.Label == "Negative") / total * 100, 1) : 0,
            neutralPercent = total > 0 ? Math.Round((double)results.Count(r => r.Label == "Neutral") / total * 100, 1) : 0
        };
    }

    public async Task<object> GetTopics()
    {
        var topics = await _context.Topics
            .Include(t => t.Feedback)
            .ThenInclude(f => f.SentimentResult)
            .ToListAsync();

        var grouped = topics
            .GroupBy(t => t.Name)
            .Select(g => new
            {
                topicName = g.Key,
                count = g.Count(),
                positiveCount = g.Count(t => t.Feedback.SentimentResult?.Label == "Positive"),
                negativeCount = g.Count(t => t.Feedback.SentimentResult?.Label == "Negative"),
                neutralCount = g.Count(t => t.Feedback.SentimentResult?.Label == "Neutral")
            })
            .OrderByDescending(t => t.count)
            .ToList();

        return grouped;
    }

    public async Task<object> GetComparison()
    {
        var today = DateTime.Today;
        var feedbacks = await _context.Feedbacks
            .Include(f => f.SentimentResult)
            .Where(f => f.IsAnalyzed)
            .ToListAsync();

        var todayData = feedbacks.Where(f => f.SubmittedAt.Date == today);
        var yesterdayData = feedbacks.Where(f => f.SubmittedAt.Date == today.AddDays(-1));
        var thisWeekData = feedbacks.Where(f => f.SubmittedAt.Date >= today.AddDays(-7));
        var lastWeekData = feedbacks.Where(f => f.SubmittedAt.Date >= today.AddDays(-14) && f.SubmittedAt.Date < today.AddDays(-7));
        var thisMonthData = feedbacks.Where(f => f.SubmittedAt.Month == today.Month && f.SubmittedAt.Year == today.Year);
        var lastMonthData = feedbacks.Where(f => f.SubmittedAt.Month == today.AddMonths(-1).Month && f.SubmittedAt.Year == today.AddMonths(-1).Year);

        return new[]
        {
            BuildPeriod("Today", todayData),
            BuildPeriod("Yesterday", yesterdayData),
            BuildPeriod("This Week", thisWeekData),
            BuildPeriod("Last Week", lastWeekData),
            BuildPeriod("This Month", thisMonthData),
            BuildPeriod("Last Month", lastMonthData)
        };
    }

    private object BuildPeriod(string period, IEnumerable<Core.Models.Feedback> data)
    {
        var list = data.ToList();
        return new
        {
            period,
            total = list.Count,
            positive = list.Count(f => f.SentimentResult?.Label == "Positive"),
            negative = list.Count(f => f.SentimentResult?.Label == "Negative"),
            neutral = list.Count(f => f.SentimentResult?.Label == "Neutral")
        };
    }

    public async Task<object> GetDashboardStats()
    {
        var totalFeedback = await _context.Feedbacks.CountAsync();
        var analyzedFeedback = await _context.Feedbacks.CountAsync(f => f.IsAnalyzed);
        var pendingFeedback = totalFeedback - analyzedFeedback;
        var totalInsights = await _context.Insights.CountAsync();
        var criticalInsights = await _context.Insights.CountAsync(i => i.UrgencyLevel == "Critical");

        var sentiments = await _context.SentimentResults.ToListAsync();
        var positivePercent = sentiments.Count > 0
            ? Math.Round((double)sentiments.Count(s => s.Label == "Positive") / sentiments.Count * 100, 1)
            : 0;

        var topTopic = await _context.Topics
            .GroupBy(t => t.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync();

        return new
        {
            totalFeedback,
            analyzedFeedback,
            pendingFeedback,
            totalInsights,
            criticalInsights,
            positivePercent,
            topTopic = topTopic ?? "N/A"
        };
    }

    public async Task<object> GetDailyReports()
    {
        var reports = await _context.DailyReports
            .OrderByDescending(r => r.ReportDate)
            .Take(30)
            .ToListAsync();

        return reports;
    }
}