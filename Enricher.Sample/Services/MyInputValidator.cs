using Enricher.Sample.Models;
using KafkaGenericProcessor.Core.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Enricher.Sample.Services;

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

    /// <summary>
    /// Validates a MyMessage
    /// </summary>
    /// <param name="input">The message to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the message is valid, otherwise false</returns>
    public Task<bool> ValidateAsync(MyInput input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(input.Id))
        {
            _logger.LogWarning("Message ID cannot be null or empty");
            return Task.FromResult(false);
        }

        if (string.IsNullOrEmpty(input.Content))
        {
            _logger.LogWarning("Message content cannot be null or empty");
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}