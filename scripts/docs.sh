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
dotnet tool install --version 10.3.1 Swashbuckle.AspNetCore.Cli

generateDocs() {
  local version=$1
  local outputFile=$2

  # Remove the openapi.json file so we can check if it was created
  rm -f $outputFile

  dotnet tool run swagger tofile --output ./$outputFile "$outputFolder/$FRAMEWORK/$RUNTIME/$application" $version &
  local pid=$!

  sleep 45

  pkill -f "_tofile --output ./$outputFile" || true
  kill $pid || true
  wait $pid || true

  if [ ! -f $outputFile ]; then
    echo "$outputFile not found, check logs for errors"
    exit 1
  fi
}

generateDocs v3 src/Sonarr.Api.V3/openapi.json
generateDocs v5 src/Sonarr.Api.V5/openapi.json

exit 0
