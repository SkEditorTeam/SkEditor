
<#
.EXAMPLE
    .\Package-MacOS.ps1
    .\Package-MacOS.ps1 -Arch osx-x64
    .\Package-MacOS.ps1 -Arch osx-arm64 -SkipBuild
#>

param(
    [ValidateSet("osx-arm64", "osx-x64")]
    [string]$Arch = "osx-arm64",

    [string]$DotnetVersion = "net10.0",

    [switch]$SkipBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot    = $PSScriptRoot
$ProjectDir  = Join-Path $RepoRoot "SkEditor"
$PublishDir  = Join-Path $ProjectDir "bin\Release\$DotnetVersion\$Arch\publish"
$PlistSrc    = Join-Path $ProjectDir "Properties\Info.plist"
$PngSrc      = Join-Path $ProjectDir "Assets\SkEditor.png"
$IcnsTemp    = Join-Path $env:TEMP   "SkEditor.icns"
$AppDir      = Join-Path $RepoRoot   "SkEditor.app"
$ZipOut      = Join-Path $RepoRoot   "$Arch-SkEditorMac.zip"

function Write-Step([string]$msg) {
    Write-Host "`n[$([char]0x2192)] $msg" -ForegroundColor Cyan
}

function Assert-Exists([string]$path, [string]$label) {
    if (-not (Test-Path $path)) {
        Write-Host "[ERROR] not found: $label`n  $path" -ForegroundColor Red
        exit 1
    }
}

if (-not $SkipBuild) {
    Write-Step "Building for $Arch..."
    Push-Location $ProjectDir
    dotnet publish -c Release -r $Arch `
        -p:PublishSingleFile=true `
        -p:PublishReadyToRun=true `
        --no-self-contained `
        --nologo -v q `
        --property WarningLevel=0 /clp:ErrorsOnly `
        /p:DebugType=None /p:DebugSymbols=false
    if ($LASTEXITCODE -ne 0) { Write-Host "[ERROR] dotnet publish error." -ForegroundColor Red; exit 1 }
    Pop-Location
}

Write-Step "checking required files"
Assert-Exists $PublishDir  "publish folder ($PublishDir)"
Assert-Exists $PlistSrc    "Info.plist"
Assert-Exists $PngSrc      "SkEditor.png"

$MagickAvailable = $null -ne (Get-Command magick -ErrorAction SilentlyContinue)

Write-Step "converting png to icns"

if ($MagickAvailable) {
    $sizes = @(1024, 512, 256, 128, 64, 32, 16)
    $tempPngs = @()

    foreach ($s in $sizes) {
        $tmp = Join-Path $env:TEMP "icon_${s}.png"
        magick $PngSrc -resize "${s}x${s}" $tmp
        $tempPngs += $tmp
    }

    magick @tempPngs $IcnsTemp
    $tempPngs | ForEach-Object { Remove-Item $_ -ErrorAction SilentlyContinue }

    if (Test-Path $IcnsTemp) {
        Write-Host "  icns convert success" -ForegroundColor Green
    } else {
        Write-Host "  [WARNING] icns convert error" -ForegroundColor Yellow
        $IcnsTemp = $null
    }
} else {
    Write-Host "  [WARNING] no imagemagick on this system :(" -ForegroundColor Yellow
    $IcnsTemp = $null
}

Write-Step "Creating .app structure..."

if (Test-Path $AppDir) {
    Remove-Item $AppDir -Recurse -Force
}

$null = New-Item -ItemType Directory -Force -Path "$AppDir\Contents\MacOS"
$null = New-Item -ItemType Directory -Force -Path "$AppDir\Contents\Resources"

Copy-Item -Path "$PublishDir\*" -Destination "$AppDir\Contents\MacOS\" -Recurse -Force
Copy-Item -Path $PlistSrc       -Destination "$AppDir\Contents\"        -Force

if ($null -ne $IcnsTemp -and (Test-Path $IcnsTemp)) {
    Copy-Item -Path $IcnsTemp -Destination "$AppDir\Contents\Resources\SkEditor.icns" -Force
}

Write-Host "  .app structure:" -ForegroundColor Gray
Get-ChildItem "$AppDir\Contents" -Recurse | ForEach-Object {
    $indent = "  " * ($_.FullName.Split([IO.Path]::DirectorySeparatorChar).Count - $AppDir.Split([IO.Path]::DirectorySeparatorChar).Count)
    Write-Host "  $indent$($_.Name)" -ForegroundColor Gray
}

Write-Step "ZIP packaging..."

if (Test-Path $ZipOut) { Remove-Item $ZipOut -Force }

if ($WslAvailable) {
    Write-Host "  WSL detected, trying to build with unix perms"

    function ConvertTo-WslPath([string]$winPath) {
        $p = $winPath -replace '\\','/'
        if ($p -match '^([A-Za-z]):(.*)') {
            return "/mnt/$($Matches[1].ToLower())$($Matches[2])"
        }
        return $p
    }

    $AppDirWsl = ConvertTo-WslPath $AppDir
    $ZipOutWsl = ConvertTo-WslPath $ZipOut
    $RepoWsl   = ConvertTo-WslPath $RepoRoot

    wsl bash -c "cd '$RepoWsl' && find 'SkEditor.app/Contents/MacOS/' -type f -exec chmod +x {} \; && zip -r '$ZipOutWsl' SkEditor.app"

    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [WARNING] WSL zip error" -ForegroundColor Yellow
        Compress-Archive -Path $AppDir -DestinationPath $ZipOut
    }
} else {
    Write-Host "  WSL not detected, unix perms won't be applied" -ForegroundColor Yellow
    Compress-Archive -Path $AppDir -DestinationPath $ZipOut
}

Write-Host "  done!" -ForegroundColor Green