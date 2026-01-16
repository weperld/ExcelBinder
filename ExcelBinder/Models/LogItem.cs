using System;

namespace ExcelBinder.Models
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public class LogItem
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string FormattedTime => Timestamp.ToString("HH:mm:ss");
    }
}
