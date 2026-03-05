using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Lihtar.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Lihtar.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public SmtpEmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        List<EmailAttachment>? attachments = null)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email is empty.", nameof(toEmail));

        var host = _config["Smtp:Host"];
        var portStr = _config["Smtp:Port"];
        var username = _config["Smtp:Username"];
        var password = _config["Smtp:Password"];
        var enableSslStr = _config["Smtp:EnableSsl"];

        if (string.IsNullOrWhiteSpace(host))
            throw new InvalidOperationException("SMTP Host is not configured (Smtp:Host).");

        if (string.IsNullOrWhiteSpace(portStr) || !int.TryParse(portStr, out var port))
            throw new InvalidOperationException("SMTP Port is not configured or invalid (Smtp:Port).");

        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidOperationException("SMTP Username is not configured (Smtp:Username).");

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("SMTP Password is not configured (Smtp:Password).");

        var enableSsl = true;
        if (!string.IsNullOrWhiteSpace(enableSslStr) && bool.TryParse(enableSslStr, out var parsedSsl))
            enableSsl = parsedSsl;

        var from = _config["Smtp:From"] ?? username;
        var fromName = _config["Smtp:FromName"] ?? "Lihtar";

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password)
        };

        using var msg = new MailMessage
        {
            From = new MailAddress(from, fromName),
            Subject = subject,
            IsBodyHtml = true
        };

        msg.To.Add(new MailAddress(toEmail));

        // ✅ Inline attachments (CID) через AlternateView
        AlternateView? htmlView = null;

        if (attachments is not null && attachments.Any(a => a.IsInline))
        {
            htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);

            foreach (var a in attachments.Where(x => x.IsInline))
            {
                var stream = new MemoryStream(a.Content);
                var lr = new LinkedResource(stream, a.ContentType)
                {
                    ContentId = string.IsNullOrWhiteSpace(a.ContentId) ? Guid.NewGuid().ToString("N") : a.ContentId,
                    TransferEncoding = TransferEncoding.Base64
                };
                htmlView.LinkedResources.Add(lr);
            }

            msg.AlternateViews.Add(htmlView);
        }
        else
        {
            msg.Body = htmlBody;
        }

        // ✅ Звичайні attachments
        if (attachments is not null)
        {
            foreach (var a in attachments.Where(x => !x.IsInline))
            {
                var stream = new MemoryStream(a.Content);
                var att = new Attachment(stream, a.FileName, a.ContentType);
                msg.Attachments.Add(att);
            }
        }

        await client.SendMailAsync(msg);

        // MemoryStream для LinkedResource/Attachment звільниться разом з MailMessage
    }
}