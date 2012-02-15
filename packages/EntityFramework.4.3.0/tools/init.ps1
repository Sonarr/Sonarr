param($installPath, $toolsPath, $package, $project)

if ([System.AppDomain]::CurrentDomain.GetAssemblies() | ?{ $_.GetName().Name -eq 'EntityFramework' })
{
    Write-Warning 'There is already a version of EntityFramework.dll loaded. You may need to restart Visual Studio for the commands to work properly.'
}

if (Get-Module | ?{ $_.Name -eq 'EntityFramework' })
{
    Remove-Module 'EntityFramework'
}

Import-Module (Join-Path $toolsPath 'EntityFramework.psd1') -ArgumentList $installPath
