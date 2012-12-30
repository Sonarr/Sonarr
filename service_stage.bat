SET TARGET=_rawPackage_service

rd %TARGET% /S /Q

xcopy NzbDrone.Services\NzbDrone.Services.Service\bin\*.*  %TARGET%\bin\ /E /V /I /Y /F /O
xcopy NzbDrone.Services\NzbDrone.Services.Service\Content\*.*  %TARGET%\Content\ /E /V /I /Y /F /O
xcopy NzbDrone.Services\NzbDrone.Services.Service\Scripts\*.*  %TARGET%\Scripts\ /E /V /I /Y /F /O
xcopy NzbDrone.Services\NzbDrone.Services.Service\Views\*.*  %TARGET%\Views\ /E /V /I /Y /F /O
xcopy NzbDrone.Services\NzbDrone.Services.Service\log.config  %TARGET% /S /V /I /Y /F /O
xcopy NzbDrone.Services\NzbDrone.Services.Service\Global.asax  %TARGET% /S /V /I /Y /F /O
xcopy service_deploy_production.bat  %TARGET% /O /Y
 
Libraries\CTT\ctt.exe  source:"NzbDrone.Services\NzbDrone.Services.Service\Web.config" transform:"NzbDrone.Services\NzbDrone.Services.Service\Web.Stage.config" destination:"%TARGET%\Web.config"
Libraries\CTT\ctt.exe  source:"NzbDrone.Services\NzbDrone.Services.Service\Web.config" transform:"NzbDrone.Services\NzbDrone.Services.Service\Web.Release.config" destination:"%TARGET%\Web.production.config"

CD %TARGET%

del nlog.xml /Q /F /S
del nlog.pdb /Q /F /S
del ninject*.pdb /Q /F /S
del ninject*.xml /Q /F /S
del Mvc*.pdb /Q /F /S
del bin\*.xml /Q /F /S

cd ..

rd C:\inetpub\services_stage /S /Q

xcopy _rawPackage_service\*.*  C:\inetpub\stage-services.nzbdrone.com /E /V /I /Y


