#!/bin/bash
set -e

FRAMEWORK="net10.0"
PLATFORM=$1
ARCHITECTURE="${2:-x64}"

if [ "$PLATFORM" = "Windows" ]; then
  RUNTIME="win-$ARCHITECTURE"
elif [ "$PLATFORM" = "Linux" ]; then
  RUNTIME="linux-$ARCHITECTURE"
elif [ "$PLATFORM" = "Mac" ]; then
  RUNTIME="osx-$ARCHITECTURE"
else
  echo "Platform must be provided as first argument: Windows, Linux or Mac"
  exit 1
fi

outputFolder='_output'
testPackageFolder='_tests'

rm -rf $outputFolder
rm -rf $testPackageFolder

slnFile=src/Sonarr.sln
outputFile=src/Sonarr.Api.V5/openapi.json
platform=Posix

if [ "$PLATFORM" = "Windows" ]; then
  application=Sonarr.Console.dll
else
  application=Sonarr.dll
fi

dotnet clean $slnFile -c Debug
dotnet clean $slnFile -c Release

dotnet msbuild -restore $slnFile -p:Configuration=Debug -p:Platform=$platform -p:RuntimeIdentifiers=$RUNTIME -t:PublishAllRids

dotnet new tool-manifest
dotnet tool install --version 10.1.0 Swashbuckle.AspNetCore.Cli

# Remove the openapi.json file so we can check if it was created
rm $outputFile

dotnet tool run swagger tofile --output ./src/Sonarr.Api.V5/openapi.json "$outputFolder/$FRAMEWORK/$RUNTIME/$application" v5 &

sleep 45

kill %1

if [ ! -f $outputFile ]; then
  echo "$outputFile not found, check logs for errors"
  exit 1
fi

exit 0
