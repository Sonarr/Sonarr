$msBuild = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe'
$outputFolder = '.\_output'
$outputFolderMono = '.\_output_mono'
$outputFolderOsx = '.\_output_osx'
$testPackageFolder = '.\_tests\'
$testSearchPattern = '*.Test\bin\x86\Release'
$sourceFolder = '.\src'
$updateFolder = $outputFolder + '\NzbDrone.Update'

Function Build()
{
    Write-Host "##teamcity[progressStart 'Build']"

    $clean = $msbuild + " src\nzbdrone.sln /t:Clean /m"
    $build = $msbuild + " src\nzbdrone.sln /p:Configuration=Release /p:Platform=x86 /t:Build /m"
    
    if(Test-Path $outputFolder)
    {
        Remove-Item -Recurse -Force $outputFolder -ErrorAction Continue
    }
       
    Invoke-Expression $clean
    CheckExitCode

    Invoke-Expression $build
    CheckExitCode

    CleanFolder $outputFolder

    AddJsonNet

    Write-Host "Removing Mono.Posix.dll"
    Remove-Item "$outputFolder\Mono.Posix.dll"

    Write-Host "##teamcity[progressFinish 'Build']"    
}

Function CleanFolder($path)
{
    Write-Host Removing XMLDoc files
    get-childitem $path -File -Filter *.xml -Recurse | foreach ($_) {remove-item $_.fullname}

    get-childitem $path -File -Filter *.transform -Recurse | foreach ($_) {remove-item $_.fullname}
    
    get-childitem $path -File -Filter *.dll.config -Recurse | foreach ($_) {remove-item $_.fullname}

    Write-Host Removing FluentValidation.Resources  files
    get-childitem $path -File -Filter FluentValidation.resources.dll -recurse | foreach ($_) {remove-item $_.fullname}

    get-childitem $path -File -Filter app.config -Recurse | foreach ($_) {remove-item $_.fullname}

    Write-Host Removing .less files
    get-childitem $path -File -Filter *.less -Recurse | foreach ($_) {remove-item $_.fullname}
 
    Write-Host Removing NuGet
    Remove-Item -Recurse -Force "$path\NuGet"
  
    Write-Host Removing Empty folders
    while (Get-ChildItem $path -recurse | where {!@(Get-ChildItem -force $_.fullname)} | Test-Path) 
    {
        Get-ChildItem $path -Directory -recurse | where {!@(Get-ChildItem -force $_.fullname)} | Remove-Item
    }
}

Function PackageMono()
{
    Write-Host "##teamcity[progressStart 'Creating Mono Package']"

    if(Test-Path $outputFolderMono)
    {
        Remove-Item -Recurse -Force $outputFolderMono -ErrorAction Continue
    }

    Copy-Item $outputFolder $outputFolderMono -recurse

    Write-Host Removing Update Client 
    Remove-Item -Recurse -Force "$outputFolderMono\NzbDrone.Update"

    Write-Host Creating MDBs
    get-childitem $outputFolderMono -File -Include @("*.exe", "*.dll") -Exclude @("MediaInfo.dll", "sqlite3.dll") -Recurse | foreach ($_) {
        Write-Host "Creating .mdb for $_"
        & "tools\pdb2mdb\pdb2mdb.exe" $_.fullname
    }

    Write-Host Removing PDBs
    get-childitem $outputFolderMono -File -Filter *.pdb -Recurse | foreach ($_) {remove-item $_.fullname}

    Write-Host Removing Service helpers
    get-childitem $outputFolderMono -File -Filter ServiceUninstall.* -Recurse | foreach ($_) {remove-item $_.fullname}
    get-childitem $outputFolderMono -File -Filter ServiceInstall.* -Recurse | foreach ($_) {remove-item $_.fullname}
    
    Write-Host Removing native windows binaries Sqlite, MediaInfo
    get-childitem $outputFolderMono -File -Filter sqlite3.* -Recurse | foreach ($_) {remove-item $_.fullname}
    get-childitem $outputFolderMono -File -Filter MediaInfo.* -Recurse | foreach ($_) {remove-item $_.fullname}

    Write-Host "Adding MediaInfoDotNet.dll.config (for dllmap)"
    Copy-Item "$sourceFolder\MediaInfoDotNet.dll.config" $outputFolderMono

    Write-Host Renaming NzbDrone.Console.exe to NzbDrone.exe
    Get-ChildItem $outputFolderMono -File -Filter "NzbDrone.exe*" -Recurse | foreach ($_) {remove-item $_.fullname}

    Write-Host Removing NzbDrone.Windows
    get-childitem $outputFolderMono -File -Filter NzbDrone.Windows.* -Recurse | foreach ($_) {remove-item $_.fullname}

    Get-ChildItem $outputFolderMono -File -Filter "NzbDrone.Console.exe*" -Recurse | foreach ($_) {
        $newName = $_.fullname -Replace ".Console",""

        Rename-Item $_.fullname $newName
    }

    Remove-Item "$outputFolderMono\NzbDrone.Console.vshost.exe"

    Write-Host "##teamcity[progressFinish 'Creating Mono Package']"
}

