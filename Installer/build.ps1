$framework = "net11.0"
$releasesDir = ".\Releases"

if (Test-Path $releasesDir) { Remove-Item -Recurse -Force $releasesDir }
New-Item -ItemType Directory -Path $releasesDir | Out-Null

Write-Host "Building SkEditor..." -ForegroundColor Green

function Build-SkEditor {
    param([string]$Runtime, [switch]$Aot)
    
    $aotText = if ($Aot) { "AOT" } else { "Non-AOT" }
    Write-Host "`n[1/3] Publishing: $Runtime ($aotText)..." -ForegroundColor Cyan
    
    $publishArgs = @("publish", "..\SkEditor\SkEditor.csproj", "-c", "Release", "-r", $Runtime)
    
    if ($Aot) {
        $publishArgs += "/p:PublishAot=true"
    } else {
        $publishArgs += "/p:PublishAot=false", "/p:PublishReadyToRun=true", "/p:PublishSingleFile=true", "--no-self-contained"
    }
    
    $process = Start-Process dotnet -ArgumentList $publishArgs -NoNewWindow -Wait -PassThru
    if ($process.ExitCode -ne 0) {
        Write-Error "Something when wrong with publishing SkEditor for $Runtime ($aotText)!"
        exit $process.ExitCode
    }
    
    return "..\SkEditor\bin\Release\$framework\$Runtime\publish"
}

function Package-Zip {
    param([string]$PublishDir, [string]$ZipName)
    Write-Host "[2/3] Packaging ZIP ($ZipName)..." -ForegroundColor Yellow
    
    $tempDir = ".\TempZipBuild"
    $innerDir = "$tempDir\SkEditor"
    
    if (Test-Path $tempDir) { Remove-Item -Recurse -Force $tempDir }
    New-Item -Type Directory -Path $innerDir | Out-Null
    
    Copy-Item -Path "$PublishDir\*" -Destination $innerDir -Recurse -Force
    
    $zipPath = "$releasesDir\$ZipName"
    Compress-Archive -Path $innerDir -DestinationPath $zipPath -Force
    
    Remove-Item -Recurse -Force $tempDir
}

function Build-Msi {
    param([string]$PublishDir, [switch]$Aot)
    
    $msiName = if ($Aot) { "SkEditorInstaller-AOT.msi" } else { "SkEditorInstaller.msi" }
    Write-Host "[3/3] Building MSI Installer ($msiName)..." -ForegroundColor Magenta
    
    $wixArgs = @("build", "SkEditorInstaller.wixproj", "-c", "Release", "-p:PublishDir=$PublishDir", "-p:IsAot=$Aot")
    
    $process = Start-Process dotnet -ArgumentList $wixArgs -NoNewWindow -Wait -PassThru
    if ($process.ExitCode -ne 0) {
        Write-Error "Something when wrong with building MSI Installer for $PublishDir (AOT: $Aot)!"
        exit $process.ExitCode
    }
    
    $wixOutName = if ($Aot) { "SkEditor-AOT.msi" } else { "SkEditor.msi" }
    $wixOutPath = "bin\x64\Release\en-US\$wixOutName"
    
    if (Test-Path $wixOutPath) {
        Move-Item -Path $wixOutPath -Destination "$releasesDir\$msiName" -Force
    }
}

try {
    $pubDirWin64 = Build-SkEditor -Runtime "win-x64" -Aot:$false
    Package-Zip -PublishDir $pubDirWin64 -ZipName "Windows-x64.zip"
    Build-Msi -PublishDir $pubDirWin64 -Aot:$false

    $pubDirWin64Aot = Build-SkEditor -Runtime "win-x64" -Aot:$true
    Build-Msi -PublishDir $pubDirWin64Aot -Aot:$true

    $pubDirWinArm = Build-SkEditor -Runtime "win-arm64" -Aot:$false
    Package-Zip -PublishDir $pubDirWinArm -ZipName "Windows-arm64.zip"

    $pubDirLinux64 = Build-SkEditor -Runtime "linux-x64" -Aot:$false
    Package-Zip -PublishDir $pubDirLinux64 -ZipName "Linux-x64.zip"

    Write-Host "`nAll done! SkEditor is ready for release." -ForegroundColor Green
    Write-Host "Cleaning up build artifacts..." -ForegroundColor Gray
    
    if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
    if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }

} catch {
    Write-Error "An unexpected error occurred: $_"
    exit 1
}