using Enricher.Sample.Models;
using KafkaGenericProcessor.Core.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Enricher.Sample.Processors;

/// <summary>
/// Custom message processor for transforming MyMessage into MyOutputMessage
/// </summary>
public class MyInputProcessor : IMessageProcessor<MyInput, MyOutput>
{
    /// <summary>
    /// Processes a MyMessage and transforms it into a MyOutputMessage
    /// </summary>
    /// <param name="input">The input message</param>
    /// <param name="correlationId">Correlation ID for tracking the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The processed output message</returns>
    public Task<MyOutput> ProcessAsync(MyInput input, string correlationId, CancellationToken cancellationToken = default)
    {
        var output = new MyOutput(
            ProducedBy: $"processor:{nameof(MyInputProcessor)}, Enricher.Sample - Input ID: {input.Id}, Content: {input.Content}, Correlation ID: {correlationId}",
            ProcessedAt: DateTime.UtcNow
        );

        return Task.FromResult(output);    }
}