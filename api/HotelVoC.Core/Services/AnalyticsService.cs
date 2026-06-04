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

    public async Task<object> GetSentimentSummary(DateTime? from = null, DateTime? to = null)
    {
        var allFeedbacks = await _context.Feedbacks
            .Include(f => f.SentimentResult)
            .Where(f => f.IsAnalyzed && f.SentimentResult != null)
            .ToListAsync();

        var filtered = allFeedbacks.AsEnumerable();

        if (from.HasValue)
            filtered = filtered.Where(f => f.SubmittedAt.Date >= from.Value.Date);
        if (to.HasValue)
            filtered = filtered.Where(f => f.SubmittedAt.Date <= to.Value.Date);

        var list = filtered.ToList();
        var total = list.Count;
        var positive = list.Count(f => f.SentimentResult!.Label == "Positive");
        var negative = list.Count(f => f.SentimentResult!.Label == "Negative");
        var neutral = list.Count(f => f.SentimentResult!.Label == "Neutral");

        return new
        {
            total,
            positive,
            negative,
            neutral,
            positivePercent = total > 0 ? Math.Round((double)positive / total * 100, 1) : 0,
            negativePercent = total > 0 ? Math.Round((double)negative / total * 100, 1) : 0,
            neutralPercent = total > 0 ? Math.Round((double)neutral / total * 100, 1) : 0
        };
    }

    public async Task<object> GetTopics(DateTime? from = null, DateTime? to = null)
    {
        var allFeedbacks = await _context.Feedbacks
            .Include(f => f.SentimentResult)
            .Include(f => f.Topic)
            .Where(f => f.IsAnalyzed && f.Topic != null)
            .ToListAsync();

        var filtered = allFeedbacks.AsEnumerable();

        if (from.HasValue)
            filtered = filtered.Where(f => f.SubmittedAt.Date >= from.Value.Date);
        if (to.HasValue)
            filtered = filtered.Where(f => f.SubmittedAt.Date <= to.Value.Date);

        var list = filtered.ToList();

        var grouped = list
            .GroupBy(f => f.Topic!.Name)
            .Select(g => new
            {
                topicName = g.Key,
                count = g.Count(),
                positiveCount = g.Count(f => f.SentimentResult?.Label == "Positive"),
                negativeCount = g.Count(f => f.SentimentResult?.Label == "Negative"),
                neutralCount = g.Count(f => f.SentimentResult?.Label == "Neutral")
            })
            .OrderByDescending(t => t.count)
            .ToList();

        return grouped;
    }

    public async Task<object> GetDashboardStats(DateTime? from = null, DateTime? to = null)
    {
        var allFeedbacks = await _context.Feedbacks
            .Include(f => f.SentimentResult)
            .Include(f => f.Topic)
            .Where(f => f.IsAnalyzed)
            .ToListAsync();

        var filtered = allFeedbacks.AsEnumerable();

        if (from.HasValue)
            filtered = filtered.Where(f => f.SubmittedAt.Date >= from.Value.Date);
        if (to.HasValue)
            filtered = filtered.Where(f => f.SubmittedAt.Date <= to.Value.Date);

        var list = filtered.ToList();

        var totalFeedback = list.Count;
        var analyzedFeedback = list.Count(f => f.IsAnalyzed);
        var pendingFeedback = totalFeedback - analyzedFeedback;
        var totalInsights = await _context.Insights.CountAsync();
        var criticalInsights = await _context.Insights
            .CountAsync(i => i.UrgencyLevel == "Critical");

        var positiveCount = list.Count(f => f.SentimentResult?.Label == "Positive");
        var positivePercent = list.Count > 0
            ? Math.Round((double)positiveCount / list.Count * 100, 1)
            : 0;

        var topTopic = list
            .Where(f => f.Topic != null)
            .GroupBy(f => f.Topic!.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

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

    public async Task<object> GetComparison(DateTime? from = null, DateTime? to = null)
    {
        var allFeedbacks = await _context.Feedbacks
            .Include(f => f.SentimentResult)
            .Where(f => f.IsAnalyzed && f.SentimentResult != null)
            .ToListAsync();

        var today = DateTime.Today;

        var todayData     = allFeedbacks.Where(f => f.SubmittedAt.Date == today);
        var yesterdayData = allFeedbacks.Where(f => f.SubmittedAt.Date == today.AddDays(-1));
        var thisWeekData  = allFeedbacks.Where(f => f.SubmittedAt.Date >= today.AddDays(-7) && f.SubmittedAt.Date <= today);
        var lastWeekData  = allFeedbacks.Where(f => f.SubmittedAt.Date >= today.AddDays(-14) && f.SubmittedAt.Date < today.AddDays(-7));
        var thisMonthData = allFeedbacks.Where(f => f.SubmittedAt.Date >= today.AddDays(-30) && f.SubmittedAt.Date <= today);
        var lastMonthData = allFeedbacks.Where(f => f.SubmittedAt.Date >= today.AddDays(-60) && f.SubmittedAt.Date < today.AddDays(-30));

        return new[]
        {
            BuildPeriod("Today",      todayData),
            BuildPeriod("Yesterday",  yesterdayData),
            BuildPeriod("This Week",  thisWeekData),
            BuildPeriod("Last Week",  lastWeekData),
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

    public async Task<object> GetDailyReports()
    {
        var reports = await _context.DailyReports
            .OrderByDescending(r => r.ReportDate)
            .Take(30)
            .ToListAsync();

        return reports;
    }
}