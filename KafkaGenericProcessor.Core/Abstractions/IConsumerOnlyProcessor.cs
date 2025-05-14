using System.Threading;
using System.Threading.Tasks;
using KafkaGenericProcessor.Core.Exceptions;

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
    /// <param name="correlationId">Correlation ID for tracking the message through the system</param>
    /// <param name="cancellationToken">A cancellation token to allow cancellation of the operation</param>
    /// <exception cref="ProcessingException">Thrown when an error occurs during processing</exception>
    /// <exception cref="ValidationException">Thrown when the input message fails validation</exception>
    Task ProcessAsync(TInput input, string correlationId, CancellationToken cancellationToken = default);
}