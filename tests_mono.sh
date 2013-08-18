EXCLUDE="/exclude:Windows"
TESTDIR="."
NUNIT="$TESTDIR/NUnit.Runners.2.6.1/tools/nunit-console-x86.exe"

mono $NUNIT $EXCLUDE  /xml:NzbDrone.Libraries.Result.xml $TESTDIR/NzbDrone.Libraries.Test.dll
mono $NUNIT $EXCLUDE  /xml:NzbDrone.Libraries.Common.Result.xm $TESTDIR/NzbDrone.Common.Test.dll