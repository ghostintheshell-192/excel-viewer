

namespace SheetAtlas.Core.Application.DTOs
{
    /// <summary>
    /// Information about the Excel file being logged
    /// </summary>
    public class FileInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string OriginalPath { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string Hash { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
    }
}
