echo "Debian Build Dev bootstrap..."

export TEST_OUTPUT=/data/output

mkdir ${TEST_OUTPUT}

mkdir /data/temp

cp -rf /data/build/debian.sh /data/temp
cp -rf /data/build/debian /data/temp
cp -rf /data/sonarr_bin /data/temp/sonarr_bin

cd /data/temp

ls -al .

fromdos debian.sh
sh debian.sh
