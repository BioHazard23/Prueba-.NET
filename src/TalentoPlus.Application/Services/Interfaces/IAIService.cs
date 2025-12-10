namespace TalentoPlus.Application.Services.Interfaces;

public interface IAIService
{
    Task<AIQueryResult> ProcessQueryAsync(string userQuery);
}

public class AIQueryResult
{
    public bool Success { get; set; }
    public string Response { get; set; } = string.Empty;
    public string? SqlExecuted { get; set; }
    public object? Data { get; set; }
    public string? Error { get; set; }
}

