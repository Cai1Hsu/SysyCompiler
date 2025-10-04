# Create a working directory, or clean it if it exists
$WorkingDir = Join-Path $PSScriptRoot "publish"

if (Test-Path $WorkingDir) {
    Remove-Item $WorkingDir -Recurse
}

New-Item -ItemType Directory -Path $WorkingDir
$SourceDir = New-Item -ItemType Directory -Path (Join-Path $WorkingDir "SourceCode")
$ArtifactDir = New-Item -ItemType Directory -Path (Join-Path $WorkingDir "Artifacts")

# Run deploy script to build the project
& .\deploy.ps1 (Join-Path $PSScriptRoot "SysyCompiler" "SysyCompiler.csproj") Release net8.0 (Join-Path $ArtifactDir "SysyCompiler") --optimize-space --no-reflection --no-stacktrace-data --no-globalization --no-exception-messages --separate-symbols --no-debug-info --os linux

# Copy remaining necessary files

Write-Host "📂 Copying remaining necessary files..."

# Copy source code except invisible files and bin/obj directories
robocopy `
    $PSScriptRoot `
    $SourceDir `
    /E `
    /XD '.*' 'bin' 'obj' 'publish' `
    /XF '*.tmp' '*.log' '*.user' '*.lock' '*.exe' '*.dll' '*.pdb'

Write-Host "✅ Source code Copy completed."

Write-Host "☕ Creating delegation java code..."

# Create a entry java code that delegates to the native binary
$JavaCode = @"
public class Compiler {
    public static void main(String[] args) {
        try {
            ProcessBuilder pb = new ProcessBuilder();
            pb.command("/coursegrader/submitdata/Artifacts/SysyCompiler");
            pb.inheritIO();
            Process process = pb.start();
            int exitCode = process.waitFor();
            System.exit(exitCode);
        } catch (Exception e) {
            e.printStackTrace();
            System.exit(1);
        }
    }
}
"@

$JavaCode | Out-File -FilePath (Join-Path $WorkingDir "Compiler.java") -Encoding UTF8

Write-Host "✅ Packaging completed, directory is at $WorkingDir"
