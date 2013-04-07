$msBuild = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe'
$outputFolder = '.\_output'

Function Build()
{


    $clean = $msbuild + " nzbdrone.sln /t:Clean /m"
    $build = $msbuild + " nzbdrone.sln /p:Configuration=Release /p:Platform=x86 /t:Build"

    if(Test-Path $outputFolder)
    {
        Remove-Item -Recurse -Force $outputFolder -ErrorAction Continue
    }

       
    Invoke-Expression $clean
    Invoke-Expression $build
}

Function Package()
{
    Write-Host Removing XMLDoc files
    get-childitem $outputFolder -include *.xml -recurse | foreach ($_) {remove-item $_.fullname}

    Write-Host Removing FluentValidation.resources
    get-childitem $outputFolder -include FluentValidation.resources.dll -recurse | foreach ($_) {remove-item $_.fullname}
}


Build
Package