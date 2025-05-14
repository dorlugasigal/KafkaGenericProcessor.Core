using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KafkaGenericProcessor.Core.Exceptions;

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
    /// <param name="correlationId">Correlation ID for tracking the message through the system</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the message is valid, otherwise false</returns>
    /// <exception cref="ValidationException">Thrown when validation fails with details about validation errors</exception>
    Task<bool> ValidateAsync(T message, string correlationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets validation errors for a message
    /// </summary>
    /// <param name="message">The message to validate</param>
    /// <param name="correlationId">Correlation ID for tracking the message through the system</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of validation errors, or empty collection if valid</returns>
    Task<IReadOnlyList<ValidationError>> GetValidationErrorsAsync(T message, string correlationId, CancellationToken cancellationToken = default);
}