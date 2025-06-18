using System;
using System.Collections.Generic;

namespace UMT.IServices
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string Source { get; set; }

        public LogEntry()
        {
            Timestamp = DateTime.Now;
        }

        public LogEntry(LogLevel level, string message, string details = null, string source = null) : this()
        {
            Level = level;
            Message = message;
            Details = details;
            Source = source;
        }

        public override string ToString()
        {
            var detailsPart = !string.IsNullOrEmpty(Details) ? $" - {Details}" : "";
            var sourcePart = !string.IsNullOrEmpty(Source) ? $" [{Source}]" : "";
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Level.ToString().ToUpper()}{sourcePart}: {Message}{detailsPart}";
        }
    }

    public interface ILogService
    {
        void LogInfo(string message, string details = null, string source = null);
        void LogWarning(string message, string details = null, string source = null);
        void LogError(string message, string details = null, string source = null);
        void LogSuccess(string message, string details = null, string source = null);
        void Log(LogEntry entry);
        
        IEnumerable<LogEntry> GetLogs();
        IEnumerable<LogEntry> GetLogs(LogLevel level);
        IEnumerable<LogEntry> GetLogs(DateTime from, DateTime to);
        void ClearLogs();
        
        event EventHandler<LogEntry> LogAdded;
    }
}
