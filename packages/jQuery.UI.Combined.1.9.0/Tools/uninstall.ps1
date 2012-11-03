param($installPath, $toolsPath, $package, $project)

. (Join-Path $toolsPath common.ps1)

# Update the _references.js file
Remove-Reference $scriptsFolderProjectItem $juiFileNameRegEx