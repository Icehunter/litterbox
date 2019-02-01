dotnet pack .\LitterBox\LitterBox.csproj --no-build --include-symbols --include-source -c Release -o ..\published
dotnet pack .\LitterBox.DocumentDB\LitterBox.DocumentDB.csproj --no-build --include-symbols --include-source -c Release -o ..\published
dotnet pack .\LitterBox.Memory\LitterBox.Memory.csproj --no-build --include-symbols --include-source -c Release -o ..\published
dotnet pack .\LitterBox.Redis\LitterBox.Redis.csproj --no-build --include-symbols --include-source -c Release -o ..\published

powershell %~dp0\RenameSymbolsToPackage.ps1 'LitterBox'
powershell %~dp0\RenameSymbolsToPackage.ps1 'LitterBox.DocumentDB'
powershell %~dp0\RenameSymbolsToPackage.ps1 'LitterBox.Memory'
powershell %~dp0\RenameSymbolsToPackage.ps1 'LitterBox.Redis'