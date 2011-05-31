SET TARGET=_deploy

rmdir /s "%TARGET%"
xcopy IISExpress %TARGET%\IISExpress /E /V /I /F /Y
xcopy NzbDrone\bin\Release\*.*  %TARGET%\ /E /V /I /F /Y

xcopy NzbDrone.Web\bin\*.*  %TARGET%\NzbDrone.Web\bin\ /E /V /I /F /Y
xcopy NzbDrone.Web\App_GlobalResources\*.*  %TARGET%\NzbDrone.Web\App_GlobalResources\ /E /V /I /F /Y
xcopy NzbDrone.Web\Content\*.*  %TARGET%\NzbDrone.Web\Content\ /E /V /I /F /Y
xcopy NzbDrone.Web\Scripts\*.*  %TARGET%\NzbDrone.Web\Scripts\ /E /V /I /F /Y
xcopy NzbDrone.Web\Views\*.*  %TARGET%\NzbDrone.Web\Views\ /E /V /I /F /Y


xcopy NzbDrone.Web\log.config  %TARGET%\NzbDrone.Web\
xcopy NzbDrone.Web\Global.asax  %TARGET%\NzbDrone.Web\
xcopy NzbDrone.Web\favicon.ico  %TARGET%\NzbDrone.Web\
xcopy NzbDrone.Web\web.config  %TARGET%\NzbDrone.Web\ 


CD "%TARGET%"
..\Libraries\7zip\7za.exe a -tzip   NzbDrone.%env.BUILDNUMBER%.zip *
..\Libraries\7zip\7za.exe t NzbDrone.zip 
cD ..