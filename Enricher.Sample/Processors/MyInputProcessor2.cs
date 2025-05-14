using Enricher.Sample.Models;
using KafkaGenericProcessor.Core.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Enricher.Sample.Processors;

/// <summary>
/// Second implementation of message processor for transforming MyInput into MyOutput with different logic
/// </summary>
public class MyInputProcessor2 : IMessageProcessor<MyInput, MyOutput>
{
    /// <summary>
    /// Processes a MyInput and transforms it into a MyOutput with alternative processing
    /// </summary>
    /// <param name="input">The input message</param>
    /// <param name="correlationId">Correlation ID for tracking the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The processed output message</returns>
    public Task<MyOutput> ProcessAsync(MyInput input, string correlationId, CancellationToken cancellationToken = default)
    {
        var output = new MyOutput(
            ProducedBy: $"processor:{nameof(MyInputProcessor2)}, Enricher.Sample (Alternative Processor) - Message '{input.Content}' from ID: {input.Id}",
            ProcessedAt: DateTime.UtcNow
        );
        return Task.FromResult(output);

    }
}