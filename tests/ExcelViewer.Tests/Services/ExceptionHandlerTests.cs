using ExcelViewer.Core.Application.Interfaces;
using ExcelViewer.Core.Application.Services;
using ExcelViewer.Core.Domain.Exceptions;
using ExcelViewer.Core.Domain.ValueObjects;
using ExcelViewer.Infrastructure.External;
using ExcelViewer.Infrastructure.External.Readers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExcelViewer.Tests.Services
{
    public class ExceptionHandlerTests
    {
        private readonly Mock<ILogger<ExceptionHandler>> _mockLogger;
        private readonly ExceptionHandler _handler;

        public ExceptionHandlerTests()
        {
            _mockLogger = new Mock<ILogger<ExceptionHandler>>();
            _handler = new ExceptionHandler(_mockLogger.Object);
        }

        [Fact]
        public void Handle_ComparisonException_ReturnsCriticalError()
        {
            // Arrange
            var exception = ComparisonException.NoCommonColumns();
            var context = "CompareFiles";

            // Act
            var result = _handler.Handle(exception, context);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(ErrorLevel.Critical);
            result.Message.Should().Contain("colonna");
        }

        [Fact]
        public void Handle_FileNotFoundException_ReturnsUserFriendlyMessage()
        {
            // Arrange
            var exception = new FileNotFoundException("File not found", "test.xlsx");
            var context = "LoadFile";

            // Act
            var result = _handler.Handle(exception, context);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(ErrorLevel.Critical);
            result.Message.Should().Contain("non trovato");
            result.Message.Should().Contain("test.xlsx");
        }

        [Fact]
        public void Handle_UnauthorizedAccessException_ReturnsAccessDeniedMessage()
        {
            // Arrange
            var exception = new UnauthorizedAccessException("Access denied");
            var context = "LoadFile";

            // Act
            var result = _handler.Handle(exception, context);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(ErrorLevel.Critical);
            result.Message.Should().Contain("Accesso");
            result.Message.Should().Contain("negato");
        }

        [Fact]
        public void Handle_IOException_ReturnsFileReadError()
        {
            // Arrange
            var exception = new IOException("IO error occurred");
            var context = "LoadFile";

            // Act
            var result = _handler.Handle(exception, context);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(ErrorLevel.Critical);
            result.Message.Should().Contain("lettura file");
        }

        [Fact]
        public void Handle_GenericException_ReturnsFallbackMessage()
        {
            // Arrange
            var exception = new InvalidOperationException("Something went wrong");
            var context = "ProcessData";

            // Act
            var result = _handler.Handle(exception, context);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(ErrorLevel.Critical);
            result.Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GetUserMessage_FileNotFoundException_ReturnsGenericMessage()
        {
            // Arrange
            var exception = new FileNotFoundException();

            // Act
            var message = _handler.GetUserMessage(exception);

            // Assert
            message.Should().Be("File non trovato");
        }

        [Fact]
        public void IsRecoverable_ArgumentNullException_ReturnsFalse()
        {
            // Arrange
            var exception = new ArgumentNullException("param");

            // Act
            var isRecoverable = _handler.IsRecoverable(exception);

            // Assert
            isRecoverable.Should().BeFalse();
        }

        [Fact]
        public void IsRecoverable_NullReferenceException_ReturnsFalse()
        {
            // Arrange
            var exception = new NullReferenceException();

            // Act
            var isRecoverable = _handler.IsRecoverable(exception);

            // Assert
            isRecoverable.Should().BeFalse();
        }

        #region Integration Tests with Real File Scenarios

        [Fact]
        public async Task Handle_RealNonExistentFile_HandlesFileNotFoundCorrectly()
        {
            // Arrange
            var excelReaderService = CreateRealExcelReaderService();
            var nonExistentPath = Path.Combine(GetTestDataPath(), "NonExistent", "missing.xlsx");

            // Act
            var result = await excelReaderService.LoadFileAsync(nonExistentPath);

            // The service returns ExcelFile with errors instead of throwing
            // Now handle those errors with ExceptionHandler
            var firstError = result.Errors.FirstOrDefault();

            // Assert
            result.Status.Should().Be(LoadStatus.Failed);
            result.Errors.Should().NotBeEmpty();
            firstError.Should().NotBeNull();
            firstError!.Level.Should().Be(ErrorLevel.Critical);
        }

        [Fact]
        public async Task Handle_RealCorruptedFile_HandlesCorruptionCorrectly()
        {
            // Arrange
            var excelReaderService = CreateRealExcelReaderService();
            var corruptedPath = GetTestFilePath("Invalid", "corrupted.xlsx");

            // Act
            var result = await excelReaderService.LoadFileAsync(corruptedPath);

            // Assert
            result.Status.Should().Be(LoadStatus.Failed);
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.Level == ErrorLevel.Critical);

            // Test that ExceptionHandler would handle this correctly
            var firstError = result.Errors.First();
            if (firstError.InnerException != null)
            {
                var handledError = _handler.Handle(firstError.InnerException, "LoadFile");
                handledError.Level.Should().Be(ErrorLevel.Critical);
            }
        }

        [Fact]
        public async Task Handle_RealUnsupportedFormat_HandlesFormatErrorCorrectly()
        {
            // Arrange
            var excelReaderService = CreateRealExcelReaderService();
            var unsupportedPath = GetTestFilePath("Invalid", "unsupported.xls");

            // Act
            var result = await excelReaderService.LoadFileAsync(unsupportedPath);

            // Assert
            result.Status.Should().Be(LoadStatus.Failed);
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e =>
                e.Level == ErrorLevel.Critical &&
                e.Message.Contains("format"));
        }

        [Fact]
        public void Handle_RealIOException_ReturnsUserFriendlyMessage()
        {
            // Arrange
            var ioException = new IOException("The process cannot access the file because it is being used by another process");
            var context = "LoadFile";

            // Act
            var result = _handler.Handle(ioException, context);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(ErrorLevel.Critical);
            result.Message.Should().Contain("lettura file");
            result.InnerException.Should().Be(ioException);
        }

        [Fact]
        public void Handle_RealUnauthorizedAccessException_ReturnsPermissionDeniedMessage()
        {
            // Arrange
            var unauthorizedException = new UnauthorizedAccessException("Access to the path is denied");
            var context = "LoadFile";

            // Act
            var result = _handler.Handle(unauthorizedException, context);

            // Assert
            result.Should().NotBeNull();
            result.Level.Should().Be(ErrorLevel.Critical);
            result.Message.Should().Contain("Accesso");
            result.Message.Should().Contain("negato");
            result.InnerException.Should().Be(unauthorizedException);
        }

        #endregion

        #region Helper Methods

        private IExcelReaderService CreateRealExcelReaderService()
        {
            var serviceLogger = new Mock<ILogger<ExcelReaderService>>();
            var readerLogger = new Mock<ILogger<OpenXmlFileReader>>();
            var cellParser = new CellReferenceParser();
            var cellValueReader = new CellValueReader();
            var mergedCellProcessor = new MergedCellProcessor(cellParser, cellValueReader);

            // Create OpenXmlFileReader with its dependencies
            var openXmlReader = new OpenXmlFileReader(readerLogger.Object, cellParser, mergedCellProcessor, cellValueReader);
            var readers = new List<IFileFormatReader> { openXmlReader };

            return new ExcelReaderService(readers, serviceLogger.Object);
        }

        private string GetTestDataPath()
        {
            return Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "..",
                "..",
                "TestData"
            );
        }

        private string GetTestFilePath(string category, string filename)
        {
            var path = Path.Combine(GetTestDataPath(), category, filename);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Test file not found: {path}. Make sure TestData files are generated.");
            }

            return path;
        }

        #endregion
    }
}
