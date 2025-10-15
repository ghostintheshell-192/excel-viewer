namespace SheetAtlas.Core.Domain.Exceptions
{
    /// <summary>
    /// Thrown when file comparison operations fail due to incompatible files.
    /// Represents business rule violations specific to comparison logic.
    /// </summary>
    public class ComparisonException : SheetAtlasException
    {
        public ComparisonException(
            string technicalMessage,
            string userMessage,
            Exception? innerException = null)
            : base(technicalMessage, userMessage, "COMPARISON_ERROR", innerException)
        {
        }

        public static ComparisonException IncompatibleStructures(string file1, string file2)
            => new(
                $"Files have incompatible structures: {file1} vs {file2}",
                "The files have incompatible structures and cannot be compared");

        public static ComparisonException MissingSheet(string sheetName, string fileName)
            => new(
                $"Sheet '{sheetName}' not found in file {fileName}",
                $"Sheet '{sheetName}' is not present in file {Path.GetFileName(fileName)}");

        public static ComparisonException NoCommonColumns()
            => new(
                "No common columns found between files",
                "No common columns found between files");
    }
}
