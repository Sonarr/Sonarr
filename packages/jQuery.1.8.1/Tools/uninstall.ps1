param($installPath, $toolsPath, $package, $project)

. (Join-Path $toolsPath common.ps1)

# Determine the file paths
$projectIntelliSenseFilePath = Join-Path $projectScriptsFolderPath $intelliSenseFileName
$origIntelliSenseFilePath = Join-Path $toolsPath $intelliSenseFileName

if (Test-Path $projectIntelliSenseFilePath) {
    if ((Get-Checksum $projectIntelliSenseFilePath) -eq (Get-Checksum $origIntelliSenseFilePath)) {
        # The intellisense file in the project matches the file in the tools folder, delete it
        
        if ($scriptsFolderProjectItem -eq $null) {
            # No Scripts folder
            exit
        }

        try {        
            # Get the project item for the intellisense file
            $intelliSenseFileProjectItem = $scriptsFolderProjectItem.ProjectItems.Item($intelliSenseFileName)
        }
        catch {
            # The item wasn't found
            exit
        }

        # Delete the project item
        Delete-ProjectItem $intelliSenseFileProjectItem
    }
    else {
        $projectScriptsFolderLeaf = Split-Path $projectScriptsFolderPath -Leaf
        Write-Host "Skipping '$projectScriptsFolderLeaf\$intelliSenseFileName' because it was modified." -ForegroundColor Magenta
    }
}
else {
    # The intellisense file was not found in project
    Write-Host "The intellisense file was not found in project at path $projectIntelliSenseFilePath"
}

# Update the _references.js file
Remove-Reference $scriptsFolderProjectItem $jqueryFileNameRegEx