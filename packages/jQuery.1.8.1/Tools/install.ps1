param($installPath, $toolsPath, $package, $project)

. (Join-Path $toolsPath common.ps1)

# VS 11 and above supports the new intellisense JS files
$vsVersion = [System.Version]::Parse($dte.Version)
$supportsJsIntelliSenseFile = $vsVersion.Major -ge 11

if (-not $supportsJsIntelliSenseFile) {
    $displayVersion = $vsVersion.Major
    Write-Host "IntelliSense JS files are not supported by your version of Visual Studio: $displayVersion"
    exit
}

if ($scriptsFolderProjectItem -eq $null) {
    # No Scripts folder
    Write-Host "No Scripts folder found"
    exit
}

# Delete the vsdoc file from the project
try {
    $vsDocProjectItem = $scriptsFolderProjectItem.ProjectItems.Item("jquery-$ver-vsdoc.js")
    Delete-ProjectItem $vsDocProjectItem
}
catch {
    Write-Host "Error deleting vsdoc file: " + $_.Exception -ForegroundColor Red
    exit
}

# Copy the intellisense file to the project from the tools folder
$intelliSenseFileSourcePath = Join-Path $toolsPath $intelliSenseFileName
try {
    $scriptsFolderProjectItem.ProjectItems.AddFromFileCopy($intelliSenseFileSourcePath)
}
catch {
    # This will throw if the file already exists, so we need to catch here
}

# Update the _references.js file
AddOrUpdate-Reference $scriptsFolderProjectItem $jqueryFileNameRegEx $jqueryFileName