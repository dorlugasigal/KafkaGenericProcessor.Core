using System.Threading;
using System.Threading.Tasks;
using KafkaGenericProcessor.Core.Exceptions;

namespace KafkaGenericProcessor.Core.Abstractions;

/// <summary>
/// Defines a processor that transforms input messages to output messages
/// </summary>
/// <typeparam name="TInput">The type of input message to process</typeparam>
/// <typeparam name="TOutput">The type of output message produced</typeparam>
public interface IMessageProcessor<TInput, TOutput>
{
    /// <summary>
    /// Processes the input message and produces an output message
    /// </summary>
    /// <param name="input">The input message to process</param>
    /// <param name="correlationId">Correlation ID for tracking the message through the system</param>
    /// <param name="cancellationToken">A cancellation token to allow cancellation of the operation</param>
    /// <returns>The processed output message</returns>
    /// <exception cref="ProcessingException">Thrown when an error occurs during processing</exception>
    /// <exception cref="ValidationException">Thrown when the input message fails validation</exception>
    Task<TOutput> ProcessAsync(TInput input, string correlationId, CancellationToken cancellationToken = default);
}