using TalentoPlus.Domain.Entities;

namespace TalentoPlus.Application.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(Empleado empleado);
    (bool isValid, string? documento) ValidateToken(string token);
}

