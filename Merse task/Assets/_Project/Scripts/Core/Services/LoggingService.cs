using UnityEngine;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Simple implementation of ILoggingService
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly bool _isLoggingEnabled;

        /// <summary>
        /// Create a new logging service
        /// </summary>
        /// <param name="isLoggingEnabled">Whether logging is enabled</param>
        public LoggingService(bool isLoggingEnabled = true)
        {
            _isLoggingEnabled = isLoggingEnabled;
        }

        /// <summary>
        /// Log a message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="type">The type of log message</param>
        public void Log(string message, Core.Interfaces.LogType type = Core.Interfaces.LogType.Info)
        {
            if (!_isLoggingEnabled) return;

            switch (type)
            {
                case Core.Interfaces.LogType.Info:
                    Debug.Log($"[INFO] {message}");
                    break;
                case Core.Interfaces.LogType.Warning:
                    Debug.LogWarning($"[WARNING] {message}");
                    break;
                case Core.Interfaces.LogType.Error:
                    Debug.LogError($"[ERROR] {message}");
                    break;
            }
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message">The warning message</param>
        public void LogWarning(string message)
        {
            Log(message, Core.Interfaces.LogType.Warning);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="message">The error message</param>
        public void LogError(string message)
        {
            Log(message, Core.Interfaces.LogType.Error);
        }
    }
}