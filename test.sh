PLATFORM=$1
TYPE=$2
WHERE=""
TEST_DIR="."
TEST_PATTERN="*Test.dll"
NUNIT="$TEST_DIR/NUnit.ConsoleRunner.3.2.0/tools/nunit3-console.exe"
NUNIT_COMMAND="$NUNIT"
NUNIT_PARAMS="--teamcity"

if [ "$PLATFORM" = "Windows" ]; then
  WHERE="cat != LINUX"
elif [ "$PLATFORM" = "Linux" ]; then
  WHERE="cat != WINDOWS"
  NUNIT_COMMAND="mono --debug --runtime=v4.0 $NUNIT"
else
  echo "Platform must be provided: Windows or Linux"
  exit 1
fi

if [ "$TYPE" = "Unit" ]; then
  WHERE="$WHERE && cat != IntegrationTest && cat != AutomationTest"
elif [ "$TYPE" = "Integration" ] ; then
  WHERE="$WHERE && cat == IntegrationTest"
elif [ "$TYPE" = "Automation" ] ; then
  WHERE="$WHERE && cat == AutomationTest"
else
  echo "Type must be provided: Unit, Integration or Automation"
  exit 2
fi

for i in `find $TEST_DIR -name "$TEST_PATTERN"`;
  do $NUNIT_COMMAND --where "$WHERE" $NUNIT_PARAMS $i;
done
