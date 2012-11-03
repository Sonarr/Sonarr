function AddOrUpdate-Reference($scriptsFolderProjectItem, $fileNamePattern, $newFileName) {
    try {
        $referencesFileProjectItem = $scriptsFolderProjectItem.ProjectItems.Item("_references.js")
    }
    catch {
        # _references.js file not found
        return
    }

    if ($referencesFileProjectItem -eq $null) {
        # _references.js file not found
        return
    }

    $referencesFilePath = $referencesFileProjectItem.FileNames(1)
    $referencesTempFilePath = Join-Path $env:TEMP "_references.tmp.js"

    if ((Select-String $referencesFilePath -pattern $fileNamePattern).Length -eq 0) {
        # File has no existing matching reference line
        # Add the full reference line to the beginning of the file
        "/// <reference path=""$newFileName"" />" | Add-Content $referencesTempFilePath -Encoding UTF8
         Get-Content $referencesFilePath | Add-Content $referencesTempFilePath
    }
    else {
        # Loop through file and replace old file name with new file name
        Get-Content $referencesFilePath | ForEach-Object { $_ -replace $fileNamePattern, $newFileName } > $referencesTempFilePath
    }

    # Copy over the new _references.js file
    Copy-Item $referencesTempFilePath $referencesFilePath -Force
    Remove-Item $referencesTempFilePath -Force
}

function Remove-Reference($scriptsFolderProjectItem, $fileNamePattern) {
    try {
        $referencesFileProjectItem = $scriptsFolderProjectItem.ProjectItems.Item("_references.js")
    }
    catch {
        # _references.js file not found
        return
    }

    if ($referencesFileProjectItem -eq $null) {
        return
    }

    $referencesFilePath = $referencesFileProjectItem.FileNames(1)
    $referencesTempFilePath = Join-Path $env:TEMP "_references.tmp.js"

    if ((Select-String $referencesFilePath -pattern $fileNamePattern).Length -eq 1) {
        # Delete the line referencing the file
        Get-Content $referencesFilePath | ForEach-Object { if (-not ($_ -match $fileNamePattern)) { $_ } } > $referencesTempFilePath

        # Copy over the new _references.js file
        Copy-Item $referencesTempFilePath $referencesFilePath -Force
        Remove-Item $referencesTempFilePath -Force
    }
}

# Extract the version number from the file in the package's content\scripts folder
$packageScriptsFolder = Join-Path $installPath Content\Scripts
$juiFileName = Join-Path $packageScriptsFolder "jquery-ui-*.js" | Get-ChildItem -Exclude "*.min.js","*-vsdoc.js" | Split-Path -Leaf
$juiFileNameRegEx = "jquery-ui-((?:\d+\.)?(?:\d+\.)?(?:\d+\.)?(?:\d+)?(?:(?:-\w*)*)).js"
$juiFileName -match $juiFileNameRegEx
$ver = $matches[1]

# Get the project item for the scripts folder
try {
    $scriptsFolderProjectItem = $project.ProjectItems.Item("Scripts")
    $projectScriptsFolderPath = $scriptsFolderProjectItem.FileNames(1)
}
catch {
    # No Scripts folder
    Write-Host "No scripts folder found"
}