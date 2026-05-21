namespace MegaSenaHub.Application.Abstractions;

/// <summary>Abstrai publicação de mensagens para o broker de mensageria.</summary>
public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken);
}
