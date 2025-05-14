using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KafkaGenericProcessor.Core.Logging
{
    /// <summary>
    /// Represents a structured logging context with standardized metadata
    /// </summary>
    public class LogContext
    {
        /// <summary>
        /// Gets the correlation ID associated with the current operation
        /// </summary>
        public string CorrelationId { get; }
        
        /// <summary>
        /// Gets the message type being processed
        /// </summary>
        public string MessageType { get; }
        
        /// <summary>
        /// Gets the operation being performed
        /// </summary>
        public string Operation { get; }
        
        /// <summary>
        /// Gets the timestamp when the operation started
        /// </summary>
        public DateTimeOffset Timestamp { get; }
        
        /// <summary>
        /// Gets the stopwatch for measuring operation duration
        /// </summary>
        public Stopwatch Stopwatch { get; }
        
        /// <summary>
        /// Gets additional custom properties for the log
        /// </summary>
        public Dictionary<string, object> Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogContext"/> class
        /// </summary>
        /// <param name="correlationId">The correlation ID for the operation</param>
        /// <param name="messageType">The type of message being processed</param>
        /// <param name="operation">The operation being performed</param>
        public LogContext(string correlationId, string messageType, string operation)
        {
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            MessageType = messageType;
            Operation = operation;
            Timestamp = DateTimeOffset.UtcNow;
            Stopwatch = new Stopwatch();
            Properties = new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Starts the performance measurement
        /// </summary>
        public void StartMeasurement()
        {
            Stopwatch.Restart();
        }
        
        /// <summary>
        /// Stops the performance measurement
        /// </summary>
        /// <returns>The elapsed milliseconds</returns>
        public long StopMeasurement()
        {
            Stopwatch.Stop();
            return Stopwatch.ElapsedMilliseconds;
        }
        
        /// <summary>
        /// Adds a property to the log context
        /// </summary>
        /// <param name="key">The property key</param>
        /// <param name="value">The property value</param>
        /// <returns>This log context instance for method chaining</returns>
        public LogContext AddProperty(string key, object value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                Properties[key] = value;
            }
            
            return this;
        }
        
        /// <summary>
        /// Creates a structured object for logging
        /// </summary>
        /// <returns>An object containing all log context properties</returns>
        public object ToStructuredLog()
        {
            var log = new Dictionary<string, object>
            {
                ["CorrelationId"] = CorrelationId,
                ["MessageType"] = MessageType,
                ["Operation"] = Operation,
                ["Timestamp"] = Timestamp,
                ["ElapsedMs"] = Stopwatch.ElapsedMilliseconds
            };
            
            foreach (var property in Properties)
            {
                log[property.Key] = property.Value;
            }
            
            return log;
        }
    }
}