dotnet build

Write-Host "Create lib.dll"
dotnet run --project CodeFusion.ASM -- lib.cf int.cf -o lib.o.bin -t obj
dotnet run --project CodeFusion.Dump -- lib.o.bin -h
dotnet run --project CodeFusion.ASM -- lib.o.bin -o lib.bin -t lib
dotnet run --project CodeFusion.Builder -- lib.bin -t lib

Write-Host "Create a.exe"
dotnet run --project CodeFusion.ASM -- test.cf int.cf -o test.o.bin -t obj -e entry
dotnet run --project CodeFusion.Dump -- lib.o.bin -h
dotnet run --project CodeFusion.ASM -- test.o.bin -o test.bin -e entry
dotnet run --project CodeFusion.Builder -- test.bin

Write-Host "Starting program";
.\bin\a.exe
Write-Host "Program finished";

