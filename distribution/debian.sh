fromdos ./debian/*

BuildVersion=${dependent_build_number:-3.10.0.999}
BuildBranch=${dependent_build_branch:-master}

echo Version: "$BuildVersion" Branch: "$BuildBranch"

rm -r ./sonarr_bin/Sonarr.Update
rm ./sonarr_bin/System.Runtime.InteropServices.RuntimeInformation.dll
rm ./sonarr_bin/UI/*.map ./sonarr_bin/UI/Content/*.map
chmod -R ugo-x,ugo+rwX,go-w ./sonarr_bin/*

echo Updating changelog for $BuildVersion
sed -i "s:{version}:$BuildVersion:g; s:{branch}:$BuildBranch:g;" debian/changelog

echo Running debuild for $BuildVersion
debuild -b

echo Moving stuff around
mv ../sonarr_*.deb ./
mv ../sonarr_*.changes ./
rm ../sonarr_*.build

echo Signing Package
dpkg-sig -k 884589CE --sign builder "sonarr_${BuildVersion}_all.deb"

echo running alien
alien -r -v ./*.deb
