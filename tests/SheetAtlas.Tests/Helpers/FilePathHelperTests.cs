using FluentAssertions;
using SheetAtlas.Core.Shared.Helpers;

namespace SheetAtlas.Tests.Helpers
{
    public class FilePathHelperTests
    {
        #region GenerateLogFolderName Tests

        [Fact]
        public void GenerateLogFolderName_WithValidPath_ReturnsFolderNameWithHashSuffix()
        {
            // Arrange
            var filePath = "/home/user/documents/report.xlsx";

            // Act
            var result = FilePathHelper.GenerateLogFolderName(filePath);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("-");
            var parts = result.Split('-');
            parts.Length.Should().BeGreaterThan(1);
            // Last part should be the hash (6 characters)
            parts[^1].Should().HaveLength(6);
            parts[^1].Should().MatchRegex("^[a-f0-9]+$");
        }

        [Fact]
        public void GenerateLogFolderName_WithNullPath_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => FilePathHelper.GenerateLogFolderName(null!));
            exception.Message.Should().Contain("File path cannot be null or empty");
            exception.ParamName.Should().Be("filePath");
        }

        [Fact]
        public void GenerateLogFolderName_WithEmptyPath_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => FilePathHelper.GenerateLogFolderName(""));
            exception.Message.Should().Contain("File path cannot be null or empty");
        }

        [Fact]
        public void GenerateLogFolderName_WithWhitespacePath_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => FilePathHelper.GenerateLogFolderName("   "));
            exception.Message.Should().Contain("File path cannot be null or empty");
        }

        [Fact]
        public void GenerateLogFolderName_WithSamePath_ReturnsSameFolderName()
        {
            // Arrange
            var filePath = "/home/user/documents/report.xlsx";

            // Act
            var result1 = FilePathHelper.GenerateLogFolderName(filePath);
            var result2 = FilePathHelper.GenerateLogFolderName(filePath);

            // Assert
            result1.Should().Be(result2);
        }

        [Fact]
        public void GenerateLogFolderName_WithDifferentPaths_ReturnsDifferentFolderNames()
        {
            // Arrange
            var filePath1 = "/home/user/documents/report.xlsx";
            var filePath2 = "/home/user/downloads/report.xlsx";

            // Act
            var result1 = FilePathHelper.GenerateLogFolderName(filePath1);
            var result2 = FilePathHelper.GenerateLogFolderName(filePath2);

            // Assert
            result1.Should().NotBe(result2);
        }

        [Fact]
        public void GenerateLogFolderName_WithComplexFileName_HandlesSanitizationCorrectly()
        {
            // Arrange
            var filePath = "/home/user/Report Q4 2024!@#.xlsx";

            // Act
            var result = FilePathHelper.GenerateLogFolderName(filePath);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().StartWith("report-q4-2024-xlsx-");
        }

        #endregion

        #region SanitizeFileName Tests

        [Theory]
        [InlineData("Report.xlsx", "report-xlsx")]
        [InlineData("report.xlsx", "report-xlsx")]
        [InlineData("Report 2024.xlsx", "report-2024-xlsx")]
        [InlineData("Q4_Report.xlsx", "q4_report-xlsx")]
        [InlineData("My Excel File (2024).xlsx", "my-excel-file-2024-xlsx")]
        public void SanitizeFileName_WithVariousNames_ReturnsSanitizedVersion(string fileName, string expected)
        {
            // Act
            var result = FilePathHelper.SanitizeFileName(fileName);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void SanitizeFileName_WithNullName_ReturnsUnknownFile()
        {
            // Act
            var result = FilePathHelper.SanitizeFileName(null!);

            // Assert
            result.Should().Be("unknown-file");
        }

        [Fact]
        public void SanitizeFileName_WithEmptyName_ReturnsUnknownFile()
        {
            // Act
            var result = FilePathHelper.SanitizeFileName("");

            // Assert
            result.Should().Be("unknown-file");
        }

        [Fact]
        public void SanitizeFileName_WithWhitespaceName_ReturnsUnknownFile()
        {
            // Act
            var result = FilePathHelper.SanitizeFileName("   ");

            // Assert
            result.Should().Be("unknown-file");
        }

        [Fact]
        public void SanitizeFileName_WithSpecialCharactersOnly_ReturnsFallback()
        {
            // Act
            var result = FilePathHelper.SanitizeFileName("@#$%!.xlsx");

            // Assert
            result.Should().Be("file-xlsx");
        }

        [Fact]
        public void SanitizeFileName_RemovesMultipleConsecutiveHyphens()
        {
            // Arrange
            var fileName = "Report   ---   2024.xlsx";

            // Act
            var result = FilePathHelper.SanitizeFileName(fileName);

            // Assert
            result.Should().NotContain("--");
            result.Should().Be("report-2024-xlsx");
        }

        [Fact]
        public void SanitizeFileName_TrimsHyphensFromEdges()
        {
            // Arrange
            var fileName = "---Report 2024---.xlsx";

            // Act
            var result = FilePathHelper.SanitizeFileName(fileName);

            // Assert
            result.Should().NotStartWith("-");
            result.Should().NotEndWith("-");
        }

        [Fact]
        public void SanitizeFileName_WithoutExtension_ReturnsSanitizedNameWithoutExt()
        {
            // Act
            var result = FilePathHelper.SanitizeFileName("Report 2024");

            // Assert
            result.Should().Be("report-2024");
        }

        [Fact]
        public void SanitizeFileName_PreservesUnderscores()
        {
            // Arrange
            var fileName = "Report_Q4_2024.xlsx";

            // Act
            var result = FilePathHelper.SanitizeFileName(fileName);

            // Assert
            result.Should().Contain("_");
            result.Should().Be("report_q4_2024-xlsx");
        }

        [Fact]
        public void SanitizeFileName_ConvertToLowercase()
        {
            // Arrange
            var fileName = "REPORT.XLSX";

            // Act
            var result = FilePathHelper.SanitizeFileName(fileName);

            // Assert
            result.Should().Be("report-xlsx");
            result.Should().MatchRegex("^[a-z0-9\\-_]+$"); // All lowercase
        }

        #endregion

        #region ComputePathHash Tests

        [Fact]
        public void ComputePathHash_WithValidPath_Returns6CharacterHash()
        {
            // Arrange
            var filePath = "/home/user/documents/report.xlsx";

            // Act
            var result = FilePathHelper.ComputePathHash(filePath);

            // Assert
            result.Should().HaveLength(6);
            result.Should().MatchRegex("^[a-f0-9]+$");
        }

        [Fact]
        public void ComputePathHash_WithSamePath_ReturnsSameHash()
        {
            // Arrange
            var filePath = "/home/user/documents/report.xlsx";

            // Act
            var hash1 = FilePathHelper.ComputePathHash(filePath);
            var hash2 = FilePathHelper.ComputePathHash(filePath);

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void ComputePathHash_WithDifferentPaths_ReturnsDifferentHashes()
        {
            // Arrange
            var filePath1 = "/home/user/documents/report.xlsx";
            var filePath2 = "/home/user/downloads/report.xlsx";

            // Act
            var hash1 = FilePathHelper.ComputePathHash(filePath1);
            var hash2 = FilePathHelper.ComputePathHash(filePath2);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void ComputePathHash_WithNullPath_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => FilePathHelper.ComputePathHash(null!));
            exception.Message.Should().Contain("File path cannot be null or empty");
        }

        [Fact]
        public void ComputePathHash_WithEmptyPath_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => FilePathHelper.ComputePathHash(""));
            exception.Message.Should().Contain("File path cannot be null or empty");
        }

        [Fact]
        public void ComputePathHash_WithWhitespacePath_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => FilePathHelper.ComputePathHash("   "));
            exception.Message.Should().Contain("File path cannot be null or empty");
        }

        [Fact]
        public void ComputePathHash_IsCaseInsensitive()
        {
            // Arrange
            var filePath1 = "/home/user/documents/report.xlsx";
            var filePath2 = "/HOME/USER/DOCUMENTS/REPORT.XLSX";

            // Act
            var hash1 = FilePathHelper.ComputePathHash(filePath1);
            var hash2 = FilePathHelper.ComputePathHash(filePath2);

            // Assert
            hash1.Should().Be(hash2);
        }

        #endregion

        #region ComputeFileHash Tests

        [Fact]
        public void ComputeFileHash_WithValidFile_ReturnsHashWithMd5Prefix()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "test content");

                // Act
                var result = FilePathHelper.ComputeFileHash(tempFile);

                // Assert
                result.Should().StartWith("md5:");
                result.Should().HaveLength("md5:".Length + 32); // MD5 is 32 hex characters
                result.Should().MatchRegex("^md5:[a-f0-9]+$");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ComputeFileHash_WithSameContent_ReturnsSameHash()
        {
            // Arrange
            var tempFile1 = Path.GetTempFileName();
            var tempFile2 = Path.GetTempFileName();
            try
            {
                const string content = "identical content";
                File.WriteAllText(tempFile1, content);
                File.WriteAllText(tempFile2, content);

                // Act
                var hash1 = FilePathHelper.ComputeFileHash(tempFile1);
                var hash2 = FilePathHelper.ComputeFileHash(tempFile2);

                // Assert
                hash1.Should().Be(hash2);
            }
            finally
            {
                File.Delete(tempFile1);
                File.Delete(tempFile2);
            }
        }

        [Fact]
        public void ComputeFileHash_WithDifferentContent_ReturnsDifferentHash()
        {
            // Arrange
            var tempFile1 = Path.GetTempFileName();
            var tempFile2 = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile1, "content 1");
                File.WriteAllText(tempFile2, "content 2");

                // Act
                var hash1 = FilePathHelper.ComputeFileHash(tempFile1);
                var hash2 = FilePathHelper.ComputeFileHash(tempFile2);

                // Assert
                hash1.Should().NotBe(hash2);
            }
            finally
            {
                File.Delete(tempFile1);
                File.Delete(tempFile2);
            }
        }

        [Fact]
        public void ComputeFileHash_WithNullPath_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => FilePathHelper.ComputeFileHash(null!));
            exception.Message.Should().Contain("File path cannot be null or empty");
        }

        [Fact]
        public void ComputeFileHash_WithEmptyPath_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => FilePathHelper.ComputeFileHash(""));
            exception.Message.Should().Contain("File path cannot be null or empty");
        }

        [Fact]
        public void ComputeFileHash_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() =>
                FilePathHelper.ComputeFileHash("/non/existent/file.xlsx"));
            exception.Message.Should().Contain("File not found");
        }

        [Fact]
        public void ComputeFileHash_WithEmptyFile_ReturnsValidHash()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "");

                // Act
                var result = FilePathHelper.ComputeFileHash(tempFile);

                // Assert
                result.Should().StartWith("md5:");
                result.Should().MatchRegex("^md5:[a-f0-9]+$");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ComputeFileHash_WithLargeFile_ReturnsValidHash()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            try
            {
                // Write 10MB of data
                var largeContent = string.Concat(Enumerable.Repeat("x", 10 * 1024 * 1024));
                File.WriteAllText(tempFile, largeContent);

                // Act
                var result = FilePathHelper.ComputeFileHash(tempFile);

                // Assert
                result.Should().StartWith("md5:");
                result.Should().MatchRegex("^md5:[a-f0-9]+$");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        #endregion

        #region GenerateLogFileName Tests

        [Fact]
        public void GenerateLogFileName_WithTimestamp_ReturnsFormattedFileName()
        {
            // Arrange
            var timestamp = new DateTime(2024, 10, 18, 14, 23, 15);

            // Act
            var result = FilePathHelper.GenerateLogFileName(timestamp);

            // Assert
            result.Should().Be("20241018_142315.json");
        }

        [Fact]
        public void GenerateLogFileName_WithDifferentTimestamps_ReturnsDifferentFileNames()
        {
            // Arrange
            var timestamp1 = new DateTime(2024, 10, 18, 14, 23, 15);
            var timestamp2 = new DateTime(2024, 10, 18, 14, 23, 16);

            // Act
            var result1 = FilePathHelper.GenerateLogFileName(timestamp1);
            var result2 = FilePathHelper.GenerateLogFileName(timestamp2);

            // Assert
            result1.Should().NotBe(result2);
        }

        [Fact]
        public void GenerateLogFileName_AlwaysEndsWithJsonExtension()
        {
            // Arrange
            var timestamps = new[]
            {
                new DateTime(2024, 1, 1, 0, 0, 0),
                new DateTime(2024, 12, 31, 23, 59, 59),
                new DateTime(2024, 6, 15, 12, 30, 45)
            };

            // Act & Assert
            foreach (var ts in timestamps)
            {
                var result = FilePathHelper.GenerateLogFileName(ts);
                result.Should().EndWith(".json");
            }
        }

        [Fact]
        public void GenerateLogFileName_FormatIsYearMonthDayUnderscoreHourMinuteSecond()
        {
            // Arrange
            var timestamp = new DateTime(2024, 10, 18, 14, 23, 15);

            // Act
            var result = FilePathHelper.GenerateLogFileName(timestamp);

            // Assert
            result.Should().MatchRegex(@"^\d{8}_\d{6}\.json$");
            result.Should().HaveLength("yyyyMMdd_HHmmss.json".Length);
        }

        [Fact]
        public void GenerateLogFileName_WithMidnightTimestamp_FormatsCorrectly()
        {
            // Arrange
            var timestamp = new DateTime(2024, 10, 18, 0, 0, 0);

            // Act
            var result = FilePathHelper.GenerateLogFileName(timestamp);

            // Assert
            result.Should().Be("20241018_000000.json");
        }

        [Fact]
        public void GenerateLogFileName_WithEarlyHourTimestamp_FormatsCorrectly()
        {
            // Arrange
            var timestamp = new DateTime(2024, 10, 18, 1, 2, 3);

            // Act
            var result = FilePathHelper.GenerateLogFileName(timestamp);

            // Assert
            result.Should().Be("20241018_010203.json");
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void CompleteWorkflow_GeneratesConsistentFolderAndFileName()
        {
            // Arrange
            var filePath = "/home/user/documents/report-q4-2024.xlsx";
            var timestamp = new DateTime(2024, 10, 18, 14, 23, 15);

            // Act
            var folderName = FilePathHelper.GenerateLogFolderName(filePath);
            var fileName = FilePathHelper.GenerateLogFileName(timestamp);

            // Assert
            folderName.Should().NotBeNullOrEmpty();
            fileName.Should().NotBeNullOrEmpty();
            fileName.Should().Be("20241018_142315.json");
            folderName.Should().Contain("-");
            folderName.Should().EndWith(FilePathHelper.ComputePathHash(filePath));
        }

        [Fact]
        public void PathHash_IsDeterministic()
        {
            // Arrange
            var filePath = "/home/user/documents/report.xlsx";
            const int iterations = 10;

            // Act
            var hashes = Enumerable.Range(0, iterations)
                .Select(_ => FilePathHelper.ComputePathHash(filePath))
                .ToList();

            // Assert
            hashes.Should().AllBe(hashes.First());
        }

        #endregion
    }
}
