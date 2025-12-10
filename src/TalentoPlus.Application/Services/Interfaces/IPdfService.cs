using TalentoPlus.Domain.Entities;

namespace TalentoPlus.Application.Services.Interfaces;

public interface IPdfService
{
    byte[] GenerarHojaVida(Empleado empleado);
}

