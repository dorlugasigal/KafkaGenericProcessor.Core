namespace KafkaGenericProcessor.Core.Abstractions;

/// <summary>
/// Interface for processors that consume messages without producing output to another topic
/// </summary>
/// <typeparam name="TInput">The type of input message to process</typeparam>
public interface IConsumerOnlyProcessor<TInput>
    where TInput : class
{
    /// <summary>
    /// Processes the input message
    /// </summary>
    /// <param name="input">The input message to process</param>
    /// <param name="cancellationToken">A cancellation token to allow cancellation of the operation</param>
    Task ProcessAsync(TInput input, CancellationToken cancellationToken = default);
}