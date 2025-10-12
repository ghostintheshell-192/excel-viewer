using System.Globalization;
using System.Text;
using SheetAtlas.Core.Application.Interfaces;

namespace SheetAtlas.Core.Application.DTOs
{
    /// <summary>
    /// Configuration options for CSV file reading
    /// </summary>
    public class CsvReaderOptions : IReaderOptions
    {
        /// <summary>
        /// Character used to separate fields (default: comma)
        /// </summary>
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// Text encoding for reading CSV file (default: UTF-8)
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Whether the first row contains headers (default: true)
        /// </summary>
        public bool HasHeaderRow { get; set; } = true;

        /// <summary>
        /// Culture for parsing numbers and dates (default: invariant)
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Default configuration (comma-separated, UTF-8, with headers)
        /// </summary>
        public static CsvReaderOptions Default => new();

        /// <summary>
        /// Configuration for semicolon-separated files
        /// </summary>
        public static CsvReaderOptions Semicolon => new() { Delimiter = ';' };

        /// <summary>
        /// Configuration for tab-separated files
        /// </summary>
        public static CsvReaderOptions Tab => new() { Delimiter = '\t' };
    }
}
