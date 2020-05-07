#!/bin/bash
# -fobjc-arc: enables ARC
# -fmodules: enables modules so you can import with `@import AppKit;`
# -mmacosx-version-min=10.6: support older OS X versions, this might increase the binary size

if [ ! -d "../dist" ]; then mkdir ../dist; fi

clang PFMoveApplication.m -fno-objc-arc -fmodules -mmacosx-version-min=10.6 -c -o PFMoveApplication.o
clang run-with-mono.m Launcher.m PFMoveApplication.o  -fobjc-arc -fmodules -mmacosx-version-min=10.6 -o ../dist/Launcher
rm PFMoveApplication.o

if [ "$1" == "install" ] && [ "$2" != "" ]; then
    echo "Installing to $2"
    cp ../dist/Launcher $2
    chmod +x $2
fi
