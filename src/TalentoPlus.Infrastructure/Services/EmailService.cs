using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using TalentoPlus.Application.Services.Interfaces;

namespace TalentoPlus.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _smtpUser = _configuration["EmailSettings:SmtpUser"] ?? string.Empty;
        _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? string.Empty;
        _fromEmail = _configuration["EmailSettings:FromEmail"] ?? string.Empty;
        _fromName = _configuration["EmailSettings:FromName"] ?? "TalentoPlus RRHH";
    }

    public async Task<bool> EnviarEmailBienvenidaAsync(string destinatario, string nombreCompleto)
    {
        var asunto = "¡Bienvenido a TalentoPlus S.A.S.!";
        var contenido = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>¡Bienvenido a TalentoPlus!</h1>
        </div>
        <div class='content'>
            <h2>Hola {nombreCompleto},</h2>
            <p>Tu registro en el sistema de gestión de empleados de <strong>TalentoPlus S.A.S.</strong> ha sido completado exitosamente.</p>
            <p>Tu cuenta ha sido creada y está pendiente de revisión por parte del administrador de Recursos Humanos.</p>
            <p>Una vez tu cuenta sea activada, podrás acceder al sistema utilizando tu documento de identidad y correo electrónico.</p>
            <h3>Próximos pasos:</h3>
            <ul>
                <li>Espera la confirmación de activación de tu cuenta</li>
                <li>Una vez activa, podrás iniciar sesión en la plataforma</li>
                <li>Podrás consultar tu información y descargar tu hoja de vida</li>
            </ul>
            <p>Si tienes alguna pregunta, no dudes en contactar al departamento de Recursos Humanos.</p>
        </div>
        <div class='footer'>
            <p>Este correo fue enviado automáticamente por TalentoPlus S.A.S.</p>
            <p>Por favor no respondas a este correo.</p>
        </div>
    </div>
</body>
</html>";

        return await EnviarEmailAsync(destinatario, asunto, contenido);
    }

    public async Task<bool> EnviarEmailAsync(string destinatario, string asunto, string contenidoHtml)
    {
        if (string.IsNullOrEmpty(_smtpUser) || string.IsNullOrEmpty(_smtpPassword))
        {
            _logger.LogWarning("Configuración SMTP incompleta. Email no enviado a {Destinatario}", destinatario);
            return false;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(new MailboxAddress(destinatario, destinatario));
            message.Subject = asunto;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = contenidoHtml
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpUser, _smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email enviado exitosamente a {Destinatario}", destinatario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a {Destinatario}", destinatario);
            return false;
        }
    }
}

