#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build SheetAtlas Windows installer with self-contained deployment
.DESCRIPTION
    This script creates an optimized self-contained build for Windows x64,
    then compiles an Inno Setup installer with code signing.
.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release
.PARAMETER SkipBuild
    Skip the build step and use existing output
.PARAMETER SkipSign
    Skip code signing the installer
.EXAMPLE
    .\build-windows-installer.ps1
.EXAMPLE
    .\build-windows-installer.ps1 -Configuration Debug -SkipSign
#>

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [switch]$SkipBuild,
    [switch]$SkipSign
)

$ErrorActionPreference = 'Stop'

# ============================================
# Configuration
# ============================================

$RootDir = Split-Path $PSScriptRoot -Parent
$ProjectFile = Join-Path $RootDir "src\SheetAtlas.UI.Avalonia\SheetAtlas.UI.Avalonia.csproj"
$PublishDir = Join-Path $RootDir "build\publish\windows-x64"
$InnoScript = Join-Path $RootDir "build\installer\SheetAtlas-Installer.iss"
$OutputDir = Join-Path $RootDir "build\output"
$CertFile = Join-Path $RootDir "build\certificates\SheetAtlas-CodeSigning.pfx"
$CertPassword = "sheetatlas-dev"

# ============================================
# Helper Functions
# ============================================

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    Write-Host "  $Message" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
    Write-Host ""
}

function Test-Command {
    param([string]$Command)
    return $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

# ============================================
# Pre-flight Checks
# ============================================

Write-Header "Pre-flight Checks"

# Check .NET SDK
if (-not (Test-Command "dotnet")) {
    Write-Error ".NET SDK not found. Please install .NET 8 SDK."
}

$dotnetVersion = dotnet --version
Write-Host "✓ .NET SDK: $dotnetVersion" -ForegroundColor Green

# Check Inno Setup
if (-not (Test-Command "iscc")) {
    Write-Warning "Inno Setup not found in PATH."
    Write-Warning "Please install Inno Setup from https://jrsoftware.org/isdl.php"
    Write-Warning "Or add to PATH: C:\Program Files (x86)\Inno Setup 6"

    # Try common installation path
    $InnoPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    if (Test-Path $InnoPath) {
        Write-Host "Found Inno Setup at: $InnoPath" -ForegroundColor Yellow
        Set-Alias -Name iscc -Value $InnoPath -Scope Script
    } else {
        Write-Error "Inno Setup not found. Cannot continue."
    }
} else {
    Write-Host "✓ Inno Setup found" -ForegroundColor Green
}

# Check signtool (optional)
if (-not $SkipSign) {
    if (-not (Test-Command "signtool")) {
        Write-Warning "signtool not found. Installer will not be signed."
        Write-Warning "Install Windows SDK to enable code signing."
        $SkipSign = $true
    } else {
        Write-Host "✓ signtool found" -ForegroundColor Green

        # Check certificate
        if (Test-Path $CertFile) {
            Write-Host "✓ Certificate found: $CertFile" -ForegroundColor Green
        } else {
            Write-Warning "Certificate not found: $CertFile"
            Write-Warning "Installer will not be signed."
            $SkipSign = $true
        }
    }
}

# ============================================
# Build Application
# ============================================

if (-not $SkipBuild) {
    Write-Header "Building SheetAtlas ($Configuration)"

    # Clean previous build
    if (Test-Path $PublishDir) {
        Write-Host "Cleaning previous build..." -ForegroundColor Yellow
        Remove-Item $PublishDir -Recurse -Force
    }

    # Publish self-contained
    Write-Host "Publishing self-contained build for Windows x64..." -ForegroundColor Yellow

    $publishArgs = @(
        'publish',
        $ProjectFile,
        '--configuration', $Configuration,
        '--runtime', 'win-x64',
        '--self-contained', 'true',
        '--output', $PublishDir,
        '/p:PublishSingleFile=false',
        '/p:PublishReadyToRun=true',
        '/p:PublishTrimmed=true',
        '/p:TrimMode=partial',
        '/p:IncludeNativeLibrariesForSelfExtract=true'
    )

    & dotnet @publishArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed with exit code $LASTEXITCODE"
    }

    # Show build size
    $buildSize = (Get-ChildItem $PublishDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "✓ Build completed: $($buildSize.ToString('F2')) MB" -ForegroundColor Green

} else {
    Write-Header "Skipping Build (using existing output)"

    if (-not (Test-Path $PublishDir)) {
        Write-Error "Publish directory not found: $PublishDir"
    }
}

# ============================================
# Build Installer
# ============================================

Write-Header "Building Installer with Inno Setup"

if (-not (Test-Path $InnoScript)) {
    Write-Error "Inno Setup script not found: $InnoScript"
}

# Create output directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# Run Inno Setup
Write-Host "Compiling installer..." -ForegroundColor Yellow

$isccArgs = @(
    $InnoScript,
    "/O$OutputDir",
    "/DConfiguration=$Configuration"
)

if ($SkipSign) {
    $isccArgs += "/DNoSign=1"
}

& iscc @isccArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Inno Setup compilation failed with exit code $LASTEXITCODE"
}

# Find generated installer
$installerName = "SheetAtlas-Setup-*.exe"
$installerPath = Get-ChildItem $OutputDir -Filter $installerName | Select-Object -First 1

if ($null -eq $installerPath) {
    Write-Error "Installer not found in: $OutputDir"
}

$installerSize = $installerPath.Length / 1MB
Write-Host "✓ Installer created: $($installerPath.Name)" -ForegroundColor Green
Write-Host "  Size: $($installerSize.ToString('F2')) MB" -ForegroundColor Gray
Write-Host "  Path: $($installerPath.FullName)" -ForegroundColor Gray

# ============================================
# Sign Installer (optional)
# ============================================

if (-not $SkipSign -and (Test-Command "signtool")) {
    Write-Header "Signing Installer"

    Write-Host "Signing with certificate: $CertFile" -ForegroundColor Yellow

    $signArgs = @(
        'sign',
        '/f', $CertFile,
        '/p', $CertPassword,
        '/tr', 'http://timestamp.digicert.com',
        '/td', 'sha256',
        '/fd', 'sha256',
        '/v',
        $installerPath.FullName
    )

    & signtool @signArgs

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Installer signed successfully" -ForegroundColor Green
    } else {
        Write-Warning "Code signing failed. Installer is unsigned."
    }
}

# ============================================
# Summary
# ============================================

Write-Header "Build Complete"

Write-Host "📦 Installer ready:" -ForegroundColor Green
Write-Host "   $($installerPath.FullName)" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Test installer on a clean Windows system" -ForegroundColor Gray
Write-Host "   2. Verify application launches correctly" -ForegroundColor Gray
Write-Host "   3. Check Start Menu shortcuts" -ForegroundColor Gray
Write-Host "   4. Test uninstaller" -ForegroundColor Gray
Write-Host ""

if ($SkipSign) {
    Write-Host "⚠️  Warning: Installer is NOT signed" -ForegroundColor Yellow
    Write-Host "   Users will see 'Unknown Publisher' warnings" -ForegroundColor Gray
    Write-Host ""
}
