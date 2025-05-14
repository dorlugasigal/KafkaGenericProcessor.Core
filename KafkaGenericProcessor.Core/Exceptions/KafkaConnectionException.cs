using System;

namespace KafkaGenericProcessor.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when there's an issue connecting to Kafka brokers or topics
    /// </summary>
    public class KafkaConnectionException : KafkaGenericProcessorException
    {
        /// <summary>
        /// Gets the broker(s) that failed to connect
        /// </summary>
        public string[] Brokers { get; }

        /// <summary>
        /// Gets the topic that failed to connect, if applicable
        /// </summary>
        public string? Topic { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConnectionException"/> class
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        public KafkaConnectionException(string message) : base(message)
        {
            Brokers = [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConnectionException"/> class with broker information
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="brokers">The brokers that failed to connect</param>
        public KafkaConnectionException(string message, string[] brokers) : base(message)
        {
            Brokers = brokers ?? [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConnectionException"/> class with broker and topic information
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="brokers">The brokers that failed to connect</param>
        /// <param name="topic">The topic that failed to connect</param>
        public KafkaConnectionException(string message, string[] brokers, string topic) : base(message)
        {
            Brokers = brokers ?? [];
            Topic = topic;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConnectionException"/> class with correlation ID
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="correlationId">The correlation ID to track this exception</param>
        public KafkaConnectionException(string message, string correlationId) : base(message, correlationId)
        {
            Brokers = [];
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConnectionException"/> class with all parameters
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="brokers">The brokers that failed to connect</param>
        /// <param name="topic">The topic that failed to connect</param>
        /// <param name="correlationId">The correlation ID to track this exception</param>
        public KafkaConnectionException(string message, string[] brokers, string topic, string correlationId) 
            : base(message, correlationId)
        {
            Brokers = brokers ?? Array.Empty<string>();
            Topic = topic;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConnectionException"/> class with inner exception
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public KafkaConnectionException(string message, Exception innerException) : base(message, innerException)
        {
            Brokers = [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConnectionException"/> class with inner exception and brokers
        /// </summary>
        /// <param name="message">The message that describes the error</param>
        /// <param name="brokers">The brokers that failed to connect</param>
        /// <param name="innerException">The exception that is the cause of the current exception</param>
        public KafkaConnectionException(string message, string[] brokers, Exception innerException) 
            : base(message, innerException)
        {
            Brokers = brokers ?? Array.Empty<string>();
        }

        /// <summary>
        /// Returns a string representation of the exception with broker information
        /// </summary>
        /// <returns>A string representation of the exception</returns>
        public override string ToString()
        {
            var brokerList = string.Join(", ", Brokers);
            var topicInfo = !string.IsNullOrEmpty(Topic) ? $", Topic: {Topic}" : string.Empty;
            return $"{base.ToString()}, Brokers: [{brokerList}]{topicInfo}";
        }
    }
}