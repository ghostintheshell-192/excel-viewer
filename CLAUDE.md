# ExcelViewer - Claude Code Project Configuration

## Project Overview

**ExcelViewer** is a cross-platform desktop application for comparing and analyzing Excel files, targeting data-sensitive industries that require on-premise processing.

- **Type**: Commercial desktop application
- **Platform**: Cross-platform (.NET 8 + Avalonia UI)
- **Target Market**: Finance, defense, healthcare, compliance professionals
- **Business Model**: Commercial licenses ($79-$499)

## Development Methodology

### Core Principles
- **Security First**: 100% local processing, no cloud dependencies
- **Professional Grade**: Enterprise-ready features and reliability
- **Cross-Platform**: Native performance on Windows, Linux, macOS
- **Clean Architecture**: Separation of concerns, testable, maintainable

### Architecture Philosophy
- **Domain-Driven Design**: Business logic in Core layer
- **MVVM Pattern**: Clean separation UI/business logic
- **Dependency Injection**: Loose coupling, testable components
- **Event-Driven**: Responsive UI with async operations

## Technology Stack

### Core Technologies
- **.NET 8**: Modern framework, LTS support
- **C# 12**: Latest language features
- **Avalonia UI**: Cross-platform native UI
- **DocumentFormat.OpenXml**: Excel file processing

### Development Tools
- **IDE**: Visual Studio Code / Visual Studio / JetBrains Rider
- **Testing**: xUnit + Moq + FluentAssertions
- **CI/CD**: GitHub Actions
- **Documentation**: Markdown with diagrams

## Project Structure Standards

### Solution Organization
```
ExcelViewer/
├── src/
│   ├── ExcelViewer.Core/           # Business logic (platform agnostic)
│   ├── ExcelViewer.UI.Avalonia/   # Avalonia UI layer
│   └── ExcelViewer.Tests/         # Test projects
├── docs/                          # Project documentation
├── assets/                        # Images, icons, resources
└── build/                         # Build scripts and configurations
```

### Namespace Conventions
- **ExcelViewer.Core**: Business entities, services, interfaces
- **ExcelViewer.Core.Models**: Domain models and DTOs
- **ExcelViewer.Core.Services**: Business logic and file processing
- **ExcelViewer.UI.Avalonia**: UI layer components
- **ExcelViewer.UI.Avalonia.ViewModels**: MVVM view models
- **ExcelViewer.UI.Avalonia.Views**: XAML views and code-behind

## Coding Standards

### C# Specific Guidelines
- **PascalCase**: Classes, methods, properties, public fields
- **camelCase**: Local variables, private fields (with _ prefix)
- **ALL_CAPS**: Constants
- **Interfaces**: Prefix with 'I' (IExcelReaderService)

### Code Quality Rules
- **Methods**: Maximum 25 lines, single responsibility
- **Classes**: Focused, cohesive responsibilities
- **Dependencies**: Inject through constructor, use interfaces
- **Error Handling**: Explicit error types, no silent failures
- **Comments**: Only when complex logic needs explanation
- **Language**: All code and comments in English

### File Organization
- **One class per file**: Class name matches file name
- **Using statements**: System first, then third-party, then project
- **Regions**: Minimal use, prefer small focused classes
- **Nested classes**: Only for closely related helper types

## Testing Strategy

### Test Structure
- **Unit Tests**: Core business logic, 90%+ coverage
- **Integration Tests**: File processing, cross-platform scenarios
- **UI Tests**: Critical user workflows
- **Performance Tests**: Large file handling benchmarks

### Testing Conventions
- **Naming**: MethodName_Scenario_ExpectedResult
- **AAA Pattern**: Arrange, Act, Assert
- **Test Data**: Use builders for complex objects
- **Mocking**: Mock external dependencies, not internal logic

## Security & Compliance

### Data Protection
- **Local Processing**: No network communication during processing
- **Memory Management**: Secure cleanup of sensitive data
- **File Access**: Read-only source files, secure temp file handling
- **Audit Trail**: User action logging for enterprise versions

### Commercial Considerations
- **Licensing**: Built-in license validation and enforcement
- **Anti-Tampering**: Code obfuscation for release builds
- **Update Mechanism**: Secure update delivery and validation
- **Support**: Comprehensive logging for customer support

## Performance Requirements

