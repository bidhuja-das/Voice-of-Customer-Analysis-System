using System.ComponentModel.DataAnnotations;

namespace HotelVoC.Core.Models;

public class Topic
{
    [Key]
    public int TopicId { get; set; }
    public int FeedbackId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Feedback Feedback { get; set; } = null!;
}