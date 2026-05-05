using System.ComponentModel.DataAnnotations;

namespace HotelVoC.Core.Models;

public class SentimentResult
{
    [Key]
    public int SentimentId { get; set; }
    public int FeedbackId { get; set; }
    public string Label { get; set; } = string.Empty;

    public Feedback Feedback { get; set; } = null!;
}