using SheetAtlas.Core.Application.DTOs;

namespace SheetAtlas.Core.Application.Interfaces
{
    /// <summary>
    /// Service for managing structured file logging
    /// Provides read/write operations for Excel file error logs in JSON format
    /// </summary>
    public interface IFileLogService
    {
        /// <summary>
        /// Saves a log entry for a file load attempt
        /// Creates folder structure and JSON file automatically
        /// </summary>
        /// <param name="logEntry">The log entry to save</param>
        Task SaveFileLogAsync(FileLogEntry logEntry);

        /// <summary>
        /// Gets the complete log history for a specific Excel file
        /// Returns logs sorted by timestamp (newest first)
        /// </summary>
        /// <param name="filePath">Path to the Excel file</param>
        /// <returns>List of log entries for this file</returns>
        Task<List<FileLogEntry>> GetFileLogHistoryAsync(string filePath);

        /// <summary>
        /// Gets the most recent log entry for a specific Excel file
        /// </summary>
        /// <param name="filePath">Path to the Excel file</param>
        /// <returns>Latest log entry or null if no logs exist</returns>
        Task<FileLogEntry?> GetLatestFileLogAsync(string filePath);

        /// <summary>
        /// Deletes all log files for a specific Excel file
        /// </summary>
        /// <param name="filePath">Path to the Excel file</param>
        Task DeleteFileLogsAsync(string filePath);

        /// <summary>
        /// Cleans up old log files based on retention policy
        /// Deletes files older than the specified number of days
        /// </summary>
        /// <param name="retentionDays">Number of days to retain logs (0 = skip cleanup)</param>
        Task CleanupOldLogsAsync(int retentionDays);

        /// <summary>
        /// Gets the root directory where file logs are stored
        /// </summary>
        string GetLogRootDirectory();
    }
}
