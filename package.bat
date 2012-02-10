SET PACKAGEROOT=_rawPackage
SET TARGET=%PACKAGEROOT%\NzbDrone

rd %TARGET% /S /Q
del nzbdrone*.zip /Q /F



xcopy IISExpress %TARGET%\IISExpress /E /V /I /Y


xcopy ServiceHelpers\ServiceInstall\bin\Release\*.exe  %TARGET%\ /E /V /I /Y
xcopy ServiceHelpers\ServiceUninstall\bin\Release\*.exe  %TARGET%\ /E /V /I /Y

xcopy NzbDrone\bin\Debug\*.*  %TARGET%\ /E /V /I /Y
xcopy NzbDrone\bin\Release\*.*  %TARGET%\ /E /V /I /Y

xcopy NzbDrone.Update\bin\Debug\*.*  %TARGET%\NzbDrone.Update\ /E /V /I /Y
xcopy NzbDrone.Update\bin\Release\*.*  %TARGET%\NzbDrone.Update\ /E /V /I /Y

xcopy NzbDrone.Web\bin\*.*  %TARGET%\NzbDrone.Web\bin\ /E /V /I /Y
xcopy NzbDrone.Web\App_GlobalResources\*.*  %TARGET%\NzbDrone.Web\App_GlobalResources\ /E /V /I /Y
xcopy NzbDrone.Web\Content\*.*  %TARGET%\NzbDrone.Web\Content\ /E /V /I /Y
xcopy NzbDrone.Web\Scripts\*.*  %TARGET%\NzbDrone.Web\Scripts\ /E /V /I /Y
xcopy NzbDrone.Web\Views\*.*  %TARGET%\NzbDrone.Web\Views\ /E /V /I /Y

del %TARGET%\NzbDrone.Web\bin\*.xml /Q /F



xcopy NzbDrone.Web\log.config  %TARGET%\NzbDrone.Web\
xcopy NzbDrone.Web\Global.asax  %TARGET%\NzbDrone.Web\
xcopy NzbDrone.Web\favicon.ico  %TARGET%\NzbDrone.Web\
xcopy NzbDrone.Web\web.config  %TARGET%\NzbDrone.Web\ 


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

..\Libraries\7zip\7za.exe a -tzip ..\NzbDrone.zip *

CD ..