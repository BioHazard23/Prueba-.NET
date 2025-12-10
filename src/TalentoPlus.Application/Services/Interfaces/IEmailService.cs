namespace TalentoPlus.Application.Services.Interfaces;

public interface IEmailService
{
    Task<bool> EnviarEmailBienvenidaAsync(string destinatario, string nombreCompleto);
    Task<bool> EnviarEmailAsync(string destinatario, string asunto, string contenidoHtml);
}

