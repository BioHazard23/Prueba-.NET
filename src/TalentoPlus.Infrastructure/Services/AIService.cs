using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TalentoPlus.Application.Services.Interfaces;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Services;

public class AIService : IAIService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public AIService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AIService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = configuration["AISettings:ApiKey"] ?? "";
        _model = configuration["AISettings:Model"] ?? "gemini-2.0-flash";
    }

    public async Task<AIQueryResult> ProcessQueryAsync(string userQuery)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return new AIQueryResult
                {
                    Success = false,
                    Error = "La API Key de Gemini no está configurada."
                };
            }

            // 1. Obtener datos actuales del sistema para contexto
            var systemContext = await GetSystemContextAsync();

            // 2. Crear el prompt para Gemini
            var prompt = CreatePrompt(userQuery, systemContext);

            // 3. Llamar a Gemini API
            var geminiResponse = await CallGeminiAsync(prompt);

            if (geminiResponse == null)
            {
                return new AIQueryResult
                {
                    Success = false,
                    Error = "No se pudo obtener respuesta de Gemini."
                };
            }

            // 4. Procesar la respuesta
            return await ProcessGeminiResponseAsync(geminiResponse, userQuery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar consulta de IA: {Query}", userQuery);
            return new AIQueryResult
            {
                Success = false,
                Error = $"Error al procesar la consulta: {ex.Message}"
            };
        }
    }

    private async Task<SystemContext> GetSystemContextAsync()
    {
        var context = new SystemContext();

        // Estadísticas generales
        context.TotalEmpleados = await _context.Empleados.CountAsync();
        context.EmpleadosActivos = await _context.Empleados.CountAsync(e => e.Estado == Domain.Enums.EstadoEmpleado.Activo);
        context.EmpleadosInactivos = await _context.Empleados.CountAsync(e => e.Estado == Domain.Enums.EstadoEmpleado.Inactivo);
        context.EmpleadosVacaciones = await _context.Empleados.CountAsync(e => e.Estado == Domain.Enums.EstadoEmpleado.Vacaciones);

        // Departamentos con conteo
        context.Departamentos = await _context.Departamentos
            .Select(d => new DepartamentoInfo
            {
                Id = d.Id,
                Nombre = d.Nombre,
                CantidadEmpleados = d.Empleados.Count
            })
            .ToListAsync();

        // Cargos con conteo
        context.Cargos = await _context.Cargos
            .Select(c => new CargoInfo
            {
                Id = c.Id,
                Nombre = c.Nombre,
                CantidadEmpleados = c.Empleados.Count
            })
            .ToListAsync();

        // Salarios
        if (context.TotalEmpleados > 0)
        {
            context.SalarioPromedio = await _context.Empleados.AverageAsync(e => e.Salario);
            context.SalarioMaximo = await _context.Empleados.MaxAsync(e => e.Salario);
            context.SalarioMinimo = await _context.Empleados.MinAsync(e => e.Salario);
            context.SumaTotalSalarios = await _context.Empleados.SumAsync(e => e.Salario);
        }

        // Niveles educativos
        context.EmpleadosPorNivelEducativo = await _context.Empleados
            .GroupBy(e => e.NivelEducativo)
            .Select(g => new NivelEducativoInfo
            {
                Nivel = g.Key.ToString(),
                Cantidad = g.Count()
            })
            .ToListAsync();

        // Empleados recientes (últimos 5 ingresados)
        context.EmpleadosRecientes = await _context.Empleados
            .OrderByDescending(e => e.FechaIngreso)
            .Take(5)
            .Select(e => new EmpleadoResumen
            {
                Nombre = e.Nombres + " " + e.Apellidos,
                Departamento = e.Departamento != null ? e.Departamento.Nombre : "Sin departamento",
                Cargo = e.Cargo != null ? e.Cargo.Nombre : "Sin cargo",
                FechaIngreso = e.FechaIngreso,
                Estado = e.Estado.ToString()
            })
            .ToListAsync();

        // Lista de todos los empleados con información básica
        context.ListaEmpleados = await _context.Empleados
            .Include(e => e.Departamento)
            .Include(e => e.Cargo)
            .Select(e => new EmpleadoCompleto
            {
                Id = e.Id,
                Documento = e.Documento,
                NombreCompleto = e.Nombres + " " + e.Apellidos,
                Email = e.Email,
                Departamento = e.Departamento != null ? e.Departamento.Nombre : "Sin departamento",
                Cargo = e.Cargo != null ? e.Cargo.Nombre : "Sin cargo",
                Salario = e.Salario,
                FechaIngreso = e.FechaIngreso,
                Estado = e.Estado.ToString(),
                NivelEducativo = e.NivelEducativo.ToString()
            })
            .ToListAsync();

        return context;
    }

    private string CreatePrompt(string userQuery, SystemContext context)
    {
        var contextJson = JsonSerializer.Serialize(context, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return $@"Eres un asistente de RRHH para la empresa TalentoPlus S.A.S. Tu trabajo es responder preguntas sobre los empleados y la organización basándote ÚNICAMENTE en los datos reales del sistema que te proporciono.

REGLAS IMPORTANTES:
1. NUNCA inventes datos. Solo usa la información que te proporciono.
2. Si no tienes información suficiente para responder, dilo claramente.
3. Responde de manera clara y concisa en español.
4. Si te preguntan por empleados específicos, busca en la lista de empleados.
5. Proporciona números exactos cuando sea posible.
6. Si te preguntan algo que no está en los datos (como predicciones futuras o datos personales sensibles), indica que no tienes esa información.

DATOS ACTUALES DEL SISTEMA:
{contextJson}

PREGUNTA DEL USUARIO:
{userQuery}

Responde de manera profesional y útil, basándote SOLO en los datos proporcionados.";
    }

    private async Task<string?> CallGeminiAsync(string prompt)
    {
        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.3,
                    topP = 0.8,
                    topK = 40,
                    maxOutputTokens = 1024
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error de Gemini API: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            if (root.TryGetProperty("candidates", out var candidates) && 
                candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var candidateContent) &&
                    candidateContent.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var firstPart = parts[0];
                    if (firstPart.TryGetProperty("text", out var textElement))
                    {
                        return textElement.GetString();
                    }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al llamar a Gemini API");
            return null;
        }
    }

    private Task<AIQueryResult> ProcessGeminiResponseAsync(string geminiResponse, string originalQuery)
    {
        return Task.FromResult(new AIQueryResult
        {
            Success = true,
            Response = geminiResponse,
            Data = new { Query = originalQuery }
        });
    }
}

