using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// Type of log message
    /// </summary>
    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Interface for logging services
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Log a message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="type">The type of log message</param>
        void Log(string message, LogType type = LogType.Info);

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message">The warning message</param>
        void LogWarning(string message);

        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="message">The error message</param>
        void LogError(string message);
    }
}