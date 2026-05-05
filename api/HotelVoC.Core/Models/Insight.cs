using System.ComponentModel.DataAnnotations;

namespace HotelVoC.Core.Models;

public class Insight
{
    [Key]
    public int InsightId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string UrgencyLevel { get; set; } = string.Empty;
    public int FeedbackCount { get; set; }
}