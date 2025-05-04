using Enricher.Sample.Models;
using KafkaGenericProcessor.Core.Abstractions;

namespace Enricher.Sample.Processors;

public class GlobalTrackConverterProcessor : IMessageProcessor<MyInput, MyOutput>
{
    public Task<MyOutput> ProcessAsync(MyInput input, CancellationToken cancellationToken = default)
    {
        // Transform the input message to output message with input data included for tracing
        var output = new MyOutput(
            ProducedBy: $"Enricher.Sample - Input ID: {input.Id}, Content: {input.Content}",
            ProcessedAt: DateTime.UtcNow
        );

        return Task.FromResult(output);
    }
}