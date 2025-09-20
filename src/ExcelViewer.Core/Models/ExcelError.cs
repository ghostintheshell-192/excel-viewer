namespace ExcelViewer.Core.Models
{
    public enum LoadStatus
    {
        Success,
        PartialSuccess,
        Failed
    }

    public enum ErrorLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public class ExcelError
    {
        public ErrorLevel Level { get; }
        public string Message { get; }
        public string Context { get; }
        public CellReference? Location { get; }
        public Exception? InnerException { get; }
        public DateTime Timestamp { get; }

        private ExcelError(ErrorLevel level, string message, string context, CellReference? location = null, Exception? innerException = null)
        {
            Level = level;
            Message = message;
            Context = context;
            Location = location;
            InnerException = innerException;
            Timestamp = DateTime.UtcNow;
        }

        public static ExcelError FileError(string message, Exception? ex = null)
        {
            return new ExcelError(ErrorLevel.Error, message, "File", null, ex);
        }

        public static ExcelError SheetError(string sheetName, string message, Exception? ex = null)
        {
            return new ExcelError(ErrorLevel.Error, message, $"Sheet:{sheetName}", null, ex);
        }

        public static ExcelError CellError(string sheetName, CellReference location, string message, Exception? ex = null)
        {
            return new ExcelError(ErrorLevel.Error, message, $"Cell:{sheetName}", location, ex);
        }

        public static ExcelError Warning(string context, string message)
        {
            return new ExcelError(ErrorLevel.Warning, message, context);
        }

        public static ExcelError Info(string context, string message)
        {
            return new ExcelError(ErrorLevel.Info, message, context);
        }

        public static ExcelError Critical(string context, string message, Exception? ex = null)
        {
            return new ExcelError(ErrorLevel.Critical, message, context, null, ex);
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