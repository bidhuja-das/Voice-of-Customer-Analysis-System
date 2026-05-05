using System.ComponentModel.DataAnnotations;

namespace HotelVoC.Core.Models;

public class FeedbackSource
{
    [Key]
    public int SourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}