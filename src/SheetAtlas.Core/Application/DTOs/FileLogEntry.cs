using SheetAtlas.Core.Domain.ValueObjects;

namespace SheetAtlas.Core.Application.DTOs
{
    /// <summary>
    /// Root object for structured file log JSON
    /// Represents a single load attempt for an Excel file
    /// </summary>
    public class FileLogEntry
    {
        public string SchemaVersion { get; set; } = "1.0";
        public FileInfoDto File { get; set; } = null!;
        public LoadAttemptInfo LoadAttempt { get; set; } = null!;
        public List<ExcelError> Errors { get; set; } = new();
        public ErrorSummary Summary { get; set; } = null!;
        public Dictionary<string, object?>? Extensions { get; set; }
    }
}
