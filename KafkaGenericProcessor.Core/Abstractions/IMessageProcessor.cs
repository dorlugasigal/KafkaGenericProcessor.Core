namespace KafkaGenericProcessor.Core.Abstractions;

public interface IMessageProcessor<TInput, TOutput>
{
    Task<TOutput> ProcessAsync(TInput input, CancellationToken cancellationToken = default);
}