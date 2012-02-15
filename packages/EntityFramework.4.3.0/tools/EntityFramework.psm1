# Copyright (c) Microsoft Corporation.  All rights reserved.

$InitialDatabase = '0'

$installPath = $args[0]
$knownExceptions = @(
    'System.Data.Entity.Migrations.Infrastructure.MigrationsException',
    'System.Data.Entity.Migrations.Infrastructure.AutomaticMigrationsDisabledException',
    'System.Data.Entity.Migrations.Infrastructure.AutomaticDataLossException'
)

<#
.SYNOPSIS
    Enables Code First Migrations in a project.

.DESCRIPTION
    Enables Migrations by scaffolding a migrations configuration class in the project. If the
    target database was created by an initializer, an initial migration will be created (unless
    automatic migrations are enabled via the EnableAutomaticMigrations parameter).

.PARAMETER EnableAutomaticMigrations
    Specifies whether automatic migrations will be enabled in the scaffolded migrations configuration.
    If ommitted, automatic migrations will be disabled.

.PARAMETER ProjectName
    Specifies the project that the scaffolded migrations configuration class will
    be added to. If omitted, the default project selected in package manager
    console is used.

.PARAMETER Force
    Specifies that the migrations configuration be overwritten when running more
	than once for a given project.
#>
function Enable-Migrations
{
    [CmdletBinding(DefaultParameterSetName = 'ProjectName')] 
    param (
        [alias("Auto")]
        [switch] $EnableAutomaticMigrations,
        [string] $ProjectName,
		[switch] $Force
    )

    try
    {
        $commands = New-MigrationsCommandsNoConfiguration $ProjectName
        $commands.EnableMigrations($EnableAutomaticMigrations, $Force)
    }
    catch [Exception]
    {
        $exception = $_.Exception
        $exceptionType = $exception.GetType()
        
        if ($exceptionType.FullName -eq 'System.Data.Entity.Migrations.Design.ToolingException')
        {
            if ($knownExceptions -notcontains $exception.InnerType)
            {
                Write-Host $exception.InnerStackTrace
            }
        }
        elseif (!(Test-TypeInherits $exceptionType 'System.Data.Entity.Migrations.Infrastructure.MigrationsException'))
        {
            Write-Host $exception
        }
        
        throw $exception.Message
    }
}

<#
.SYNOPSIS
    Scaffolds a migration script for any pending model changes.

.DESCRIPTION
    Scaffolds a new migration script and adds it to the project.

.PARAMETER Name
    Specifies the name of the custom script.

.PARAMETER Force
    Specifies that the migration user code be overwritten when re-scaffolding an
    existing migration.

.PARAMETER ProjectName
    Specifies the project that contains the migration configuration type to be
    used. If ommitted, the default project selected in package manager console
    is used.

.PARAMETER StartUpProjectName
    Specifies the configuration file to use for named connection strings. If
    omitted, the specified project's configuration file is used.

.PARAMETER ConfigurationTypeName
    Specifies the migrations configuration to use. If omitted, migrations will
    attempt to locate a single migrations configuration type in the target
    project.

.PARAMETER ConnectionStringName
    Specifies the name of a connection string to use from the application's
    configuration file.

.PARAMETER ConnectionString
    Specifies the the connection string to use. If omitted, the context's
    default connection will be used.

.PARAMETER ConnectionProviderName
    Specifies the provider invariant name of the connection string.
#>
function Add-Migration
{
    [CmdletBinding(DefaultParameterSetName = 'ConnectionStringName')]
    param (
        [parameter(Position = 0,
            Mandatory = $true)]
        [string] $Name,
        [switch] $Force,
        [string] $ProjectName,
        [string] $StartUpProjectName,
        [string] $ConfigurationTypeName,
        [parameter(ParameterSetName = 'ConnectionStringName')]
        [string] $ConnectionStringName,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionString,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionProviderName)

    try
    {
        $commands = New-MigrationsCommands $ProjectName $StartUpProjectName $ConfigurationTypeName $ConnectionStringName $ConnectionString $ConnectionProviderName
        $commands.AddMigration($Name, $Force)
    }
    catch [Exception]
    {
        $exception = $_.Exception
        $exceptionType = $exception.GetType()
        
        if ($exceptionType.FullName -eq 'System.Data.Entity.Migrations.Design.ToolingException')
        {
            if ($knownExceptions -notcontains $exception.InnerType)
            {
                Write-Host $exception.InnerStackTrace
            }
        }
        elseif (!(Test-TypeInherits $exceptionType 'System.Data.Entity.Migrations.Infrastructure.MigrationsException'))
        {
            Write-Host $exception
        }
        
        throw $exception.Message
    }
}

