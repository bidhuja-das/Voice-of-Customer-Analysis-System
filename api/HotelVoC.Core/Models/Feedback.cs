using System.ComponentModel.DataAnnotations;

namespace HotelVoC.Core.Models;

public class Feedback
{
    [Key]
    public int FeedbackId { get; set; }
    public int SourceId { get; set; }
    public string CustomerIdentifier { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public bool IsAnalyzed { get; set; } = false;

    public FeedbackSource Source { get; set; } = null!;
    public SentimentResult? SentimentResult { get; set; }
    public Topic? Topic { get; set; }
}