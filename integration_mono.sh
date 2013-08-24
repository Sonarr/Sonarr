EXCLUDE="-exclude:Windows -include:IntegrationTest"
TESTDIR="."
NUNIT="$TESTDIR/NUnit.Runners.2.6.1/tools/nunit-console-x86.exe"

mono $NUNIT $EXCLUDE  -xml:NzbDrone.Api.Result.xml $TESTDIR/NzbDrone.Api.Test.dll
mono $NUNIT $EXCLUDE  -xml:NzbDrone.Core.Result.xml $TESTDIR/NzbDrone.Core.Test.dll
mono $NUNIT $EXCLUDE  -xml:NzbDrone.Integration.Result.xml $TESTDIR/NzbDrone.Integration.Test.dll