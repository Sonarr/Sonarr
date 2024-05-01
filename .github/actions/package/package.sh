#!/bin/bash

outputFolder=_output
artifactsFolder=_artifacts
uiFolder="$outputFolder/UI"
framework="${FRAMEWORK:=net8.0}"

rm -rf $artifactsFolder
mkdir $artifactsFolder

for runtime in _output/*
do
  name="${runtime##*/}"
  folderName="$runtime/$framework"
  sonarrFolder="$folderName/Sonarr"
  archiveName="Sonarr.$BRANCH.$SONARR_VERSION.$name"

  if [[ "$name" == 'UI' ]]; then
    continue
  fi
    
  echo "Creating package for $name"

  echo "Copying UI"
  cp -r $uiFolder $sonarrFolder
  
  echo "Setting permissions"
  find $sonarrFolder -name "ffprobe" -exec chmod a+x {} \;
  find $sonarrFolder -name "Sonarr" -exec chmod a+x {} \;
  find $sonarrFolder -name "Sonarr.Update" -exec chmod a+x {} \;
  
  if [[ "$name" == *"osx"* ]]; then
    echo "Creating macOS package"
      
    packageName="$name-app"
    packageFolder="$outputFolder/$packageName"
      
    rm -rf $packageFolder
    mkdir $packageFolder
      
    cp -r distribution/macOS/Sonarr.app $packageFolder
    mkdir -p $packageFolder/Sonarr.app/Contents/MacOS
      
    echo "Copying Binaries"
    cp -r $sonarrFolder/* $packageFolder/Sonarr.app/Contents/MacOS
      
    echo "Removing Update Folder"
    rm -r $packageFolder/Sonarr.app/Contents/MacOS/Sonarr.Update
              
    echo "Packaging macOS app Artifact"
    (cd $packageFolder; zip -rq "../../$artifactsFolder/$archiveName-app.zip" ./Sonarr.app)
  fi

  echo "Packaging Artifact"
  if [[ "$name" == *"linux"* ]] || [[ "$name" == *"osx"* ]] || [[ "$name" == *"freebsd"* ]]; then
    tar -zcf "./$artifactsFolder/$archiveName.tar.gz" -C $folderName Sonarr
	fi
    
  if [[ "$name" == *"win"* ]]; then
    if [ "$RUNNER_OS" = "Windows" ]
      then
        (cd $folderName; 7z a -tzip "../../../$artifactsFolder/$archiveName.zip" ./Sonarr)
      else
      (cd $folderName; zip -rq "../../../$artifactsFolder/$archiveName.zip" ./Sonarr)
    fi
	fi
done
