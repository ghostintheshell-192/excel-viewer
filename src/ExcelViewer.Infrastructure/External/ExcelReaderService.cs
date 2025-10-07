using ExcelViewer.Core.Domain.Entities;
using ExcelViewer.Core.Domain.ValueObjects;
using ExcelViewer.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ExcelViewer.Infrastructure.External
{
    /// <summary>
    /// Service for loading Excel files using format-specific readers
    /// </summary>
    public interface IExcelReaderService
    {
        Task<ExcelFile> LoadFileAsync(string filePath, CancellationToken cancellationToken = default);
        Task<List<ExcelFile>> LoadFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Orchestrator that delegates file reading to appropriate format-specific readers
    /// </summary>
    public class ExcelReaderService : IExcelReaderService
    {
        private readonly IEnumerable<IFileFormatReader> _readers;
        private readonly ILogger<ExcelReaderService> _logger;

        public ExcelReaderService(
            IEnumerable<IFileFormatReader> readers,
            ILogger<ExcelReaderService> logger)
        {
            _readers = readers ?? throw new ArgumentNullException(nameof(readers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ExcelFile>> LoadFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default)
        {
            var results = new List<ExcelFile>();

            foreach (var filePath in filePaths)
            {
                var file = await LoadFileAsync(filePath, cancellationToken);
                results.Add(file);
            }

            return results;
        }

        public async Task<ExcelFile> LoadFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            // Validation: Fail fast for invalid input
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            _logger.LogInformation("Loading file {FilePath} with extension {Extension}", filePath, extension);

            // Find appropriate reader based on file extension
            var reader = _readers.FirstOrDefault(r => r.SupportedExtensions.Contains(extension));

            if (reader == null)
            {
                _logger.LogWarning("No reader found for extension {Extension}", extension);

                var supportedFormats = string.Join(", ",
                    _readers.SelectMany(r => r.SupportedExtensions).Distinct().OrderBy(e => e));

                var errors = new List<ExcelError>
                {
                    ExcelError.Critical("File",
                        $"Unsupported file format '{extension}'. Supported formats: {supportedFormats}")
                };

                return new ExcelFile(filePath, LoadStatus.Failed,
                    new Dictionary<string, DataTable>(), errors);
            }

            _logger.LogDebug("Using {ReaderType} for {Extension}",
                reader.GetType().Name, extension);

            // Delegate to format-specific reader
            return await reader.ReadAsync(filePath, cancellationToken);
        }
    }
}
