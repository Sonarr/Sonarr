#! /bin/bash
msBuild='/c/Program Files (x86)/MSBuild/14.0/Bin'
outputFolder='./_output'
outputFolderMono='./_output_mono'
outputFolderOsx='./_output_osx'
outputFolderOsxApp='./_output_osx_app'
testPackageFolder='./_tests/'
testSearchPattern='*.Test/bin/x86/Release'
sourceFolder='./src'
slnFile=$sourceFolder/NzbDrone.sln
updateFolder=$outputFolder/NzbDrone.Update
updateFolderMono=$outputFolderMono/NzbDrone.Update

nuget='tools/nuget/nuget.exe';
CheckExitCode()
{
    "$@"
    local status=$?
    if [ $status -ne 0 ]; then
        echo "error with $1" >&2
        exit 1
    fi
    return $status
}

CleanFolder()
{
    local path=$1
    local keepConfigFiles=$2


    find $path -name "*.transform" -exec rm "{}" \;

    if [ $keepConfigFiles != true ] ; then
        find $path -name "*.dll.config" -exec rm "{}" \;
    fi

    echo "Removing FluentValidation.Resources files"
    find $path -name "FluentValidation.resources.dll" -exec rm "{}" \;
    find $path -name "App.config" -exec rm "{}" \;

    echo "Removing .less files"
    find $path -name "*.less" -exec rm "{}" \;

    echo "Removing vshost files"
    find $path -name "*.vshost.exe" -exec rm "{}" \;

    echo "Removing dylib files"
    find $path -name "*.dylib" -exec rm "{}" \;

    echo "Removing Empty folders"
    find $path -depth -empty -type d -exec rm -r "{}" \;
}

BuildWithMSBuild()
{
    export PATH=$msBuild:$PATH
    CheckExitCode MSBuild.exe $slnFile //t:Clean //m
    $nuget restore $slnFile
    CheckExitCode MSBuild.exe $slnFile //p:Configuration=Release //p:Platform=x86 //t:Build //m //p:AllowedReferenceRelatedFileExtensions=.pdb
}

BuildWithXbuild()
{
    export MONO_IOMAP=case
    CheckExitCode xbuild /t:Clean $slnFile
    mono $nuget restore $slnFile
    CheckExitCode xbuild /p:Configuration=Release /p:Platform=x86 /t:Build /p:AllowedReferenceRelatedFileExtensions=.pdb $slnFile
}

Build()
{
    echo "##teamcity[progressStart 'Build']"

    rm -rf $outputFolder

    if [ $runtime = "dotnet" ] ; then
        BuildWithMSBuild
    else
        BuildWithXbuild
    fi

    CleanFolder $outputFolder false

    echo "Removing Mono.Posix.dll"
    rm $outputFolder/Mono.Posix.dll

    echo "##teamcity[progressFinish 'Build']"
}

RunGulp()
{
    ProgressStart 'yarn install'
    yarn install
    ProgressEnd 'yarn install'

    echo "##teamcity[progressStart 'Running gulp']"
    CheckExitCode npm run build
    echo "##teamcity[progressFinish 'Running gulp']"

    echo "##teamcity[progressStart 'Running gulp (phantom)']"
    CheckExitCode yarn run build -- --production
    echo "##teamcity[progressFinish 'Running gulp (phantom)']"
}

CreateMdbs()
{
    local path=$1
    if [ $runtime = "dotnet" ] ; then
        local pdbFiles=( $(find $path -name "*.pdb") )
        for filename in "${pdbFiles[@]}"
        do
          if [ -e ${filename%.pdb}.dll ]  ; then
            tools/pdb2mdb/pdb2mdb.exe ${filename%.pdb}.dll
          fi
          if [ -e ${filename%.pdb}.exe ]  ; then
            tools/pdb2mdb/pdb2mdb.exe ${filename%.pdb}.exe
          fi
        done
    fi
}

