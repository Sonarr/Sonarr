SET PACKAGEROOT=_rawPackage
SET TARGET=%PACKAGEROOT%\NzbDrone
SET COPY_FLAGS=/S /V /I /Y
SET DELETE_FLAGS=/Q /F /S

rd %PACKAGEROOT% /S /Q
del nzbdrone*.zip %DELETE_FLAGS%
del _output\FluentValidation.resources.dll %DELETE_FLAGS%

echo ##teamcity[progressMessage 'Packaging release']

xcopy ServiceHelpers\ServiceInstall\bin\Release\*.exe  %TARGET%\ %COPY_FLAGS%
xcopy ServiceHelpers\ServiceUninstall\bin\Release\*.exe  %TARGET%\ %COPY_FLAGS%

xcopy _output\*.*  %TARGET%\ %COPY_FLAGS%
xcopy NzbDrone.Update\bin\Release\*.*  %TARGET%\NzbDrone.Update\ %COPY_FLAGS%

CD %PACKAGEROOT%

del *.xml %DELETE_FLAGS%
del *.vshost.exe.* %DELETE_FLAGS%

..\Libraries\7zip\7za.exe a -tzip ..\NzbDrone.zip *

CD ..

echo ##teamcity[progressMessage 'Release packaged']