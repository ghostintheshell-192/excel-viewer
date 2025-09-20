# ExcelViewer - Development Environment Setup

## Overview

This guide covers setting up the development environment for ExcelViewer on different platforms, with focus on VSCode + Avalonia UI development.

## Prerequisites

### .NET 8 SDK

#### Windows
```bash
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
# Or via winget
winget install Microsoft.DotNet.SDK.8
```

#### Debian 12 (Current Development Environment)
```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update

# Install .NET 8 SDK
sudo apt install dotnet-sdk-8.0

# Verify installation
dotnet --version  # Should show 8.0.x
```

#### macOS
```bash
# Via Homebrew
brew install dotnet

# Or download from Microsoft website
```

### Avalonia UI Dependencies

#### Linux (Debian/Ubuntu)
```bash
# Required system libraries for Avalonia
sudo apt install -y \
    libx11-dev \
    libice-dev \
    libsm-dev \
    libfontconfig1-dev \
    libfreetype6-dev \
    libxext-dev \
    libxrender-dev \
    libxrandr-dev \
    libxcursor-dev \
    libxi-dev \
    libxinerama-dev

# Optional: For hardware acceleration
sudo apt install -y mesa-utils libgl1-mesa-dev
```

#### Windows
No additional dependencies required.

#### macOS
No additional dependencies required.

## VSCode Setup

### Required Extensions

Install these extensions in VSCode:

1. **C# Dev Kit** (`ms-dotnettools.csdevkit`)
   - Primary C# support
   - IntelliSense and debugging
   - Project management

2. **Avalonia for VSCode** (`avalonia.avalonia-vscode`)
   - Avalonia-specific support
   - XAML IntelliSense
   - Preview functionality

3. **.NET Extension Pack** (`ms-dotnettools.vscode-dotnet-pack`)
   - Comprehensive .NET support
   - NuGet package management
   - Test runner integration

4. **XAML** (`ms-dotnettools.dotnet-interactive-vscode`)
   - Enhanced XAML editing
   - Syntax highlighting
   - Auto-completion

### Optional but Recommended Extensions

5. **GitLens** (`eamodio.gitlens`)
   - Enhanced Git integration
   - Blame annotations
   - Repository insights

6. **Error Lens** (`usernamehw.errorlens`)
   - Inline error display
   - Improved debugging experience

7. **TODO Highlight** (`wayou.vscode-todo-highlight`)
   - Highlight TODO comments
   - Task management

### VSCode Configuration

Create or update `.vscode/settings.json`:

```json
{
  "dotnet.defaultSolution": "ExcelViewer.sln",
  "omnisharp.enableRoslynAnalyzers": true,
  "omnisharp.enableEditorConfigSupport": true,
  "omnisharp.enableImportCompletion": true,
  "files.exclude": {
    "**/bin": true,
    "**/obj": true
  },
  "avalonia.suggestionsEnabled": true,
  "avalonia.previewerEnabled": true
}
```

Create `.vscode/launch.json` for debugging:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch ExcelViewer (Avalonia)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/ExcelViewer.UI.Avalonia/bin/Debug/net8.0/ExcelViewer.UI.Avalonia.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/ExcelViewer.UI.Avalonia",
      "console": "internalConsole",
      "stopAtEntry": false
    },
    {
      "name": "Launch Tests",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build-tests",
      "program": "dotnet",
      "args": ["test", "--logger", "console"],
      "cwd": "${workspaceFolder}",
      "console": "internalConsole"
    }
  ]
}
```

Create `.vscode/tasks.json`:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "${workspaceFolder}/ExcelViewer.sln",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "build-tests",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "${workspaceFolder}/src/ExcelViewer.Tests/ExcelViewer.Tests.csproj"
      ],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "publish",
      "command": "dotnet",
      "type": "process",
      "args": [
        "publish",
        "${workspaceFolder}/src/ExcelViewer.UI.Avalonia/ExcelViewer.UI.Avalonia.csproj",
        "--configuration",
        "Release",
        "--self-contained",
        "true"
      ],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

## Project Structure Verification

After setup, your project structure should look like:

```
excel-viewer/
├── .vscode/
│   ├── settings.json
│   ├── launch.json
│   └── tasks.json
├── src/
│   ├── ExcelViewer.Core/
│   ├── ExcelViewer.UI.Avalonia/    # Will be created
│   └── ExcelViewer.Tests/          # Will be created
├── docs/
├── ExcelViewer.sln
└── README.md
```

## Development Workflow

### Building the Project

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/ExcelViewer.Core/

# Build with specific configuration
dotnet build --configuration Release
```

