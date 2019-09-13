echo "Preparing Test..."
mkdir -p /data/test
cp -r /data/_tests_linux/* /data/test/
cp -r /data/_output_linux /data/test/bin

cd /data/test

runTest()
{
    bash test.sh Linux $1
    cp TestResult.xml /data/_tests_results/TestResult_$1.xml
}

runTest Integration
runTest Unit