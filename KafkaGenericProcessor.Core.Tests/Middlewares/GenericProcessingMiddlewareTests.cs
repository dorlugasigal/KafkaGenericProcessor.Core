using KafkaFlow;
using KafkaFlow.Producers;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Exceptions;
using KafkaGenericProcessor.Core.Middlewares;
using KafkaGenericProcessor.Core.Tests.TestHelpers;
using KafkaGenericProcessor.Core.Tests.TestHelpers.KafkaFlow;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace KafkaGenericProcessor.Core.Tests.Middlewares
{
    public class GenericProcessingMiddlewareTests
    {
        private readonly IMessageProcessor<TestInput, TestOutput> _processor;
        private readonly IConsumerOnlyProcessor<TestInput> _consumerOnlyProcessor;
        private readonly IMessageValidator<TestInput> _validator;
        private readonly IProducerAccessor _producerAccessor;
        private readonly IMessageProducer _messageProducer;
        private readonly KafkaProcessorSettings _settings;
        private readonly ILogger<GenericProcessingMiddleware<TestInput, TestOutput>> _logger;
        private readonly MiddlewareDelegate _next;
        private readonly TestMessageContext _messageContext;
        private readonly TestInput _testInput;

        public GenericProcessingMiddlewareTests()
        {
            _processor = Substitute.For<IMessageProcessor<TestInput, TestOutput>>();
            _consumerOnlyProcessor = Substitute.For<IConsumerOnlyProcessor<TestInput>>();
            _validator = Substitute.For<IMessageValidator<TestInput>>();
            _producerAccessor = Substitute.For<IProducerAccessor>();
            _messageProducer = Substitute.For<IMessageProducer>();
            _logger = Substitute.For<ILogger<GenericProcessingMiddleware<TestInput, TestOutput>>>();
            _next = _ => Task.FromResult(MiddlewareExecutionResult.Success);
            
            _settings = new KafkaProcessorSettings
            {
                ProcessorKey = "test-processor",
                Brokers = new[] { "broker1:9092" },
                ConsumerTopic = "consumer-topic",
                ProducerTopic = "producer-topic",
            };
            
            _producerAccessor.GetProducer(_settings.ProducerName).Returns(_messageProducer);
            
            // Default validator setup - passes validation
            _validator.ValidateAsync(Arg.Any<TestInput>(), Arg.Any<string>())
                .Returns(Task.FromResult(true));
            
            _testInput = new TestInput { Id = 1, Value = "test" };
            _messageContext = new TestMessageContext(_testInput);
        }

        [Fact]
        public void Constructor_WithNullValidator_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Should.Throw<ArgumentNullException>(() => new GenericProcessingMiddleware<TestInput, TestOutput>(
                _processor,
                _consumerOnlyProcessor,
                null!,
                _producerAccessor,
                _settings,
                _logger));

            exception.ParamName.ShouldBe("messageValidator");
        }

        [Fact]
        public void Constructor_WithNullProducerAccessor_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Should.Throw<ArgumentNullException>(() => new GenericProcessingMiddleware<TestInput, TestOutput>(
                _processor,
                _consumerOnlyProcessor,
                _validator,
                null!,
                _settings,
                _logger));

            exception.ParamName.ShouldBe("producerAccessor");
        }

        [Fact]
        public void Constructor_WithNullSettings_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Should.Throw<ArgumentNullException>(() => new GenericProcessingMiddleware<TestInput, TestOutput>(
                _processor,
                _consumerOnlyProcessor,
                _validator,
                _producerAccessor,
                null!,
                _logger));

            exception.ParamName.ShouldBe("settings");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Should.Throw<ArgumentNullException>(() => new GenericProcessingMiddleware<TestInput, TestOutput>(
                _processor,
                _consumerOnlyProcessor,
                _validator,
                _producerAccessor,
                _settings,
                null!));

            exception.ParamName.ShouldBe("logger");
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

        public class WrongInputType
        {
            public string? Name { get; set; }
        }

        #endregion
    }
}