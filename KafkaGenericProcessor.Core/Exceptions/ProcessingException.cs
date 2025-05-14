using System;

namespace KafkaGenericProcessor.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when an error occurs during message processing
    /// </summary>
    public class ProcessingException : KafkaGenericProcessorException
    {
        /// <summary>
        /// Gets the type of message that failed to process
        /// </summary>
        public Type? MessageType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingException"/> class
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public ProcessingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingException"/> class with message type information
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="messageType">The type of message that failed to process</param>
        public ProcessingException(string message, Type messageType) : base(message)
        {
            MessageType = messageType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingException"/> class with correlation ID
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="correlationId">The correlation ID to track this exception</param>
        public ProcessingException(string message, string correlationId) : base(message, correlationId)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingException"/> class with message type and correlation ID
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="messageType">The type of message that failed to process</param>
        /// <param name="correlationId">The correlation ID to track this exception</param>
        public ProcessingException(string message, Type messageType, string correlationId) 
            : base(message, correlationId)
        {
            MessageType = messageType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingException"/> class
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public ProcessingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingException"/> class with message type
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="messageType">The type of message that failed to process</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public ProcessingException(string message, Type messageType, Exception innerException) 
            : base(message, innerException)
        {
            MessageType = messageType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingException"/> class with all parameters
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="messageType">The type of message that failed to process</param>
        /// <param name="correlationId">The correlation ID to track this exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public ProcessingException(string message, Type messageType, string correlationId, Exception innerException) 
            : base(message, correlationId, innerException)
        {
            MessageType = messageType;
        }
    }
}