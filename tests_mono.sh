EXCLUDE="-exclude:Windows,IntegrationTest"
TESTDIR="."
NUNIT="$TESTDIR/NUnit.ConsoleRunner.3.2.0/tools/nunit3-console.exe"

mono --debug --runtime=v4.0 $NUNIT $EXCLUDE -xml:NzbDrone.Api.Result.xml $TESTDIR/NzbDrone.Api.Test.dll
mono --debug --runtime=v4.0 $NUNIT $EXCLUDE -xml:NzbDrone.Common.Result.xml $TESTDIR/NzbDrone.Common.Test.dll
mono --debug --runtime=v4.0 $NUNIT $EXCLUDE -xml:NzbDrone.Core.Result.xml $TESTDIR/NzbDrone.Core.Test.dll
mono --debug --runtime=v4.0 $NUNIT $EXCLUDE -xml:NzbDrone.Host.Result.xml $TESTDIR/NzbDrone.Host.Test.dll
mono --debug --runtime=v4.0 $NUNIT $EXCLUDE -xml:NzbDrone.Libraries.Result.xml $TESTDIR/NzbDrone.Libraries.Test.dll
mono --debug --runtime=v4.0 $NUNIT $EXCLUDE -xml:NzbDrone.Mono.Result.xml $TESTDIR/NzbDrone.Mono.Test.dll
