using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Application.DTOs;
using TalentoPlus.Application.Services.Interfaces;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Enums;
using TalentoPlus.Domain.Interfaces;

namespace TalentoPlus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUnitOfWork unitOfWork, 
        IJwtService jwtService, 
        IEmailService emailService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _emailService = emailService;
        _configuration = configuration;
    }

    /// <summary>
    /// Autoregistro de empleado (público)
    /// </summary>
    [HttpPost("registro")]
    [ProducesResponseType(typeof(ApiResponse<EmpleadoApiDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registro([FromBody] EmpleadoRegistroDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Fail("Datos de registro inválidos", errors));
        }

        // Verificar que el documento no exista
        var existeDocumento = await _unitOfWork.Empleados.ExistsAsync(e => e.Documento == dto.Documento);
        if (existeDocumento)
        {
            return BadRequest(ApiResponse.Fail("Ya existe un empleado con este documento"));
        }

        // Verificar que el email no exista
        var existeEmail = await _unitOfWork.Empleados.ExistsAsync(e => e.Email.ToLower() == dto.Email.ToLower());
        if (existeEmail)
        {
            return BadRequest(ApiResponse.Fail("Ya existe un empleado con este email"));
        }

        // Verificar que el departamento exista
        var departamento = await _unitOfWork.Departamentos.GetByIdAsync(dto.DepartamentoId);
        if (departamento == null)
        {
            return BadRequest(ApiResponse.Fail("El departamento seleccionado no existe"));
        }

        // Crear el empleado (autoregistro tiene valores por defecto)
        var empleado = new Empleado
        {
            Documento = dto.Documento,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            FechaNacimiento = DateTime.SpecifyKind(dto.FechaNacimiento, DateTimeKind.Utc),
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            Email = dto.Email,
            DepartamentoId = dto.DepartamentoId,
            PerfilProfesional = dto.PerfilProfesional,
            // Valores por defecto para autoregistro
            Salario = 0,
            FechaIngreso = DateTime.UtcNow,
            Estado = EstadoEmpleado.Inactivo, // Inactivo hasta que el admin lo active
            NivelEducativo = NivelEducativo.Tecnico,
            CargoId = 3 // Auxiliar por defecto
        };

        await _unitOfWork.Empleados.AddAsync(empleado);
        await _unitOfWork.SaveChangesAsync();

        // Enviar correo de bienvenida
        _ = _emailService.EnviarEmailBienvenidaAsync(empleado.Email, $"{empleado.Nombres} {empleado.Apellidos}");

        var response = new EmpleadoApiDto
        {
            Id = empleado.Id,
            Documento = empleado.Documento,
            Nombres = empleado.Nombres,
            Apellidos = empleado.Apellidos,
            FechaNacimiento = empleado.FechaNacimiento,
            Direccion = empleado.Direccion,
            Telefono = empleado.Telefono,
            Email = empleado.Email,
            Salario = empleado.Salario,
            FechaIngreso = empleado.FechaIngreso,
            Estado = empleado.Estado.ToString(),
            NivelEducativo = empleado.NivelEducativo.ToString(),
            PerfilProfesional = empleado.PerfilProfesional,
            Departamento = departamento.Nombre,
            Cargo = "Auxiliar"
        };

        return CreatedAtAction(nameof(Registro), ApiResponse<EmpleadoApiDto>.Ok(response, 
            "Registro exitoso. Recibirás un correo de confirmación. Un administrador revisará tu solicitud."));
    }

    /// <summary>
    /// Login de empleado (público) - Retorna JWT
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] EmpleadoLoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.Fail("Datos de login inválidos", errors));
        }

        // Buscar empleado por documento y email
        var empleado = await _unitOfWork.Empleados.GetByDocumentoAndEmailAsync(dto.Documento, dto.Email);
        
        if (empleado == null)
        {
            return Unauthorized(ApiResponse.Fail("Credenciales inválidas. Verifique su documento y email."));
        }

        // Verificar que el empleado esté activo
        if (empleado.Estado != EstadoEmpleado.Activo && empleado.Estado != EstadoEmpleado.Vacaciones)
        {
            return Unauthorized(ApiResponse.Fail("Su cuenta no está activa. Contacte al administrador."));
        }

        // Generar token JWT
        var token = _jwtService.GenerateToken(empleado);
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");

        var response = new TokenResponseDto
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Documento = empleado.Documento,
            NombreCompleto = empleado.NombreCompleto,
            Email = empleado.Email
        };

        return Ok(ApiResponse<TokenResponseDto>.Ok(response, "Login exitoso"));
    }
}

