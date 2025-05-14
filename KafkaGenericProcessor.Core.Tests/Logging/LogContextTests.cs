using System;
using System.Collections.Generic;
using KafkaGenericProcessor.Core.Logging;
using Shouldly;
using Xunit;

namespace KafkaGenericProcessor.Core.Tests.Logging
{
    public class LogContextTests
    {
        private readonly string _correlationId = "test-correlation-id";
        private readonly string _messageType = "TestMessage";
        private readonly string _operation = "TestOperation";

        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeProperties()
        {
            // Act
            var logContext = new LogContext(_correlationId, _messageType, _operation);

            // Assert
            logContext.CorrelationId.ShouldBe(_correlationId);
            logContext.MessageType.ShouldBe(_messageType);
            logContext.Operation.ShouldBe(_operation);
            logContext.Timestamp.ShouldNotBe(default);
            logContext.Stopwatch.ShouldNotBeNull();
            logContext.Properties.ShouldNotBeNull();
            logContext.Properties.Count.ShouldBe(0);
        }

        [Fact]
        public void Constructor_WithNullCorrelationId_ShouldGenerateNewId()
        {
            // Act
            var logContext = new LogContext(null, _messageType, _operation);

            // Assert
            logContext.CorrelationId.ShouldNotBeNull();
            logContext.CorrelationId.ShouldNotBeEmpty();
            // Should be a valid GUID
            Guid.TryParse(logContext.CorrelationId, out _).ShouldBeTrue();
        }

        [Fact]
        public void StartMeasurement_ShouldResetAndStartStopwatch()
        {
            // Arrange
            var logContext = new LogContext(_correlationId, _messageType, _operation);
            
            // Act
            logContext.StartMeasurement();
            
            // Assert
            logContext.Stopwatch.IsRunning.ShouldBeTrue();
        }

        [Fact]
        public void StopMeasurement_ShouldStopStopwatchAndReturnElapsed()
        {
            // Arrange
            var logContext = new LogContext(_correlationId, _messageType, _operation);
            logContext.StartMeasurement();
            System.Threading.Thread.Sleep(10); // Ensure some time passes

            // Act
            var elapsed = logContext.StopMeasurement();

            // Assert
            logContext.Stopwatch.IsRunning.ShouldBeFalse();
            elapsed.ShouldBeGreaterThan(0);
            elapsed.ShouldBe(logContext.Stopwatch.ElapsedMilliseconds);
        }

        [Fact]
        public void AddProperty_WithValidKeyValue_ShouldAddToProperties()
        {
            // Arrange
            var logContext = new LogContext(_correlationId, _messageType, _operation);
            var key = "TestKey";
            var value = "TestValue";

            // Act
            var result = logContext.AddProperty(key, value);

            // Assert
            logContext.Properties.ContainsKey(key).ShouldBeTrue();
            logContext.Properties[key].ShouldBe(value);
            result.ShouldBe(logContext); // Should return itself for method chaining
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AddProperty_WithInvalidKey_ShouldNotAddToProperties(string key)
        {
            // Arrange
            var logContext = new LogContext(_correlationId, _messageType, _operation);
            var value = "TestValue";
            var initialCount = logContext.Properties.Count;

            // Act
            var result = logContext.AddProperty(key, value);

            // Assert
            logContext.Properties.Count.ShouldBe(initialCount);
            result.ShouldBe(logContext); // Should still return itself
        }

        [Fact]
        public void AddProperty_WithMultipleProperties_ShouldAddAllToProperties()
        {
            // Arrange
            var logContext = new LogContext(_correlationId, _messageType, _operation);
            
            // Act
            logContext
                .AddProperty("Key1", "Value1")
                .AddProperty("Key2", 123)
                .AddProperty("Key3", true);

            // Assert
            logContext.Properties.Count.ShouldBe(3);
            logContext.Properties["Key1"].ShouldBe("Value1");
            logContext.Properties["Key2"].ShouldBe(123);
            logContext.Properties["Key3"].ShouldBe(true);
        }

        [Fact]
        public void ToStructuredLog_ShouldReturnDictionaryWithAllProperties()
        {
            // Arrange
            var logContext = new LogContext(_correlationId, _messageType, _operation);
            logContext
                .AddProperty("CustomKey1", "Value1")
                .AddProperty("CustomKey2", 123);

            // Simulate some elapsed time
            logContext.StartMeasurement();
            System.Threading.Thread.Sleep(5);
            logContext.StopMeasurement();

            // Act
            var structuredLog = logContext.ToStructuredLog() as Dictionary<string, object>;

            // Assert
            structuredLog.ShouldNotBeNull();
            structuredLog.Count.ShouldBe(7); // 5 default properties + 2 custom properties

            // Check built-in properties
            structuredLog["CorrelationId"].ShouldBe(_correlationId);
            structuredLog["MessageType"].ShouldBe(_messageType);
            structuredLog["Operation"].ShouldBe(_operation);
            structuredLog["Timestamp"].ShouldBe(logContext.Timestamp);
            structuredLog["ElapsedMs"].ShouldBe(logContext.Stopwatch.ElapsedMilliseconds);
            
            // Check custom properties
            structuredLog["CustomKey1"].ShouldBe("Value1");
            structuredLog["CustomKey2"].ShouldBe(123);
        }
    }
}