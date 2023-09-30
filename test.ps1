dotnet build

Set-Location proj;
Write-Host "Compile test.ils";
dotnet run --project ../ISC/ISC.csproj -- test.ils
Write-Host "Starting program";
./bin/a.exe
Write-Host "Finish program";
Set-Location ..