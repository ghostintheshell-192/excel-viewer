using ExcelViewer.Core.Services;
using FluentAssertions;

namespace ExcelViewer.Tests.Services
{
    public class CellReferenceParserTests
    {
        private readonly CellReferenceParser _parser = new();

        [Theory]
        [InlineData("A1", "A")]
        [InlineData("Z10", "Z")]
        [InlineData("AA1", "AA")]
        [InlineData("AB123", "AB")]
        public void GetColumnName_WithVariousReferences_ReturnsCorrectColumn(string cellReference, string expectedColumn)
        {
            // Act
            var result = _parser.GetColumnName(cellReference);

            // Assert
            result.Should().Be(expectedColumn);
        }

        [Theory]
        [InlineData("A1", 0)]
        [InlineData("B1", 1)]
        [InlineData("Z1", 25)]
        [InlineData("AA1", 26)]
        [InlineData("AB1", 27)]
        public void GetColumnIndex_WithVariousReferences_ReturnsCorrectIndex(string cellReference, int expectedIndex)
        {
            // Act
            var result = _parser.GetColumnIndex(cellReference);

            // Assert
            result.Should().Be(expectedIndex);
        }

        [Theory]
        [InlineData("A1", 0)]
        [InlineData("B5", 4)]
        [InlineData("Z100", 99)]
        public void GetRowIndex_WithVariousReferences_ReturnsCorrectIndex(string cellReference, int expectedIndex)
        {
            // Act
            var result = _parser.GetRowIndex(cellReference);

            // Assert
            result.Should().Be(expectedIndex);
        }

        [Theory]
        [InlineData(0, 0, "A1")]
        [InlineData(1, 0, "B1")]
        [InlineData(25, 0, "Z1")]
        [InlineData(26, 0, "AA1")]
        [InlineData(0, 4, "A5")]
        public void CreateCellReference_WithIndices_ReturnsCorrectReference(int columnIndex, int rowIndex, string expectedReference)
        {
            // Act
            var result = _parser.CreateCellReference(columnIndex, rowIndex);

            // Assert
            result.Should().Be(expectedReference);
        }

        [Theory]
        [InlineData(0, "A")]
        [InlineData(1, "B")]
        [InlineData(25, "Z")]
        [InlineData(26, "AA")]
        [InlineData(27, "AB")]
        public void GetColumnNameFromIndex_WithVariousIndices_ReturnsCorrectName(int columnIndex, string expectedName)
        {
            // Act
            var result = _parser.GetColumnNameFromIndex(columnIndex);

            // Assert
            result.Should().Be(expectedName);
        }
    }
}