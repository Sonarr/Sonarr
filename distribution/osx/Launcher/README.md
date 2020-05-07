Code reused from duplicati, licensed under LGPL 2.1
Modified for Sonarr by Taloth Saldono

see here for the original source: https://github.com/duplicati/duplicati/tree/679981d29f8a6e445d3c1e6d41e72a673ffaa653/Installer/OSX

License
-------

Sonarr as a whole is licensed under GPL 3.0 as specified in the git repository root.

But to preserve the original intent of the duplicati project, the modified versions of the sources in this folder are dual licensed under LGPL 2.1 and GPL 3.0.
Note: This exception can be freely removed in any copy of Sonarr sources as per LGPL/GPL licensing terms.

A copy of the LGPL 2.1 license is included in the LICENSE.LGPL.md file.

Purpose
-------

The Launcher is a bootstrap/shim application that checks if the appropriate version of mono is installed and subsequently use it to execute Sonarr.
By using a separate application, instead of a shell script, this allows the user to assign certain operating system permissions to Sonarr specifically.

Compiling the Launcher
----------------------

You need an OSX installation with xcode
Then run compile.sh in a terminal

The generated dist/Launcher can be renamed to Sonarr and Sonarr.Update to serve as shims to run Sonarr.exe and Sonarr.Update.exe respectively.
