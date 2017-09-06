dotnet pack .\LitterBox\LitterBox.csproj --no-build --include-symbols --include-source -c Release -o ..\published
dotnet pack .\LitterBox.Memory\LitterBox.Memory.csproj --no-build --include-symbols --include-source -c Release -o ..\published
dotnet pack .\LitterBox.Redis\LitterBox.Redis.csproj --no-build --include-symbols --include-source -c Release -o ..\published

powershell .\RenameSymbolsToPackage.ps1 'LitterBox'
powershell .\RenameSymbolsToPackage.ps1 'LitterBox.Memory'
powershell .\RenameSymbolsToPackage.ps1 'LitterBox.Redis'