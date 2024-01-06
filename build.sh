#! /usr/bin/env bash
set -e

outputFolder='_output'
testPackageFolder='_tests'
artifactsFolder="_artifacts";
framework="${FRAMEWORK:=net6.0}"

ProgressStart()
{
    echo "::group::$1"
    echo "Start '$1'"
}

ProgressEnd()
{
    echo "Finish '$1'"
    echo "::endgroup::"
}

UpdateVersionNumber()
{
    if [ "$SONARR_VERSION" != "" ]; then
        echo "Updating version info to: $SONARR_VERSION"
        sed -i'' -e "s/<AssemblyVersion>[0-9.*]\+<\/AssemblyVersion>/<AssemblyVersion>$SONARR_VERSION<\/AssemblyVersion>/g" src/Directory.Build.props
        sed -i'' -e "s/<AssemblyConfiguration>[\$()A-Za-z-]\+<\/AssemblyConfiguration>/<AssemblyConfiguration>${BRANCH}<\/AssemblyConfiguration>/g" src/Directory.Build.props
        sed -i'' -e "s/<string>10.0.0.0<\/string>/<string>$SONARR_VERSION<\/string>/g" distribution/macOS/Sonarr.app/Contents/Info.plist
    fi
}

EnableExtraPlatformsInSDK()
{
    BUNDLEDVERSIONS="${SDK_PATH}/Microsoft.NETCoreSdk.BundledVersions.props"
    if grep -q freebsd-x64 "$BUNDLEDVERSIONS"; then
        echo "Extra platforms already enabled"
    else
        echo "Enabling extra platform support"
        sed -i.ORI 's/osx-x64/osx-x64;freebsd-x64/' "$BUNDLEDVERSIONS"
    fi
}

EnableExtraPlatforms()
{
    if grep -qv freebsd-x64 src/Directory.Build.props; then
        sed -i'' -e "s^<RuntimeIdentifiers>\(.*\)</RuntimeIdentifiers>^<RuntimeIdentifiers>\1;freebsd-x64</RuntimeIdentifiers>^g" src/Directory.Build.props
    fi
}

LintUI()
{
    ProgressStart 'ESLint'
    yarn lint
    ProgressEnd 'ESLint'

    ProgressStart 'Stylelint'
    yarn stylelint
    ProgressEnd 'Stylelint'
}

Build()
{
    ProgressStart 'Build'

    rm -rf $outputFolder
    rm -rf $testPackageFolder

    slnFile=src/Sonarr.sln

    if [ $os = "windows" ]; then
        platform=Windows
    else
        platform=Posix
    fi

    dotnet clean $slnFile -c Debug
    dotnet clean $slnFile -c Release

    if [[ -z "$RID" || -z "$FRAMEWORK" ]];
    then
        dotnet msbuild -restore $slnFile -p:Configuration=Release -p:Platform=$platform -t:PublishAllRids
    else
        dotnet msbuild -restore $slnFile -p:Configuration=Release -p:Platform=$platform -p:RuntimeIdentifiers=$RID -t:PublishAllRids
    fi

    ProgressEnd 'Build'
}

YarnInstall()
{
    ProgressStart 'yarn install'
    yarn install --frozen-lockfile --network-timeout 120000
    ProgressEnd 'yarn install'
}

RunWebpack()
{
    ProgressStart 'Running webpack'
    yarn run build --env production
    ProgressEnd 'Running webpack'
}

