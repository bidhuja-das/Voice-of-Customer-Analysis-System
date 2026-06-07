using HotelVoC.Core.Data;
using HotelVoC.Core.Interfaces;
using HotelVoC.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelVoC.Core.Services;

public class FeedbackService : IFeedbackService
{
    private readonly AppDbContext _context;

    public FeedbackService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Feedback> IngestOne(int sourceId, string customerIdentifier, string rawText, DateTime submittedAt)
    {
        var feedback = new Feedback
        {
            SourceId = sourceId,
            CustomerIdentifier = customerIdentifier,
            RawText = rawText,
            SubmittedAt = submittedAt,
            IsAnalyzed = false
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();
        return feedback;
    }

    public async Task<List<Feedback>> IngestBulk(List<(int sourceId, string customerIdentifier, string rawText, DateTime submittedAt)> feedbacks)
    {
        var list = feedbacks.Select(f => new Feedback
        {
            SourceId = f.sourceId,
            CustomerIdentifier = f.customerIdentifier,
            RawText = f.rawText,
            SubmittedAt = f.submittedAt,
            IsAnalyzed = false
        }).ToList();

        _context.Feedbacks.AddRange(list);
        await _context.SaveChangesAsync();
        return list;
    }

    public async Task<List<Feedback>> GetAll()
    {
        return await _context.Feedbacks
            .Include(f => f.Source)
            .Include(f => f.SentimentResult)
            .Include(f => f.Topic)
            .OrderByDescending(f => f.SubmittedAt)       
            .ToListAsync();
    }

    public async Task<Feedback?> GetById(int id)
    {
    return await _context.Feedbacks
        .Include(f => f.Source)
        .Include(f => f.SentimentResult)
        .Include(f => f.Topic)
        .FirstOrDefaultAsync(f => f.FeedbackId == id);
    }
}