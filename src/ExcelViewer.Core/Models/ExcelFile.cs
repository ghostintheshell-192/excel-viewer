using System.Data;

namespace ExcelViewer.Core.Models
{
    public class ExcelFile
    {
        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);
        public LoadStatus Status { get; }
        public DateTime LoadedAt { get; }
        public IReadOnlyDictionary<string, DataTable> Sheets { get; }
        public IReadOnlyList<ExcelError> Errors { get; }

        public ExcelFile(string filePath, LoadStatus status, Dictionary<string, DataTable> sheets, List<ExcelError> errors)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Status = status;
            Sheets = sheets?.AsReadOnly() ?? throw new ArgumentNullException(nameof(sheets));
            Errors = errors?.AsReadOnly() ?? throw new ArgumentNullException(nameof(errors));
            LoadedAt = DateTime.UtcNow;
        }

        public DataTable? GetSheet(string sheetName)
        {
            return Sheets.TryGetValue(sheetName, out var sheet) ? sheet : null;
        }

        public bool HasErrors => Errors.Any(e => e.Level == ErrorLevel.Error || e.Level == ErrorLevel.Critical);

        public bool HasWarnings => Errors.Any(e => e.Level == ErrorLevel.Warning);

        public bool HasCriticalErrors => Errors.Any(e => e.Level == ErrorLevel.Critical);

        public IEnumerable<ExcelError> GetErrorsByLevel(ErrorLevel level)
        {
            return Errors.Where(e => e.Level == level);
        }

        public IEnumerable<string> GetSheetNames()
        {
            return Sheets.Keys;
        }
    }
}
