namespace SheetAtlas.UI.Avalonia.Models
{
    /// <summary>
    /// Represents the type of difference found when comparing cells across rows
    /// </summary>
    public enum ComparisonType
    {
        /// <summary>
        /// All values in this column position are the same across rows
        /// </summary>
        Match,

        /// <summary>
        /// Values differ between rows in this column position
        /// </summary>
        Different,

        /// <summary>
        /// This cell has a value but corresponding cells in other rows are empty
        /// </summary>
        New,

        /// <summary>
        /// This cell is empty but corresponding cells in other rows have values
        /// </summary>
        Missing
    }
}
