using System.ComponentModel.DataAnnotations;

namespace HotelVoC.Core.Models;

public class DailyReport
{
    [Key]
    public int ReportId { get; set; }
    public DateOnly ReportDate { get; set; }
    public int TotalFeedback { get; set; }
    public int PositiveCount { get; set; }
    public int NegativeCount { get; set; }
    public int NeutralCount { get; set; }
    public string TopIssue { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}