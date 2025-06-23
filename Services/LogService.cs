using System.Text;

namespace TreaYT.Services;

public class LogService : ILogService
{
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private readonly object _logLock = new();
    private readonly int _maxLogFiles = 5;
    private readonly long _maxLogSize = 10 * 1024 * 1024; // 10MB

    public LogService()
    {
        _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TreaYT",
            "Logs"
        );

        _logFilePath = Path.Combine(_logDirectory, $"TreaYT_{DateTime.Now:yyyy-MM-dd}.log");

        // Ensure log directory exists
        Directory.CreateDirectory(_logDirectory);

        // Clean up old log files
        CleanupOldLogs();
    }

    public void Debug(string message, params object[] args)
    {
        WriteLog("DEBUG", string.Format(message, args));
    }

    public void Info(string message, params object[] args)
    {
        WriteLog("INFO", string.Format(message, args));
    }

    public void Warning(string message, params object[] args)
    {
        WriteLog("WARNING", string.Format(message, args));
    }

    public void Error(string message, params object[] args)
    {
        WriteLog("ERROR", string.Format(message, args));
    }

    public void Error(Exception ex, string message = null, params object[] args)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(message))
        {
            sb.AppendLine(string.Format(message, args));
        }

        sb.AppendLine($"Exception: {ex.GetType().Name}");
        sb.AppendLine($"Message: {ex.Message}");
        sb.AppendLine($"StackTrace: {ex.StackTrace}");

        if (ex.InnerException != null)
        {
            sb.AppendLine("Inner Exception:");
            sb.AppendLine($"Type: {ex.InnerException.GetType().Name}");
            sb.AppendLine($"Message: {ex.InnerException.Message}");
            sb.AppendLine($"StackTrace: {ex.InnerException.StackTrace}");
        }

        WriteLog("ERROR", sb.ToString());
    }

    public string GetCurrentLogFilePath()
    {
        return _logFilePath;
    }

    public async Task<string> GetCurrentLogContentsAsync()
    {
        if (!File.Exists(_logFilePath))
        {
            return string.Empty;
        }

        try
        {
            return await File.ReadAllTextAsync(_logFilePath);
        }
        catch (Exception ex)
        {
            Error(ex, "Failed to read log file");
            return string.Empty;
        }
    }

    public void ClearLog()
    {
        lock (_logLock)
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    File.Delete(_logFilePath);
                }
            }
            catch (Exception ex)
            {
                Error(ex, "Failed to clear log file");
            }
        }
    }

    private void WriteLog(string level, string message)
    {
        var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";

        lock (_logLock)
        {
            try
            {
                // Check if log file size exceeds limit
                if (File.Exists(_logFilePath))
                {
                    var fileInfo = new FileInfo(_logFilePath);
                    if (fileInfo.Length > _maxLogSize)
                    {
                        // Archive current log file
                        var archivePath = Path.Combine(_logDirectory, $"TreaYT_{DateTime.Now:yyyy-MM-dd_HHmmss}.log");
                        File.Move(_logFilePath, archivePath);
                        CleanupOldLogs();
                    }
                }

                File.AppendAllText(_logFilePath, logMessage);
            }
            catch (Exception ex)
            {
                // If we can't write to the log file, write to debug output
                System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(logMessage);
            }
        }
    }

    private void CleanupOldLogs()
    {
        try
        {
            var logFiles = Directory.GetFiles(_logDirectory, "TreaYT_*.log")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .Skip(_maxLogFiles);

            foreach (var file in logFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Ignore errors while cleaning up old logs
                }
            }
        }
        catch
        {
            // Ignore errors while cleaning up old logs
        }
    }
}