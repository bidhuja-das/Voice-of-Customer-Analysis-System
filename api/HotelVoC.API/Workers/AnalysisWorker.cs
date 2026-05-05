using HotelVoC.Core.Data;
using HotelVoC.Core.Models;
using HotelVoC.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelVoC.API.Workers;

public class AnalysisWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OllamaService _ollamaService;
    private readonly EmailService _emailService;
    private readonly ILogger<AnalysisWorker> _logger;

    public AnalysisWorker(
        IServiceScopeFactory scopeFactory,
        OllamaService ollamaService,
        EmailService emailService,
        ILogger<AnalysisWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _ollamaService = ollamaService;
        _emailService = emailService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AnalyzePendingFeedbacks();
                await GenerateDailyReportIfNeeded();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in analysis worker");
            }

            // Wait 3 minutes before next run
            await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
        }
    }

    private async Task AnalyzePendingFeedbacks()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Get unanalyzed feedbacks
        var pendingFeedbacks = await context.Feedbacks
            .Include(f => f.Source)
            .Where(f => !f.IsAnalyzed)
            .Take(10) // Process 10 at a time
            .ToListAsync();

        if (!pendingFeedbacks.Any())
        {
            _logger.LogInformation("No pending feedbacks to analyze.");
            return;
        }

        _logger.LogInformation($"Analyzing {pendingFeedbacks.Count} feedbacks...");

        foreach (var feedback in pendingFeedbacks)
        {
            try
            {
                // Call Ollama
                var (sentiment, topic) = await _ollamaService.AnalyzeFeedback(feedback.RawText);

                // Save sentiment
                context.SentimentResults.Add(new SentimentResult
                {
                    FeedbackId = feedback.FeedbackId,
                    Label = sentiment
                });

                // Save topic
                context.Topics.Add(new Topic
                {
                    FeedbackId = feedback.FeedbackId,
                    Name = topic
                });

                // Mark as analyzed
                feedback.IsAnalyzed = true;

                await context.SaveChangesAsync();

                // Check urgency — send email if negative
                if (sentiment == "Negative")
                {
                    var urgentTopics = new[] { "delivery", "refund", "damaged", "missing", "fraud", "scam" };
                    bool isUrgent = urgentTopics.Any(t => 
                        topic.ToLower().Contains(t) || 
                        feedback.RawText.ToLower().Contains(t));

                    if (isUrgent)
                    {
                        await _emailService.SendUrgentAlert(
                            feedback.CustomerIdentifier,
                            feedback.Source?.Name ?? "Unknown",
                            topic,
                            sentiment,
                            feedback.RawText
                        );

                        _logger.LogInformation($"Urgent alert sent for feedback {feedback.FeedbackId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to analyze feedback {feedback.FeedbackId}");
            }
        }
    }

    private async Task GenerateDailyReportIfNeeded()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateOnly.FromDateTime(DateTime.Today);

        // Check if report already exists for today
        var exists = await context.DailyReports
            .AnyAsync(r => r.ReportDate == today);

        if (exists) return;

        // Get today's analyzed feedbacks
        var todayFeedbacks = await context.Feedbacks
            .Include(f => f.SentimentResult)
            .Include(f => f.Topic)
            .Where(f => f.SubmittedAt.Date == DateTime.Today && f.IsAnalyzed)
            .ToListAsync();

        if (!todayFeedbacks.Any()) return;

        var positive = todayFeedbacks.Count(f => f.SentimentResult?.Label == "Positive");
        var negative = todayFeedbacks.Count(f => f.SentimentResult?.Label == "Negative");
        var neutral = todayFeedbacks.Count(f => f.SentimentResult?.Label == "Neutral");

        // Find top issue
        var topIssue = todayFeedbacks
            .GroupBy(f => f.Topic?.Name ?? "General")
            .OrderByDescending(g => g.Count())
            .First().Key;

        // Generate AI summary
        var feedbackTexts = string.Join(". ", todayFeedbacks.Select(f => f.RawText).Take(10));
        var summary = await _ollamaService.GenerateDailySummary(feedbackTexts);

        context.DailyReports.Add(new DailyReport
        {
            ReportDate = today,
            TotalFeedback = todayFeedbacks.Count,
            PositiveCount = positive,
            NegativeCount = negative,
            NeutralCount = neutral,
            TopIssue = topIssue,
            Summary = summary,
            GeneratedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        _logger.LogInformation($"Daily report generated for {today}");
    }
}