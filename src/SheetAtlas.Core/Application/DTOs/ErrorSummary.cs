using System.Collections.Generic;

namespace SheetAtlas.Core.Application.DTOs
{
    /// <summary>
    /// Pre-calculated aggregations for UI performance
    /// </summary>
    public class ErrorSummary
    {
        public int TotalErrors { get; set; }
        public Dictionary<string, int> BySeverity { get; set; } = new();
        public Dictionary<string, int> ByContext { get; set; } = new();
    }
}
