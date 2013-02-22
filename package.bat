SET PACKAGEROOT=_rawPackage
SET TARGET=%PACKAGEROOT%\NzbDrone

rd %PACKAGEROOT% /S /Q
del nzbdrone*.zip /Q /F

echo ##teamcity[progressMessage 'Packaging release']

xcopy IISExpress %TARGET%\IISExpress /E /V /I /Y


xcopy ServiceHelpers\ServiceInstall\bin\Release\*.exe  %TARGET%\ /E /V /I /Y
xcopy ServiceHelpers\ServiceUninstall\bin\Release\*.exe  %TARGET%\ /E /V /I /Y

xcopy NzbDrone\bin\Debug\*.*  %TARGET%\ /E /V /I /Y
xcopy NzbDrone\bin\Release\*.*  %TARGET%\ /E /V /I /Y

xcopy NzbDrone.Update\bin\Debug\*.*  %TARGET%\NzbDrone.Update\ /E /V /I /Y
xcopy NzbDrone.Update\bin\Release\*.*  %TARGET%\NzbDrone.Update\ /E /V /I /Y


CD %PACKAGEROOT%

del nlog.xml /Q /F /S
del nlog.pdb /Q /F /S
del Twitterizer2.pdb /Q /F /S
del *.vshost.exe.* /Q /F /S
del ninject*.pdb /Q /F /S
del ninject*.xml /Q /F /S
del nlog.pdb /Q /F /S
del Newtonsoft.Json.xml /Q /F /S
del Newtonsoft.Json.pdb /Q /F /S
del Mvc*.pdb /Q /F /S

del *debug.js /Q /F /S
del *-vsdoc.js /Q /F /S


..\Libraries\7zip\7za.exe a -tzip ..\NzbDrone.zip *

CD ..

echo ##teamcity[progressMessage 'Release packaged']