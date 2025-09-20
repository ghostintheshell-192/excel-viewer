# ExcelViewer - Sonnet Execution Guide

## 🎯 **Mission**: Migrate ExcelViewer from WPF to Avalonia UI

**Project Location**: `/data/repos/apps-desktop/excel-viewer/`
**Backup Location**: `/data/repos/apps-desktop/excel-viewer-backup/` ⚠️ **DO NOT TOUCH**

## 📋 **Execution Checklist**

### ✅ **COMPLETED** (by Claude Planning Phase)
- [x] Project renamed to `excel-viewer`
- [x] Documentation structure created
- [x] CLAUDE.md project configuration
- [x] Dual license strategy (MIT + Commercial)
- [x] Development environment guide created
- [x] Migration plan documented
- [x] Core refactoring specifications prepared

### 🚀 **YOUR MISSION** (Execute in Order)

## **PHASE 0: Core Layer Cleanup** ⚠️ **CRITICAL FIRST**

### **Why Core First?**
- Current Core layer has serious architectural issues
- Clean foundation = smooth UI migration
- Core issues will block Avalonia migration
- Testing strategy requires stable Core

### **Step 0.1: Create Unit Test Project**
```bash
# Create test project structure
mkdir -p src/ExcelViewer.Tests
cd src/ExcelViewer.Tests

# Create test project file
# Follow specs in docs/core-refactoring-specs.md section "Unit Tests Structure"
```

### **Step 0.2: Fix ExcelFile Model** 🚨 **High Priority**
**Problem**: ExcelFile.cs has duplicate properties and broken design
**Target**: See `docs/core-refactoring-specs.md` section "Priority 1"

**Specific Actions**:
1. Remove duplicate properties (`Sheets` vs `SheetData` vs `ValidSheets`)
2. Remove broken `LoadFile()` method
3. Consolidate to single sheet dictionary
4. Make model immutable after construction
5. Update all references in codebase

### **Step 0.3: Consolidate Error Models** 🚨 **High Priority**
**Problem**: 4 different error classes with overlapping responsibilities
**Target**: Single `ExcelError` class with enum levels

**Specific Actions**:
1. Create new `ExcelError` class (see specs)
2. Replace all `FileLoadError`, `SheetError`, `CellError` usage
3. Consolidate enum types (`LoadStatus` vs `FileLoadStatus`)
4. Update error handling throughout codebase

### **Step 0.4: Refactor ExcelReaderService** 🚨 **Critical**
**Problem**: `ReadSheetData()` method is 160+ lines, unmaintainable
**Target**: Break into focused methods <25 lines each

**Specific Actions**:
1. Extract `CellReferenceParser` service
2. Extract `MergedCellProcessor` service
3. Split `ReadSheetData()` into 5-6 focused methods
4. Remove regex operations from tight loops
5. Separate error handling from business logic

### **Step 0.5: Validation**
**Before moving to Phase 1, ensure**:
- [ ] All Core tests passing
- [ ] Code builds without warnings
- [ ] No methods >25 lines in Core layer
- [ ] Memory usage stable with test files

## **PHASE 1: Avalonia Project Setup**

### **Step 1.1: Create Avalonia UI Project**
```bash
# Create new Avalonia project
cd src/
dotnet new avalonia -n ExcelViewer.UI.Avalonia
```

**Follow exact configuration from**:
- `docs/migration-plan.md` section "1.2 Avalonia Project Configuration"
- Package references and project setup

### **Step 1.2: Dependency Injection Setup**
**Target**: Maintain same DI container, adapt services for Avalonia

**Files to create**:
- `Program.cs` - Entry point
- `App.axaml` + `App.axaml.cs` - Application setup
- Service configuration identical to WPF version

### **Step 1.3: Create Avalonia-Specific Services**
**Required new services**:
- `AvaloniaDialogService` (replace WPF dialogs)
- `AvaloniaFilePickerService` (replace OpenFileDialog)

**Follow implementations in**: `docs/migration-plan.md` section "2.2 New Services"

### **Step 1.4: Validation**
- [ ] Avalonia app launches without errors
- [ ] DI container resolves all services
- [ ] Basic window displays

## **PHASE 2: ViewModels Migration**

### **Step 2.1: Migrate ViewModels** ✅ **Low Risk**
**Good news**: ViewModels are 95% compatible!

**Migration priority**:
1. `MainViewModel` - Change only file dialog service calls
2. `FileLoadResultViewModel` - Zero changes expected
3. `SearchViewModel` - Minor adaptations

**Critical**: Test each ViewModel in isolation before UI

### **Step 2.2: Update Command Pattern** (Optional)
**Current**: `RelayCommand`
**Option**: Upgrade to `ReactiveCommand` for better Avalonia integration

