using KafkaGenericProcessor.Core.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaGenericProcessor.Core.Services;

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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Always returns true</returns>
    public Task<bool> ValidateAsync(T message, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Default validation passed for message of type {MessageType}", typeof(T).Name);
        return Task.FromResult(true);
    }
}