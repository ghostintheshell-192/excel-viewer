# ExcelViewer Core - Refactoring Specifications

## Critical Issues Analysis

### 🚨 **Priority 1: ExcelFile Model Confusion**

#### Current Problems in ExcelFile.cs:
```csharp
public class ExcelFile
{
    // ❌ DUPLICATE DATA STRUCTURES
    public List<string> Sheets { get; private set; }                    // Sheet names only
    public Dictionary<string, DataTable> ValidSheets { get; }           // Same as SheetData?
    public Dictionary<string, DataTable> SheetData { get; private set; } // Actual sheet data

    // ❌ MIXED ERROR TYPES
    public List<SheetError> Errors { get; } = new();                   // File-level errors?
    public Dictionary<string, List<CellError>> SheetErrors { get; }     // Sheet-level errors

    // ❌ ENUM MISMATCH
    public FileLoadStatus Status { get; private set; }                 // Uses FileLoadStatus
    // But FileLoadResult uses LoadStatus - different enums!

    // ❌ BROKEN METHOD
    public bool LoadFile() { throw new NotImplementedException(); }    // Should not exist
}
```

#### Target Simplified Model:
```csharp
public class ExcelFile
{
    public string FilePath { get; }
    public string FileName => Path.GetFileName(FilePath);
    public LoadStatus Status { get; }
    public DateTime LoadedAt { get; }

    // SINGLE SOURCE OF TRUTH
    public IReadOnlyDictionary<string, DataTable> Sheets { get; }

    // SIMPLIFIED ERROR MODEL
    public IReadOnlyList<ExcelError> Errors { get; }

    // CONSTRUCTOR - Immutable after creation
    public ExcelFile(string filePath, LoadStatus status,
                    Dictionary<string, DataTable> sheets,
                    List<ExcelError> errors)
    {
        FilePath = filePath;
        Status = status;
        Sheets = sheets.AsReadOnly();
        Errors = errors.AsReadOnly();
        LoadedAt = DateTime.UtcNow;
    }

    // HELPER METHODS
    public DataTable? GetSheet(string sheetName) => Sheets.TryGetValue(sheetName, out var sheet) ? sheet : null;
    public bool HasErrors => Errors.Any(e => e.Level == ErrorLevel.Error);
    public bool HasWarnings => Errors.Any(e => e.Level == ErrorLevel.Warning);
}
```

### 🚨 **Priority 2: Error Model Consolidation**

#### Current Chaos:
```csharp
// FileLoadResult.cs
public enum LoadStatus { Success, PartialSuccess, Failed }
public class FileLoadError { string Message; Exception Exception; }
public class SheetError { string SheetName; ErrorType Type; string Message; CellReference Location; Exception Exception; }
public class CellError { string SheetName; ErrorType Type; string Message; CellReference Location; string CellValue; Exception Exception; }

// ExcelFile.cs
public enum FileLoadStatus { Success, PartialSuccess, Failed } // DUPLICATE!
public enum ErrorType { Error, Warning } // Limited scope
```

#### Target Unified Model:
```csharp
public enum LoadStatus { Success, PartialSuccess, Failed }
public enum ErrorLevel { Info, Warning, Error, Critical }

public class ExcelError
{
    public ErrorLevel Level { get; }
    public string Message { get; }
    public string Context { get; }              // "File", "Sheet:SheetName", "Cell:A1"
    public CellReference? Location { get; }     // Null for file-level errors
    public Exception? InnerException { get; }   // For debugging
    public DateTime Timestamp { get; }

    // FACTORY METHODS for type safety
    public static ExcelError FileError(string message, Exception? ex = null);
    public static ExcelError SheetError(string sheetName, string message, Exception? ex = null);
    public static ExcelError CellError(string sheetName, CellReference location, string message, Exception? ex = null);
    public static ExcelError Warning(string context, string message);
}
```

### 🚨 **Priority 3: ExcelReaderService Complexity**

#### Current Issues Analysis:

