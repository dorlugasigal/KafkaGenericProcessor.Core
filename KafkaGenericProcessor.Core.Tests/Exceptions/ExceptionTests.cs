using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using KafkaGenericProcessor.Core.Exceptions;
using Shouldly;
using Xunit;
using SerializationException = KafkaGenericProcessor.Core.Exceptions.SerializationException;

namespace KafkaGenericProcessor.Core.Tests.Exceptions
{
    public class ExceptionTests
    {
        private const string TestExceptionMessage = "Test exception message";
        private const string TestCorrelationId = "test-correlation-id";
        private readonly Exception TestInnerException = new InvalidOperationException("Inner exception");

        [Fact]
        public void KafkaGenericProcessorException_ShouldInheritFromException()
        {
            // Act
            var exception = new KafkaGenericProcessorException(TestExceptionMessage);

            // Assert
            exception.ShouldBeAssignableTo<Exception>();
            exception.Message.ShouldBe(TestExceptionMessage);
        }

        [Fact]
        public void KafkaGenericProcessorException_WithInnerException_ShouldSetInnerException()
        {
            // Act
            var exception = new KafkaGenericProcessorException(TestExceptionMessage, TestInnerException);

            // Assert
            exception.Message.ShouldBe(TestExceptionMessage);
            exception.InnerException.ShouldBe(TestInnerException);
        }

        [Fact]
        public void ValidationException_ShouldInheritFromKafkaGenericProcessorException()
        {
            // Arrange
            ValidationError[] errors =
            [
                new("Field1", "Error1"),
                new("Field2", "Error2")
            ];

            // Act
            var exception = new ValidationException(TestExceptionMessage, errors, TestCorrelationId);

            // Assert
            exception.ShouldBeAssignableTo<KafkaGenericProcessorException>();
            exception.Message.ShouldBe(TestExceptionMessage);
            exception.CorrelationId.ShouldBe(TestCorrelationId);
            exception.ValidationErrors.ShouldBe(errors);
        }

        [Fact]
        public void ValidationException_WithInnerException_ShouldSetInnerException()
        {
            // Arrange
            var errors = new[] { "Error1", "Error2" };

            // Act
            var exception = new ValidationException(TestExceptionMessage, TestInnerException);

            // Assert
            exception.Message.ShouldBe(TestExceptionMessage);
            exception.InnerException.ShouldBe(TestInnerException);
            exception.ValidationErrors.ShouldBeEmpty();
        }

        [Fact]
        public void ProcessingException_ShouldInheritFromKafkaGenericProcessorException()
        {
            // Arrange
            var messageType = typeof(TestMessage);

            // Act
            var exception = new ProcessingException(TestExceptionMessage, messageType, TestCorrelationId);

            // Assert
            exception.ShouldBeAssignableTo<KafkaGenericProcessorException>();
            exception.Message.ShouldBe(TestExceptionMessage);
            exception.CorrelationId.ShouldBe(TestCorrelationId);
            exception.MessageType.ShouldBe(messageType);
        }

        [Fact]
        public void ProcessingException_WithInnerException_ShouldSetInnerException()
        {
            // Arrange
            var messageType = typeof(TestMessage);

            // Act
            var exception = new ProcessingException(TestExceptionMessage, messageType, TestCorrelationId, TestInnerException);

            // Assert
            exception.Message.ShouldBe(TestExceptionMessage);
            exception.InnerException.ShouldBe(TestInnerException);
            exception.MessageType.ShouldBe(messageType);
            exception.CorrelationId.ShouldBe(TestCorrelationId);
        }

        [Fact]
        public void SerializationException_WithInnerException_ShouldSetInnerException()
        {
            // Arrange
            var messageType = typeof(TestMessage);
            var isDeserializationError = false;

            // Act
            var exception = new SerializationException(
                TestExceptionMessage, messageType, TestCorrelationId,TestInnerException,  isDeserializationError);

            // Assert
            exception.Message.ShouldBe(TestExceptionMessage);
            exception.InnerException.ShouldBe(TestInnerException);
            exception.DataType.ShouldBe(messageType);
            exception.CorrelationId.ShouldBe(TestCorrelationId);
            exception.IsSerializationError.ShouldBe(isDeserializationError);
        }

        [Fact]
        public void KafkaConnectionException_ShouldInheritFromKafkaGenericProcessorException()
        {
            // Arrange
            var brokers = new[] { "broker1:9092", "broker2:9092" };

            // Act
            var exception = new KafkaConnectionException(TestExceptionMessage, brokers,"topic", TestCorrelationId);

            // Assert
            exception.ShouldBeAssignableTo<KafkaGenericProcessorException>();
            exception.Message.ShouldBe(TestExceptionMessage);
            exception.CorrelationId.ShouldBe(TestCorrelationId);
            exception.Brokers.ShouldBe(brokers);
        }

        #region Test Classes

        private class TestMessage { }

        #endregion
    }
}