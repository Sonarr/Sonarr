Param( 
    [Parameter(Mandatory=$true, Position=0, HelpMessage="A branch name is #requires required")] 
    [string]$branch, 
    [Parameter(Mandatory=$true, Position=1, HelpMessage="A version is required")] 
    [string]$version
)

if ($branch -eq "<default>")
{
    $branch = "teamcity";
}

Write-Host $branch;
Write-Host $version;
Write-Host "NzbDrone.$branch.$version.zip";

Rename-Item "nzbdrone.zip" "NzbDrone.$branch.$version.zip"