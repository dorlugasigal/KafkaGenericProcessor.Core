using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace KafkaGenericProcessor.Core.Logging
{
    /// <summary>
    /// Extension methods for ILogger to provide structured logging with standardized metadata
    /// </summary>
    public static class StructuredLoggerExtensions
    {
        /// <summary>
        /// Logs a debug message with structured context
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="context">The log context</param>
        /// <param name="message">The log message</param>
        /// <param name="args">Optional format arguments for the message</param>
        public static void LogStructuredDebug(this ILogger logger, LogContext context, string message, params object[] args)
        {
            LogStructured(logger, LogLevel.Debug, context, null, message, args);
        }

        /// <summary>
        /// Logs an informational message with structured context
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="context">The log context</param>
        /// <param name="message">The log message</param>
        /// <param name="args">Optional format arguments for the message</param>
        public static void LogStructuredInformation(this ILogger logger, LogContext context, string message, params object[] args)
        {
            LogStructured(logger, LogLevel.Information, context, null, message, args);
        }

        /// <summary>
        /// Logs a warning message with structured context
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="context">The log context</param>
        /// <param name="message">The log message</param>
        /// <param name="args">Optional format arguments for the message</param>
        public static void LogStructuredWarning(this ILogger logger, LogContext context, string message, params object[] args)
        {
            LogStructured(logger, LogLevel.Warning, context, null, message, args);
        }

        /// <summary>
        /// Logs a warning message with structured context and exception
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="context">The log context</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The log message</param>
        /// <param name="args">Optional format arguments for the message</param>
        public static void LogStructuredWarning(this ILogger logger, LogContext context, Exception exception, string message, params object[] args)
        {
            LogStructured(logger, LogLevel.Warning, context, exception, message, args);
        }

        /// <summary>
        /// Logs an error message with structured context
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="context">The log context</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The log message</param>
        /// <param name="args">Optional format arguments for the message</param>
        public static void LogStructuredError(this ILogger logger, LogContext context, Exception exception, string message, params object[] args)
        {
            LogStructured(logger, LogLevel.Error, context, exception, message, args);
        }

        /// <summary>
        /// Logs a critical message with structured context
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="context">The log context</param>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">The log message</param>
        /// <param name="args">Optional format arguments for the message</param>
        public static void LogStructuredCritical(this ILogger logger, LogContext context, Exception exception, string message, params object[] args)
        {
            LogStructured(logger, LogLevel.Critical, context, exception, message, args);
        }

        /// <summary>
        /// Logs a performance metric with structured context
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="context">The log context</param>
        /// <param name="operation">The operation being measured</param>
        /// <param name="elapsedMs">The elapsed time in milliseconds</param>
        public static void LogPerformance(this ILogger logger, LogContext context, string operation, long elapsedMs)
        {
            // Add performance-specific properties
            context.AddProperty("Operation", operation);
            context.AddProperty("ElapsedMs", elapsedMs);
            
            LogStructured(logger, LogLevel.Information, context, null, 
                "Performance measurement: {Operation} completed in {ElapsedMs}ms", operation, elapsedMs);
        }

        /// <summary>
        /// Core structured logging method
        /// </summary>
        private static void LogStructured(
            ILogger logger, 
            LogLevel logLevel, 
            LogContext context, 
            Exception? exception, 
            string message, 
            params object[] args)
        {
            // Create a scope with all the properties from the LogContext
            using (logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = context.CorrelationId,
                ["MessageType"] = context.MessageType,
                ["Operation"] = context.Operation,
                ["Timestamp"] = context.Timestamp
            }))
            {
                // Add any custom properties from the context
                using (logger.BeginScope(context.Properties))
                {
                    // Use the built-in ILogger extension methods that properly handle structured logging
                    // These extension methods correctly preserve the template structure needed for placeholders
                    if (exception == null)
                    {
                        // No exception - use LoggerExtensions for proper structured logging
                        switch (args.Length)
                        {
                            case 0:
                                Microsoft.Extensions.Logging.LoggerExtensions.Log(logger, logLevel, message);
                                break;
                            case 1:
                                Microsoft.Extensions.Logging.LoggerExtensions.Log(logger, logLevel, message, args[0]);
                                break;
                            case 2:
                                Microsoft.Extensions.Logging.LoggerExtensions.Log(logger, logLevel, message, args[0], args[1]);
                                break;
                            case 3:
                                Microsoft.Extensions.Logging.LoggerExtensions.Log(logger, logLevel, message, args[0], args[1], args[2]);
                                break;
                            default:
                                // For more arguments, pass the array directly
                                Microsoft.Extensions.Logging.LoggerExtensions.Log(logger, logLevel, message, args);
                                break;
                        }
                    }
                    else
                    {
                        // With exception
                        switch (args.Length)
                        {
                            case 0:
                                Microsoft.Extensions.Logging.LoggerExtensions.Log(logger, logLevel, exception, message);
                                break;
                            case 1:
                                Microsoft.Extensions.Logging.LoggerExtensions.Log(logger, logLevel, exception, message, args[0]);
                                break;
                            case 2:
                                Microsoft.Extensions.Logging.LoggerExtensions.Log(logger, logLevel, exception, message, args[0], args[1]);
                                break;
                            case 3:
                                Microsoft.Extensions.Logging.LoggerExtensions.Log(logger, logLevel, exception, message, args[0], args[1], args[2]);
                                break;
                            default:
                                // For more arguments, pass the array directly
                                Microsoft.Extensions.Logging.LoggerExtensions.Log(logger, logLevel, exception, message, args);
                                break;
                        }
                    }
                }
            }
        }
    }
}