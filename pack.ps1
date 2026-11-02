param([string]$Version = '3.0.0')
$ErrorActionPreference = 'Stop'
$app = Join-Path $PSScriptRoot 'build/PixelCat.exe'
if (!(Test-Path -LiteralPath $app)) { throw 'Run ./build.ps1 -Test first.' }
if ((Get-Item -LiteralPath $app).VersionInfo.FileVersion -ne "$Version.0") { throw 'Binary and package versions differ.' }
$output = Join-Path $PSScriptRoot 'build'
$stage = Join-Path $output ('package-' + [Guid]::NewGuid().ToString('N'))
$product = Join-Path $stage 'Persik'
New-Item -ItemType Directory -Path $product | Out-Null
Copy-Item -LiteralPath $app -Destination $product
foreach ($file in @('LICENSE', 'NOTICE', 'docs/QUICKSTART.txt')) {
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot $file) -Destination $product
}
$zip = Join-Path $output "Persik-$Version-Windows.zip"
Compress-Archive -LiteralPath $product -DestinationPath $zip -Force
$hashes = foreach ($file in @($zip, $app)) {
    $hash = Get-FileHash -LiteralPath $file -Algorithm SHA256
    "$($hash.Hash.ToLowerInvariant())  $([IO.Path]::GetFileName($file))"
}
[IO.File]::WriteAllLines((Join-Path $output 'SHA256SUMS.txt'), $hashes, [Text.UTF8Encoding]::new($false))
# The staging directory is deliberately retained inside ignored build/ for inspection.
Write-Host "Packaged: $zip"
