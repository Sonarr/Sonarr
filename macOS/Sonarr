#!/bin/sh

#get the bundle's MacOS directory full path
DIR=$(cd "$(dirname "$0")"; pwd)

#change these values to match your app
EXE_PATH="$DIR/Sonarr.exe"
APPNAME="Sonarr"
 
#set up environment
if [[ -x '/opt/local/bin/mono' ]]; then
    # Macports and mono-supplied installer path
    export PATH="/opt/local/bin:$PATH"
elif [[ -x '/usr/local/bin/mono' ]]; then
    # Homebrew-supplied path to mono
    export PATH="/usr/local/bin:$PATH"
fi

export DYLD_FALLBACK_LIBRARY_PATH="$DIR"

if [ -e /Library/Frameworks/Mono.framework ]; then
	MONO_FRAMEWORK_PATH=/Library/Frameworks/Mono.framework/Versions/Current
	export PATH="$MONO_FRAMEWORK_PATH/bin:$PATH"
	export DYLD_FALLBACK_LIBRARY_PATH="$DYLD_FALLBACK_LIBRARY_PATH:$MONO_FRAMEWORK_PATH/lib"
fi

if [[ -f '/opt/local/lib/libsqlite3.0.dylib' ]]; then
	export DYLD_FALLBACK_LIBRARY_PATH="/opt/local/lib:$DYLD_FALLBACK_LIBRARY_PATH"
fi

export DYLD_FALLBACK_LIBRARY_PATH="$DYLD_FALLBACK_LIBRARY_PATH:$HOME/lib:/usr/local/lib:/lib:/usr/lib"

#mono version check
REQUIRED_MAJOR=4
REQUIRED_MINOR=6
 
VERSION_TITLE="Cannot launch $APPNAME"
VERSION_MSG="$APPNAME requires Mono Runtime Environment(MRE) $REQUIRED_MAJOR.$REQUIRED_MINOR or later."
DOWNLOAD_URL="http://www.mono-project.com/download/#download-mac"
 
MONO_VERSION="$(mono --version | grep 'Mono JIT compiler version ' |  cut -f5 -d\ )"
# if [[ -o DEBUG ]]; then osascript -e "display dialog \"MONO_VERSION: $MONO_VERSION\""; fi


MONO_VERSION_MAJOR="$(echo $MONO_VERSION | cut -f1 -d.)"
MONO_VERSION_MINOR="$(echo $MONO_VERSION | cut -f2 -d.)"
if [ -z "$MONO_VERSION" ] \
    || [ $MONO_VERSION_MAJOR -lt $REQUIRED_MAJOR ] \
    || [ $MONO_VERSION_MAJOR -eq $REQUIRED_MAJOR -a $MONO_VERSION_MINOR -lt $REQUIRED_MINOR ] 
then
    osascript \
    -e "set question to display dialog \"$VERSION_MSG\" with title \"$VERSION_TITLE\" buttons {\"Cancel\", \"Download...\"} default button 2" \
    -e "if button returned of question is equal to \"Download...\" then open location \"$DOWNLOAD_URL\""
    echo "$VERSION_TITLE"
    echo "$VERSION_MSG"
    exit 1
fi
 
MONO_EXEC="exec mono --debug"
 
#run app using mono
$MONO_EXEC "$EXE_PATH"