<#
.SYNOPSIS
    Applies any pending migrations to the database.

.DESCRIPTION
    Updates the database to the current model by applying pending migrations.

.PARAMETER SourceMigration
    Only valid with -Script. Specifies the name of a particular migration to use
    as the update's starting point. If ommitted, the last applied migration in
    the database will be used.

.PARAMETER TargetMigration
    Specifies the name of a particular migration to update the database to. If
    ommitted, the current model will be used.

.PARAMETER Script
    Generate a SQL script rather than executing the pending changes directly.

.PARAMETER Force
    Specifies that data loss is acceptable during automatic migration of the
    database.

.PARAMETER ProjectName
    Specifies the project that contains the migration configuration type to be
    used. If ommitted, the default project selected in package manager console
    is used.

.PARAMETER StartUpProjectName
    Specifies the configuration file to use for named connection strings. If
    omitted, the specified project's configuration file is used.

.PARAMETER ConfigurationTypeName
    Specifies the migrations configuration to use. If omitted, migrations will
    attempt to locate a single migrations configuration type in the target
    project.

.PARAMETER ConnectionStringName
    Specifies the name of a connection string to use from the application's
    configuration file.

.PARAMETER ConnectionString
    Specifies the the connection string to use. If omitted, the context's
    default connection will be used.

.PARAMETER ConnectionProviderName
    Specifies the provider invariant name of the connection string.
#>
function Update-Database
{
    [CmdletBinding(DefaultParameterSetName = 'ConnectionStringName')]
    param (
        [string] $SourceMigration,
        [string] $TargetMigration,
        [switch] $Script,
        [switch] $Force,
        [string] $ProjectName,
        [string] $StartUpProjectName,
        [string] $ConfigurationTypeName,
        [parameter(ParameterSetName = 'ConnectionStringName')]
        [string] $ConnectionStringName,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionString,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionProviderName)

    # TODO: If possible, convert this to a ParameterSet
    if ($SourceMigration -and !$script)
    {
        throw '-SourceMigration can only be specified with -Script.'
    }

    try
    {
        $commands = New-MigrationsCommands $ProjectName $StartUpProjectName $ConfigurationTypeName $ConnectionStringName $ConnectionString $ConnectionProviderName
        $commands.UpdateDatabase($SourceMigration, $TargetMigration, $Script, $Force)
    }
    catch [Exception]
    {
        $exception = $_.Exception
        $exceptionType = $exception.GetType()
        
        if ($exceptionType.FullName -eq 'System.Data.Entity.Migrations.Design.ToolingException')
        {
            if ($knownExceptions -notcontains $exception.InnerType)
            {
                Write-Host $exception.InnerStackTrace
            }
        }
        elseif (!(Test-TypeInherits $exceptionType 'System.Data.Entity.Migrations.Infrastructure.MigrationsException'))
        {
            Write-Host $exception
        }
        
        throw $exception.Message
    }
}

<#
.SYNOPSIS
    Displays the migrations that have been applied to the target database.

.DESCRIPTION
    Displays the migrations that have been applied to the target database.

.PARAMETER ProjectName
    Specifies the project that contains the migration configuration type to be
    used. If ommitted, the default project selected in package manager console
    is used.

.PARAMETER StartUpProjectName
    Specifies the configuration file to use for named connection strings. If
    omitted, the specified project's configuration file is used.

.PARAMETER ConfigurationTypeName
    Specifies the migrations configuration to use. If omitted, migrations will
    attempt to locate a single migrations configuration type in the target
    project.

.PARAMETER ConnectionStringName
    Specifies the name of a connection string to use from the application's
    configuration file.

.PARAMETER ConnectionString
    Specifies the the connection string to use. If omitted, the context's
    default connection will be used.

.PARAMETER ConnectionProviderName
    Specifies the provider invariant name of the connection string.
#>
function Get-Migrations
{
    [CmdletBinding(DefaultParameterSetName = 'ConnectionStringName')]
    param (
        [string] $ProjectName,
        [string] $StartUpProjectName,
        [string] $ConfigurationTypeName,
        [parameter(ParameterSetName = 'ConnectionStringName')]
        [string] $ConnectionStringName,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionString,
        [parameter(ParameterSetName = 'ConnectionStringAndProviderName',
            Mandatory = $true)]
        [string] $ConnectionProviderName)

    try
    {
        $commands = New-MigrationsCommands $ProjectName $StartUpProjectName $ConfigurationTypeName $ConnectionStringName $ConnectionString $ConnectionProviderName
        $commands.GetMigrations()
    }
    catch [Exception]
    {
        $exception = $_.Exception
        $exceptionType = $exception.GetType()
        
        if ($exceptionType.FullName -eq 'System.Data.Entity.Migrations.Design.ToolingException')
        {
            if ($knownExceptions -notcontains $exception.InnerType)
            {
                Write-Host $exception.InnerStackTrace
            }
        }
        elseif (!(Test-TypeInherits $exceptionType 'System.Data.Entity.Migrations.Infrastructure.MigrationsException'))
        {
            Write-Host $exception
        }
        
        throw $exception.Message
    }
}

