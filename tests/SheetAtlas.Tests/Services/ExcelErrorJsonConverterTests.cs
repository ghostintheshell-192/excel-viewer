using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using SheetAtlas.Core.Application.DTOs;
using SheetAtlas.Core.Application.Services;
using SheetAtlas.Core.Domain.ValueObjects;
using SheetAtlas.Logging.Models;

namespace SheetAtlas.Tests.Services
{
    public class ExcelErrorJsonConverterTests
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public ExcelErrorJsonConverterTests()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new System.Text.Json.Serialization.JsonStringEnumConverter(),
                    new ExcelErrorJsonConverter()
                },
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                MaxDepth = 64
            };
        }

        #region Write (Serialization) Tests

        [Fact]
        public void Write_FileError_SerializesAllProperties()
        {
            // Arrange
            var error = ExcelError.FileError("File is corrupted");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            json.Should().Contain("\"errors\"");
            var parsed = JsonDocument.Parse(json);
            var errorObj = parsed.RootElement.GetProperty("errors")[0];
            errorObj.TryGetProperty("severity", out _).Should().BeTrue();
            errorObj.TryGetProperty("message", out _).Should().BeTrue();
            errorObj.TryGetProperty("context", out _).Should().BeTrue();
        }

        [Fact]
        public void Write_FileError_SetsCorrectContext()
        {
            // Arrange
            var error = ExcelError.FileError("File is corrupted");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            json.Should().Contain("\"context\": \"File\"");
        }

        [Fact]
        public void Write_SheetError_SetsCorrectContext()
        {
            // Arrange
            var error = ExcelError.SheetError("Sheet1", "Sheet has invalid format");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            json.Should().Contain("\"context\": \"Sheet:Sheet1\"");
        }

        [Fact]
        public void Write_CellError_IncludesLocation()
        {
            // Arrange
            var cellRef = new CellReference(0, 0);
            var error = ExcelError.CellError("Sheet1", cellRef, "Invalid value");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            json.Should().Contain("\"location\"");
            json.Should().Contain("\"sheet\": \"Sheet1\"");
            json.Should().Contain("\"cell\": \"A1\"");
            json.Should().Contain("\"cellReference\": \"Sheet1!A1\"");
        }

        [Fact]
        public void Write_ErrorWithException_SerializesExceptionInfo()
        {
            // Arrange
            var ex = new InvalidOperationException("Invalid operation occurred");
            var error = ExcelError.FileError("File operation failed", ex);
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            json.Should().Contain("\"exception\"");
            json.Should().Contain("System.InvalidOperationException");
            json.Should().Contain("Invalid operation occurred");
        }

        [Fact]
        public void Write_ErrorWithStackTrace_IncludesStackTrace()
        {
            // Arrange
            Exception? caughtEx = null;
            try
            {
                throw new ArgumentException("Test exception");
            }
            catch (Exception ex)
            {
                caughtEx = ex;
            }

            var error = ExcelError.FileError("Error with stacktrace", caughtEx);
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            json.Should().Contain("\"stackTrace\"");
        }

        [Fact]
        public void Write_ErrorWithoutException_DoesNotIncludeExceptionProperty()
        {
            // Arrange
            var error = ExcelError.FileError("Simple error without exception");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            var parsed = JsonDocument.Parse(json);
            var errorObj = parsed.RootElement.GetProperty("errors")[0];
            errorObj.TryGetProperty("exception", out _).Should().BeFalse();
        }

        [Fact]
        public void Write_Error_IncludesAllRequiredFields()
        {
            // Arrange
            var error = ExcelError.FileError("Test error");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            var parsed = JsonDocument.Parse(json);
            var errorObj = parsed.RootElement.GetProperty("errors")[0];
            errorObj.TryGetProperty("id", out _).Should().BeTrue();
            errorObj.TryGetProperty("timestamp", out _).Should().BeTrue();
            errorObj.TryGetProperty("severity", out _).Should().BeTrue();
            errorObj.TryGetProperty("code", out _).Should().BeTrue();
            errorObj.TryGetProperty("message", out _).Should().BeTrue();
            errorObj.TryGetProperty("context", out _).Should().BeTrue();
            errorObj.TryGetProperty("isRecoverable", out _).Should().BeTrue();
        }

        [Fact]
        public void Write_ErrorCode_IsCorrectBasedOnContext()
        {
            // Arrange
            var fileError = ExcelError.FileError("File error");
            var sheetError = ExcelError.SheetError("Sheet1", "Sheet error");
            var cellError = ExcelError.CellError("Sheet1", new CellReference(0, 0), "Cell error");

            var fileLogEntry1 = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { fileError }
            };

            var fileLogEntry2 = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { sheetError }
            };

            var fileLogEntry3 = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { cellError }
            };

            // Act
            var json1 = JsonSerializer.Serialize(fileLogEntry1, _jsonOptions);
            var json2 = JsonSerializer.Serialize(fileLogEntry2, _jsonOptions);
            var json3 = JsonSerializer.Serialize(fileLogEntry3, _jsonOptions);

            // Assert
            json1.Should().Contain("\"code\": \"FILE\"");
            json2.Should().Contain("\"code\": \"SHEET\"");
            json3.Should().Contain("\"code\": \"CELL\"");
        }

        [Fact]
        public void Write_IsRecoverable_BasedOnExceptionType()
        {
            // Arrange
            var fileNotFoundError = ExcelError.FileError("File not found", new FileNotFoundException());
            var unauthorizedError = ExcelError.FileError("Access denied", new UnauthorizedAccessException());
            var ioError = ExcelError.FileError("IO error", new IOException());
            var invalidOpError = ExcelError.FileError("Invalid op", new InvalidOperationException());

            var fileLogEntry1 = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { fileNotFoundError }
            };

            var fileLogEntry2 = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { unauthorizedError }
            };

            var fileLogEntry3 = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { ioError }
            };

            var fileLogEntry4 = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { invalidOpError }
            };

            // Act
            var json1 = JsonSerializer.Serialize(fileLogEntry1, _jsonOptions);
            var json2 = JsonSerializer.Serialize(fileLogEntry2, _jsonOptions);
            var json3 = JsonSerializer.Serialize(fileLogEntry3, _jsonOptions);
            var json4 = JsonSerializer.Serialize(fileLogEntry4, _jsonOptions);

            // Assert
            json1.Should().Contain("\"isRecoverable\": true");
            json2.Should().Contain("\"isRecoverable\": true");
            json3.Should().Contain("\"isRecoverable\": true");
            json4.Should().Contain("\"isRecoverable\": false");
        }

        #endregion

        #region Read (Deserialization) Tests

        [Fact]
        public void Read_ValidJson_DeserializesFileError()
        {
            // Arrange
            var originalError = ExcelError.FileError("Test error");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { originalError }
            };

            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Act
            var deserialized = JsonSerializer.Deserialize<FileLogEntry>(json, _jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Errors.Should().NotBeEmpty();
            deserialized.Errors[0].Message.Should().Contain("Test error");
        }

        [Fact]
        public void Read_ValidJson_DeserializesSheetError()
        {
            // Arrange
            var originalError = ExcelError.SheetError("Sheet1", "Sheet error");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { originalError }
            };

            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Act
            var deserialized = JsonSerializer.Deserialize<FileLogEntry>(json, _jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Errors.Should().NotBeEmpty();
            deserialized.Errors[0].Context.Should().Contain("Sheet1");
        }

        [Fact]
        public void Read_ValidJson_DeserializesCellError()
        {
            // Arrange
            var cellRef = new CellReference(5, 2); // C6
            var originalError = ExcelError.CellError("Sheet1", cellRef, "Cell error");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { originalError }
            };

            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Act
            var deserialized = JsonSerializer.Deserialize<FileLogEntry>(json, _jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Errors.Should().NotBeEmpty();
            deserialized.Errors[0].Location.Should().NotBeNull();
        }

        [Fact]
        public void Read_JsonWithException_DeserializesExceptionInfo()
        {
            // Arrange
            var originalEx = new ArgumentException("Invalid argument");
            var originalError = ExcelError.FileError("Error with exception", originalEx);
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { originalError }
            };

            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Act
            var deserialized = JsonSerializer.Deserialize<FileLogEntry>(json, _jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Errors.Should().NotBeEmpty();
            deserialized.Errors[0].InnerException.Should().NotBeNull();
            deserialized.Errors[0].InnerException!.Message.Should().Contain("Invalid argument");
        }

        [Fact]
        public void Read_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "{ invalid json }";

            // Act & Assert
            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<ExcelError>(invalidJson, _jsonOptions));
        }

        [Fact]
        public void Read_JsonMissingStartObject_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "[]"; // Array instead of object

            // Act & Assert
            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<ExcelError>(invalidJson, _jsonOptions));
        }

        [Fact]
        public void Read_JsonWithoutRequiredFields_StillDeserializes()
        {
            // Arrange
            var minimalJson = "{}";

            // Act
            var result = JsonSerializer.Deserialize<ExcelError>(minimalJson, _jsonOptions);

            // Assert
            result.Should().NotBeNull();
            // Should have default values
            result!.Message.Should().Be(string.Empty);
        }

        #endregion

        #region Round-Trip Tests

        [Fact]
        public void RoundTrip_FileError_PreservesData()
        {
            // Arrange
            var originalError = ExcelError.FileError("Critical file error");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { originalError }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);
            var deserialized = JsonSerializer.Deserialize<FileLogEntry>(json, _jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Errors.Should().NotBeEmpty();
            deserialized.Errors[0].Message.Should().Be(originalError.Message);
            deserialized.Errors[0].Context.Should().Be(originalError.Context);
            deserialized.Errors[0].Level.Should().Be(originalError.Level);
        }

        [Fact]
        public void RoundTrip_SheetError_PreservesData()
        {
            // Arrange
            var originalError = ExcelError.SheetError("DataSheet", "Invalid sheet format");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { originalError }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);
            var deserialized = JsonSerializer.Deserialize<FileLogEntry>(json, _jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Errors.Should().NotBeEmpty();
            deserialized.Errors[0].Context.Should().Contain("DataSheet");
            deserialized.Errors[0].Message.Should().Be("Invalid sheet format");
        }

        [Fact]
        public void RoundTrip_CellError_PreservesLocationData()
        {
            // Arrange
            var cellRef = new CellReference(9, 25); // Z10
            var originalError = ExcelError.CellError("Sheet1", cellRef, "Invalid cell value");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { originalError }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);
            var deserialized = JsonSerializer.Deserialize<FileLogEntry>(json, _jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Errors.Should().NotBeEmpty();
            deserialized.Errors[0].Location.Should().NotBeNull();
            deserialized.Errors[0].Location!.Row.Should().Be(cellRef.Row);
            deserialized.Errors[0].Location!.Column.Should().Be(cellRef.Column);
        }

        [Fact]
        public void RoundTrip_MultipleErrors_PreservesAllData()
        {
            // Arrange
            var error1 = ExcelError.FileError("File error");
            var error2 = ExcelError.SheetError("Sheet1", "Sheet error");
            var error3 = ExcelError.CellError("Sheet1", new CellReference(0, 0), "Cell error");

            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error1, error2, error3 }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);
            var deserialized = JsonSerializer.Deserialize<FileLogEntry>(json, _jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Errors.Should().HaveCount(3);
            deserialized.Errors[0].Message.Should().Be("File error");
            deserialized.Errors[1].Context.Should().Contain("Sheet1");
            deserialized.Errors[2].Location.Should().NotBeNull();
        }

        [Fact]
        public void RoundTrip_ErrorWithException_PreservesExceptionInfo()
        {
            // Arrange
            var originalEx = new FileNotFoundException("File not found: test.xlsx");
            var originalError = ExcelError.FileError("Could not open file", originalEx);
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { originalError }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);
            var deserialized = JsonSerializer.Deserialize<FileLogEntry>(json, _jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Errors.Should().NotBeEmpty();
            deserialized.Errors[0].InnerException.Should().NotBeNull();
            deserialized.Errors[0].InnerException!.Message.Should().Contain("File not found");
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void Write_ErrorWithNullMessage_SerializesEmptyString()
        {
            // Arrange - We use reflection to create an error with a null message for testing
            var error = ExcelError.FileError("");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            json.Should().Contain("\"message\": \"\"");
        }

        [Fact]
        public void Write_CellErrorWithMultipleColumnLetters_SerializesCorrectly()
        {
            // Arrange
            var cellRef = new CellReference(100, 702); // ZZ101 (702 is 26*26 + 26)
            var error = ExcelError.CellError("Sheet1", cellRef, "Error in far cell");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            var parsed = JsonDocument.Parse(json);
            var errorObj = parsed.RootElement.GetProperty("errors")[0];
            var cellValue = errorObj.GetProperty("location").GetProperty("cell").GetString();
            cellValue.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Read_JsonWithUnknownProperties_IgnoresExtraFields()
        {
            // Arrange
            var json = @"{
                ""severity"": ""Error"",
                ""message"": ""Test message"",
                ""context"": ""File"",
                ""unknownField"": ""should be ignored"",
                ""anotherUnknown"": 123
            }";

            // Act
            var result = JsonSerializer.Deserialize<ExcelError>(json, _jsonOptions);

            // Assert
            result.Should().NotBeNull();
            result!.Message.Should().Be("Test message");
        }

        [Fact]
        public void Write_WarningError_SerializesCorrectly()
        {
            // Arrange
            var error = ExcelError.Warning("File:test.xlsx", "This is a warning");
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            json.Should().Contain("\"severity\": \"Warning\"");
            json.Should().Contain("\"message\": \"This is a warning\"");
        }

        [Fact]
        public void Write_CriticalError_SerializesCorrectly()
        {
            // Arrange
            var ex = new OutOfMemoryException("Not enough memory");
            var error = ExcelError.Critical("File:large.xlsx", "Failed to process file", ex);
            var fileLogEntry = new FileLogEntry
            {
                File = new FileInfoDto { OriginalPath = "/test/file.xlsx" },
                LoadAttempt = new LoadAttemptInfo { Timestamp = DateTime.UtcNow },
                Errors = new List<ExcelError> { error }
            };

            // Act
            var json = JsonSerializer.Serialize(fileLogEntry, _jsonOptions);

            // Assert
            json.Should().Contain("\"severity\": \"Critical\"");
            json.Should().Contain("System.OutOfMemoryException");
        }

        #endregion
    }
}
