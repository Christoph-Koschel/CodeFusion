dotnet build
dotnet run --project CodeFusion.ASM -- .\test.cf -o test.bin
dotnet run --project CodeFusion.Builder -- test.bin
Write-Host "Starting program";
.\bin\a.exe
Write-Host "Program finished";
