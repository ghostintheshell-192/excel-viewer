using SheetAtlas.Logging.Models;

namespace SheetAtlas.Core.Domain.ValueObjects
{
    public enum LoadStatus
    {
        Success,
        PartialSuccess,
        Failed
    }

    public class ExcelError
    {
        public LogSeverity Level { get; }
        public string Message { get; }
        public string Context { get; }
        public CellReference? Location { get; }
        public Exception? InnerException { get; }
        public DateTime Timestamp { get; }

        private ExcelError(LogSeverity level, string message, string context, CellReference? location = null, Exception? innerException = null)
        {
            Level = level;
            Message = message;
            Context = context;
            Location = location;
            InnerException = innerException;
            Timestamp = DateTime.UtcNow;
        }

        // Constructor overload for JSON deserialization (preserves original timestamp)
        private ExcelError(LogSeverity level, string message, string context, DateTime timestamp, CellReference? location = null, Exception? innerException = null)
        {
            Level = level;
            Message = message;
            Context = context;
            Location = location;
            InnerException = innerException;
            Timestamp = timestamp;
        }

        public static ExcelError FileError(string message, Exception? ex = null)
        {
            return new ExcelError(LogSeverity.Error, message, "File", null, ex);
        }

        public static ExcelError SheetError(string sheetName, string message, Exception? ex = null)
        {
            return new ExcelError(LogSeverity.Error, message, $"Sheet:{sheetName}", null, ex);
        }

        public static ExcelError CellError(string sheetName, CellReference location, string message, Exception? ex = null)
        {
            return new ExcelError(LogSeverity.Error, message, $"Cell:{sheetName}", location, ex);
        }

        public static ExcelError Warning(string context, string message)
        {
            return new ExcelError(LogSeverity.Warning, message, context);
        }

        public static ExcelError Info(string context, string message)
        {
            return new ExcelError(LogSeverity.Info, message, context);
        }

        public static ExcelError Critical(string context, string message, Exception? ex = null)
        {
            return new ExcelError(LogSeverity.Critical, message, context, null, ex);
        }

        // Factory method for JSON deserialization (preserves timestamp)
        public static ExcelError FromJson(LogSeverity level, string message, string context, DateTime timestamp, CellReference? location = null, Exception? innerException = null)
        {
            return new ExcelError(level, message, context, timestamp, location, innerException);
        }

        public override string ToString()
        {
            var locationStr = Location != null ? $" at {Location}" : "";
            return $"[{Level}] {Context}: {Message}{locationStr}";
        }
    }

    public class CellReference
    {
        public int Row { get; }
        public int Column { get; }

        public CellReference(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public override string ToString() => $"R{Row}C{Column}";

        public string ToExcelNotation()
        {
            string columnName = "";
            int col = Column;
            while (col >= 0)
            {
                columnName = (char)('A' + (col % 26)) + columnName;
                col = col / 26 - 1;
            }
            return $"{columnName}{Row + 1}";
        }
    }
}
