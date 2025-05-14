using System;

namespace KafkaGenericProcessor.Core.Exceptions
{
    /// <summary>
    /// Represents a validation error that occurred during message validation
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Gets or sets the property name that failed validation
        /// </summary>
        public string? PropertyName { get; set; }
        
        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the error code
        /// </summary>
        public string? ErrorCode { get; set; }
        
        /// <summary>
        /// Gets or sets the severity of the validation error
        /// </summary>
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        public ValidationError(string errorMessage)
        {
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class
        /// </summary>
        /// <param name="propertyName">The name of the property that failed validation</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="errorCode">Optional error code</param>
        /// <param name="severity">The severity of the error</param>
        public ValidationError(string propertyName, string errorMessage, string? errorCode = null, ValidationSeverity severity = ValidationSeverity.Error)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            ErrorCode = errorCode;
            Severity = severity;
        }

        /// <summary>
        /// Returns a string representation of the validation error
        /// </summary>
        /// <returns>A string containing the property name and error message</returns>
        public override string ToString()
        {
            return PropertyName != null 
                ? $"{PropertyName}: {ErrorMessage}" 
                : ErrorMessage;
        }
    }

    /// <summary>
    /// Defines the severity levels for validation errors
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// Information-level validation message, not blocking
        /// </summary>
        Information = 0,
        
        /// <summary>
        /// Warning-level validation message, not blocking but should be addressed
        /// </summary>
        Warning = 1,
        
        /// <summary>
        /// Error-level validation message, blocking
        /// </summary>
        Error = 2
    }
}