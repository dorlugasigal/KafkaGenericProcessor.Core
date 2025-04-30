using Enricher.Sample.Models;
using KafkaGenericProcessor.Core.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Enricher.Sample.Services;

/// <summary>
/// Custom message processor for transforming MyMessage into MyOutputMessage
/// </summary>
public class MyInputProcessor : IMessageProcessor<MyInput, MyOutput>
{
    /// <summary>
    /// Processes a MyMessage and transforms it into a MyOutputMessage
    /// </summary>
    /// <param name="input">The input message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The processed output message</returns>
    public Task<MyOutput> ProcessAsync(MyInput input, CancellationToken cancellationToken = default)
    {
        // Transform the input message to output message
        var output = new MyOutput(
            ProducedBy: "Enricher.Sample",
            ProcessedAt: DateTime.UtcNow
        );

        return Task.FromResult(output);
    }
}