function New-MigrationsCommandsNoConfiguration($ProjectName)
{
    $project = Get-MigrationsProject $ProjectName

    Build-Project $project

    Load-EntityFramework

    try
    {
        return New-Object 'System.Data.Entity.Migrations.MigrationsCommands' @(
        $project,
        $project,
        $null,
        $null,
        $null,
        $null,
        $PSCmdlet )
    }
    catch [System.Management.Automation.MethodInvocationException]
    {
        throw $_.Exception.InnerException
    }
}

function New-MigrationsCommands($ProjectName, $StartUpProjectName, $ConfigurationTypeName, $ConnectionStringName, $ConnectionString, $ConnectionProviderName)
{
    $project = Get-MigrationsProject $ProjectName
    $startUpProject = Get-MigrationsStartUpProject $StartUpProjectName

    Build-Project $project
    Build-Project $startUpProject

    Load-EntityFramework

    try
    {
        return New-Object 'System.Data.Entity.Migrations.MigrationsCommands' @(
            $project,
            $startUpProject,
            $ConfigurationTypeName,
            $ConnectionStringName,
            $ConnectionString,
            $ConnectionProviderName,
            $PSCmdlet )
    }
    catch [System.Management.Automation.MethodInvocationException]
    {
        throw $_.Exception.InnerException
    }
}

function Get-MigrationsProject($name)
{
    if ($name)
    {
        return Get-SingleProject $name
    }

    $project = Get-Project

    Write-Verbose ('Using NuGet project ''' + $project.Name + '''.')

    return $project
}

function Get-MigrationsStartUpProject($name)
{
    if ($name)
    {
        return Get-SingleProject $name
    }

    $startupProjectPaths = $DTE.Solution.SolutionBuild.StartupProjects

    if (!$startupProjectPaths)
    {
        throw 'No start-up project found. Please use the -StartupProject parameter.'
    }
    if ($startupProjectPaths.Length -gt 1)
    {
        throw 'More than one start-up project found. Please use the -StartUpProject parameter.'
    }

    $startupProjectPath = $startupProjectPaths[0]

    if (!(Split-Path -IsAbsolute $startupProjectPath))
    {
        $solutionPath = Split-Path $DTE.Solution.Properties.Item('Path').Value
        $startupProjectPath = Join-Path $solutionPath $startupProjectPath -Resolve
    }

    $startupProject = $DTE.Solution.Projects | ?{
        $fullName = $_.FullName

        if ($fullName -and $fullName.EndsWith('\'))
        {
            $fullName = $fullName.Substring(0, $fullName.Length - 1)
        }

        return $fullName -eq $startupProjectPath
    }

    Write-Verbose ('Using StartUp project ''' + $startupProject.Name + '''.')

    return $startupProject
}

function Get-SingleProject($name)
{
    $project = Get-Project $name

    if ($project -is [array])
    {
        throw "More than one project '$name' was found. Specify the full name of the one to use."
    }

    return $project
}

function Load-EntityFramework()
{
    [System.AppDomain]::CurrentDomain.SetShadowCopyFiles()
    [System.Reflection.Assembly]::LoadFrom((Join-Path $installPath 'lib\net40\EntityFramework.dll')) | Out-Null
    [System.Reflection.Assembly]::LoadFrom((Join-Path $installPath 'tools\EntityFramework.PowerShell.dll')) | Out-Null
}

function Build-Project($project)
{
    $configuration = $DTE.Solution.SolutionBuild.ActiveConfiguration.Name

    $DTE.Solution.SolutionBuild.BuildProject($configuration, $project.UniqueName, $true)

    if ($DTE.Solution.SolutionBuild.LastBuildInfo)
    {
        throw 'The project ''' + $project.Name + ''' failed to build.'
    }
}

function Test-TypeInherits($type, $baseTypeName)
{
    if ($type.FullName -eq $baseTypeName)
    {
        return $true
    }
    
    $baseType = $type.BaseType
    
    if ($baseType)
    {
        return Test-TypeInherits $baseType $baseTypeName
    }
    
    return $false
}

Export-ModuleMember @( 'Enable-Migrations', 'Add-Migration', 'Update-Database', 'Get-Migrations' ) -Variable 'InitialDatabase'
