#!/bin/bash
set -e

FRAMEWORK="net8.0"
PLATFORM=$1

if [ "$PLATFORM" = "Windows" ]; then
  RUNTIME="win-x64"
elif [ "$PLATFORM" = "Linux" ]; then
  RUNTIME="linux-x64"
elif [ "$PLATFORM" = "Mac" ]; then
  RUNTIME="osx-x64"
else
  echo "Platform must be provided as first argument: Windows, Linux or Mac"
  exit 1
fi

outputFolder='_output'
testPackageFolder='_tests'

rm -rf $outputFolder
rm -rf $testPackageFolder

slnFile=src/Sonarr.sln

platform=Posix

dotnet clean $slnFile -c Debug
dotnet clean $slnFile -c Release

dotnet msbuild -restore $slnFile -p:Configuration=Debug -p:Platform=$platform -p:RuntimeIdentifiers=$RUNTIME -t:PublishAllRids

dotnet new tool-manifest
dotnet tool install --version 6.5.0 Swashbuckle.AspNetCore.Cli

dotnet tool run swagger tofile --output ./src/Sonarr.Api.V3/openapi.json "$outputFolder/$FRAMEWORK/$RUNTIME/Sonarr.dll" v3 &

sleep 30

kill %1

exit 0
