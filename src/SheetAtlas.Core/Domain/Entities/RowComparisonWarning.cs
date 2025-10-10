namespace SheetAtlas.Core.Domain.Entities
{
    /// <summary>
    /// Represents a warning about column structure inconsistencies during row comparison
    /// </summary>
    public class RowComparisonWarning
    {
        public WarningType Type { get; }
        public string Message { get; }
        public string ColumnName { get; }
        public List<string> AffectedFiles { get; }
        public string Suggestion { get; }

        public RowComparisonWarning(WarningType type, string columnName, string message,
                                  List<string> affectedFiles, string suggestion)
        {
            Type = type;
            ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            AffectedFiles = affectedFiles ?? throw new ArgumentNullException(nameof(affectedFiles));
            Suggestion = suggestion ?? throw new ArgumentNullException(nameof(suggestion));
        }

        public static RowComparisonWarning CreateMissingHeaderWarning(string columnName, List<string> filesWithMissingHeader)
        {
            var message = $"Column '{columnName}' has missing or inconsistent headers across files";
            var suggestion = $"Consider adding a proper header '{columnName}' to maintain data consistency";

            return new RowComparisonWarning(
                WarningType.MissingHeader,
                columnName,
                message,
                filesWithMissingHeader,
                suggestion);
        }

        public static RowComparisonWarning CreateStructureMismatchWarning(string columnName, List<string> affectedFiles)
        {
            var message = $"Column structure mismatch detected for '{columnName}' - data appears consistent but positioned differently";
            var suggestion = $"The comparison will proceed using intelligent column mapping, but standardizing file structures is recommended";

            return new RowComparisonWarning(
                WarningType.StructureMismatch,
                columnName,
                message,
                affectedFiles,
                suggestion);
        }
    }

    public enum WarningType
    {
        MissingHeader,
        StructureMismatch,
        DataInconsistency
    }
}
