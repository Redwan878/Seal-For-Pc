namespace TreaYT.Services;

public interface ILogService
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments.</param>
    void Debug(string message, params object[] args);

    /// <summary>
    /// Logs an information message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments.</param>
    void Info(string message, params object[] args);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments.</param>
    void Warning(string message, params object[] args);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">Optional format arguments.</param>
    void Error(string message, params object[] args);

    /// <summary>
    /// Logs an error message with exception details.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="message">Optional additional message.</param>
    /// <param name="args">Optional format arguments.</param>
    void Error(Exception ex, string message = null, params object[] args);

    /// <summary>
    /// Gets the path to the current log file.
    /// </summary>
    /// <returns>The full path to the current log file.</returns>
    string GetCurrentLogFilePath();

    /// <summary>
    /// Gets the contents of the current log file.
    /// </summary>
    /// <returns>The contents of the current log file as a string.</returns>
    Task<string> GetCurrentLogContentsAsync();

    /// <summary>
    /// Clears the current log file.
    /// </summary>
    void ClearLog();
}