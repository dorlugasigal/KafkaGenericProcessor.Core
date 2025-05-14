using Enricher.Sample.Models;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Enricher.Sample.Validators;

/// <summary>
/// Custom validator for MyMessage
/// </summary>
public class MyInputValidator : IMessageValidator<MyInput>
{
    private readonly ILogger<MyInputValidator> _logger;

    /// <summary>
    /// Creates a new instance of MyMessageValidator
    /// </summary>
    /// <param name="logger">Logger</param>
    public MyInputValidator(ILogger<MyInputValidator> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<ValidationError>> GetValidationErrorsAsync(MyInput message, string correlationId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Validates a MyMessage
    /// </summary>
    /// <param name="input">The message to validate</param>
    /// <param name="correlationId">Correlation ID for tracking the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the message is valid, otherwise false</returns>

    public Task<bool> ValidateAsync(MyInput message, string correlationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.Id))
        {
            _logger.LogWarning("Message ID cannot be null or empty");
            return Task.FromResult(false);
        }

        if (string.IsNullOrEmpty(message.Content))
        {
            _logger.LogWarning("Message content cannot be null or empty");
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}