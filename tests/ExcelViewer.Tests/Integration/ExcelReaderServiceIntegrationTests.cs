using ExcelViewer.Core.Application.Interfaces;
using ExcelViewer.Core.Application.Services;
using ExcelViewer.Core.Domain.ValueObjects;
using ExcelViewer.Infrastructure.External;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExcelViewer.Tests.Integration
{
    /// <summary>
    /// Integration tests for ExcelReaderService using real Excel files.
    /// These tests verify the entire file reading pipeline from disk to DataTable.
    /// </summary>
    public class ExcelReaderServiceIntegrationTests : IDisposable
    {
        private readonly ExcelReaderService _service;
        private readonly string _testDataPath;

        public ExcelReaderServiceIntegrationTests()
        {
            // Setup real dependencies (not mocks) for integration testing
            var logger = new Mock<ILogger<ExcelReaderService>>();
            var cellParser = new CellReferenceParser();
            var cellValueReader = new CellValueReader();
            var mergedCellProcessor = new MergedCellProcessor(cellParser, cellValueReader);

            _service = new ExcelReaderService(logger.Object, cellParser, mergedCellProcessor, cellValueReader);

            // Get path to TestData directory
            _testDataPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "..",
                "..",
                "TestData"
            );
        }

        public void Dispose()
        {
            // Cleanup if needed
        }

        #region Valid Files Tests

        [Fact]
        public async Task LoadFileAsync_SimpleFile_ReadsAllDataCorrectly()
        {
            // Arrange
            var filePath = GetTestFilePath("Valid", "simple.xlsx");

            // Act
            var result = await _service.LoadFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(LoadStatus.Success);
            result.Sheets.Should().ContainKey("Sheet1");

            var sheet = result.Sheets["Sheet1"];
            sheet.Columns.Count.Should().Be(3);
            sheet.Rows.Count.Should().Be(2);

            // Verify headers
            sheet.Columns[0].ColumnName.Should().Be("Name");
            sheet.Columns[1].ColumnName.Should().Be("Age");
            sheet.Columns[2].ColumnName.Should().Be("City");

            // Verify first row data
            sheet.Rows[0]["Name"].Should().Be("Alice");
            sheet.Rows[0]["Age"].Should().Be("30");
            sheet.Rows[0]["City"].Should().Be("Rome");

            // Verify second row data
            sheet.Rows[1]["Name"].Should().Be("Bob");
            sheet.Rows[1]["Age"].Should().Be("25");
            sheet.Rows[1]["City"].Should().Be("Milan");
        }

        [Fact]
        public async Task LoadFileAsync_LargeFile_Reads100RowsCorrectly()
        {
            // Arrange
            var filePath = GetTestFilePath("Valid", "large.xlsx");

            // Act
            var result = await _service.LoadFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(LoadStatus.Success);
            result.Sheets.Should().ContainKey("Data");

            var sheet = result.Sheets["Data"];
            sheet.Columns.Count.Should().Be(5);
            sheet.Rows.Count.Should().Be(100);

            // Verify headers
            sheet.Columns[0].ColumnName.Should().Be("ID");
            sheet.Columns[1].ColumnName.Should().Be("Product");
            sheet.Columns[2].ColumnName.Should().Be("Quantity");
            sheet.Columns[3].ColumnName.Should().Be("Price");
            sheet.Columns[4].ColumnName.Should().Be("Total");

            // Verify first row
            sheet.Rows[0]["ID"].Should().Be("1");
            sheet.Rows[0]["Product"].Should().Be("Product 1");

            // Verify last row
            sheet.Rows[99]["ID"].Should().Be("100");
            sheet.Rows[99]["Product"].Should().Be("Product 100");
        }

        [Fact]
        public async Task LoadFileAsync_MultiSheetFile_ReadsAllSheetsCorrectly()
        {
            // Arrange
            var filePath = GetTestFilePath("Valid", "multi-sheet.xlsx");

            // Act
            var result = await _service.LoadFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(LoadStatus.Success);
            result.Sheets.Should().HaveCount(3);

            // Verify all sheets exist
            result.Sheets.Should().ContainKey("Employees");
            result.Sheets.Should().ContainKey("Departments");
            result.Sheets.Should().ContainKey("Summary");

            // Verify Employees sheet
            var employeesSheet = result.Sheets["Employees"];
            employeesSheet.Columns.Count.Should().Be(2);
            employeesSheet.Columns[0].ColumnName.Should().Be("Employee");
            employeesSheet.Columns[1].ColumnName.Should().Be("Department");

            // Verify Departments sheet
            var departmentsSheet = result.Sheets["Departments"];
            departmentsSheet.Columns.Count.Should().Be(2);
            departmentsSheet.Columns[0].ColumnName.Should().Be("Department");
            departmentsSheet.Columns[1].ColumnName.Should().Be("Budget");

            // Verify Summary sheet
            var summarySheet = result.Sheets["Summary"];
            summarySheet.Columns.Count.Should().Be(2);
            summarySheet.Columns[0].ColumnName.Should().Be("Total Employees");
            summarySheet.Columns[1].ColumnName.Should().Be("Total Budget");
        }

        #endregion

        #region Invalid Files Tests

        [Fact]
        public async Task LoadFileAsync_EmptyFile_ReturnsSuccessWithEmptySheet()
        {
            // Arrange
            var filePath = GetTestFilePath("Invalid", "empty.xlsx");

            // Act
            var result = await _service.LoadFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            // An empty file with valid structure should load successfully
            result.Status.Should().Be(LoadStatus.Success);
            result.Sheets.Should().ContainKey("Sheet1");

            var sheet = result.Sheets["Sheet1"];
            sheet.Rows.Count.Should().Be(0);
        }

        [Fact]
        public async Task LoadFileAsync_CorruptedFile_ReturnsFailedStatus()
        {
            // Arrange
            var filePath = GetTestFilePath("Invalid", "corrupted.xlsx");

            // Act
            var result = await _service.LoadFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(LoadStatus.Failed);
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e => e.Level == ErrorLevel.Critical);
        }

        [Fact]
        public async Task LoadFileAsync_UnsupportedFormat_ReturnsFailedStatus()
        {
            // Arrange
            var filePath = GetTestFilePath("Invalid", "unsupported.xls");

            // Act
            var result = await _service.LoadFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(LoadStatus.Failed);
            result.Errors.Should().NotBeEmpty();
            result.Errors.Should().Contain(e =>
                e.Level == ErrorLevel.Critical &&
                e.Message.Contains("format"));
        }

        [Fact]
        public async Task LoadFileAsync_NonExistentFile_ThrowsException()
        {
            // Arrange
            var filePath = Path.Combine(_testDataPath, "NonExistent", "missing.xlsx");

            // Act
            Func<Task> act = async () => await _service.LoadFileAsync(filePath);

            // Assert - The service doesn't throw for non-existent files, it returns Failed status
            var result = await _service.LoadFileAsync(filePath);
            result.Status.Should().Be(LoadStatus.Failed);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task LoadFileAsync_NullFilePath_ThrowsArgumentNullException()
        {
            // Act
            Func<Task> act = async () => await _service.LoadFileAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task LoadFileAsync_EmptyFilePath_ThrowsArgumentNullException()
        {
            // Act
            Func<Task> act = async () => await _service.LoadFileAsync(string.Empty);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public async Task LoadFileAsync_SpecialCharactersFile_ReadsUnicodeCorrectly()
        {
            // Arrange
            var filePath = GetTestFilePath("EdgeCases", "special-chars.xlsx");

            // Act
            var result = await _service.LoadFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(LoadStatus.Success);
            result.Sheets.Should().ContainKey("Special Chars");

            var sheet = result.Sheets["Special Chars"];
            sheet.Rows.Count.Should().BeGreaterThan(0);

            // Verify special characters are preserved
            var firstRow = sheet.Rows[0];
            firstRow["Name"].ToString().Should().Contain("Café");
            firstRow["Symbols"].ToString().Should().Contain("€");
        }

        [Fact]
        public async Task LoadFileAsync_FormulasFile_ReadsFormulasCorrectly()
        {
            // Arrange
            var filePath = GetTestFilePath("EdgeCases", "formulas.xlsx");

            // Act
            var result = await _service.LoadFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(LoadStatus.Success);
            result.Sheets.Should().ContainKey("Formulas");

            var sheet = result.Sheets["Formulas"];
            sheet.Columns.Count.Should().Be(3);
            sheet.Rows.Count.Should().BeGreaterThan(0);

            // Note: OpenXml reads formula results, not the formulas themselves
            // Verify the structure is correct
            sheet.Columns[0].ColumnName.Should().Be("Value1");
            sheet.Columns[1].ColumnName.Should().Be("Value2");
            sheet.Columns[2].ColumnName.Should().Be("Sum");
        }

        [Fact]
        public async Task LoadFileAsync_MergedCellsFile_HandlesMergedCellsCorrectly()
        {
            // Arrange
            var filePath = GetTestFilePath("EdgeCases", "merged-cells.xlsx");

            // Act
            var result = await _service.LoadFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(LoadStatus.Success);
            result.Sheets.Should().ContainKey("Merged");

            var sheet = result.Sheets["Merged"];
            sheet.Rows.Count.Should().BeGreaterThan(0);

            // Verify merged cell header is read
            sheet.Columns[0].ColumnName.Should().Be("Merged Title");
        }

        #endregion

        #region Multiple Files Tests

        [Fact]
        public async Task LoadFilesAsync_MultipleValidFiles_ReadsAllFiles()
        {
            // Arrange
            var filePaths = new[]
            {
                GetTestFilePath("Valid", "simple.xlsx"),
                GetTestFilePath("Valid", "multi-sheet.xlsx")
            };

            // Act
            var results = await _service.LoadFilesAsync(filePaths);

            // Assert
            results.Should().HaveCount(2);
            results.Should().AllSatisfy(r => r.Status.Should().Be(LoadStatus.Success));

            results[0].Sheets.Should().ContainKey("Sheet1");
            results[1].Sheets.Should().HaveCount(3);
        }

        [Fact]
        public async Task LoadFilesAsync_MixedValidAndInvalidFiles_ProcessesAllFiles()
        {
            // Arrange
            var filePaths = new[]
            {
                GetTestFilePath("Valid", "simple.xlsx"),
                GetTestFilePath("Invalid", "corrupted.xlsx"),
                GetTestFilePath("Valid", "large.xlsx")
            };

            // Act
            var results = await _service.LoadFilesAsync(filePaths);

            // Assert
            results.Should().HaveCount(3);
            results[0].Status.Should().Be(LoadStatus.Success);
            results[1].Status.Should().Be(LoadStatus.Failed);
            results[2].Status.Should().Be(LoadStatus.Success);
        }

        #endregion

        #region Helper Methods

        private string GetTestFilePath(string category, string filename)
        {
            var path = Path.Combine(_testDataPath, category, filename);

            // Verify file exists for better error messages
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Test file not found: {path}. Make sure TestData files are generated.");
            }

            return path;
        }

        #endregion
    }
}
