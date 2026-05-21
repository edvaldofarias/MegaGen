namespace MegaSenaHub.Application.Abstractions;

/// <summary>Abstrai o relógio do sistema para permitir testes determinísticos.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
