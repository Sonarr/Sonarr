@REM SET SONARR_MAJOR_VERSION=4
@REM SET SONARR_VERSION=4.0.0.5
@REM SET BRANCH=develop
@REM SET FRAMEWORK=net6.0
@REM SET RUNTIME=win-x64
echo ##teamcity[progressStart 'Building setup file']
inno\ISCC.exe sonarr.iss
echo ##teamcity[progressFinish 'Building setup file']

echo ##teamcity[publishArtifacts 'distribution\windows\setup\output\*%RUNTIME%*.exe']
