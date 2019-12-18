#! /bin/bash
msBuildVersion='15.0'
outputFolder='./_output'
outputFolderWindows='./_output_windows'
outputFolderLinux='./_output_linux'
outputFolderMacOS='./_output_macos'
outputFolderMacOSApp='./_output_macos_app'
testPackageFolder='./_tests'
testPackageFolderWindows='./_tests_windows'
testPackageFolderLinux='./_tests_linux'
sourceFolder='./src'
slnFile=$sourceFolder/Sonarr.sln
updateSubFolder=Sonarr.Update

nuget='tools/nuget/nuget.exe';
vswhere='tools/vswhere/vswhere.exe';

. ./version.sh

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

ProgressStart()
{
    echo "##teamcity[blockOpened name='$1']"
    echo "##teamcity[progressStart '$1']"
}

ProgressEnd()
{
    echo "##teamcity[progressFinish '$1']"
    echo "##teamcity[blockClosed name='$1']"
}

UpdateVersionNumber()
{
    if [ "$BUILD_NUMBER" != "" ]; then
        echo "Updating Version Info"
        verMajorMinorRevision=`echo "$buildVersion" | cut -d. -f1,2,3`
        verBuild=`echo "${BUILD_NUMBER}" | cut -d. -f4`
        BUILD_NUMBER=$verMajorMinorRevision.$verBuild
        echo "##teamcity[buildNumber '$BUILD_NUMBER']"
        sed -i "s/<AssemblyVersion>[0-9.*]\+<\/AssemblyVersion>/<AssemblyVersion>$BUILD_NUMBER<\/AssemblyVersion>/g" ./src/Directory.Build.props
        sed -i "s/<AssemblyConfiguration>[\$()A-Za-z-]\+<\/AssemblyConfiguration>/<AssemblyConfiguration>${BRANCH:-dev}<\/AssemblyConfiguration>/g" ./src/Directory.Build.props
    fi
}

CreateReleaseInfo()
{
    if [ "$BUILD_NUMBER" != "" ]; then
        echo "Create Release Info"
        echo -e "# Do Not Edit\nReleaseVersion=$BUILD_NUMBER\nBranch=${BRANCH:-dev}" > $outputFolder/release_info
    fi
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

    echo "Removing vshost files"
    find $path -name "*.vshost.exe" -exec rm "{}" \;

    echo "Removing dylib files"
    find $path -name "*.dylib" -exec rm "{}" \;

    echo "Removing Empty folders"
    find $path -depth -empty -type d -exec rm -r "{}" \;
}