#region Context Models

public class SystemContext
{
    public int TotalEmpleados { get; set; }
    public int EmpleadosActivos { get; set; }
    public int EmpleadosInactivos { get; set; }
    public int EmpleadosVacaciones { get; set; }
    public decimal SalarioPromedio { get; set; }
    public decimal SalarioMaximo { get; set; }
    public decimal SalarioMinimo { get; set; }
    public decimal SumaTotalSalarios { get; set; }
    public List<DepartamentoInfo> Departamentos { get; set; } = new();
    public List<CargoInfo> Cargos { get; set; } = new();
    public List<NivelEducativoInfo> EmpleadosPorNivelEducativo { get; set; } = new();
    public List<EmpleadoResumen> EmpleadosRecientes { get; set; } = new();
    public List<EmpleadoCompleto> ListaEmpleados { get; set; } = new();
}

public class DepartamentoInfo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadEmpleados { get; set; }
}

public class CargoInfo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadEmpleados { get; set; }
}

public class NivelEducativoInfo
{
    public string Nivel { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class EmpleadoResumen
{
    public string Nombre { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class EmpleadoCompleto
{
    public int Id { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public decimal Salario { get; set; }
    public DateTime FechaIngreso { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string NivelEducativo { get; set; } = string.Empty;
}

#endregion

