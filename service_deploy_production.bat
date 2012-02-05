rd C:\inetpub\services /S /Q

xcopy C:\inetpub\services_stage\*.* C:\inetpub\services\ /E /V /I /Y /F /C /o
xcopy C:\inetpub\services\web.production.config c:\inetpub\services\web.config /o /y

pause