BuildWithMSBuild()
{
    installationPath=`$vswhere -latest -products \* -requires Microsoft.Component.MSBuild -property installationPath`
    installationPath=${installationPath/C:\\/\/c\/}
    installationPath=${installationPath//\\/\/}
    msBuild="$installationPath/MSBuild/$msBuildVersion/Bin"
    echo $msBuild

    export PATH=$msBuild:$PATH
    CheckExitCode MSBuild.exe $slnFile //p:Configuration=Release //p:Platform=x86 //t:Clean //m
    $nuget restore $slnFile
    CheckExitCode MSBuild.exe $slnFile //p:Configuration=Release //p:Platform=x86 //t:Build //m //p:AllowedReferenceRelatedFileExtensions=.pdb
}

BuildWithXbuild()
{
    export MONO_IOMAP=case
    CheckExitCode msbuild /t:Clean $slnFile
    mono $nuget restore $slnFile
    CheckExitCode msbuild /p:Configuration=Release /p:Platform=x86 /t:Build /p:AllowedReferenceRelatedFileExtensions=.pdb $slnFile
}

LintUI()
{
    ProgressStart 'ESLint'
    CheckExitCode yarn lint
    ProgressEnd 'ESLint'

    ProgressStart 'Stylelint'
    CheckExitCode yarn stylelint
    ProgressEnd 'Stylelint'
}

Build()
{
    ProgressStart 'Build'

    rm -rf $outputFolder
    rm -rf $testPackageFolder

    if [ $runtime = "dotnet" ] ; then
        BuildWithMSBuild
    else
        BuildWithXbuild
    fi

    CleanFolder $outputFolder false

    echo "Removing Mono.Posix.dll"
    rm $outputFolder/Mono.Posix.dll

    ProgressEnd 'Build'
}

RunGulp()
{
    ProgressStart 'yarn install'
    yarn install
    ProgressEnd 'yarn install'

    LintUI

    ProgressStart 'Running gulp'
    CheckExitCode yarn run build --production
    ProgressEnd 'Running gulp'
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

PatchMono()
{
    local path=$1

    # Below we deal with some mono incompatibilities with windows-only dotnet core/standard libs    
    # See: https://github.com/mono/mono/blob/master/tools/nuget-hash-extractor/download.sh
    # That list defines assemblies that are prohibited from being loaded from the appdir, instead loading from mono GAC.

    # We have debian dependencies to get these installed or facades from mono 5.10+
    for assembly in System.IO.Compression System.Runtime.InteropServices.RuntimeInformation System.Net.Http System.Globalization.Extensions System.Text.Encoding.CodePages System.Threading.Overlapped
    do
        if [ -e $path/$assembly.dll ]; then
            if [ -e $sourceFolder/Libraries/Mono/$assembly.dll ]; then
                echo "Copy Mono-specific facade $assembly.dll (uses win32 interop)"
                cp $sourceFolder/Libraries/Mono/$assembly.dll $path/$assembly.dll
            else
                echo "Remove $assembly.dll (uses win32 interop)"
                rm $path/$assembly.dll
            fi
            
        fi
    done

    # Copy more stable version of Vectors for mono <5.12
    if [ -e $path/System.Numerics.Vectors.dll ]; then
        packageDir="$HOME/.nuget/packages/system.numerics.vectors/4.5.0"

        if [ ! -d "$HOME/.nuget/packages/system.numerics.vectors/4.5.0" ]; then
            # May reside in the NuGetFallback folder, which is harder to find
            # Download somewhere to get the real cache populated
            if [ $runtime = "dotnet" ] ; then
                $nuget install System.Numerics.Vectors -Version 4.5.0 -Output ./_temp/System.Numerics.Vectors
            else
                mono $nuget install System.Numerics.Vectors -Version 4.5.0 -Output ./_temp/System.Numerics.Vectors
            fi
            rm -rf ./_temp/System.Numerics.Vectors
        fi
        # Copy the netstandard2.0 version rather than net46
        cp "$packageDir/lib/netstandard2.0/System.Numerics.Vectors.dll" $path/
    fi
}

PackageMono()
{
    ProgressStart 'Creating Mono Package'

    rm -rf $outputFolderLinux

    echo "Copying Binaries"
    cp -r $outputFolder $outputFolderLinux

    echo "Creating MDBs"
    CreateMdbs $outputFolderLinux

    echo "Removing PDBs"
    find $outputFolderLinux -name "*.pdb" -exec rm "{}" \;

    echo "Removing Service helpers"
    rm -f $outputFolderLinux/ServiceUninstall.*
    rm -f $outputFolderLinux/ServiceInstall.*

    echo "Removing native windows binaries Sqlite, MediaInfo"
    rm -f $outputFolderLinux/sqlite3.*
    rm -f $outputFolderLinux/MediaInfo.*

    PatchMono $outputFolderLinux

    echo "Adding Sonarr.Core.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Core/Sonarr.Core.dll.config $outputFolderLinux

    # Remove Http binding redirect by renaming it
    # We don't need this anymore once our minimum mono version is 5.10
    sed -i "s/System.Net.Http/System.Net.Http.Mono/g" $outputFolderLinux/Sonarr.Console.exe.config

    echo "Renaming Sonarr.Console.exe to Sonarr.exe"
    rm $outputFolderLinux/Sonarr.exe*
    for file in $outputFolderLinux/Sonarr.Console.exe*; do
        mv "$file" "${file//.Console/}"
    done

    echo "Removing Sonarr.Windows"
    rm $outputFolderLinux/Sonarr.Windows.*

    echo "Adding Sonarr.Mono to UpdatePackage"
    cp $outputFolderLinux/Sonarr.Mono.* $outputFolderLinux/$updateSubFolder/

    ProgressEnd 'Creating Mono Package'
}

PackageMacOS()
{
    ProgressStart 'Creating MacOS Package'

    rm -rf $outputFolderMacOS
    mkdir $outputFolderMacOS

    echo "Adding Startup script"
    cp ./macOS/Sonarr $outputFolderMacOS
    dos2unix $outputFolderMacOS/Sonarr

    echo "Copying Binaries"
    cp -r $outputFolderLinux/* $outputFolderMacOS

    echo "Adding sqlite dylibs"
    cp $sourceFolder/Libraries/Sqlite/*.dylib $outputFolderMacOS

    echo "Adding MediaInfo dylib"
    cp $sourceFolder/Libraries/MediaInfo/*.dylib $outputFolderMacOS

    ProgressEnd 'Creating MacOS Package'
}

PackageMacOSApp()
{
    ProgressStart 'Creating macOS App Package'

    rm -rf $outputFolderMacOSApp
    mkdir $outputFolderMacOSApp
    cp -r ./macOS/Sonarr.app $outputFolderMacOSApp
    mkdir -p $outputFolderMacOSApp/Sonarr.app/Contents/MacOS

    echo "Adding Startup script"
    cp ./macOS/Sonarr $outputFolderMacOSApp/Sonarr.app/Contents/MacOS
    dos2unix $outputFolderMacOSApp/Sonarr.app/Contents/MacOS/Sonarr

    echo "Copying Binaries"
    cp -r $outputFolderLinux/* $outputFolderMacOSApp/Sonarr.app/Contents/MacOS

    echo "Adding sqlite dylibs"
    cp $sourceFolder/Libraries/Sqlite/*.dylib $outputFolderMacOSApp/Sonarr.app/Contents/MacOS

    echo "Adding MediaInfo dylib"
    cp $sourceFolder/Libraries/MediaInfo/*.dylib $outputFolderMacOSApp/Sonarr.app/Contents/MacOS

    echo "Removing Update Folder"
    rm -r $outputFolderMacOSApp/Sonarr.app/Contents/MacOS/Sonarr.Update

    ProgressEnd 'Creating macOS App Package'
}

PackageTestsMono()
{
    ProgressStart 'Creating Mono Test Package'

    rm -rf $testPackageFolderLinux

    echo "Copying Binaries"
    cp -r $testPackageFolder $testPackageFolderLinux

    if [ $runtime = "dotnet" ] ; then
        $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolderLinux
    else
        mono $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolderLinux
    fi

    echo "Creating MDBs"
    CreateMdbs $testPackageFolderLinux

    echo "Removing PDBs"
    find $testPackageFolderLinux -name "*.pdb" -exec rm "{}" \;

    PatchMono $testPackageFolderLinux

    echo "Adding Sonarr.Core.dll.config (for dllmap)"
    cp $sourceFolder/NzbDrone.Core/Sonarr.Core.dll.config $testPackageFolderLinux

    # Remove Http binding redirect by renaming it
    # We don't need this anymore once our minimum mono version is 5.10
    sed -i "s/System.Net.Http/System.Net.Http.Mono/g" $testPackageFolderLinux/Sonarr.Common.Test.dll.config

    cp ./test.sh $testPackageFolderLinux/
    dos2unix $testPackageFolderLinux/test.sh

    echo "Removing Sonarr.Windows"
    rm $testPackageFolderLinux/Sonarr.Windows.*

    rm -f $testPackageFolderLinux/*.log.config

    CleanFolder $testPackageFolderLinux true

    ProgressEnd 'Creating Linux Test Package'
}

PackageTestsWindows()
{
    ProgressStart 'Creating Windows Test Package'

    rm -rf $testPackageFolderWindows

    echo "Copying Binaries"
    cp -r $testPackageFolder $testPackageFolderWindows

    if [ $runtime = "dotnet" ] ; then
        $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolderWindows
    else
        mono $nuget install NUnit.ConsoleRunner -Version 3.10.0 -Output $testPackageFolderWindows
    fi

    cp ./test.sh $testPackageFolderWindows

    echo "Removing Sonarr.Mono"
    rm -f $testPackageFolderWindows/Sonarr.Mono.*

    rm -f $testPackageFolderWindows/*.log.config

    CleanFolder $testPackageFolderWindows true

    ProgressEnd 'Creating Windows Test Package'
}

PackageWindows()
{
    ProgressStart 'Creating Windows Package'

    rm -rf $outputFolderWindows
    
    echo "Copying Binaries"
    cp -r $outputFolder $outputFolderWindows

    echo "Removing Sonarr.Mono"
    rm -f $outputFolderWindows/Sonarr.Mono.*

    echo "Adding Sonarr.Windows to UpdatePackage"
    cp $outputFolderWindows/Sonarr.Windows.* $outputFolderWindows/$updateSubFolder/

    ProgressEnd 'Creating Windows Package'
}

PublishArtifacts()
{
    ProgressStart 'Publishing Artifacts'

    # Tests
    echo "##teamcity[publishArtifacts '$testPackageFolderWindows/** => tests.windows.zip']"
    echo "##teamcity[publishArtifacts '$testPackageFolderLinux/** => tests.linux.zip']"

    # Releases
    echo "##teamcity[publishArtifacts '$outputFolderWindows/** => Sonarr.$BRANCH.$BUILD_NUMBER.windows.zip!Sonarr']"
    echo "##teamcity[publishArtifacts '$outputFolderLinux/** => Sonarr.$BRANCH.$BUILD_NUMBER.linux.tar.gz!Sonarr']"
    echo "##teamcity[publishArtifacts '$outputFolderMacOS/** => Sonarr.$BRANCH.$BUILD_NUMBER.macos.tar.gz!Sonarr']"
    echo "##teamcity[publishArtifacts '$outputFolderMacOSApp/** => Sonarr.$BRANCH.$BUILD_NUMBER.macos.zip']"
    
    # Debian Package
    echo "##teamcity[publishArtifacts 'distribution/** => distribution.zip']"
    
    ProgressEnd 'Publishing Artifacts'
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

UpdateVersionNumber
Build
CreateReleaseInfo
RunGulp
PackageMono
PackageMacOS
PackageMacOSApp
PackageTestsMono
PackageTestsWindows
PackageWindows
PublishArtifacts