**ReadSheetData() Method** (Lines 142-308):
- **160+ lines** in single method
- **Multiple responsibilities**: parsing, error handling, data structure creation
- **Nested complexity**: 5+ levels of nesting
- **Regex in loops**: Performance impact
- **Mixed concerns**: UI table naming + business logic

#### Refactoring Strategy:

```csharp
public class ExcelReaderService : IExcelReaderService
{
    private readonly ILogger<ExcelReaderService> _logger;
    private readonly ICellReferenceParser _cellParser;
    private readonly IMergedCellProcessor _mergedCellProcessor;

    // SIMPLIFIED PUBLIC API
    public async Task<ExcelFile> LoadFileAsync(string filePath);
    public async Task<List<ExcelFile>> LoadFilesAsync(IEnumerable<string> filePaths);

    // PRIVATE FOCUSED METHODS
    private SpreadsheetDocument OpenDocument(string filePath);
    private IEnumerable<Sheet> GetSheets(WorkbookPart workbookPart);
    private DataTable ProcessSheet(string fileName, string sheetName, WorkbookPart workbookPart, WorksheetPart worksheetPart);
    private Dictionary<string, string> ExtractMergedCells(WorksheetPart worksheetPart);
    private Dictionary<int, string> ProcessHeaderRow(Row headerRow, SharedStringTable? sharedStringTable);
    private void PopulateDataRows(DataTable table, WorksheetPart worksheetPart, SharedStringTable? sharedStringTable, Dictionary<string, string> mergedCells);
}
```

#### Extracted Services:

```csharp
public interface ICellReferenceParser
{
    string GetColumnName(string cellReference);
    int GetColumnIndex(string cellReference);
    int GetRowIndex(string cellReference);
    string CreateCellReference(int columnIndex, int rowIndex);
}

public interface IMergedCellProcessor
{
    Dictionary<string, string> ProcessMergedCells(WorksheetPart worksheetPart, SharedStringTable? sharedStringTable);
}
```

## Detailed Refactoring Steps

### Step 1: Create New Error Model
```csharp
// New file: ExcelViewer.Core/Models/ExcelError.cs
public class ExcelError
{
    // Implementation as specified above
}

// Update: ExcelViewer.Core/Models/LoadStatus.cs (consolidate enums)
public enum LoadStatus { Success, PartialSuccess, Failed }
public enum ErrorLevel { Info, Warning, Error, Critical }
```

### Step 2: Refactor ExcelFile Model
```csharp
// BEFORE (ExcelFile.cs - current):
public class ExcelFile
{
    public FileLoadStatus Status { get; private set; }
    public List<SheetError> Errors { get; } = new();
    public Dictionary<string, DataTable> ValidSheets { get; }
    public Dictionary<string, List<CellError>> SheetErrors { get; }
    public string FilePath { get; private set; }
    public List<string> Sheets { get; private set; }
    public Dictionary<string, DataTable> SheetData { get; private set; }
    // + broken LoadFile() method
}

// AFTER (ExcelFile.cs - target):
public class ExcelFile
{
    public string FilePath { get; }
    public string FileName => Path.GetFileName(FilePath);
    public LoadStatus Status { get; }
    public DateTime LoadedAt { get; }
    public IReadOnlyDictionary<string, DataTable> Sheets { get; }
    public IReadOnlyList<ExcelError> Errors { get; }

    // Immutable constructor
    public ExcelFile(string filePath, LoadStatus status,
                    Dictionary<string, DataTable> sheets,
                    List<ExcelError> errors);

    // Helper methods
    public DataTable? GetSheet(string sheetName);
    public bool HasErrors { get; }
    public bool HasWarnings { get; }
}
```

