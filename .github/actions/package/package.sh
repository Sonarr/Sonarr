#!/bin/bash

outputFolder=_output
artifactsFolder=_artifacts
uiFolder="$outputFolder/UI"
framework="${FRAMEWORK:=net6.0}"

for runtime in _artifacts/*
do
  name="${runtime##*/}"
  folderName="$runtime/$framework/Sonarr"
  sonarrFolder="$folderName/Sonarr"
    
  echo "Creating package for $name"

  echo "Copying UI runtime"
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
    (cd $packageFolder; zip -rq "../../$artifactsFolder/$name-app.zip" ./Sonarr.app)
  fi

  echo "Packaging Artifact"
  if [[ "$name" == *"linux"* ]] || [[ "$name" == *"osx"* ]] || [[ "$name" == *"freebsd"* ]]; then
    tar -zcf "$artifactsFolder/$name.tar.gz" -C $folderName Sonarr
	fi
    
  if [[ "$name" == *"win"* ]]; then
    if [ "$RUNNER_OS" = "Windows" ]
      then
        7z a -tzip "$artifactsFolder/$name.zip" $folderName
      else
      (cd $folderName; zip -rq "../../$artifactsFolder/$name.zip" ./Sonarr)
    fi
	fi
done
