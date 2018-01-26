fromdos ./debian/*
echo Version: "$dependent_build_number" Branch: "$dependent_build_branch"

rm -r ./sonarr_bin/Sonarr.Update

echo Updating changelog
sed -i "s/{version}/$dependent_build_number/g" debian/changelog
sed -i "s/{branch}/$dependent_build_branch/g" debian/changelog

echo Running debuild
debuild -b

echo Moving stuff around
mv ../sonarr_*.deb ./
mv ../sonarr_*.changes ./
rm ../sonarr_*.build

echo Signing Package
dpkg-sig -k 884589CE --sign builder "sonarr_${dependent_build_number}_all.deb"

echo running alien
alien -r -v ./*.deb
