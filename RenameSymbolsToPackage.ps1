$source = $args[0]

Set-Location $PSScriptRoot\published

Dir |
Where-Object { $_.Name -match "$source.[\d\.]+.nupkg" -and $_.Name -notmatch "\.(symbols)"} |
Remove-Item

Dir |
Where-Object { $_.Name -match "$source.[\d\.]+.symbols.nupkg" } |
Rename-Item -NewName { $_.Name -replace ".symbols","" }