PackageFiles()
{
    local folder="$1"
    local framework="$2"
    local runtime="$3"

    rm -rf $folder
    mkdir -p $folder
    cp -r $outputFolder/$framework/$runtime/publish/* $folder
    cp -r $outputFolder/Sonarr.Update/$framework/$runtime/publish $folder/Sonarr.Update
    
    if [ "$FRONTEND" = "YES" ];
    then
        cp -r $outputFolder/UI $folder
    fi

    echo "Adding LICENSE"
    cp LICENSE.md $folder
}

PackageLinux()
{
    local framework="$1"
    local runtime="$2"

    ProgressStart "Creating $runtime Package for $framework"

    local folder=$artifactsFolder/$runtime/$framework/Sonarr

    PackageFiles "$folder" "$framework" "$runtime"

    echo "Removing Service helpers"
    rm -f $folder/ServiceUninstall.*
    rm -f $folder/ServiceInstall.*

    echo "Removing Sonarr.Windows"
    rm $folder/Sonarr.Windows.*

    echo "Adding Sonarr.Mono to UpdatePackage"
    cp $folder/Sonarr.Mono.* $folder/Sonarr.Update
    if [ "$framework" = "$framework" ]; then
        cp $folder/Mono.Posix.NETStandard.* $folder/Sonarr.Update
        cp $folder/libMonoPosixHelper.* $folder/Sonarr.Update
    fi

    ProgressEnd "Creating $runtime Package for $framework"
}

PackageMacOS()
{
    local framework="$1"
    local runtime="$2"

    ProgressStart "Creating $runtime Package for $framework"

    local folder=$artifactsFolder/$runtime/$framework/Sonarr

    PackageFiles "$folder" "$framework" "$runtime"

    echo "Removing Service helpers"
    rm -f $folder/ServiceUninstall.*
    rm -f $folder/ServiceInstall.*

    echo "Removing Sonarr.Windows"
    rm $folder/Sonarr.Windows.*

    echo "Adding Sonarr.Mono to UpdatePackage"
    cp $folder/Sonarr.Mono.* $folder/Sonarr.Update
    if [ "$framework" = "$framework" ]; then
        cp $folder/Mono.Posix.NETStandard.* $folder/Sonarr.Update
        cp $folder/libMonoPosixHelper.* $folder/Sonarr.Update
    fi

    ProgressEnd "Creating $runtime Package for $framework"
}

PackageMacOSApp()
{
    local framework="$1"
    local runtime="$2"

    ProgressStart "Creating $runtime App Package for $framework"

    local folder=$artifactsFolder/$runtime-app/$framework

    rm -rf $folder
    mkdir -p $folder
    cp -r distribution/macOS/Sonarr.app $folder
    mkdir -p $folder/Sonarr.app/Contents/MacOS

    echo "Copying Binaries"
    cp -r $artifactsFolder/$runtime/$framework/Sonarr/* $folder/Sonarr.app/Contents/MacOS

    echo "Removing Update Folder"
    rm -r $folder/Sonarr.app/Contents/MacOS/Sonarr.Update

    ProgressEnd "Creating $runtime App Package for $framework"
}

PackageWindows()
{
    local framework="$1"
    local runtime="$2"

    ProgressStart "Creating Windows Package for $framework"

    local folder=$artifactsFolder/$runtime/$framework/Sonarr

    PackageFiles "$folder" "$framework" "$runtime"
    cp -r $outputFolder/$framework-windows/$runtime/publish/* $folder

    echo "Removing Sonarr.Mono"
    rm -f $folder/Sonarr.Mono.*
    rm -f $folder/Mono.Posix.NETStandard.*
    rm -f $folder/libMonoPosixHelper.*

    echo "Adding Sonarr.Windows to UpdatePackage"
    cp $folder/Sonarr.Windows.* $folder/Sonarr.Update

    ProgressEnd "Creating Windows Package for $framework"
}

Package()
{
    local framework="$1"
    local runtime="$2"
    local SPLIT

    IFS='-' read -ra SPLIT <<< "$runtime"

    case "${SPLIT[0]}" in
        linux|freebsd*)
            PackageLinux "$framework" "$runtime"
            ;;
        win)
            PackageWindows "$framework" "$runtime"
            ;;
        osx)
            PackageMacOS "$framework" "$runtime"
            ;;
    esac
}

PackageTests()
{
    local framework="$1"
    local runtime="$2"

    ProgressStart "Creating $runtime Test Package for $framework"

    cp test.sh "$testPackageFolder/$framework/$runtime/publish"

    rm -f $testPackageFolder/$framework/$runtime/*.log.config

    ProgressEnd "Creating $runtime Test Package for $framework"
}

UploadTestArtifacts()
{
    local framework="$1"

    ProgressStart 'Publishing Test Artifacts'

    # Tests
    for dir in $testPackageFolder/$framework/*
    do
        local runtime=$(basename "$dir")
        echo "##teamcity[publishArtifacts '$testPackageFolder/$framework/$runtime/publish/** => tests.$runtime.zip']"
    done

    ProgressEnd 'Publishing Test Artifacts'
}

UploadArtifacts()
{
    local framework="$1"

    ProgressStart 'Publishing Artifacts'

    # Releases
    for dir in $artifactsFolder/*
    do
        local runtime=$(basename "$dir")

        echo "##teamcity[publishArtifacts '$artifactsFolder/$runtime/$framework/** => Sonarr.$BRANCH.$SONARR_VERSION.$runtime.zip']"
    done

    # Debian Package / Windows installer / macOS app
    echo "##teamcity[publishArtifacts 'distribution/** => distribution.zip']"

    ProgressEnd 'Publishing Artifacts'
}

UploadUIArtifacts()
{
    local framework="$1"

    ProgressStart 'Publishing UI Artifacts'

    # UI folder
    echo "##teamcity[publishArtifacts '$outputFolder/UI/** => UI.zip']"

    ProgressEnd 'Publishing UI Artifacts'
}

# Use mono or .net depending on OS
case "$(uname -s)" in
    CYGWIN*|MINGW32*|MINGW64*|MSYS*)
        # on windows, use dotnet
        os="windows"
        ;;
    *)
        # otherwise use mono
        os="posix"
        ;;
esac

POSITIONAL=()

if [ $# -eq 0 ]; then
    echo "No arguments provided, building everything"
    BACKEND=YES
    FRONTEND=YES
    PACKAGES=YES
    LINT=YES
    ENABLE_EXTRA_PLATFORMS=NO
    ENABLE_EXTRA_PLATFORMS_IN_SDK=NO
fi

while [[ $# -gt 0 ]]
do
key="$1"

case $key in
    --backend)
        BACKEND=YES
        shift # past argument
        ;;
    --enable-bsd|--enable-extra-platforms)
        ENABLE_EXTRA_PLATFORMS=YES
        shift # past argument
        ;;
    --enable-extra-platforms-in-sdk)
        ENABLE_EXTRA_PLATFORMS_IN_SDK=YES
        shift # past argument
        ;;
    -r|--runtime)
        RID="$2"
        shift # past argument
        shift # past value
        ;;
    -f|--framework)
        FRAMEWORK="$2"
        shift # past argument
        shift # past value
        ;;
    --frontend)
        FRONTEND=YES
        shift # past argument
        ;;
    --packages)
        PACKAGES=YES
        shift # past argument
        ;;
    --lint)
        LINT=YES
        shift # past argument
        ;;
    --all)
        BACKEND=YES
        FRONTEND=YES
        PACKAGES=YES
        LINT=YES
        shift # past argument
        ;;
    *)    # unknown option
        POSITIONAL+=("$1") # save it in an array for later
        shift # past argument
        ;;
esac
done
set -- "${POSITIONAL[@]}" # restore positional parameters

if [ "$ENABLE_EXTRA_PLATFORMS_IN_SDK" = "YES" ];
then
    EnableExtraPlatformsInSDK
fi

if [ "$BACKEND" = "YES" ];
then
    UpdateVersionNumber
    if [ "$ENABLE_EXTRA_PLATFORMS" = "YES" ];
    then
        EnableExtraPlatforms
    fi

    Build

    if [[ -z "$RID" || -z "$FRAMEWORK" ]];
    then
        PackageTests "$framework" "win-x64"
        PackageTests "$framework" "win-x86"
        PackageTests "$framework" "linux-x64"
        PackageTests "$framework" "linux-musl-x64"
        PackageTests "$framework" "osx-x64"
        if [ "$ENABLE_EXTRA_PLATFORMS" = "YES" ];
        then
            PackageTests "$framework" "freebsd-x64"
        fi
    else
        PackageTests "$FRAMEWORK" "$RID"
    fi

    UploadTestArtifacts "$framework"
fi

if [ "$FRONTEND" = "YES" ];
then
    YarnInstall

    if [ "$LINT" = "YES" ];
    then
        LintUI
    fi

    RunWebpack
    UploadUIArtifacts
fi

if [ "$PACKAGES" = "YES" ];
then
    UpdateVersionNumber

    if [[ -z "$RID" || -z "$FRAMEWORK" ]];
    then
        Package "$framework" "win-x64"
        Package "$framework" "win-x86"
        Package "$framework" "linux-x64"
        Package "$framework" "linux-musl-x64"
        Package "$framework" "linux-arm64"
        Package "$framework" "linux-musl-arm64"
        Package "$framework" "linux-arm"
        Package "$framework" "osx-x64"
        Package "$framework" "osx-arm64"
        if [ "$ENABLE_EXTRA_PLATFORMS" = "YES" ];
        then
            Package "$framework" "freebsd-x64"
        fi
    else
        Package "$FRAMEWORK" "$RID"
    fi

    UploadArtifacts "$framework"
fi
