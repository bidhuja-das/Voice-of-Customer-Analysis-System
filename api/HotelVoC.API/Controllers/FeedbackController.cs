using HotelVoC.API.DTOs;
using HotelVoC.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelVoC.API.Controllers;

[ApiController]
[Route("api/feedback")]

public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpPost("ingest")]
    public async Task<IActionResult> IngestOne([FromBody] IngestFeedbackDto dto)
    {
        var feedback = await _feedbackService.IngestOne(
            dto.SourceId,
            dto.CustomerIdentifier,
            dto.RawText,
            dto.SubmittedAt
        );

        return Ok(new { message = "Feedback saved", feedbackId = feedback.FeedbackId });
    }

    [HttpPost("bulk-ingest")]
    public async Task<IActionResult> BulkIngest(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        var feedbacks = new List<(int, string, string, DateTime)>();
        var errors = new List<string>();

        using var reader = new StreamReader(file.OpenReadStream());
        
        // Skip header row
        var header = await reader.ReadLineAsync();
        int rowNumber = 1;

        while (!reader.EndOfStream)
        {
            rowNumber++;
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var columns = line.Split(',');

            if (columns.Length < 4)
            {
                errors.Add($"Row {rowNumber}: not enough columns");
                continue;
            }

            if (!int.TryParse(columns[0].Trim(), out int sourceId))
            {
                errors.Add($"Row {rowNumber}: invalid SourceId");
                continue;
            }

            if (!DateTime.TryParse(columns[3].Trim(), out DateTime submittedAt))
            {
                errors.Add($"Row {rowNumber}: invalid date");
                continue;
            }

            feedbacks.Add((sourceId, columns[1].Trim(), columns[2].Trim(), submittedAt));
        }

        if (feedbacks.Count > 0)
            await _feedbackService.IngestBulk(feedbacks);

        return Ok(new
        {
            message = $"{feedbacks.Count} feedbacks imported",
            errors = errors
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var feedbacks = await _feedbackService.GetAll();

        var result = feedbacks.Select(f => new FeedbackResponseDto
        {
            FeedbackId = f.FeedbackId,
            SourceName = f.Source?.Name ?? "",
            CustomerIdentifier = f.CustomerIdentifier,
            RawText = f.RawText,
            SubmittedAt = f.SubmittedAt,
            IsAnalyzed = f.IsAnalyzed,
            Sentiment = f.SentimentResult?.Label,
            Topic = f.Topic?.Name
        }).ToList();

        return Ok(result);
    }
}