param(
    [string]$Version,
    [string]$Configuration = "Release",
    [string]$ProjectPath = "CTI/CTI/CTI.csproj",
    [string]$OutputDir = "build",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $repoRoot $ProjectPath
$dotnetHome = Join-Path $repoRoot ".dotnet"
$nugetPackages = Join-Path $repoRoot ".nuget/packages"

if (-not $env:DOTNET_CLI_HOME) {
    New-Item -ItemType Directory -Path $dotnetHome -Force | Out-Null
    $env:DOTNET_CLI_HOME = $dotnetHome
}

if (-not $env:NUGET_PACKAGES) {
    New-Item -ItemType Directory -Path $nugetPackages -Force | Out-Null
    $env:NUGET_PACKAGES = $nugetPackages
}

if (-not $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE) {
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
}

if (-not (Test-Path $projectFile)) {
    throw "Project file not found: $projectFile"
}

[xml]$projectXml = Get-Content -Path $projectFile -Raw

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = $projectXml.Project.PropertyGroup.Version | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "Version was not provided and could not be read from $ProjectPath"
}

if ($Version.StartsWith("v")) {
    $Version = $Version.Substring(1)
}

$targetFramework = $projectXml.Project.PropertyGroup.TargetFramework | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($targetFramework)) {
    throw "TargetFramework could not be read from $ProjectPath"
}

if (-not $SkipBuild) {
    & dotnet build $projectFile -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
}

$projectDir = Split-Path -Parent $projectFile
$libraryDir = Join-Path $projectDir "bin/$Configuration/$targetFramework"
$libraryPath = Join-Path $libraryDir "CTI.dll"
$xmlDocPath = Join-Path $libraryDir "CTI.xml"
$readmePath = Join-Path $repoRoot "README.md"
$docsPath = Join-Path $repoRoot "build/apidoc"

if (-not (Test-Path $libraryPath)) {
    throw "Compiled library not found: $libraryPath"
}

$outputRoot = Join-Path $repoRoot $OutputDir
$directoryName = "cti-dotnet-$Version"
$stageRoot = Join-Path $outputRoot "_zipstage"
$stageDir = Join-Path $stageRoot $directoryName
$archivePath = Join-Path $outputRoot "$directoryName.zip"

Remove-Item -Path $stageRoot -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path $archivePath -Force -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Path $stageDir -Force | Out-Null

Copy-Item -Path $libraryPath -Destination $stageDir
if (Test-Path $xmlDocPath) {
    Copy-Item -Path $xmlDocPath -Destination $stageDir
}
Copy-Item -Path $readmePath -Destination $stageDir

if (Test-Path $docsPath) {
    Copy-Item -Path $docsPath -Destination (Join-Path $stageDir "apidoc") -Recurse
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory(
    $stageRoot,
    $archivePath,
    [System.IO.Compression.CompressionLevel]::Optimal,
    $false
)

Remove-Item -Path $stageRoot -Recurse -Force

Write-Host "Created $archivePath"
