using System;

namespace KafkaGenericProcessor.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when an error occurs during serialization or deserialization of messages
    /// </summary>
    public class SerializationException : KafkaGenericProcessorException
    {
        /// <summary>
        /// Gets the type that failed serialization/deserialization
        /// </summary>
        public Type? DataType { get; }

        /// <summary>
        /// Indicates whether this was a serialization (true) or deserialization (false) error
        /// </summary>
        public bool IsSerializationError { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="isSerializationError">Whether this was a serialization (true) or deserialization (false) error</param>
        public SerializationException(string message, bool isSerializationError = true) : base(message)
        {
            IsSerializationError = isSerializationError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class with data type information
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="dataType">The type that failed serialization/deserialization</param>
        /// <param name="isSerializationError">Whether this was a serialization (true) or deserialization (false) error</param>
        public SerializationException(string message, Type dataType, bool isSerializationError = true) : base(message)
        {
            DataType = dataType;
            IsSerializationError = isSerializationError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class with correlation ID
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="correlationId">The correlation ID to track this exception</param>
        /// <param name="isSerializationError">Whether this was a serialization (true) or deserialization (false) error</param>
        public SerializationException(string message, string correlationId, bool isSerializationError = true) 
            : base(message, correlationId)
        {
            IsSerializationError = isSerializationError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class with all parameters
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="dataType">The type that failed serialization/deserialization</param>
        /// <param name="correlationId">The correlation ID to track this exception</param>
        /// <param name="isSerializationError">Whether this was a serialization (true) or deserialization (false) error</param>
        public SerializationException(string message, Type dataType, string correlationId, bool isSerializationError = true) 
            : base(message, correlationId)
        {
            DataType = dataType;
            IsSerializationError = isSerializationError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class with inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        /// <param name="isSerializationError">Whether this was a serialization (true) or deserialization (false) error</param>
        public SerializationException(string message, Exception innerException, bool isSerializationError = true) 
            : base(message, innerException)
        {
            IsSerializationError = isSerializationError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class with all parameters
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="dataType">The type that failed serialization/deserialization</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        /// <param name="isSerializationError">Whether this was a serialization (true) or deserialization (false) error</param>
        public SerializationException(string message, Type dataType, Exception innerException, bool isSerializationError = true) 
            : base(message, innerException)
        {
            DataType = dataType;
            IsSerializationError = isSerializationError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class with all parameters
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="dataType">The type that failed serialization/deserialization</param>
        /// <param name="correlationId">The correlation ID to track this exception</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        /// <param name="isSerializationError">Whether this was a serialization (true) or deserialization (false) error</param>
        public SerializationException(
            string message,
            Type dataType,
            string correlationId,
            Exception innerException,
            bool isSerializationError = true) 
            : base(message, correlationId, innerException)
        {
            DataType = dataType;
            IsSerializationError = isSerializationError;
        }

        /// <summary>
        /// Returns a string representation of the exception with type information
        /// </summary>
        /// <returns>A string representation of the exception</returns>
        public override string ToString()
        {
            var operationType = IsSerializationError ? "Serialization" : "Deserialization";
            var typeInfo = DataType != null ? $", Type: {DataType.FullName}" : string.Empty;
            return $"{base.ToString()}, Operation: {operationType}{typeInfo}";
        }
    }
}