### Running the Application

```bash
# Run Avalonia UI (once created)
dotnet run --project src/ExcelViewer.UI.Avalonia/

# Run with specific configuration
dotnet run --project src/ExcelViewer.UI.Avalonia/ --configuration Debug
```

### Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test src/ExcelViewer.Tests/
```

### Debugging

1. **VSCode Debugging**:
   - Set breakpoints in code
   - Press F5 or use "Launch ExcelViewer (Avalonia)" configuration
   - Use Debug Console for runtime evaluation

2. **Console Debugging**:
   ```bash
   # Run with debug logging
   DOTNET_ENVIRONMENT=Development dotnet run --project src/ExcelViewer.UI.Avalonia/
   ```

## Platform-Specific Notes

### Debian 12 Development (Primary)

- **X11 vs Wayland**: Avalonia works on both, prefer X11 for debugging
- **Font rendering**: Install `fonts-liberation` for better font support
- **Hardware acceleration**: Ensure Mesa drivers are installed

```bash
# Check graphics driver
glxinfo | grep "OpenGL renderer"

# Install additional fonts
sudo apt install fonts-liberation fonts-dejavu-core
```

### Windows Development (VM)

- **Hardware acceleration**: Ensure VM has 3D acceleration enabled
- **File sharing**: Map Linux development folder for cross-platform testing
- **Performance**: Allocate adequate RAM (4GB+) to VM

### Testing Commands

```bash
# Verify .NET installation
dotnet --info

# Check Avalonia template availability
dotnet new list | grep avalonia

# Test project compilation
dotnet build --verbosity minimal

# Platform runtime check
dotnet --list-runtimes
```

## Troubleshooting

### Common Issues

1. **"SDK not found" error**:
   ```bash
   # Check SDK installation
   dotnet --list-sdks

   # Reinstall if needed (Debian)
   sudo apt remove dotnet-sdk-8.0
   sudo apt install dotnet-sdk-8.0
   ```

2. **Avalonia UI not rendering**:
   ```bash
   # Check X11 libraries
   ldd /usr/lib/x86_64-linux-gnu/libX11.so.6

   # Install missing dependencies
   sudo apt install --fix-missing libx11-dev
   ```

3. **VSCode IntelliSense not working**:
   - Restart OmniSharp: `Ctrl+Shift+P` → "OmniSharp: Restart OmniSharp"
   - Check `.vscode/settings.json` configuration
   - Verify C# Dev Kit extension is enabled

4. **Build errors after migration**:
   ```bash
   # Clean build artifacts
   dotnet clean

   # Restore packages
   dotnet restore

   # Rebuild
   dotnet build
   ```

### Performance Optimization

1. **VSCode Settings**:
   ```json
   {
     "omnisharp.maxFindSymbolsItems": 1000,
     "omnisharp.autoStart": true,
     "dotnet.completion.showCompletionItemsFromUnimportedNamespaces": false
   }
   ```

2. **System Optimization**:
   ```bash
   # Increase file watchers (Linux)
   echo fs.inotify.max_user_watches=524288 | sudo tee -a /etc/sysctl.conf
   sudo sysctl -p
   ```

## Next Steps

After completing this setup:

1. ✅ Verify all extensions are installed and working
2. ✅ Test building the existing Core project
3. ✅ Prepare for Avalonia UI project creation
4. ✅ Set up debugging workflow

---

*Last updated: September 2025*
*For issues or questions, refer to the project documentation or create an issue.*