REM SET BUILD_NUMBER=1
REM SET BRANCH=develop
echo ##teamcity[progressStart 'Building setup file']
inno\ISCC.exe nzbdrone.iss
echo ##teamcity[progressFinish 'Building setup file']

echo ##teamcity[publishArtifacts 'setup\output\*.exe']
