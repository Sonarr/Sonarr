SET PACKAGEROOT=_rawPackage
SET TARGET=%PACKAGEROOT%\NzbDrone

xcopy IISExpress %TARGET%\IISExpress /E /V /I /Y
xcopy NzbDrone\bin\Release\*.*  %TARGET%\ /E /V /I /Y

del %TARGET%\nlog.xml
del %TARGET%\*.vshost.exe.*

xcopy NzbDrone.Web\bin\*.*  %TARGET%\NzbDrone.Web\bin\ /E /V /I /Y
xcopy NzbDrone.Web\App_GlobalResources\*.*  %TARGET%\NzbDrone.Web\App_GlobalResources\ /E /V /I /Y
xcopy NzbDrone.Web\Content\*.*  %TARGET%\NzbDrone.Web\Content\ /E /V /I /Y
xcopy NzbDrone.Web\Scripts\*.*  %TARGET%\NzbDrone.Web\Scripts\ /E /V /I /Y
xcopy NzbDrone.Web\Views\*.*  %TARGET%\NzbDrone.Web\Views\ /E /V /I /Y

del %TARGET%\NzbDrone.Web\bin\*.xml /q
del %TARGET%\NzbDrone.Web\bin\ninject*.pdb /q

xcopy NzbDrone.Web\log.config  %TARGET%\NzbDrone.Web\
xcopy NzbDrone.Web\Global.asax  %TARGET%\NzbDrone.Web\
xcopy NzbDrone.Web\favicon.ico  %TARGET%\NzbDrone.Web\
xcopy NzbDrone.Web\web.config  %TARGET%\NzbDrone.Web\ 


CD %PACKAGEROOT%
..\Libraries\7zip\7za.exe a -tzip ..\NzbDrone.zip *

CD ..
Pause