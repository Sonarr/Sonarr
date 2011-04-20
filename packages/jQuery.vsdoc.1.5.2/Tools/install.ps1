param($installPath, $toolsPath, $package, $project)

$extId = "JScriptIntelliSenseParaExtension.Microsoft.039ee76c-3c7f-4281-ad23-f6528ab18623"
$extManager = [Microsoft.VisualStudio.Shell.Package]::GetGlobalService([Microsoft.VisualStudio.ExtensionManager.SVsExtensionManager])
$copyOverParaFile = $false
try {
    $copyOverParaFile = $extManager.GetInstalledExtension($extId).State -eq "Enabled"
}
catch [Microsoft.VisualStudio.ExtensionManager.NotInstalledException] {
    #Extension is not installed
}

if ($copyOverParaFile) {
    #Copy the -vsdoc-para file over the -vsdoc file
    #$projectFolder = Split-Path -Parent $project.FileName
    $projectFolder = $project.Properties.Item("FullPath").Value
    $paraVsDocPath = Join-Path $toolsPath jquery-1.5.2-vsdoc-para.js
    $vsDocPath = Join-Path $projectFolder Scripts\jquery-1.5.2-vsdoc.js
    Copy-Item $paraVsDocPath $vsDocPath -Force
}