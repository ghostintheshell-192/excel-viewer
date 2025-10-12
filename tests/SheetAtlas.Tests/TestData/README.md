# Test Data Files

This directory contains Excel files used for integration and unit testing.

## Structure

### Valid Files (`Valid/`)
- **simple.xlsx**: Simple Excel file with 3 columns (Name, Age, City) and 2 data rows. Use for basic read/comparison tests.
- **large.xlsx**: File with 100 rows of data (ID, Product, Quantity, Price, Total). Use for performance tests.
- **multi-sheet.xlsx**: File with 3 sheets (Employees, Departments, Summary). Use for multi-sheet navigation tests.

### Invalid Files (`Invalid/`)
- **empty.xlsx**: Valid Excel file structure but contains no data. Use for empty sheet handling tests.
- **corrupted.xlsx**: Intentionally corrupted file (truncated). Use for error handling tests.
- **unsupported.xls**: Text file with .xls extension. Use for format validation tests.

### Edge Cases (`EdgeCases/`)
- **special-chars.xlsx**: Contains special characters (accents, symbols, emojis). Use for encoding tests.
- **formulas.xlsx**: Contains Excel formulas (SUM, arithmetic operations). Use for formula evaluation tests.
- **merged-cells.xlsx**: Contains merged cells. Use for cell merging handling tests.

## Regenerating Test Files

If you need to regenerate these files:

```bash
cd tests/ExcelViewer.Tests/TestDataGenerator
dotnet run
```

This will recreate all test files with fresh data.

## Usage in Tests

Example of using test files in xUnit tests:

```csharp
private string GetTestFilePath(string category, string filename)
{
    return Path.Combine(
        Directory.GetCurrentDirectory(),
        "..",
        "..",
        "..",
        "TestData",
        category,
        filename
    );
}

[Fact]
public async Task ReadSimpleFile_Success()
{
    var filePath = GetTestFilePath("Valid", "simple.xlsx");
    var result = await _excelReaderService.ReadFileAsync(filePath);

    result.Should().NotBeNull();
    result.Sheets.Should().ContainKey("Sheet1");
}
```

## Notes

- All files are generated programmatically using DocumentFormat.OpenXml
- Files are committed to source control for consistent test behavior
- Do not modify these files manually - regenerate them instead
