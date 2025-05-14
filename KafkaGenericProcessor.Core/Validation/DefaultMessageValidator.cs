using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace KafkaGenericProcessor.Core.Validation;

/// <summary>
/// Default implementation of IMessageValidator that always returns true
/// </summary>
/// <typeparam name="T">The type of message to validate</typeparam>
public class DefaultMessageValidator<T> : IMessageValidator<T>
{
    private readonly ILogger<DefaultMessageValidator<T>> _logger;

    /// <summary>
    /// Creates a new instance of DefaultMessageValidator
    /// </summary>
    /// <param name="logger">Logger</param>
    public DefaultMessageValidator(ILogger<DefaultMessageValidator<T>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Default validation that simply returns true for all messages
    /// </summary>
    /// <param name="message">The message to validate</param>
    /// <param name="correlationId">Correlation ID for tracking the message through the system</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Always returns true</returns>
    public Task<bool> ValidateAsync(T message, string correlationId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Using default validator for {MessageType}, CorrelationId: {CorrelationId}",
            typeof(T).Name, correlationId);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Gets validation errors for a message. This default implementation always returns an empty list.
    /// </summary>
    /// <param name="message">The message to validate</param>
    /// <param name="correlationId">Correlation ID for tracking the message through the system</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Empty collection as default validator doesn't perform any validation</returns>
    public Task<IReadOnlyList<ValidationError>> GetValidationErrorsAsync(T message, string correlationId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Using default validator to get validation errors for {MessageType}, CorrelationId: {CorrelationId}",
            typeof(T).Name, correlationId);
        return Task.FromResult<IReadOnlyList<ValidationError>>(new List<ValidationError>());
    }
}