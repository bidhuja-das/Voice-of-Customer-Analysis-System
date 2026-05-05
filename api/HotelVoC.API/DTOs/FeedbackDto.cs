namespace HotelVoC.API.DTOs;

public class IngestFeedbackDto
{
    public int SourceId { get; set; }
    public string CustomerIdentifier { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class FeedbackResponseDto
{
    public int FeedbackId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public string CustomerIdentifier { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public bool IsAnalyzed { get; set; }
    public string? Sentiment { get; set; }
    public string? Topic { get; set; }
}