### Target Metrics
- **File Loading**: <2 seconds for 10MB Excel files
- **Comparison**: <5 seconds for medium complexity files
- **Memory Usage**: <500MB for largest supported files
- **UI Responsiveness**: <100ms response time for user interactions

### Optimization Strategies
- **Async Operations**: Background processing with progress indication
- **Lazy Loading**: Load data on demand for large datasets
- **Memory Efficiency**: Dispose resources promptly, avoid leaks
- **Parallel Processing**: Multi-threaded comparison algorithms

## Business Logic Guidelines

### Domain Models
- **Immutable Entities**: Use readonly properties where possible
- **Value Objects**: For domain concepts without identity
- **Aggregate Roots**: Clear boundaries for data consistency
- **Domain Events**: For decoupled communication between modules

### Service Design
- **Interface Segregation**: Small, focused interfaces
- **Single Responsibility**: One reason to change per service
- **Stateless Services**: Avoid service state, use parameters
- **Error Handling**: Custom exceptions for business rule violations

## UI/UX Principles

### Avalonia UI Guidelines
- **MVVM Compliance**: No code-behind logic, use view models
- **Data Binding**: Prefer declarative XAML over procedural code
- **Commands**: Use ICommand for user actions
- **Converters**: For display logic that doesn't belong in view models

### User Experience
- **Responsive Design**: Handle long operations gracefully
- **Error Communication**: Clear, actionable error messages
- **Professional Appearance**: Consistent with business software
- **Accessibility**: Keyboard navigation, screen reader support

## Deployment & Distribution

### Platform Support
- **Windows**: MSI installer + portable executable
- **Linux**: AppImage, .deb/.rpm packages
- **macOS**: .dmg installer + app bundle

### Build Configuration
- **Debug**: Development builds with full debugging symbols
- **Release**: Optimized builds with code obfuscation
- **Self-Contained**: Include .NET runtime for easy deployment
- **Platform-Specific**: Optimize for each target platform

## Documentation Standards

### Code Documentation
- **XML Comments**: Public APIs and complex methods
- **README Files**: Setup and usage instructions
- **Architecture Docs**: High-level system design
- **API Documentation**: For any exposed interfaces

### Project Documentation
- **Overview**: Business goals and technical approach
- **Technical Specs**: Detailed implementation guidelines
- **Roadmap**: Feature development timeline
- **Release Notes**: Version history and changes

## Git Workflow

### Branch Strategy
- **main**: Production-ready code only (releases commerciali)
- **develop**: Integration branch (branch principale per lo sviluppo)
- **feature/***: Individual feature development
- **experiment/***: Testing e prove
- **fix/***: Bug fixes specifici

### Workflow Commands
```bash
# Sviluppo quotidiano
git checkout develop
git pull origin develop
git checkout -b feature/nome-funzionalità

# Integrazione
git checkout develop
git merge feature/nome-funzionalità
git push origin develop

# Release
git checkout main
git merge develop
git tag v1.0.0
git push origin main --tags
```

### Commit Standards
- **Conventional Commits**: feat, fix, docs, style, refactor, test
- **Clear Messages**: Explain why, not just what
- **Atomic Commits**: One logical change per commit
- **Code Reviews**: All changes reviewed before merge

## Commercial Considerations

### Intellectual Property
- **Source Code**: Dual Licensed (MIT + Commercial)
- **Open Source**: MIT license for community and non-commercial use
- **Commercial**: Separate license for enterprise features and support
- **Third-Party**: License compliance, attribution
- **Customer Data**: No collection, local processing only
- **Branding**: Trademark and logo protection

### Customer Support
- **Error Logging**: Comprehensive for support scenarios
- **Version Tracking**: Clear versioning for support
- **Documentation**: User guides, troubleshooting
- **Update Mechanism**: Seamless, secure updates

---

## Quick Start Commands

```bash
# Build and run development version
dotnet build && dotnet run --project src/ExcelViewer.UI.Avalonia

# Run tests
dotnet test

# Create release build
dotnet publish -c Release --self-contained

# Run code analysis
dotnet format --verify-no-changes
```

---

*This document defines the development standards and guidelines for the ExcelViewer project. All team members should follow these conventions to ensure consistent, maintainable, and professional code quality.*

**Last Updated**: September 2025
**Version**: 1.0
**Next Review**: October 2025