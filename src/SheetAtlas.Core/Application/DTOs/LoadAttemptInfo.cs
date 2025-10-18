using System;

namespace SheetAtlas.Core.Application.DTOs
{
    /// <summary>
    /// Information about the file load attempt
    /// </summary>
    public class LoadAttemptInfo
    {
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public long DurationMs { get; set; }
        public string AppVersion { get; set; } = string.Empty;
    }
}
