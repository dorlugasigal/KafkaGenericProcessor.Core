using System;
using KafkaGenericProcessor.Core.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace KafkaGenericProcessor.Core.Tests.Logging
{
    public class StructuredLoggerExtensionsTests
    {
        private readonly ILogger _logger;
        private readonly LogContext _logContext;
        private readonly string _correlationId = "test-correlation-id";
        private readonly string _messageType = "TestMessage";
        private readonly string _operation = "TestOperation";
        private readonly string _testMessage = "This is a test message";

        public StructuredLoggerExtensionsTests()
        {
            _logger = Substitute.For<ILogger>();
            _logContext = new LogContext(_correlationId, _messageType, _operation);
        }

        [Fact]
        public void LogStructuredDebug_ShouldCallLogWithDebugLevel()
        {
            // Act
            _logger.LogStructuredDebug(_logContext, _testMessage);

            // Assert
            _logger.Received(1).Log(
                Arg.Is(LogLevel.Debug),
                Arg.Is<EventId>(e => e.Name == _operation),
                Arg.Any<object>(),
                Arg.Is<Exception>(ex => ex == null),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogStructuredInformation_ShouldCallLogWithInformationLevel()
        {
            // Act
            _logger.LogStructuredInformation(_logContext, _testMessage);

            // Assert
            _logger.Received(1).Log(
                Arg.Is(LogLevel.Information),
                Arg.Is<EventId>(e => e.Name == _operation),
                Arg.Any<object>(),
                Arg.Is<Exception>(ex => ex == null),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogStructuredWarning_WithoutException_ShouldCallLogWithWarningLevel()
        {
            // Act
            _logger.LogStructuredWarning(_logContext, _testMessage);

            // Assert
            _logger.Received(1).Log(
                Arg.Is(LogLevel.Warning),
                Arg.Is<EventId>(e => e.Name == _operation),
                Arg.Any<object>(),
                Arg.Is<Exception>(ex => ex == null),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogStructuredWarning_WithException_ShouldCallLogWithWarningLevelAndException()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act
            _logger.LogStructuredWarning(_logContext, exception, _testMessage);

            // Assert
            _logger.Received(1).Log(
                Arg.Is(LogLevel.Warning),
                Arg.Is<EventId>(e => e.Name == _operation),
                Arg.Any<object>(),
                Arg.Is<Exception>(ex => ex == exception),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogStructuredError_ShouldCallLogWithErrorLevelAndException()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act
            _logger.LogStructuredError(_logContext, exception, _testMessage);

            // Assert
            _logger.Received(1).Log(
                Arg.Is(LogLevel.Error),
                Arg.Is<EventId>(e => e.Name == _operation),
                Arg.Any<object>(),
                Arg.Is<Exception>(ex => ex == exception),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogStructuredCritical_ShouldCallLogWithCriticalLevelAndException()
        {
            // Arrange
            var exception = new Exception("Test exception");

            // Act
            _logger.LogStructuredCritical(_logContext, exception, _testMessage);

            // Assert
            _logger.Received(1).Log(
                Arg.Is(LogLevel.Critical),
                Arg.Is<EventId>(e => e.Name == _operation),
                Arg.Any<object>(),
                Arg.Is<Exception>(ex => ex == exception),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogPerformance_ShouldAddOperationPropertiesAndCallLog()
        {
            // Arrange
            string operation = "TestPerformanceOperation";
            long elapsedMs = 123;

            // Act
            _logger.LogPerformance(_logContext, operation, elapsedMs);

            // Assert
            // Check that properties were added to the context
            _logContext.Properties["Operation"].ShouldBe(operation);
            _logContext.Properties["ElapsedMs"].ShouldBe(elapsedMs);

            // Check that log was called with proper level
            _logger.Received(1).Log(
                Arg.Is(LogLevel.Information),
                Arg.Is<EventId>(e => e.Name == _operation),
                Arg.Any<object>(),
                Arg.Is<Exception>(ex => ex == null),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void LogStructured_WithInvalidFormatParameters_ShouldHandleException()
        {
            // Arrange
            var formatMessage = "Test {0} with {1} and {2}"; // Requires 3 params
            var testValue = "Value1"; // Only providing one param
            
            // Act - Should not throw even though format string is invalid
            _logger.LogStructuredInformation(_logContext, formatMessage, testValue);

            // Assert - Should still call Log
            _logger.Received(1).Log(
                Arg.Any<LogLevel>(),
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}