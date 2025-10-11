using SheetAtlas.Core.Domain.ValueObjects;

namespace SheetAtlas.Core.Domain.Entities
{
    public class ExcelFile : IDisposable
    {
        private bool _disposed = false;

        public string FilePath { get; }
        public string FileName => Path.GetFileName(FilePath);
        public LoadStatus Status { get; }
        public DateTime LoadedAt { get; }
        public IReadOnlyDictionary<string, SASheetData> Sheets { get; }
        public IReadOnlyList<ExcelError> Errors { get; }

        public ExcelFile(string filePath, LoadStatus status, Dictionary<string, SASheetData> sheets, List<ExcelError> errors)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Status = status;
            Sheets = sheets?.AsReadOnly() ?? throw new ArgumentNullException(nameof(sheets));
            Errors = errors?.AsReadOnly() ?? throw new ArgumentNullException(nameof(errors));
            LoadedAt = DateTime.UtcNow;
        }

        public SASheetData? GetSheet(string sheetName)
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

        public void Dispose()
        {
            Dispose(true);

            // NOTE: GC.SuppressFinalize() intentionally NOT called
            // REASON: DataTable has 10-14x memory overhead (managed but memory-intensive)
            // ISSUE: If lingering references keep this object alive, finalizer ensures cleanup
            // TODO: When DataTable is replaced with lightweight structures, add SuppressFinalize()
            // and follow standard IDisposable pattern for better GC performance
        }

        ~ExcelFile()
        {
            // Finalizer as safety net for aggressive cleanup
            // Critical for releasing large DataTable memory (hundreds of MB per file)
            // Ensures disposal even if external references prevent immediate collection
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose all SASheetData instances to free memory
                // SASheetData contains large SACellData arrays that should be cleared promptly
                foreach (var sheet in Sheets.Values)
                {
                    sheet?.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
