NUNIT="./Libraries/nunit/nunit-console-x86.exe"
EXCLUDE="/exclude:Windows"
TESTDIR="."
mono $NUNIT $EXCLUDE  /xml:NzbDrone.Libraries.Result.xml $TESTDIR/NzbDrone.Libraries.Test.dll
mono $NUNIT $EXCLUDE  /xml:NzbDrone.Libraries.Common.Result.xm $TESTDIR/NzbDrone.Common.Test.dll