Function PackageOsx()
{
    Write-Host "##teamcity[progressStart 'Creating OS X Package']"

    if(Test-Path $outputFolderOsx)
    {
        Remove-Item -Recurse -Force $outputFolderOsx -ErrorAction Continue
    }

    Copy-Item $outputFolderMono $outputFolderOsx -recurse

    Write-Host "Adding sqlite dylibs"
    Copy-Item "$sourceFolder\Libraries\sqlite\*.dylib" "$outputFolderOsx"

    Write-Host "##teamcity[progressFinish 'Creating OS X Package']"
}

Function AddJsonNet()
{
    get-childitem $outputFolder -File -Filter Newtonsoft.Json.* -Recurse | foreach ($_) {remove-item $_.fullname}
    Copy-Item .\src\packages\Newtonsoft.Json.5.*\lib\net35\*.dll -Destination $outputFolder
    Copy-Item .\src\packages\Newtonsoft.Json.5.*\lib\net35\*.dll -Destination $outputFolder\NzbDrone.Update
}

Function PackageTests()
{

    Write-Host Packaging Tests
    Write-Host "##teamcity[progressStart 'Creating Test Package']"
  
    if(Test-Path $testPackageFolder)
    {
        Remove-Item -Recurse -Force $testPackageFolder -ErrorAction Continue
    }

    Get-ChildItem -Recurse -Directory | Where-Object {$_.FullName -like $testSearchPattern} |  foreach($_){ 
        Copy-Item -Recurse ($_.FullName + "\*")  $testPackageFolder -ErrorAction Ignore
    }

    .\src\.nuget\NuGet.exe install NUnit.Runners -Version 2.6.1 -Output $testPackageFolder   

    Copy-Item $outputFolder\*.dll -Destination $testPackageFolder -Force
    Copy-Item $outputFolder\*.pdb -Destination $testPackageFolder -Force

    Copy-Item .\*.sh               -Destination $testPackageFolder -Force

    get-childitem $testPackageFolder -File -Filter *log.config | foreach ($_) {remove-item $_.fullname}

    CleanFolder $testPackageFolder

    Write-Host "Adding MediaInfoDotNet.dll.config (for dllmap)"
    Copy-Item "$sourceFolder\MediaInfoDotNet.dll.config" -Destination $testPackageFolder -Force

    Write-Host "##teamcity[progressFinish 'Creating Test Package']"
}

Function RunGrunt()
{
   Write-Host "##teamcity[progressStart 'Running Grunt']"
   $gruntPath = [environment]::getfolderpath("applicationdata") + '\npm\node_modules\grunt-cli\bin\grunt'
   Invoke-Expression  'npm install'
   CheckExitCode
    
   Invoke-Expression  ('node ' + $gruntPath + ' packagerjs') -ErrorAction Continue -Verbose
   CheckExitCode

   Remove-Item $outputFolder\UI\build.txt -ErrorAction Continue

   Write-Host "##teamcity[progressFinish 'Running Grunt']"
}

Function CheckExitCode()
{
        if ($lastexitcode -ne 0)
        {
            Write-Host $errorMessage
            exit 1
        }
}

Function CleanupWindowsPackage()
{
    Write-Host Removing NzbDrone.Mono
    get-childitem $outputFolder -File -Filter NzbDrone.Mono.* -Recurse | foreach ($_) {remove-item $_.fullname}

    Write-Host Adding NzbDrone.Windows to UpdatePackage
    Copy-Item $outputFolder\* $updateFolder -Filter NzbDrone.Windows.*
}

Build
RunGrunt
PackageMono
PackageOsx
PackageTests
CleanupWindowsPackage
