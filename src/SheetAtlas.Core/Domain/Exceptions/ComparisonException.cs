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
                "I file hanno strutture incompatibili e non possono essere confrontati");

        public static ComparisonException MissingSheet(string sheetName, string fileName)
            => new(
                $"Sheet '{sheetName}' not found in file {fileName}",
                $"Il foglio '{sheetName}' non è presente nel file {Path.GetFileName(fileName)}");

        public static ComparisonException NoCommonColumns()
            => new(
                "No common columns found between files",
                "Nessuna colonna in comune trovata tra i file");
    }
}
