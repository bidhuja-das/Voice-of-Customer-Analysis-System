using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace HotelVoC.Core.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendUrgentAlert(string customerIdentifier, string source, string topic, string sentiment, string feedbackText)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"]!;
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]!);
        var senderEmail = _configuration["EmailSettings:SenderEmail"]!;
        var senderPassword = _configuration["EmailSettings:SenderPassword"]!;
        var managerEmail = _configuration["EmailSettings:ManagerEmail"]!;

        var subject = $" Critical Feedback Alert — {topic}";

        var body = "A critical feedback was just received and requires immediate attention.\n\n" +
           "Customer: " + customerIdentifier + "\n" +
           "Source: " + source + "\n" +
           "Topic: " + topic + "\n" +
           "Sentiment: " + sentiment + "\n\n" +
           "Feedback:\n" +
           "\"" + feedbackText + "\"\n\n" +
           "Please log into the VoC dashboard and take necessary action.\n\n" +
           "— VoC Analysis System";

        using var smtp = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        var mail = new MailMessage(senderEmail, managerEmail, subject, body);
        await smtp.SendMailAsync(mail);
    }
}