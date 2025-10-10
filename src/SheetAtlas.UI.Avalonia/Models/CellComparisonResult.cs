namespace SheetAtlas.UI.Avalonia.Models
{
    /// <summary>
    /// Represents the result of comparing a cell value with other values in the same column
    /// </summary>
    public class CellComparisonResult
    {
        /// <summary>
        /// The type of comparison result (Match, Different, New, Missing)
        /// </summary>
        public ComparisonType Type { get; }

        /// <summary>
        /// Intensity of the difference (0.0 = most common, 1.0 = unique/rarest)
        /// Used to determine color gradients for highlighting
        /// </summary>
        public double Intensity { get; }

        /// <summary>
        /// Number of occurrences of this value in the column
        /// </summary>
        public int Frequency { get; }

        /// <summary>
        /// Total number of non-empty values in the column
        /// </summary>
        public int TotalValues { get; }

        public CellComparisonResult(ComparisonType type, double intensity, int frequency, int totalValues)
        {
            Type = type;
            Intensity = Math.Clamp(intensity, 0.0, 1.0);
            Frequency = frequency;
            TotalValues = totalValues;
        }

        /// <summary>
        /// Creates a comparison result for a matching value (no highlighting needed)
        /// </summary>
        public static CellComparisonResult CreateMatch(int frequency = 1, int totalValues = 1)
        {
            return new CellComparisonResult(ComparisonType.Match, 0.0, frequency, totalValues);
        }

        /// <summary>
        /// Creates a comparison result for a different value with calculated intensity using logarithmic distribution
        /// </summary>
        public static CellComparisonResult CreateDifferent(int frequency, int rank, int totalGroups, int totalValues)
        {
            // Use logarithmic distribution for optimal visual separation
            // rank=0 (most frequent) → intensity=0.0 (lightest color)
            // rank=n (least frequent) → intensity=1.0 (darkest color)
            double intensity = CalculateLogarithmicIntensity(rank, totalGroups);
            return new CellComparisonResult(ComparisonType.Different, intensity, frequency, totalValues);
        }


        /// <summary>
        /// Calculates logarithmic intensity distribution for optimal visual separation
        /// </summary>
        private static double CalculateLogarithmicIntensity(int rank, int totalGroups)
        {
            if (totalGroups <= 1) return 0.0;

            // Normalize rank to 0.0-1.0 range
            double normalizedRank = (double)rank / (totalGroups - 1);

            // Apply logarithmic scaling: log(1 + x*(e-1)) / log(e)
            // This creates a smooth curve from 0.0 to 1.0 with better separation for early ranks
            double intensity = Math.Log(1 + normalizedRank * (Math.E - 1)) / Math.Log(Math.E);

            return Math.Clamp(intensity, 0.0, 1.0);
        }

        /// <summary>
        /// Creates a comparison result for a new value
        /// </summary>
        public static CellComparisonResult CreateNew(int totalValues = 1)
        {
            return new CellComparisonResult(ComparisonType.New, 1.0, 1, totalValues);
        }

        /// <summary>
        /// Creates a comparison result for a missing value
        /// </summary>
        public static CellComparisonResult CreateMissing(int totalValues = 1)
        {
            return new CellComparisonResult(ComparisonType.Missing, 0.5, 0, totalValues);
        }

        public override string ToString()
        {
            return $"{Type} (Intensity: {Intensity:F2}, Frequency: {Frequency}/{TotalValues})";
        }
    }
}
