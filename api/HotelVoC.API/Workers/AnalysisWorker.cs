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

    var pendingFeedbacks = await context.Feedbacks
        .Include(f => f.Source)
        .Where(f => !f.IsAnalyzed)
        .Take(10)
        .ToListAsync();

    if (!pendingFeedbacks.Any())
    {
        _logger.LogInformation("No pending feedbacks to analyze.");
        return;
    }

    _logger.LogInformation($"Analyzing {pendingFeedbacks.Count} feedbacks...");

    foreach (var feedback in pendingFeedbacks)
    {
        int attempts = 0;
        bool success = false;

        while (attempts < 3 && !success)
        {
            attempts++;
            try
            {
                var (sentiment, topic) = await _ollamaService.AnalyzeFeedback(feedback.RawText);

                // Remove existing results if retrying
                var existingSentiment = context.SentimentResults
                    .FirstOrDefault(s => s.FeedbackId == feedback.FeedbackId);
                if (existingSentiment != null)
                    context.SentimentResults.Remove(existingSentiment);

                var existingTopic = context.Topics
                    .FirstOrDefault(t => t.FeedbackId == feedback.FeedbackId);
                if (existingTopic != null)
                    context.Topics.Remove(existingTopic);

                context.SentimentResults.Add(new SentimentResult
                {
                    FeedbackId = feedback.FeedbackId,
                    Label = sentiment
                });

                context.Topics.Add(new Topic
                {
                    FeedbackId = feedback.FeedbackId,
                    Name = topic
                });

                feedback.IsAnalyzed = true;
                await context.SaveChangesAsync();
                success = true;

                // Send email if urgent
                if (sentiment == "Negative")
                {
                    var urgentTopics = new[] { "delivery", "refund", "damaged", "missing", "fraud", "scam" };
                    bool isUrgent = urgentTopics.Any(t =>
                        topic.ToLower().Contains(t) ||
                        feedback.RawText.ToLower().Contains(t));

                    if (isUrgent)
                    {
                        try
                        {
                            await _emailService.SendUrgentAlert(
                                feedback.CustomerIdentifier,
                                feedback.Source?.Name ?? "Unknown",
                                topic,
                                sentiment,
                                feedback.RawText
                            );
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning(emailEx, "Failed to send urgent email alert");
                        }
                    }
                }

                _logger.LogInformation($"Feedback {feedback.FeedbackId} analyzed: {sentiment} / {topic}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Attempt {attempts} failed for feedback {feedback.FeedbackId}");

                if (attempts >= 3)
                {
                    // Mark as failed after 3 attempts
                    feedback.IsAnalyzed = true;
                    context.SentimentResults.Add(new SentimentResult
                    {
                        FeedbackId = feedback.FeedbackId,
                        Label = "Neutral"
                    });
                    context.Topics.Add(new Topic
                    {
                        FeedbackId = feedback.FeedbackId,
                        Name = "Unclassified"
                    });
                    await context.SaveChangesAsync();
                    _logger.LogWarning($"Feedback {feedback.FeedbackId} marked as Unclassified after 3 failed attempts");
                }
                else
                {
                    await Task.Delay(2000); // Wait 2 seconds before retry
                }
            }
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