PackageMono()
{
    echo "##teamcity[progressStart 'Creating Mono Package']"
    rm -rf $outputFolderMono
    cp -r $outputFolder $outputFolderMono

    echo "Creating MDBs"
    CreateMdbs $outputFolderMono

    echo "Removing PDBs"
    find $outputFolderMono -name "*.pdb" -exec rm "{}" \;

    echo "Removing Service helpers"
    rm -f $outputFolderMono/ServiceUninstall.*
    rm -f $outputFolderMono/ServiceInstall.*

    echo "Removing native windows binaries Sqlite, MediaInfo"
    rm -f $outputFolderMono/sqlite3.*
    rm -f $outputFolderMono/MediaInfo.*

    echo "Adding NzbDrone.Core.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Core/NzbDrone.Core.dll.config $outputFolderMono

    echo "Adding CurlSharp.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Common/CurlSharp.dll.config $outputFolderMono

    echo "Renaming NzbDrone.Console.exe to NzbDrone.exe"
    rm $outputFolderMono/NzbDrone.exe*
    for file in $outputFolderMono/NzbDrone.Console.exe*; do
        mv "$file" "${file//.Console/}"
    done

    echo "Removing NzbDrone.Windows"
    rm $outputFolderMono/NzbDrone.Windows.*

    echo "Adding NzbDrone.Mono to UpdatePackage"
    cp $outputFolderMono/NzbDrone.Mono.* $updateFolderMono

    echo "##teamcity[progressFinish 'Creating Mono Package']"
}

PackageOsx()
{
    echo "##teamcity[progressStart 'Creating OS X Package']"
    rm -rf $outputFolderOsx
    cp -r $outputFolderMono $outputFolderOsx

    echo "Adding sqlite dylibs"
    cp $sourceFolder/Libraries/Sqlite/*.dylib $outputFolderOsx

    echo "Adding MediaInfo dylib"
    cp $sourceFolder/Libraries/MediaInfo/*.dylib $outputFolderOsx

    echo "Adding Startup script"
    cp  ./osx/Sonarr $outputFolderOsx

    echo "##teamcity[progressFinish 'Creating OS X Package']"
}

PackageOsxApp()
{
    echo "##teamcity[progressStart 'Creating OS X App Package']"
    rm -rf $outputFolderOsxApp
    mkdir $outputFolderOsxApp

    cp -r ./osx/Sonarr.app $outputFolderOsxApp
    cp -r $outputFolderOsx $outputFolderOsxApp/Sonarr.app/Contents/MacOS

    echo "##teamcity[progressFinish 'Creating OS X App Package']"
}

PackageTests()
{
    echo "Packaging Tests"
    echo "##teamcity[progressStart 'Creating Test Package']"
    rm -rf $testPackageFolder
    mkdir $testPackageFolder

    find $sourceFolder -path $testSearchPattern -exec cp -r -u -T "{}" $testPackageFolder \;

    if [ $runtime = "dotnet" ] ; then
        $nuget install NUnit.ConsoleRunner -Version 3.2.0 -Output $testPackageFolder
    else
        mono $nuget install NUnit.ConsoleRunner -Version 3.2.0 -Output $testPackageFolder
    fi

    cp $outputFolder/*.dll $testPackageFolder
    cp ./*.sh $testPackageFolder

    echo "Creating MDBs for tests"
    CreateMdbs $testPackageFolder

    rm -f $testPackageFolder/*.log.config

    CleanFolder $testPackageFolder true

    echo "Adding NzbDrone.Core.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Core/NzbDrone.Core.dll.config $testPackageFolder

    echo "Adding CurlSharp.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Common/CurlSharp.dll.config $testPackageFolder

    echo "Copying CurlSharp libraries"
    cp $sourceFolder/ExternalModules/CurlSharp/libs/i386/* $testPackageFolder

    echo "##teamcity[progressFinish 'Creating Test Package']"
}

CleanupWindowsPackage()
{
    echo "Removing NzbDrone.Mono"
    rm -f $outputFolder/NzbDrone.Mono.*

    echo "Adding NzbDrone.Windows to UpdatePackage"
    cp $outputFolder/NzbDrone.Windows.* $updateFolder
}

# Use mono or .net depending on OS
case "$(uname -s)" in
    CYGWIN*|MINGW32*|MINGW64*|MSYS*)
        # on windows, use dotnet
        runtime="dotnet"
        ;;
    *)
        # otherwise use mono
        runtime="mono"
        ;;
esac

Build
RunGulp
PackageMono
PackageOsx
PackageOsxApp
PackageTests
CleanupWindowsPackage
