using HotelVoC.Core.Models;

namespace HotelVoC.Core.Interfaces;

public interface IFeedbackService
{
    Task<Feedback> IngestOne(int sourceId, string customerIdentifier, string rawText, DateTime submittedAt);
    Task<List<Feedback>> IngestBulk(List<(int sourceId, string customerIdentifier, string rawText, DateTime submittedAt)> feedbacks);
    Task<List<Feedback>> GetAll();
    Task<Feedback?> GetById(int id);
}