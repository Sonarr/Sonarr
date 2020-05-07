fromdos ./debian/*
cp -r ./debian ./debian_backup

BuildVersion=${dependent_build_number:-3.10.0.999}
BuildBranch=${dependent_build_branch:-master}
BootstrapVersion=`echo "$BuildVersion" | cut -d. -f1,2,3`
BootstrapUpdater="BuiltIn"
PackageUpdater="apt"

echo Version: "$BuildVersion" Branch: "$BuildBranch"

rm -r ./sonarr_bin/Sonarr.Update
chmod -R ugo-x,ugo+rwX,go-w ./sonarr_bin/*

echo Updating changelog for $BuildVersion
sed -i "s:{version}:$BuildVersion:g; s:{branch}:$BuildBranch:g;" debian/changelog
sed -i "s:{version}:$BuildVersion:g; s:{updater}:$PackageUpdater:g" debian/preinst debian/postinst debian/postrm
sed -i '/#BEGIN BUILTIN UPDATER/,/#END BUILTIN UPDATER/d' debian/preinst debian/postinst debian/postrm
echo "# Do Not Edit\nPackageVersion=$BuildVersion\nPackageAuthor=[Team Sonarr](https://sonarr.tv)\nReleaseVersion=$BuildVersion\nUpdateMethod=$PackageUpdater\nBranch=$BuildBranch" > package_info

echo Running debuild for $BuildVersion
if [ -z "${TEST_OUTPUT}" ]; then
    debuild -b
else
    debuild -us -uc -b
fi

# Restore debian directory to the original files
rm -rf ./debian
mv ./debian_backup ./debian

echo Updating changelog for $BootstrapVersion
sed -i "s:{version}:$BootstrapVersion:g; s:{branch}:$BuildBranch:g;" debian/changelog
sed -i "s:{version}:$BuildVersion:g; s:{updater}:$BootstrapUpdater:g" debian/preinst debian/postinst debian/postrm
sed -i '/#BEGIN BUILTIN UPDATER/d; /#END BUILTIN UPDATER/d' debian/preinst debian/postinst debian/postrm
echo "# Do Not Edit\nPackageVersion=$BootstrapVersion\nPackageAuthor=[Team Sonarr](https://sonarr.tv)\nReleaseVersion=$BuildVersion\nUpdateMethod=$BootstrapUpdater\nBranch=$BuildBranch" > package_info

echo Running debuild for $BootstrapVersion
if [ -z "${TEST_OUTPUT}" ]; then
    debuild -b
else
    debuild -us -uc -b
fi

echo Moving stuff around
mv ../sonarr_*.deb ./
mv ../sonarr_*.changes ./
rm ../sonarr_*.build

if [ -z "${TEST_OUTPUT}" ]; then
    echo Signing Package
    dpkg-sig -k 884589CE --sign builder "sonarr_${BuildVersion}_all.deb"
    dpkg-sig -k 884589CE --sign builder "sonarr_${BootstrapVersion}_all.deb"

    echo running alien
    alien -r -v ./*.deb
else
    echo "Exporting packages to ${TEST_OUTPUT}"
    dpkg -e "sonarr_${BuildVersion}_all.deb" ${TEST_OUTPUT}/sonarr-build
    dpkg -e "sonarr_${BootstrapVersion}_all.deb" ${TEST_OUTPUT}/sonarr-release

    cp *.deb ${TEST_OUTPUT}/
fi
