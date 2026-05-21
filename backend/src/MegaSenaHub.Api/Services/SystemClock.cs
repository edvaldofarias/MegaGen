using MegaSenaHub.Application.Abstractions;

namespace MegaSenaHub.Api.Services;

/// <summary>Implementação de IClock que usa o relógio real do sistema.</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