**Decision point**: Keep RelayCommand if working, upgrade only if benefits clear

### **Step 2.3: Validation**
- [ ] All ViewModels instantiate correctly
- [ ] Commands bind properly
- [ ] Data binding works with simple UI

## **PHASE 3: XAML Migration**

### **Step 3.1: Convert MainWindow**
**Reference**: `docs/migration-plan.md` section "3.1 WPF → Avalonia XAML Differences"

**Key changes**:
- Namespace declarations
- Window chrome handling
- Theme references

### **Step 3.2: Migrate Data Views**
**Priority order**:
1. `MainWindow.xaml` - Core layout
2. `FileLoadResultView.xaml` - Data display
3. `ComparisonView.xaml` - Main functionality
4. Search-related views

### **Step 3.3: Style and Theme Setup**
**Target**: Professional business appearance
**Reference**: Fluent theme + custom styles

### **Step 3.4: Validation**
- [ ] All views render correctly
- [ ] Data binding functional
- [ ] User interactions work
- [ ] Cross-platform UI consistency

## **PHASE 4: Testing & Polish**

### **Step 4.1: Cross-Platform Testing**
**Test environments**:
- ✅ **Debian 12** (native development environment)
- ✅ **Windows** (existing VM)
- ✅ **macOS** (GitHub Actions CI)

### **Step 4.2: Performance Validation**
**Benchmarks**:
- File loading times match WPF version
- Memory usage stable or improved
- UI responsiveness maintained

### **Step 4.3: Feature Parity Check**
- [ ] All WPF features working in Avalonia
- [ ] Error handling maintains same quality
- [ ] Export functionality preserved
- [ ] Search capabilities identical

## 🛡️ **Risk Mitigation**

### **If Issues Arise**
1. **Core refactoring problems**: Revert to backup, analyze issue
2. **Avalonia compatibility**: Check platform-specific documentation
3. **Performance degradation**: Profile and optimize specific areas
4. **UI layout issues**: Reference WPF XAML for comparison

### **Rollback Strategy**
- Original WPF version preserved in backup
- Each phase has validation gates
- Core layer changes are backward compatible
- Can deploy WPF version while fixing Avalonia issues

## 📊 **Success Metrics**

### **Phase Completion Criteria**
- **Phase 0**: Core tests pass, architecture clean
- **Phase 1**: Avalonia app runs, services work
- **Phase 2**: ViewModels functional, data flows
- **Phase 3**: Full UI working, feature complete
- **Phase 4**: Performance optimized, ready for beta

### **Final Validation**
- [ ] 100% feature parity with WPF version
- [ ] Cross-platform builds successful
- [ ] Performance meets requirements
- [ ] Code quality standards maintained
- [ ] Documentation updated

## 📚 **Reference Documents**

### **Required Reading** (in order)
1. `docs/development-setup.md` - Environment setup
2. `docs/core-refactoring-specs.md` - Core layer changes
3. `docs/migration-plan.md` - Full technical migration
4. `docs/technical-specs.md` - Architecture reference
5. `CLAUDE.md` - Project standards

### **Quick Reference**
- **Coding standards**: Follow C# guidelines in CLAUDE.md
- **Testing**: xUnit + Moq + FluentAssertions
- **Architecture**: Clean Architecture, MVVM, DI
- **Error handling**: Fail fast, explicit errors, logging

## ⚡ **Development Commands**

```bash
# Build and test (after .NET installation)
dotnet build
dotnet test

# Run Core tests specifically
dotnet test src/ExcelViewer.Tests/

# Run Avalonia app (Phase 1+)
dotnet run --project src/ExcelViewer.UI.Avalonia/

# Cross-platform builds (Phase 4)
dotnet publish -r linux-x64 --self-contained
dotnet publish -r win-x64 --self-contained
```

## 🚨 **Critical Success Factors**

1. **Core cleanup MUST be completed first** - No exceptions
2. **Test each phase independently** - Don't skip validation
3. **Maintain feature parity** - Business requirements unchanged
4. **Follow architectural standards** - Quality over speed
5. **Cross-platform compatibility** - Test on multiple platforms

## 📞 **Support Resources**

- **Project documentation**: `docs/` folder
- **Architecture decisions**: `CLAUDE.md`
- **Technical specs**: `docs/technical-specs.md`
- **Troubleshooting**: Development setup guide

---

**Remember**: This is a commercial project targeting data-sensitive industries. Quality, security, and reliability are paramount. Take time to do each phase correctly rather than rushing to completion.

**End Goal**: Production-ready Avalonia application with 100% feature parity, improved architecture, and cross-platform support for Windows, Linux, and macOS.

**Your role**: Execute this plan step-by-step, validate each phase, and maintain code quality throughout the migration process.

**Good luck! 🚀**