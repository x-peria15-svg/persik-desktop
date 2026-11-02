param([switch]$Test, [string]$Screenshots)
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (!(Test-Path -LiteralPath $compiler)) { throw 'Install .NET Framework 4.8 on Windows.' }
$output = Join-Path $PSScriptRoot 'build'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$sources = @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'src') -Filter '*.cs' | Sort-Object Name | ForEach-Object FullName)
$references = @('/reference:System.Windows.Forms.dll', '/reference:System.Drawing.dll', '/reference:System.Runtime.Serialization.dll')
$app = Join-Path $output 'PixelCat.exe'
$iconPath = Join-Path $PSScriptRoot 'src/persik.ico'
$manifestPath = Join-Path $PSScriptRoot 'src/app.manifest'
& $compiler /nologo /optimize+ /warnaserror+ /target:winexe "/out:$app" "/win32icon:$iconPath" "/win32manifest:$manifestPath" @references @sources
if ($LASTEXITCODE -ne 0) { throw 'Application build failed.' }
if ($Test) {
    $testExe = Join-Path $output 'Persik.Tests.exe'
    $testSources = @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'tests') -Filter '*.cs' | Sort-Object Name | ForEach-Object FullName)
    & $compiler /nologo /optimize+ /warnaserror+ /target:exe /main:Persik.TestRunner "/out:$testExe" @references @sources @testSources
    if ($LASTEXITCODE -ne 0) { throw 'Test build failed.' }
    $arguments = @()
    if ($Screenshots) { $arguments += $Screenshots }
    & $testExe @arguments
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
}
Write-Host "Built: $app"
