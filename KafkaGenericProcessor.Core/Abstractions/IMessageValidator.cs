using System.Threading;
using System.Threading.Tasks;

namespace KafkaGenericProcessor.Core.Abstractions;

/// <summary>
/// Defines a validator for message processing
/// </summary>
/// <typeparam name="T">The message type to validate</typeparam>
public interface IMessageValidator<T>
{
    /// <summary>
    /// Validates a message
    /// </summary>
    /// <param name="message">The message to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the message is valid, otherwise false</returns>
    Task<bool> ValidateAsync(T message, CancellationToken cancellationToken = default);
}