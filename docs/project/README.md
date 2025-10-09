# ExcelViewer - Documentation Hub

## 📁 **Documentation Overview**

Complete documentation for ExcelViewer, a cross-platform desktop application for Excel file comparison built with Avalonia UI.

## 🚀 **Quick Start**

### **For Users**
1. **Download** the latest release
2. **Install** .NET 8 Runtime if not present
3. **Run** `ExcelViewer` executable
4. **Load** Excel files (.xlsx, .xls) to compare

### **For Developers**
```bash
# Clone and setup
git clone <repository-url>
cd excel-viewer
dotnet restore && dotnet build

# Run the application
dotnet run --project src/ExcelViewer.UI.Avalonia/ExcelViewer.UI.Avalonia.csproj

# Run tests
dotnet test
```

**Requirements:**
- .NET 8 SDK
- VSCode (recommended) with C# Dev Kit + Avalonia extensions
- Linux: `sudo apt install libx11-dev libice-dev libsm-dev libfontconfig1-dev`

## 📖 **Document Index**

### **Project Overview**
- **[overview.md](./overview.md)** - Executive summary, vision, target market, competitive advantage
- **[technical-specs.md](./technical-specs.md)** - Architecture, technology stack, performance requirements
- **[roadmap.md](./roadmap.md)** - Product roadmap, release timeline, success metrics

### **For Project Management**
1. Review [overview.md](./overview.md) for business context
2. Check [roadmap.md](./roadmap.md) for timeline and milestones
3. Monitor [technical-specs.md](./technical-specs.md) for architecture compliance

### **For Technical Review**
1. Start with [technical-specs.md](./technical-specs.md) for architecture overview
2. Deep dive into implementation details and patterns
3. Reference project conventions in [CLAUDE.md](../CLAUDE.md)

## 📋 **Project Structure**

```
ExcelViewer/
├── src/
│   ├── ExcelViewer.Core/                    # Core business logic (Clean Architecture)
│   │   ├── Application/                     # Application services & DTOs
│   │   ├── Domain/                          # Domain entities & value objects
│   │   └── Shared/                          # Shared utilities & extensions
│   ├── ExcelViewer.Infrastructure/          # Infrastructure layer (separated)
│   │   └── External/                        # External services (Excel file processing)
│   ├── ExcelViewer.UI.Avalonia/             # Avalonia UI layer (MVVM)
│   │   ├── ViewModels/                      # MVVM ViewModels
│   │   ├── Views/                           # XAML Views
│   │   ├── Services/                        # UI-specific services
│   │   └── Converters/                      # XAML converters
├── tests/
│   └── ExcelViewer.Tests/                   # Unit tests
├── docs/                                    # Documentation
├── assets/                                  # Images, icons, resources
├── build/                                   # Build scripts
├── CLAUDE.md                                # Development conventions
└── README.md                                # Main project overview
```

## 🎯 **Key Project Decisions**

### **Technology Stack**
- **Framework**: .NET 8 (LTS)
- **UI**: Avalonia UI (cross-platform)
- **Architecture**: Clean Architecture + MVVM
- **Testing**: xUnit + Moq + FluentAssertions

### **Business Model**
- **Licensing**: MIT License (simple and permissive)
- **Target Market**: Data-sensitive industries (finance, defense, healthcare)
- **Distribution**: Cross-platform desktop (Windows/Linux/macOS)

### **Current Status**
- ✅ **Avalonia migration completed** - Core infrastructure working
- 🔧 **Active development** - Feature enhancement and optimization phase
- 📝 **Documentation** - Clean, focused, and maintainable

## 📊 **Development Milestones**

### **Phase 0**: Core Migration ✅ **COMPLETED**
- ✅ Avalonia UI project setup
- ✅ Core library migration
- ✅ Basic file loading and comparison
- ✅ Cross-platform compatibility

### **Phase 1**: Feature Development (Current)
- Enhanced comparison algorithms
- Professional UI/UX polish
- Export capabilities
- Performance optimization

## 🐛 **Troubleshooting**

**Build Issues:**
- Verify .NET 8: `dotnet --version`
- Clean build: `dotnet clean && dotnet restore && dotnet build`

**Runtime Issues:**
- Ensure .NET 8 Runtime is installed
- File permissions for Excel files
- On Linux: Install required system libraries

## 🔗 **External Resources**

### **Technology Documentation**
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [DocumentFormat.OpenXml](https://docs.microsoft.com/en-us/office/open-xml/open-xml-sdk)

### **Development Tools**
- [VSCode](https://code.visualstudio.com/)
- [Avalonia VSCode Extension](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.vscode-avalonia)
- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

---

*This documentation hub provides complete guidance for ExcelViewer development and usage. For development standards and conventions, see [CLAUDE.md](../CLAUDE.md).*

**Last Updated**: September 2025 | **Version**: 2.0