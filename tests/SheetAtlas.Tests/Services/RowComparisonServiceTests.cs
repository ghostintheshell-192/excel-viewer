using System.Data;
using SheetAtlas.Core.Application.Interfaces;
using SheetAtlas.Core.Application.Services;
using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.Core.Domain.Exceptions;
using SheetAtlas.Core.Domain.ValueObjects;
using SheetAtlas.Infrastructure.External;
using SheetAtlas.Infrastructure.External.Readers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace SheetAtlas.Tests.Services
{
    public class RowComparisonServiceTests
    {
        private readonly Mock<ILogger<RowComparisonService>> _mockLogger;
        private readonly RowComparisonService _service;

        public RowComparisonServiceTests()
        {
            _mockLogger = new Mock<ILogger<RowComparisonService>>();
            _service = new RowComparisonService(_mockLogger.Object);
        }

        [Fact]
        public async Task ExtractRowFromSearchResultAsync_SheetNotFound_ThrowsComparisonException()
        {
            // Arrange
            var excelFile = CreateExcelFileWithSheet("ExistingSheet");
            var searchResult = new SearchResult(
                excelFile,
                "NonExistentSheet",
                0,
                0,
                "test value"
            );

            // Act
            Func<Task> act = async () => await _service.ExtractRowFromSearchResultAsync(searchResult);

            // Assert
            await act.Should().ThrowAsync<ComparisonException>()
                .Where(ex => ex.UserMessage.Contains("NonExistentSheet"))
                .Where(ex => ex.UserMessage.Contains("non è presente"));
        }

        [Fact]
        public async Task ExtractRowFromSearchResultAsync_NullSearchResult_ThrowsArgumentNullException()
        {
            // Act
            Func<Task> act = async () => await _service.ExtractRowFromSearchResultAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task ExtractRowFromSearchResultAsync_InvalidCellCoordinates_ThrowsArgumentException()
        {
            // Arrange
            var excelFile = CreateExcelFileWithSheet("Sheet1");
            var searchResult = new SearchResult(
                excelFile,
                "Sheet1",
                -1,
                -1,
                "test value"
            );

            // Act
            Func<Task> act = async () => await _service.ExtractRowFromSearchResultAsync(searchResult);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*does not represent a valid cell*");
        }

        [Fact]
        public async Task GetColumnHeadersAsync_SheetNotFound_ThrowsComparisonException()
        {
            // Arrange
            var excelFile = CreateExcelFileWithSheet("ExistingSheet");

            // Act
            Func<Task> act = async () => await _service.GetColumnHeadersAsync(excelFile, "NonExistentSheet");

            // Assert
            await act.Should().ThrowAsync<ComparisonException>()
                .Where(ex => ex.UserMessage.Contains("NonExistentSheet"));
        }

        [Fact]
        public async Task GetColumnHeadersAsync_NullFile_ThrowsArgumentNullException()
        {
            // Act
            Func<Task> act = async () => await _service.GetColumnHeadersAsync(null!, "Sheet1");

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetColumnHeadersAsync_ValidSheet_ReturnsColumnHeaders()
        {
            // Arrange
            var excelFile = CreateExcelFileWithSheet("Sheet1", new[] { "Name", "Age", "City" });

            // Act
            var headers = await _service.GetColumnHeadersAsync(excelFile, "Sheet1");

            // Assert
            headers.Should().NotBeNull();
            headers.Should().HaveCount(3);
            headers.Should().Contain("Name");
            headers.Should().Contain("Age");
            headers.Should().Contain("City");
        }

        [Fact]
        public async Task CreateRowComparisonAsync_LessThanTwoResults_ThrowsArgumentException()
        {
            // Arrange
            var excelFile = CreateExcelFileWithSheet("Sheet1");
            var searchResult = new SearchResult(
                excelFile,
                "Sheet1",
                0,
                0,
                "test value"
            );

            var request = new RowComparisonRequest(
                new List<SearchResult> { searchResult },
                "Comparison1"
            );

            // Act
            Func<Task> act = async () => await _service.CreateRowComparisonAsync(request);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*At least two search results*");
        }

        #region Integration Tests with Real Files

        [Fact]
        public async Task GetColumnHeadersAsync_RealSimpleFile_ReturnsCorrectHeaders()
        {
            // Arrange
            var excelReaderService = CreateRealExcelReaderService();
            var filePath = GetTestFilePath("Valid", "simple.xlsx");
            var excelFile = await excelReaderService.LoadFileAsync(filePath);

            // Act
            var headers = await _service.GetColumnHeadersAsync(excelFile, "Sheet1");

            // Assert
            headers.Should().NotBeNull();
            headers.Should().HaveCount(3);
            headers.Should().Contain("Name");
            headers.Should().Contain("Age");
            headers.Should().Contain("City");
        }

        [Fact]
        public async Task GetColumnHeadersAsync_RealMultiSheetFile_ReturnsCorrectHeadersForEachSheet()
        {
            // Arrange
            var excelReaderService = CreateRealExcelReaderService();
            var filePath = GetTestFilePath("Valid", "multi-sheet.xlsx");
            var excelFile = await excelReaderService.LoadFileAsync(filePath);

            // Act - Test Employees sheet
            var employeeHeaders = await _service.GetColumnHeadersAsync(excelFile, "Employees");

            // Assert
            employeeHeaders.Should().HaveCount(2);
            employeeHeaders.Should().Contain("Employee");
            employeeHeaders.Should().Contain("Department");

            // Act - Test Departments sheet
            var departmentHeaders = await _service.GetColumnHeadersAsync(excelFile, "Departments");

            // Assert
            departmentHeaders.Should().HaveCount(2);
            departmentHeaders.Should().Contain("Department");
            departmentHeaders.Should().Contain("Budget");
        }

        [Fact]
        public async Task ExtractRowFromSearchResultAsync_RealSimpleFile_ExtractsCorrectRow()
        {
            // Arrange
            var excelReaderService = CreateRealExcelReaderService();
            var filePath = GetTestFilePath("Valid", "simple.xlsx");
            var excelFile = await excelReaderService.LoadFileAsync(filePath);

            // Create a search result pointing to the first data row
            // Note: Row index is 0-based in the DataTable (header is already removed)
            var searchResult = new SearchResult(
                excelFile,
                "Sheet1",
                0,  // Row index (0-based in DataTable, this is Alice)
                0,  // Column index (0-based)
                "Alice"
            );

            // Act
            var extractedRow = await _service.ExtractRowFromSearchResultAsync(searchResult);

            // Assert
            extractedRow.Should().NotBeNull();
            extractedRow.Cells.Should().HaveCount(3);
            extractedRow.GetCellAsString(0).Should().Be("Alice");
            extractedRow.GetCellAsString(1).Should().Be("30");
            extractedRow.GetCellAsString(2).Should().Be("Rome");
        }

        [Fact]
        public async Task CreateRowComparisonAsync_RealFiles_CreatesValidComparison()
        {
            // Arrange
            var excelReaderService = CreateRealExcelReaderService();
            var filePath = GetTestFilePath("Valid", "simple.xlsx");
            var excelFile = await excelReaderService.LoadFileAsync(filePath);

            // Create two search results from the same file but different rows
            // Note: Row indices are 0-based in DataTable (header already removed)
            var searchResult1 = new SearchResult(excelFile, "Sheet1", 0, 0, "Alice");  // First data row
            var searchResult2 = new SearchResult(excelFile, "Sheet1", 1, 0, "Bob");    // Second data row

            var request = new RowComparisonRequest(
                new List<SearchResult> { searchResult1, searchResult2 },
                "Alice vs Bob Comparison"
            );

            // Act
            var comparison = await _service.CreateRowComparisonAsync(request);

            // Assert
            comparison.Should().NotBeNull();
            comparison.Name.Should().Be("Alice vs Bob Comparison");
            comparison.Rows.Should().HaveCount(2);

            comparison.Rows[0].GetCellAsString(0).Should().Be("Alice");
            comparison.Rows[0].GetCellAsString(1).Should().Be("30");
            comparison.Rows[0].GetCellAsString(2).Should().Be("Rome");

            comparison.Rows[1].GetCellAsString(0).Should().Be("Bob");
            comparison.Rows[1].GetCellAsString(1).Should().Be("25");
            comparison.Rows[1].GetCellAsString(2).Should().Be("Milan");
        }

        #endregion

        #region Helper Methods

        private ExcelFile CreateExcelFileWithSheet(string sheetName, string[]? columnNames = null)
        {
            var dataTable = new DataTable(sheetName);

            if (columnNames != null)
            {
                foreach (var colName in columnNames)
                {
                    dataTable.Columns.Add(colName);
                }

                // Add a sample row
                var row = dataTable.NewRow();
                for (int i = 0; i < columnNames.Length; i++)
                {
                    row[i] = $"Value{i}";
                }
                dataTable.Rows.Add(row);
            }
            else
            {
                dataTable.Columns.Add("Column1");
                dataTable.Rows.Add("Value1");
            }

            var sheets = new Dictionary<string, DataTable>
            {
                { sheetName, dataTable }
            };

            return new ExcelFile(
                "test.xlsx",
                LoadStatus.Success,
                sheets,
                new List<ExcelError>()
            );
        }

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

        private string GetTestFilePath(string category, string filename)
        {
            var testDataPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "..",
                "..",
                "TestData"
            );

            var path = Path.Combine(testDataPath, category, filename);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Test file not found: {path}. Make sure TestData files are generated.");
            }

            return path;
        }

        #endregion
    }
}
