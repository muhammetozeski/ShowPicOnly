using System.Diagnostics;

namespace ShowPicOnly;

/// <summary>
/// Appends error reports to ShowPicOnly.log next to the executable. The file is created by the first error.
/// </summary>
static class ErrorLog
{
    const string FileName = "ShowPicOnly.log";

    /// <summary>
    /// Appends one entry with the time, the message and the exception. When another ShowPicOnly window is writing the
    /// file at the same moment, the entry goes to a file named after this process instead, and when that fails too,
    /// to the debugger output.
    /// </summary>
    /// <param name="message">What failed.</param>
    /// <param name="exception">The exception that caused the failure, if there is one.</param>
    public static void Write(string message, Exception? exception = null)
    {
        string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}{exception}{Environment.NewLine}";
        string filePath = Path.Combine(AppContext.BaseDirectory, FileName);
        try
        {
            File.AppendAllText(filePath, entry);
        }
        catch (Exception writeException)
        {
            try
            {
                File.AppendAllText(Path.ChangeExtension(filePath, $"{Environment.ProcessId}.log"), entry);
            }
            catch (Exception alternativeWriteException)
            {
                Debug.WriteLine($"{entry}Writing the log failed: {writeException}{Environment.NewLine}{alternativeWriteException}");
            }
        }
    }
}
