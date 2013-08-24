EXCLUDE="-exclude:Windows,IntegrationTest"
TESTDIR="."
NUNIT="$TESTDIR/NUnit.Runners.2.6.1/tools/nunit-console-x86.exe"

mono $NUNIT $EXCLUDE  -xml:NzbDrone.Api.Result.xml $TESTDIR/NzbDrone.Api.Test.dll
mono $NUNIT $EXCLUDE  -xml:NzbDrone.Common.Result.xml $TESTDIR/NzbDrone.Common.Test.dll
mono $NUNIT $EXCLUDE  -xml:NzbDrone.Core.Result.xml $TESTDIR/NzbDrone.Core.Test.dll
mono $NUNIT $EXCLUDE  -xml:NzbDrone.Host.Result.xml $TESTDIR/NzbDrone.Host.Test.dll
mono $NUNIT $EXCLUDE  -xml:NzbDrone.Libraries.Result.xml $TESTDIR/NzbDrone.Libraries.Test.dll