using SheetAtlas.Core.Domain.Entities;

namespace SheetAtlas.Core.Application.Interfaces
{
    /// <summary>
    /// Reader for specific Excel file formats
    /// </summary>
    /// <remarks>
    /// Implementations must be STATELESS and thread-safe for Singleton registration.
    /// Returns ExcelFile with errors (Result Pattern), never throws for business errors.
    /// Extension ownership is strict - each extension belongs to exactly one reader.
    /// </remarks>
    public interface IFileFormatReader
    {
        /// <summary>
        /// File extensions supported by this reader (lowercase with leading dot)
        /// </summary>
        /// <example>new[] { ".xlsx", ".xlsm" }.AsReadOnly()</example>
        IReadOnlyList<string> SupportedExtensions { get; }

        /// <summary>
        /// Reads Excel file and converts to domain entity
        /// </summary>
        /// <param name="filePath">Absolute path to file (must not be null/empty)</param>
        /// <param name="cancellationToken">Cancellation token for long operations</param>
        /// <returns>ExcelFile with Status (Success/PartialSuccess/Failed). Check Errors for details.</returns>
        /// <exception cref="ArgumentNullException">If filePath is null or whitespace</exception>
        /// <exception cref="OperationCanceledException">If cancellation requested</exception>
        Task<ExcelFile> ReadAsync(string filePath, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Extended interface for readers that require runtime configuration
    /// </summary>
    /// <remarks>
    /// Use for formats like CSV with variable delimiters, encodings, etc.
    /// Readers should have sensible defaults and work without explicit configuration.
    /// </remarks>
    public interface IConfigurableFileReader : IFileFormatReader
    {
        /// <summary>
        /// Applies configuration options (e.g., CsvReaderOptions)
        /// </summary>
        /// <param name="options">Format-specific options</param>
        /// <exception cref="ArgumentNullException">If options is null</exception>
        /// <exception cref="ArgumentException">If options type is not supported</exception>
        void Configure(IReaderOptions options);
    }

    /// <summary>
    /// Base interface for reader configuration options
    /// </summary>
    public interface IReaderOptions { }
}