### Step 3: Extract Cell Reference Parser
```csharp
// New file: ExcelViewer.Core/Services/CellReferenceParser.cs
public class CellReferenceParser : ICellReferenceParser
{
    private static readonly Regex ColumnRegex = new("[A-Za-z]+", RegexOptions.Compiled);
    private static readonly Regex RowRegex = new("[0-9]+", RegexOptions.Compiled);

    public string GetColumnName(string cellReference)
    {
        var match = ColumnRegex.Match(cellReference);
        return match.Success ? match.Value : string.Empty;
    }

    public int GetColumnIndex(string cellReference)
    {
        string columnName = GetColumnName(cellReference);
        int columnIndex = 0;

        for (int i = 0; i < columnName.Length; i++)
        {
            columnIndex = columnIndex * 26 + (columnName[i] - 'A' + 1);
        }

        return columnIndex - 1;
    }

    // Additional methods...
}
```

### Step 4: Simplify ExcelReaderService
```csharp
// BEFORE: One massive ReadSheetData() method (160+ lines)
public DataTable ReadSheetData(string filePath, string sheetName, WorkbookPart workbookPart, WorksheetPart worksheetPart)
{
    // 160+ lines of mixed responsibilities
}

// AFTER: Multiple focused methods
private DataTable ProcessSheet(string fileName, string sheetName, WorkbookPart workbookPart, WorksheetPart worksheetPart)
{
    var tableName = CreateTableName(fileName, sheetName);
    var dataTable = new DataTable(tableName);
    var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;

    var mergedCells = ExtractMergedCells(worksheetPart);
    var headerColumns = ProcessHeaderRow(worksheetPart, sharedStringTable, mergedCells);

    CreateDataTableColumns(dataTable, headerColumns);
    PopulateDataRows(dataTable, worksheetPart, sharedStringTable, mergedCells);

    return dataTable;
}

private Dictionary<string, string> ExtractMergedCells(WorksheetPart worksheetPart)
{
    // Focused on merged cell logic only (20-30 lines)
}

private Dictionary<int, string> ProcessHeaderRow(WorksheetPart worksheetPart, SharedStringTable? sharedStringTable, Dictionary<string, string> mergedCells)
{
    // Focused on header processing only (15-20 lines)
}

private void PopulateDataRows(DataTable table, WorksheetPart worksheetPart, SharedStringTable? sharedStringTable, Dictionary<string, string> mergedCells)
{
    // Focused on data row population only (30-40 lines)
}
```

### Step 5: Update FileLoadResult
```csharp
// BEFORE: Mixed with multiple error types
public class FileLoadResult
{
    public List<FileLoadError> Errors { get; } = new();
    public Dictionary<string, List<SheetError>> SheetErrors { get; } = new();
}

// AFTER: Simplified with unified error model
public class FileLoadResult
{
    public string FilePath { get; }
    public string FileName => Path.GetFileName(FilePath);
    public LoadStatus Status { get; }
    public ExcelFile? File { get; }
    public IReadOnlyList<ExcelError> Errors { get; }

    public FileLoadResult(string filePath, LoadStatus status, ExcelFile? file = null, List<ExcelError>? errors = null)
    {
        FilePath = filePath;
        Status = status;
        File = file;
        Errors = (errors ?? new List<ExcelError>()).AsReadOnly();
    }
}
```

## Unit Tests Structure

### Test Project Setup
```csharp
// ExcelViewer.Tests/ExcelViewer.Tests.csproj
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xUnit" Version="2.6.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
    <PackageReference Include="Moq" Version="4.20.69" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../src/ExcelViewer.Core/ExcelViewer.Core.csproj" />
  </ItemGroup>
</Project>
```

