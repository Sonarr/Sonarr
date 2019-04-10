# Sonarr

Sonarr is a PVR for Usenet and BitTorrent users. It can monitor multiple RSS feeds for new episodes of your favorite shows and will grab, sort and rename them. It can also be configured to automatically upgrade the quality of files already downloaded when a better quality format becomes available.

## Major Features Include:

* Support for major platforms: Windows, Linux, macOS, Raspberry Pi, etc.
* Automatically detects new episodes
* Can scan your existing library and download any missing episodes
* Can watch for better quality of the episodes you already have and do an automatic upgrade. *eg. from DVD to Blu-Ray*
* Automatic failed download handling will try another release if one fails
* Manual search so you can pick any release or to see why a release was not downloaded automatically
* Fully configurable episode renaming
* Full integration with SABnzbd and NZBGet
* Full integration with Kodi, Plex (notification, library update, metadata)
* Full support for specials and multi-episode releases
* And a beautiful UI

## Configuring Development Environment:

### Requirements

* [Visual Studio 2017] (https://www.visualstudio.com/vs/)
* [Git](https://git-scm.com/downloads)
* [NodeJS](https://nodejs.org/en/download/)
* [Yarn](https://yarnpkg.com/)

### Setup

* Make sure all the required software mentioned above are installed
* Clone the repository into your development machine. [*info*](https://help.github.com/en/articles/working-with-forks)
* Grab the submodules `git submodule init && git submodule update`
* Install the required Node Packages `yarn`

### Backend Development

* Run `yarn build` to build the UI
* Open `Sonarr.sln` in Visual Studio
* Make sure `NzbDrone.Console` is set as the startup project
* Build `NzbDrone.Windows` and `NzbDrone.Mono` projects
* Build Solution

### UI Development

* Run `yarn watch` to build UI and rebuild automatically when changes are detected
* Run Sonarr.Console.exe (or debug in Visual Studio)

### License


* [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
* Copyright 2010-2019

### Sponsors

* [JetBrains](http://www.jetbrains.com/) for providing us with free licenses to their great tools
    * [ReSharper](http://www.jetbrains.com/resharper/)
    * [TeamCity](http://www.jetbrains.com/teamcity/)
