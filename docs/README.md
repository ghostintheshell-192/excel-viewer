# ExcelViewer - Documentation Index

## 📁 **Documentation Overview**

This folder contains comprehensive documentation for the ExcelViewer project migration from WPF to Avalonia UI.

## 📖 **Document Index**

### **Project Overview**
- **[overview.md](./overview.md)** - Executive summary, vision, target market, competitive advantage
- **[technical-specs.md](./technical-specs.md)** - Architecture, technology stack, performance requirements
- **[roadmap.md](./roadmap.md)** - Product roadmap, release timeline, success metrics

### **Development Documentation**
- **[development-setup.md](./development-setup.md)** - VSCode + Avalonia environment setup (Windows/Linux/macOS)
- **[migration-plan.md](./migration-plan.md)** - Complete technical migration strategy WPF → Avalonia
- **[core-refactoring-specs.md](./core-refactoring-specs.md)** - Detailed Core layer cleanup specifications

### **Execution Guide**
- **[sonnet-execution-guide.md](./sonnet-execution-guide.md)** - Step-by-step execution checklist for Sonnet

## 🚀 **Quick Start**

### **For Developers**
1. Read [development-setup.md](./development-setup.md) to configure your environment
2. Follow [sonnet-execution-guide.md](./sonnet-execution-guide.md) for implementation steps
3. Reference [core-refactoring-specs.md](./core-refactoring-specs.md) for Core layer changes

### **For Project Management**
1. Review [overview.md](./overview.md) for business context
2. Check [roadmap.md](./roadmap.md) for timeline and milestones
3. Monitor [technical-specs.md](./technical-specs.md) for architecture compliance

### **For Technical Review**
1. Start with [technical-specs.md](./technical-specs.md) for architecture overview
2. Deep dive into [migration-plan.md](./migration-plan.md) for implementation details
3. Validate against [core-refactoring-specs.md](./core-refactoring-specs.md) for code quality

## 📋 **Document Relationships**

```
overview.md                    ← Business context and goals
    ↓
technical-specs.md            ← Architecture and requirements
    ↓
roadmap.md                    ← Timeline and milestones
    ↓
development-setup.md          ← Environment preparation
    ↓
migration-plan.md             ← Technical migration strategy
    ↓
core-refactoring-specs.md     ← Detailed implementation specs
    ↓
sonnet-execution-guide.md     ← Step-by-step execution
```

## 🎯 **Key Project Decisions**

### **Technology Stack**
- **Framework**: .NET 8 (LTS)
- **UI**: Avalonia UI (cross-platform)
- **Architecture**: Clean Architecture + MVVM
- **Testing**: xUnit + Moq + FluentAssertions

### **Business Model**
- **Licensing**: Dual License (MIT + Commercial)
- **Target Market**: Data-sensitive industries (finance, defense, healthcare)
- **Distribution**: Desktop applications (Windows/Linux/macOS)

### **Migration Strategy**
- **Phase 0**: Core layer cleanup (CRITICAL FIRST)
- **Phase 1**: Avalonia project setup
- **Phase 2**: ViewModels migration
- **Phase 3**: XAML/UI migration
- **Phase 4**: Testing and polish

## ⚠️ **Critical Notes**

### **For Implementation**
- **Core cleanup is mandatory first step** - Do not skip Phase 0
- **Test each phase independently** - Validate before proceeding
- **Maintain 100% feature parity** - No functionality loss
- **Follow coding standards** - Reference CLAUDE.md in project root

### **For Quality Assurance**
- **Cross-platform testing required** - Windows, Linux, macOS
- **Performance benchmarks** - Must match or exceed WPF
- **Security validation** - Local processing only, no cloud dependencies
- **Commercial readiness** - Enterprise-grade quality standards

## 📊 **Progress Tracking**

### **Phase Completion Checklist**
- [ ] **Phase 0**: Core refactoring completed
- [ ] **Phase 1**: Avalonia infrastructure working
- [ ] **Phase 2**: ViewModels migrated and functional
- [ ] **Phase 3**: UI fully converted and styled
- [ ] **Phase 4**: Cross-platform tested and optimized

### **Quality Gates**
- [ ] All unit tests passing (>90% coverage)
- [ ] No methods >25 lines in Core layer
- [ ] Cross-platform builds successful
- [ ] Performance benchmarks met
- [ ] Commercial features functional

## 🔗 **External References**

### **Technology Documentation**
- [Avalonia UI Documentation](https://docs.avaloniaui.net/)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [DocumentFormat.OpenXml](https://docs.microsoft.com/en-us/office/open-xml/open-xml-sdk)

### **Development Tools**
- [VSCode](https://code.visualstudio.com/)
- [Avalonia VSCode Extension](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.vscode-avalonia)
- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

## 📞 **Support and Communication**

### **For Questions or Issues**
1. Check relevant documentation first
2. Review CLAUDE.md for project standards
3. Reference technical specifications for architecture decisions
4. Follow troubleshooting guides in development setup

### **Document Maintenance**
- **Last Updated**: September 2025
- **Version**: 1.0
- **Next Review**: After Phase 0 completion

---

*This documentation package provides complete guidance for the ExcelViewer WPF to Avalonia migration project. Follow the documents in order for optimal results.*