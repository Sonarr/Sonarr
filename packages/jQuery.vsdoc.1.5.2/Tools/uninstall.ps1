param($installPath, $toolsPath, $package, $project)

#Forcibly delete the -vsdoc file
#$projectFolder = Split-Path -Parent $project.FileName
$projectFolder = $project.Properties.Item("FullPath").Value
$projVsDocPath = Join-Path $projectFolder Scripts\jquery-1.5.2-vsdoc.js
$origVsDocPath = Join-Path $installPath Content\Scripts\jquery-1.5.2-vsdoc.js
$origVsDocParaPath = Join-Path $toolsPath jquery-1.5.2-vsdoc-para.js

function Get-Checksum($file) {
    $cryptoProvider = New-Object "System.Security.Cryptography.MD5CryptoServiceProvider"
	
    $fileInfo = Get-Item "$file"
	trap { ;
	continue } $stream = $fileInfo.OpenRead()
    if ($? -eq $false) {
		#Write-Host "Couldn't open file for reading"
        return $null
	}
    
    $bytes = $cryptoProvider.ComputeHash($stream)
    $checksum = ''
	foreach ($byte in $bytes) {
		$checksum += $byte.ToString('x2')
	}
    
	$stream.Close() | Out-Null
    
    return $checksum
}

if (Test-Path $projVsDocPath) {
    #Copy the original -vsdoc file over the -vsdoc file modified during install
    #Normal uninstall logic will then kick in
    
    if ((Get-Checksum $projVsDocPath) -eq (Get-Checksum $origVsDocParaPath)) {
        #Write-Host "Copying orig vsdoc file over"
        Copy-Item $origVsDocPath $projVsDocPath -Force
    }
    else {
        #Write-Host "vsdoc file has changed"   
    }
}
else {
    #Write-Host "vsdoc file not found in project"
}
