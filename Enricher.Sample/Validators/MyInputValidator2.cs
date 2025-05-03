using Enricher.Sample.Models;
using KafkaGenericProcessor.Core.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Enricher.Sample.Validators;

/// <summary>
/// Alternative validator for MyInput with different validation rules
/// </summary>
public class MyInputValidator2 : IMessageValidator<MyInput>
{
    private readonly ILogger<MyInputValidator2> _logger;

    /// <summary>
    /// Creates a new instance of MyInputValidator2
    /// </summary>
    /// <param name="logger">Logger</param>
    public MyInputValidator2(ILogger<MyInputValidator2> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates a MyInput with stricter rules
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
        
        // Additional validation: check if content has a minimum length
        if (input.Content.Length < 10)
        {
            _logger.LogWarning("Message content must be at least 10 characters");
            return Task.FromResult(false);
        }

        // Additional validation: check if ID follows certain format (e.g., starts with a letter)
        if (!char.IsLetter(input.Id[0]))
        {
            _logger.LogWarning("Message ID must start with a letter");
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}