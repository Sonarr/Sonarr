# NzbDrone #


NZBDrone is a PVR for newsgroup users. It can monitor multiple RSS feeds for new episodes of your favourite shows and will grab, sorts and renames them. It can also be configured to automatically upgrade the quality of files already downloaded if a better quality format becomes available.

## Major Features Include: ##

* Support for major platforms: Windows, Linux, OSX
* Automatically detects new episodes
* can scan your existing library and then download any old seasons that are missing
* can watch for better versions and upgrade your existing episodes. *eg. from DVD to Blu-Ray*
* automatic failed download handling
* manual search so you can pick any release or to see why a release was rejected
* fully configurable episode renaming
* full integration with SABNzbd and NzbGet
* full integration with XBMC,Plex (notification, library update, metadata)
* full support for specials and multi-episode releases
* beautiful UI


## Configuring Development Enviroment: ##

### Requirements ###
- Visual Studio 2013 ([Express Edition](http://www.microsoft.com/visualstudio/eng/products/visual-studio-express-for-web "Express Edition") might work but not tested.)
- [Git](http://git-scm.com/downloads)
- [NodeJS](http://nodejs.org/download/)
- [Gulp](http://gulpjs.com)

### Setup ###

- Make sure all the required software mentioned above are installed.
- Clone the repository into your development machine. [*info*](https://help.github.com/articles/working-with-repositories)
- install the required Node Packages `npm install`
- install gulp `npm install gulp -g`
- start gulp to monitor your dev environment for any changes that need post processing using `gulp watch` command.

*Please note gulp must be running at all times while you are working with NzbDrone client source files.*


### Development ###
- Open `NzbDrone.sln` in Visual Studio
- Make sure `NzbDrone.Console` is set as the startup project


### License ###
* [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
Copyright 2010-2014


### Sponsors ###
- [JetBrains](http://www.jetbrains.com/) for providing us with free licenses to their great tools
	- [ReSharper](http://www.jetbrains.com/resharper/)
	- [WebStorm](http://www.jetbrains.com/webstorm/)
	- [TeamCity](http://www.jetbrains.com/teamcity/)
