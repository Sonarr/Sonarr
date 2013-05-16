param (
    [switch]$runTests = $false
 )


$msBuild = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe'
$outputFolder = '.\_output'
$testSearchPattern = '*.Test\bin\x86\Release'
$testPackageFolder = '.\_tests\'

Function Build()
{
    $clean = $msbuild + " nzbdrone.sln /t:Clean /m"
    $build = $msbuild + " nzbdrone.sln /p:Configuration=Release /p:Platform=x86 /t:Build /m"

    if(Test-Path $outputFolder)
    {
        Remove-Item -Recurse -Force $outputFolder -ErrorAction Continue
    }
       
    Invoke-Expression $clean
    Invoke-Expression $build
    CleanFolder $outputFolder
}

Function CleanFolder($path)
{
    Write-Host Removing XMLDoc files
    get-childitem $path -File -Filter *.xml | foreach ($_) {remove-item $_.fullname}

    Write-Host Removing FluentValidation.Resources  files
    get-childitem $path -File -Filter FluentValidation.resources.dll -recurse | foreach ($_) {remove-item $_.fullname}

    get-childitem $path -File -Filter app.config | foreach ($_) {remove-item $_.fullname}
  
    Write-Host Removing Empty folders
    while (Get-ChildItem $path -recurse | where {!@(Get-ChildItem -force $_.fullname)} | Test-Path) 
    {
        Get-ChildItem $path -Directory -recurse | where {!@(Get-ChildItem -force $_.fullname)} | Remove-Item
    }
}

Function PackageTests()
{
    Write-Host Packagin Tests

      if(Test-Path $testPackageFolder)
    {
        Remove-Item -Recurse -Force $testPackageFolder -ErrorAction Continue
    }


    Get-ChildItem -Recurse -Directory  | Where-Object {$_.FullName -like $testSearchPattern} |  foreach($_){ 
        Copy-Item -Recurse ($_.FullName + "\*")  $testPackageFolder -ErrorAction Ignore
    }

    CleanFolder $testPackageFolder

    get-childitem $testPackageFolder -File -Filter *log.config | foreach ($_) {remove-item $_.fullname}

}


Function Nunit()
{
    $testFiles

    get-childitem $testPackageFolder -File -Filter *test.dll | foreach ($_) {
       $testFiles = $testFiles + $_.FullName + " "
       
    }

     $nunitExe =  '.\Libraries\nunit\nunit-console-x86.exe ' + $testFiles + ' /process:multiple /noxml'
     Invoke-Expression  $nunitExe 
}

Function RunGrunt()
{
   $gruntPath = [environment]::getfolderpath("applicationdata") + '\npm\node_modules\grunt-cli\bin\grunt'

    if(!(Test-Path $gruntPath))
    {
      Invoke-Expression  'npm install grunt-cli -g'
    }

    Invoke-Expression  'npm install'
    
    Invoke-Expression  ('node ' + $gruntPath + ' package')
}

Build
RunGrunt
PackageTests

if($runTests)
{
    Nunit
}