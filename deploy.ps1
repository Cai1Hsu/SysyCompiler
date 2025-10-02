param(
    [string]$Project = "SysyCompiler/SysyCompiler.csproj",
    [string]$Configuration = "Release",
    [string]$TargetFramework = "net8.0",
    [string]$OutputExe = "SysyCompiler.exe",
    [Parameter(ValueFromRemainingArguments)]
    [string[]]$BflatArgs
)

Write-Host "🚀 Building project with dotnet..."
dotnet build $Project -c $Configuration

$OutDir = Join-Path (Split-Path $Project -Parent) "bin\$Configuration\$TargetFramework"
$ExeDll = Join-Path $OutDir "SysyCompiler.dll"

if (-not (Test-Path $ExeDll)) {
    Write-Error "❌ $ExeDll was not found."
    exit 1
}

$dlls = Get-ChildItem $OutDir -Filter *.dll | ForEach-Object { "-r", $_.FullName }

Write-Host "⚙️ Using bflat to compile to native binary..."

$bflatParams = @('build') + $dlls + @('SysyCompiler\Program.cs', '--target', 'Exe', '--out', $OutputExe)

if ($BflatArgs) {
    $bflatParams += $BflatArgs
    Write-Host "📋 Additional bflat arguments: $($BflatArgs -join ' ')"
}

& bflat @bflatParams

Write-Host "✅ Build completed: $OutputExe"
