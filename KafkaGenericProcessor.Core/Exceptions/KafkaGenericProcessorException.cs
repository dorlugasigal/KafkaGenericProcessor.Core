using System;

namespace KafkaGenericProcessor.Core.Exceptions
{
    /// <summary>
    /// Base exception class for all exceptions in the KafkaGenericProcessor library
    /// </summary>
    public class KafkaGenericProcessorException : Exception
    {
        /// <summary>
        /// Correlation identifier to track related operations across components
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaGenericProcessorException"/> class
        /// </summary>
        public KafkaGenericProcessorException() : base()
        {
            CorrelationId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaGenericProcessorException"/> class with a specified error message
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public KafkaGenericProcessorException(string message) : base(message)
        {
            CorrelationId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaGenericProcessorException"/> class with a specified error message 
        /// and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public KafkaGenericProcessorException(string message, Exception innerException) : base(message, innerException)
        {
            CorrelationId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaGenericProcessorException"/> class with a specified error message
        /// and correlation ID to track this exception across components
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="correlationId">The correlation ID to track this exception</param>
        public KafkaGenericProcessorException(string message, string correlationId) : base(message)
        {
            CorrelationId = correlationId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaGenericProcessorException"/> class with a specified error message,
        /// correlation ID, and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="correlationId">The correlation ID to track this exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public KafkaGenericProcessorException(string message, string correlationId, Exception innerException) 
            : base(message, innerException)
        {
            CorrelationId = correlationId;
        }
    }
}