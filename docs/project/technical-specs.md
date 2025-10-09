# ExcelViewer - Technical Specifications

## Architecture Overview

### High-Level Architecture
```
┌─────────────────────────────────────────┐
│               Avalonia UI               │
│  ┌─────────────┐  ┌─────────────────┐  │
│  │  Views      │  │  ViewModels     │  │
│  │  (XAML)     │  │  (MVVM)        │  │
│  └─────────────┘  └─────────────────┘  │
└─────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────┐
│           Application Layer             │
│  ┌─────────────┐  ┌─────────────────┐  │
│  │  Services   │  │   Managers      │  │
│  │  (UI Logic) │  │  (Orchestration)│  │
│  └─────────────┘  └─────────────────┘  │
└─────────────────────────────────────────┘
                    │
┌─────────────────────────────────────────┐
│              Core Layer                 │
│  ┌─────────────┐  ┌─────────────────┐  │
│  │   Models    │  │    Services     │  │
│  │ (Entities)  │  │ (Business Logic)│  │
│  └─────────────┘  └─────────────────┘  │
└─────────────────────────────────────────┘
```

### Project Structure
```
ExcelViewer/
├── src/
│   ├── ExcelViewer.Core/           # Business logic layer
│   │   ├── Models/                 # Domain entities
│   │   ├── Services/              # Business services
│   │   └── Interfaces/            # Abstractions
│   │
│   ├── ExcelViewer.UI.Avalonia/   # Presentation layer
│   │   ├── Views/                 # XAML views
│   │   ├── ViewModels/            # View models
│   │   ├── Services/              # UI services
│   │   ├── Converters/            # Value converters
│   │   └── Controls/              # Custom controls
│   │
│   └── ExcelViewer.Tests/          # Test projects
│       ├── Core.Tests/            # Core layer tests
│       └── UI.Tests/              # UI layer tests
│
├── docs/                          # Documentation
├── build/                         # Build scripts
└── assets/                        # Resources and assets
```

## Technology Stack

### Core Technologies
- **.NET 8**: Latest LTS framework
- **C# 12**: Modern language features
- **Avalonia UI 11.x**: Cross-platform UI framework
- **DocumentFormat.OpenXml**: Excel file processing

### Dependencies
```xml
<!-- Core Dependencies -->
<PackageReference Include="DocumentFormat.OpenXml" Version="3.2.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />

<!-- UI Dependencies -->
<PackageReference Include="Avalonia" Version="11.0.x" />
<PackageReference Include="Avalonia.Desktop" Version="11.0.x" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.x" />

<!-- Testing Dependencies -->
<PackageReference Include="xUnit" Version="2.6.x" />
<PackageReference Include="Moq" Version="4.20.x" />
<PackageReference Include="FluentAssertions" Version="6.12.x" />
```

## Core Components

### Data Models

#### ExcelFile Entity
```csharp
public class ExcelFile
{
    public string FilePath { get; }
    public string FileName { get; }
    public LoadStatus Status { get; }
    public Dictionary<string, DataTable> Sheets { get; }
    public List<FileError> Errors { get; }
    public DateTime LoadedAt { get; }
}
```

#### Comparison Result
```csharp
public class ComparisonResult
{
    public ExcelFile LeftFile { get; }
    public ExcelFile RightFile { get; }
    public List<SheetComparison> SheetComparisons { get; }
    public ComparisonSummary Summary { get; }
    public DateTime ComparedAt { get; }
}
```

### Services

#### IExcelReaderService
```csharp
public interface IExcelReaderService
{
    Task<ExcelFile> LoadFileAsync(string filePath);
    Task<List<ExcelFile>> LoadFilesAsync(IEnumerable<string> filePaths);
    bool ValidateFile(string filePath);
}
```

#### IComparisonService
```csharp
public interface IComparisonService
{
    Task<ComparisonResult> CompareFilesAsync(ExcelFile left, ExcelFile right);
    Task<ComparisonResult> CompareFilesAsync(string leftPath, string rightPath);
    ComparisonOptions Options { get; set; }
}
```

#### IExportService
```csharp
public interface IExportService
{
    Task ExportToHtmlAsync(ComparisonResult result, string outputPath);
    Task ExportToPdfAsync(ComparisonResult result, string outputPath);
    Task ExportToExcelAsync(ComparisonResult result, string outputPath);
}
```

## Performance Requirements

### File Processing
- **Small files** (<1MB): <500ms loading time
- **Medium files** (1-10MB): <2 seconds loading time
- **Large files** (10-100MB): <10 seconds loading time
- **Memory usage**: <500MB for largest supported files

### Comparison Performance
- **Sheet comparison**: <1 second for 1000x100 cells
- **Full file comparison**: <5 seconds for medium files
- **Real-time updates**: <100ms response time

### UI Responsiveness
- **File loading**: Progress indication with cancellation
- **Comparison**: Background processing with updates
- **Rendering**: Smooth scrolling for large data sets

## Security Requirements

### Data Protection
- **Local processing only**: No network communication
- **Memory management**: Secure cleanup of sensitive data
- **File access**: Read-only access to source files
- **Temp files**: Secure deletion after processing

### Licensing & Protection
- **License validation**: Local license file verification
- **Anti-tampering**: Basic code obfuscation
- **Audit trail**: Usage logging for enterprise versions

## Platform Support

### Target Platforms
- **Windows 10/11**: x64, ARM64
- **Linux**: Ubuntu 20.04+, RHEL 8+, Debian 11+
- **macOS**: 10.15+ (Intel/Apple Silicon)

### Distribution
- **Windows**: MSI installer, portable executable
- **Linux**: AppImage, .deb, .rpm packages
- **macOS**: .dmg installer, portable app

## Testing Strategy

### Unit Testing
- **Core services**: 90%+ code coverage
- **Business logic**: Comprehensive test cases
- **Error handling**: Exception scenarios
- **Performance**: Benchmark tests

### Integration Testing
- **File processing**: Real Excel files
- **Cross-platform**: Automated testing on all platforms
- **UI integration**: View model interactions

### Manual Testing
- **User experience**: Usability testing
- **Performance**: Large file handling
- **Edge cases**: Corrupted/unusual files

## Build & Deployment

### Build Process
```bash
# Development build
dotnet build --configuration Debug

# Release build
dotnet publish --configuration Release --self-contained true

# Platform-specific builds
dotnet publish -r win-x64 --configuration Release
dotnet publish -r linux-x64 --configuration Release
dotnet publish -r osx-x64 --configuration Release
```

### CI/CD Pipeline
1. **Code quality**: Linting, formatting checks
2. **Testing**: Unit and integration tests
3. **Building**: Multi-platform builds
4. **Packaging**: Platform-specific installers
5. **Deployment**: Release artifacts

## Configuration

### Application Settings
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "ExcelViewer": "Debug"
    }
  },
  "Comparison": {
    "DefaultIgnoreCase": true,
    "DefaultIgnoreWhitespace": false,
    "MaxFileSize": "100MB"
  },
  "UI": {
    "Theme": "Auto",
    "Language": "en-US"
  }
}
```

### License Configuration
```json
{
  "License": {
    "Type": "Commercial",
    "ExpiryDate": "2025-12-31",
    "Features": ["Comparison", "Export", "Batch"]
  }
}
```

---

*Last updated: September 2025*
*Version: 1.0*