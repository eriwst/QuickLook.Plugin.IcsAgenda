$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root "bin\Release"
$name = "QuickLook.Plugin.IcsAgenda"
$target = Join-Path $root "$name.qlplugin"
$temp = Join-Path $env:TEMP "$name-package"
$metadataName = "QuickLook.Plugin.Metadata.config"
$metadataSource = Join-Path $root $metadataName
$metadataOutput = Join-Path $out $metadataName

function Assert-ValidMetadata([string]$path) {
    if (!(Test-Path $path)) {
        throw "$metadataName not found: $path`nCreate it in the project root before packaging."
    }

    try {
        [xml]$metadata = Get-Content $path -Raw
    }
    catch {
        throw "$metadataName is not valid XML: $($_.Exception.Message)"
    }

    $versionText = [string]$metadata.Metadata.Version

    if ([string]::IsNullOrWhiteSpace($versionText)) {
        throw "Plugin version is not defined in $metadataName."
    }

    $version = 0
    if (![int]::TryParse($versionText.Trim(), [ref]$version)) {
        throw "Plugin version '$versionText' is invalid. Use a positive integer, for example <Version>1</Version>."
    }

    if ($version -le 0) {
        throw "Plugin version must be greater than 0. Update $metadataName, for example to <Version>1</Version>."
    }

    if ([string]::IsNullOrWhiteSpace([string]$metadata.Metadata.Namespace)) {
        throw "Plugin namespace is not defined in $metadataName."
    }

    return $version
}

if (!(Test-Path $out)) {
    throw "Release output not found. Build first: dotnet build -c Release"
}

$pluginDll = Join-Path $out "$name.dll"
if (!(Test-Path $pluginDll)) {
    throw "$name.dll not found in bin\Release. Build first: dotnet build -c Release"
}

# Validate the source metadata first. This prevents packaging Base.config
# or an old build output by accident.
$version = Assert-ValidMetadata $metadataSource

if (!(Test-Path $metadataOutput)) {
    throw "$metadataName was not copied to bin\Release. Run: dotnet build -c Release"
}

$outputVersion = Assert-ValidMetadata $metadataOutput
if ($outputVersion -ne $version) {
    throw "Metadata version mismatch: project=$version, bin\Release=$outputVersion. Run a clean Release build."
}

if (Test-Path $temp) {
    Remove-Item $temp -Recurse -Force
}
New-Item -ItemType Directory -Path $temp | Out-Null

Get-ChildItem $out -File | Where-Object {
    $_.Extension -in ".dll", ".config" -and
    $_.Name -notlike "QuickLook.Common.*" -and
    $_.Name -ne "QuickLook.Plugin.Metadata.Base.config"
} | Copy-Item -Destination $temp

$packagedMetadata = Join-Path $temp $metadataName
if (!(Test-Path $packagedMetadata)) {
    throw "$metadataName is missing from the package staging directory."
}

if (Test-Path $target) {
    Remove-Item $target -Force
}

$zipTarget = "$target.zip"
if (Test-Path $zipTarget) {
    Remove-Item $zipTarget -Force
}

Compress-Archive -Path "$temp\*" -DestinationPath $zipTarget -Force
Move-Item $zipTarget $target -Force
Remove-Item $temp -Recurse -Force

Write-Host ""
Write-Host "Created $target"
Write-Host "Plugin version: $version"
Write-Host "Ready to install with QuickLook."
