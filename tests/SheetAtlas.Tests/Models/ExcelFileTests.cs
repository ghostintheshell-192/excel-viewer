using SheetAtlas.Core.Domain.Entities;
using SheetAtlas.Core.Domain.ValueObjects;
using FluentAssertions;
using System.Data;

namespace SheetAtlas.Tests.Models
{
    public class ExcelFileTests
    {
        [Fact]
        public void Constructor_WithValidData_SetsPropertiesCorrectly()
        {
            // Arrange
            var filePath = "/test/file.xlsx";
            var status = LoadStatus.Success;
            var sheets = new Dictionary<string, DataTable>
            {
                ["Sheet1"] = new DataTable("Sheet1")
            };
            var errors = new List<ExcelError>();

            // Act
            var excelFile = new ExcelFile(filePath, status, sheets, errors);

            // Assert
            excelFile.FilePath.Should().Be(filePath);
            excelFile.FileName.Should().Be("file.xlsx");
            excelFile.Status.Should().Be(status);
            excelFile.Sheets.Should().HaveCount(1);
            excelFile.Errors.Should().BeEmpty();
            excelFile.LoadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void GetSheet_WithExistingSheet_ReturnsDataTable()
        {
            // Arrange
            var sheet = new DataTable("TestSheet");
            var sheets = new Dictionary<string, DataTable> { ["TestSheet"] = sheet };
            var excelFile = new ExcelFile("/test/file.xlsx", LoadStatus.Success, sheets, new List<ExcelError>());

            // Act
            var result = excelFile.GetSheet("TestSheet");

            // Assert
            result.Should().BeSameAs(sheet);
        }

        [Fact]
        public void GetSheet_WithNonExistentSheet_ReturnsNull()
        {
            // Arrange
            var excelFile = new ExcelFile("/test/file.xlsx", LoadStatus.Success, new Dictionary<string, DataTable>(), new List<ExcelError>());

            // Act
            var result = excelFile.GetSheet("NonExistent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void HasErrors_WithErrorLevelErrors_ReturnsTrue()
        {
            // Arrange
            var errors = new List<ExcelError> { ExcelError.FileError("Test error") };
            var excelFile = new ExcelFile("/test/file.xlsx", LoadStatus.Failed, new Dictionary<string, DataTable>(), errors);

            // Act & Assert
            excelFile.HasErrors.Should().BeTrue();
        }

        [Fact]
        public void HasErrors_WithOnlyWarnings_ReturnsFalse()
        {
            // Arrange
            var errors = new List<ExcelError> { ExcelError.Warning("File", "Test warning") };
            var excelFile = new ExcelFile("/test/file.xlsx", LoadStatus.Success, new Dictionary<string, DataTable>(), errors);

            // Act & Assert
            excelFile.HasErrors.Should().BeFalse();
            excelFile.HasWarnings.Should().BeTrue();
        }
    }
}
