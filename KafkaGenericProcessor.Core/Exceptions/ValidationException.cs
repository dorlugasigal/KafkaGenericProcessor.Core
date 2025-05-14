using System;
using System.Collections.Generic;

namespace KafkaGenericProcessor.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when message validation fails
    /// </summary>
    public class ValidationException : KafkaGenericProcessorException
    {
        /// <summary>
        /// Collection of validation errors that caused this exception
        /// </summary>
        public IReadOnlyList<ValidationError> ValidationErrors { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public ValidationException(string message) : base(message)
        {
            ValidationErrors = Array.Empty<ValidationError>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with validation errors
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="validationErrors">The collection of validation errors</param>
        public ValidationException(string message, IReadOnlyList<ValidationError> validationErrors) : base(message)
        {
            ValidationErrors = validationErrors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class with validation errors and correlation ID
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="validationErrors">The collection of validation errors</param>
        /// <param name="correlationId">The correlation ID to track this exception</param>
        public ValidationException(string message, IReadOnlyList<ValidationError> validationErrors, string correlationId) 
            : base(message, correlationId)
        {
            ValidationErrors = validationErrors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
            ValidationErrors = Array.Empty<ValidationError>();
        }
    }

    // ValidationError class has been moved to its own file: ValidationError.cs
}