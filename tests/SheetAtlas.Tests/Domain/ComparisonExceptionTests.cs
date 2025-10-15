using SheetAtlas.Core.Domain.Exceptions;
using FluentAssertions;

namespace SheetAtlas.Tests.Domain
{
    public class ComparisonExceptionTests
    {
        [Fact]
        public void IncompatibleStructures_CreatesExceptionWithCorrectMessage()
        {
            // Arrange
            var file1 = "sales.xlsx";
            var file2 = "products.xlsx";

            // Act
            var exception = ComparisonException.IncompatibleStructures(file1, file2);

            // Assert
            exception.UserMessage.Should().Contain("incompatible");
            exception.Message.Should().Contain(file1);
            exception.Message.Should().Contain(file2);
            exception.ErrorCode.Should().Be("COMPARISON_ERROR");
        }

        [Fact]
        public void MissingSheet_CreatesExceptionWithSheetAndFileName()
        {
            // Arrange
            var sheetName = "DataSheet";
            var fileName = "report.xlsx";

            // Act
            var exception = ComparisonException.MissingSheet(sheetName, fileName);

            // Assert
            exception.UserMessage.Should().Contain(sheetName);
            exception.UserMessage.Should().Contain("report.xlsx");
            exception.UserMessage.Should().Contain("is not present");
            exception.Message.Should().Contain(sheetName);
            exception.Message.Should().Contain(fileName);
        }

        [Fact]
        public void NoCommonColumns_CreatesExceptionWithCorrectMessage()
        {
            // Act
            var exception = ComparisonException.NoCommonColumns();

            // Assert
            exception.UserMessage.Should().Contain("common columns");
            exception.Message.Should().Contain("No common columns");
        }

        [Fact]
        public void Constructor_WithInnerException_PreservesInnerException()
        {
            // Arrange
            var innerException = new InvalidOperationException("Inner error");
            var technicalMessage = "Technical message";
            var userMessage = "User message";

            // Act
            var exception = new ComparisonException(
                technicalMessage,
                userMessage,
                innerException);

            // Assert
            exception.Message.Should().Be(technicalMessage);
            exception.UserMessage.Should().Be(userMessage);
            exception.InnerException.Should().Be(innerException);
        }
    }
}
