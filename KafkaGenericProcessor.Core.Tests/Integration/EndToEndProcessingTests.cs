using KafkaFlow;
using KafkaFlow.Producers;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Exceptions;
using KafkaGenericProcessor.Core.Middlewares;
using KafkaGenericProcessor.Core.Tests.TestHelpers;
using KafkaGenericProcessor.Core.Tests.TestHelpers.KafkaFlow;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace KafkaGenericProcessor.Core.Tests.Integration
{
    public class EndToEndProcessingTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProducerAccessor _producerAccessor;
        private readonly IMessageProducer _messageProducer;
        private readonly TestProcessor _processor;
        private readonly List<TestOutput> _producedMessages;
        private readonly KafkaProcessorSettings _settings;
        private readonly TestValidator _validator;
        
        public EndToEndProcessingTests()
        {
            // Setup service collection and provider
            var services = new ServiceCollection();
            
            // Add required services
            services.AddLogging();
            
            // Setup mocks
            _producerAccessor = Substitute.For<IProducerAccessor>();
            _messageProducer = Substitute.For<IMessageProducer>();
            _producedMessages = new List<TestOutput>();
            
            // Configure producer to store messages in a list
            _messageProducer.ProduceAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<TestOutput>()
            ).Returns(callInfo => {
                var message = callInfo.ArgAt<TestOutput>(2);
                _producedMessages.Add(message);
                return Task.CompletedTask;
            });
            
            _settings = new KafkaProcessorSettings
            {
                ProcessorKey = "test-processor",
                Brokers = new[] { "broker1:9092" },
                ConsumerTopic = "consumer-topic",
                ProducerTopic = "producer-topic",
            };
            
            _producerAccessor.GetProducer(_settings.ProducerName).Returns(_messageProducer);
            
            // Create real implementations for processor and validator
            _processor = new TestProcessor();
            _validator = new TestValidator();
            
            // Register services
            services.AddSingleton<IMessageProcessor<TestInput, TestOutput>>(_processor);
            services.AddSingleton<IMessageValidator<TestInput>>(_validator);
            services.AddSingleton<IProducerAccessor>(_producerAccessor);
            services.AddSingleton(_settings);

            // Create service provider
            _serviceProvider = services.BuildServiceProvider();
        }

        #region Test Classes

        public class TestInput
        {
            public int Id { get; set; }
            public string? Value { get; set; }
        }

        public class TestOutput
        {
            public int Id { get; set; }
            public string? Result { get; set; }
        }

        public class TestProcessor : IMessageProcessor<TestInput, TestOutput>
        {
            public int ProcessCallCount { get; private set; }
            public bool ShouldThrowException { get; set; }
            public Exception? ExceptionToThrow { get; set; }

            public Task<TestOutput> ProcessAsync(TestInput input, string correlationId, CancellationToken cancellationToken = default)
            {
                ProcessCallCount++;
                
                if (ShouldThrowException && ExceptionToThrow != null)
                {
                    throw ExceptionToThrow;
                }
                
                return Task.FromResult(new TestOutput
                {
                    Id = input.Id,
                    Result = $"Processed: {input.Value}"
                });
            }
        }

        public class TestValidator : IMessageValidator<TestInput>
        {
            public int ValidateCallCount { get; private set; }
            public bool ShouldPassValidation { get; set; } = true;
            public IReadOnlyList<ValidationError> ValidationErrors { get; set; } = Array.Empty<ValidationError>();
            private IEnumerable<string> _validationErrorStrings = Array.Empty<string>();

            public IEnumerable<string> ValidationErrorStrings
            {
                get => _validationErrorStrings;
                set
                {
                    _validationErrorStrings = value;
                    var errorList = new List<ValidationError>();
                    foreach (var error in value)
                    {
                        errorList.Add(new ValidationError(error));
                    }
                    ValidationErrors = errorList;
                }
            }

            public Task<bool> ValidateAsync(TestInput message, string correlationId, CancellationToken cancellationToken = default)
            {
                ValidateCallCount++;
                return Task.FromResult(ShouldPassValidation);
            }

            public Task<IReadOnlyList<ValidationError>> GetValidationErrorsAsync(TestInput message, string correlationId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyList<ValidationError>>(ValidationErrors);
            }
        }

        #endregion
    }
}