using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using ExcelBinder.Models;

namespace ExcelBinder.Services
{
    public class LogService
    {
        private static LogService? _instance;
        public static LogService Instance => _instance ??= new LogService();

        public ObservableCollection<LogItem> Logs { get; } = new();

        // CLI 모드에서 UI 스레드가 블로킹된 동안에는 Logs(BeginInvoke 마셜링)가 갱신되지
        // 않으므로, exit code 판정용 에러 수는 동기 카운터로 별도 관리한다.
        private int _errorCount;
        public int ErrorCount => _errorCount;

        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Error(string message) => Log(LogLevel.Error, message);

        public void Log(LogLevel level, string message)
        {
            if (level == LogLevel.Error) System.Threading.Interlocked.Increment(ref _errorCount);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}");
            // BeginInvoke(비동기 마셜링) 사용: CLI 모드에서 UI 스레드가 GetResult()로 블로킹된 채
            // 백그라운드 작업이 로그를 남기면 Invoke(동기)는 데드락을 유발한다.
            App.Current?.Dispatcher?.BeginInvoke(() =>
            {
                Logs.Add(new LogItem { Level = level, Message = message });
            });
        }

        public void Clear()
        {
            System.Threading.Interlocked.Exchange(ref _errorCount, 0);
            App.Current?.Dispatcher?.BeginInvoke(() => Logs.Clear());
        }

        public void SaveToFile(string filePath)
        {
            var sb = new StringBuilder();
            foreach (var log in Logs)
            {
                sb.AppendLine($"[{log.FormattedTime}] [{log.Level}] {log.Message}");
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
