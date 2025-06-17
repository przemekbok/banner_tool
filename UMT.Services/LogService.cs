using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UMT.IServices;
using UMT.Services.Constants;

namespace UMT.Services
{
    public class LogService : ILogService
    {
        private readonly ConcurrentQueue<LogEntry> _logs;
        private readonly object _fileLock = new object();
        private readonly string _logFilePath;
        private readonly int _maxLogEntries = 1000; // Keep last 1000 entries in memory

        public event EventHandler<LogEntry> LogAdded;

        public LogService()
        {
            _logs = new ConcurrentQueue<LogEntry>();
            
            // Create logs directory in app folder
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Common.AppName);
            Directory.CreateDirectory(appDataFolder);
            
            _logFilePath = Path.Combine(appDataFolder, $"logs_{DateTime.Now:yyyyMMdd}.txt");
        }

        public void LogInfo(string message, string details = null, string source = null)
        {
            Log(new LogEntry(LogLevel.Info, message, details, source));
        }

        public void LogWarning(string message, string details = null, string source = null)
        {
            Log(new LogEntry(LogLevel.Warning, message, details, source));
        }

        public void LogError(string message, string details = null, string source = null)
        {
            Log(new LogEntry(LogLevel.Error, message, details, source));
        }

        public void LogSuccess(string message, string details = null, string source = null)
        {
            Log(new LogEntry(LogLevel.Success, message, details, source));
        }

        public void Log(LogEntry entry)
        {
            if (entry == null) return;

            // Add to memory collection
            _logs.Enqueue(entry);

            // Maintain memory limit
            while (_logs.Count > _maxLogEntries)
            {
                _logs.TryDequeue(out _);
            }

            // Write to file
            WriteToFile(entry);

            // Raise event
            LogAdded?.Invoke(this, entry);
        }

        private void WriteToFile(LogEntry entry)
        {
            try
            {
                lock (_fileLock)
                {
                    File.AppendAllText(_logFilePath, entry.ToString() + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // Silent fail - don't want logging to crash the app
                System.Diagnostics.Debug.WriteLine($"Failed to write log to file: {ex.Message}");
            }
        }

        public IEnumerable<LogEntry> GetLogs()
        {
            return _logs.ToArray().Reverse(); // Most recent first
        }

        public IEnumerable<LogEntry> GetLogs(LogLevel level)
        {
            return GetLogs().Where(l => l.Level == level);
        }

        public IEnumerable<LogEntry> GetLogs(DateTime from, DateTime to)
        {
            return GetLogs().Where(l => l.Timestamp >= from && l.Timestamp <= to);
        }

        public void ClearLogs()
        {
            while (_logs.TryDequeue(out _)) { }
            
            try
            {
                lock (_fileLock)
                {
                    if (File.Exists(_logFilePath))
                    {
                        File.WriteAllText(_logFilePath, string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear log file: {ex.Message}");
            }
        }
    }
}
