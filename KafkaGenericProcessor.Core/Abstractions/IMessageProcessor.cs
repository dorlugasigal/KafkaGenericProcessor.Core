using System.Threading;
using System.Threading.Tasks;

namespace KafkaGenericProcessor.Core.Abstractions;

/// <summary>
/// Defines a processor for transforming messages from one type to another
/// </summary>
/// <typeparam name="TInput">The input message type</typeparam>
/// <typeparam name="TOutput">The output message type</typeparam>
public interface IMessageProcessor<TInput, TOutput>
{
    /// <summary>
    /// Processes an input message and transforms it into an output message
    /// </summary>
    /// <param name="input">The input message to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The processed output message</returns>
    Task<TOutput> ProcessAsync(TInput input, CancellationToken cancellationToken = default);
}