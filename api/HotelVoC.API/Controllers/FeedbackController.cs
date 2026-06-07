using HotelVoC.API.DTOs;
using HotelVoC.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelVoC.API.Controllers;

[Authorize]
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
        if (string.IsNullOrWhiteSpace(dto.RawText))
            return BadRequest(new { error = "Feedback text is required." });

        if (dto.SourceId <= 0)
            return BadRequest(new { error = "Valid SourceId is required." });

        var feedback = await _feedbackService.IngestOne(
            dto.SourceId,
            dto.CustomerIdentifier,
            dto.RawText,
            dto.SubmittedAt
        );

        return Ok(new { message = "Feedback saved successfully.", feedbackId = feedback.FeedbackId });
    }

    [HttpPost("bulk-ingest")]
    public async Task<IActionResult> BulkIngest(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded. Please select a CSV file." });

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Invalid file format. Only CSV files are accepted." });

        var feedbacks = new List<(int, string, string, DateTime)>();
        var errors = new List<string>();

        using var reader = new StreamReader(file.OpenReadStream());
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
                errors.Add($"Row {rowNumber}: not enough columns — expected 4, got {columns.Length}");
                continue;
            }

            if (!int.TryParse(columns[0].Trim(), out int sourceId))
            {
                errors.Add($"Row {rowNumber}: invalid SourceId '{columns[0].Trim()}'");
                continue;
            }

            if (string.IsNullOrWhiteSpace(columns[2]))
            {
                errors.Add($"Row {rowNumber}: feedback text is empty");
                continue;
            }

            if (!DateTime.TryParse(columns[3].Trim(), out DateTime submittedAt))
            {
                errors.Add($"Row {rowNumber}: invalid date '{columns[3].Trim()}'");
                continue;
            }

            feedbacks.Add((sourceId, columns[1].Trim(), columns[2].Trim(), submittedAt));
        }

        if (feedbacks.Count > 0)
            await _feedbackService.IngestBulk(feedbacks);

        return Ok(new
        {
            message = $"{feedbacks.Count} feedbacks imported successfully.",
            errors
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var feedback = await _feedbackService.GetById(id);

        if (feedback == null)
            return NotFound(new { error = $"Feedback with ID {id} not found." });

        return Ok(new FeedbackResponseDto
        {
            FeedbackId = feedback.FeedbackId,
            SourceName = feedback.Source?.Name ?? "",
            CustomerIdentifier = feedback.CustomerIdentifier,
            RawText = feedback.RawText,
            SubmittedAt = feedback.SubmittedAt,
            IsAnalyzed = feedback.IsAnalyzed,
            Sentiment = feedback.SentimentResult?.Label,
            Topic = feedback.Topic?.Name
        });
    }
}