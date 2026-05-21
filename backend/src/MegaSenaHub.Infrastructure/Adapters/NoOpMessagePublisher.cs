using MegaSenaHub.Application.Abstractions;
using MegaSenaHub.Application.DTOs;

namespace MegaSenaHub.Infrastructure.Adapters;

/// <summary>
/// Implementação no-op do publisher de mensagens para desenvolvimento e testes.
/// Não envia mensagens reais — simula o comportamento sem RabbitMQ.
/// RabbitMQ real será implementado na Etapa 5.
/// </summary>
internal sealed class NoOpMessagePublisher : IMessagePublisher
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