### Test Classes Structure
```csharp
// Tests/Models/ExcelFileTests.cs
public class ExcelFileTests
{
    [Fact]
    public void Constructor_WithValidData_SetsPropertiesCorrectly() { }

    [Fact]
    public void GetSheet_WithExistingSheet_ReturnsDataTable() { }

    [Fact]
    public void GetSheet_WithNonExistentSheet_ReturnsNull() { }

    [Fact]
    public void HasErrors_WithErrorLevelErrors_ReturnsTrue() { }
}

// Tests/Services/ExcelReaderServiceTests.cs
public class ExcelReaderServiceTests
{
    private readonly ExcelReaderService _service;
    private readonly Mock<ILogger<ExcelReaderService>> _mockLogger;

    [Fact]
    public async Task LoadFileAsync_WithValidFile_ReturnsSuccessResult() { }

    [Fact]
    public async Task LoadFileAsync_WithInvalidFile_ReturnsFailedResult() { }

    [Fact]
    public async Task LoadFilesAsync_WithMultipleFiles_ProcessesAll() { }
}

// Tests/Services/CellReferenceParserTests.cs
public class CellReferenceParserTests
{
    [Theory]
    [InlineData("A1", "A")]
    [InlineData("Z10", "Z")]
    [InlineData("AA1", "AA")]
    public void GetColumnName_WithVariousReferences_ReturnsCorrectColumn(string cellReference, string expectedColumn) { }

    [Theory]
    [InlineData("A1", 0)]
    [InlineData("B1", 1)]
    [InlineData("Z1", 25)]
    [InlineData("AA1", 26)]
    public void GetColumnIndex_WithVariousReferences_ReturnsCorrectIndex(string cellReference, int expectedIndex) { }
}
```

## Breaking Changes Impact

### ViewModels Impact (Minimal)
```csharp
// MainViewModel - BEFORE
var results = await _excelReader.ReadFilesAsync(paths);
foreach (var fileResult in results)
{
    if (fileResult.File != null)
    {
        _loadedFiles.Add(new FileLoadResultViewModel(fileResult));
    }
}

// MainViewModel - AFTER (same interface, different implementation)
var results = await _excelReader.LoadFilesAsync(paths);
foreach (var fileResult in results)
{
    if (fileResult.File != null)
    {
        _loadedFiles.Add(new FileLoadResultViewModel(fileResult));
    }
}
```

### Service Interface Changes
```csharp
// BEFORE
public interface IExcelReaderService
{
    ExcelFile ReadFile(string path);
    Task<List<FileLoadResult>> ReadFilesAsync(IEnumerable<string> paths);
    DataTable ReadSheetData(string filePath, string sheetName, WorkbookPart workbookPart, WorksheetPart worksheetPart); // REMOVE - internal only
}

// AFTER
public interface IExcelReaderService
{
    Task<ExcelFile> LoadFileAsync(string filePath);
    Task<List<ExcelFile>> LoadFilesAsync(IEnumerable<string> filePaths);
    // ReadSheetData removed from public interface
}
```

## Migration Timeline

### Day 1: Error Model + ExcelFile
- Create ExcelError class
- Refactor ExcelFile model
- Update constructors and factory methods
- ⚠️ Breaking changes to ExcelFile API

### Day 2: Extract Cell Reference Parser
- Create ICellReferenceParser interface
- Implement CellReferenceParser
- Extract from ExcelReaderService
- Add comprehensive unit tests

### Day 3: Refactor ExcelReaderService
- Split ReadSheetData into focused methods
- Extract merged cell processing
- Simplify error handling
- Update method signatures

### Day 4: Update FileLoadResult + Integration
- Simplify FileLoadResult model
- Update all references in codebase
- Fix ViewModels integration
- End-to-end testing

### Day 5: Testing + Documentation
- Complete unit test coverage
- Integration tests
- Update documentation
- Performance validation

## Validation Criteria

### Code Quality Metrics
- **Method Length**: No method >25 lines
- **Cyclomatic Complexity**: <10 per method
- **Test Coverage**: >90% for Core layer
- **Build Time**: <30 seconds for full build

### Business Logic Validation
- ✅ All existing Excel files process correctly
- ✅ Error reporting maintains same level of detail
- ✅ Performance matches or exceeds current implementation
- ✅ Memory usage stable or improved

---

*This refactoring specification provides step-by-step guidance for Sonnet to execute the Core layer cleanup while maintaining system stability and functionality.*

**Execution Priority**: Complete Core refactoring BEFORE any UI migration to ensure solid foundation.