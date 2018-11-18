fromdos ./debian/*
cp -r ./debian ./debian_backup

BuildVersion=${dependent_build_number:-3.10.0.999}
BuildBranch=${dependent_build_branch:-master}
BootstrapVersion=`echo "$BuildVersion" | cut -d. -f1,2,3`
BootstrapUpdater="BuiltIn"
PackageUpdater="apt"

echo Version: "$BuildVersion" Branch: "$BuildBranch"

rm -r ./sonarr_bin/Sonarr.Update
rm ./sonarr_bin/System.Runtime.InteropServices.RuntimeInformation.dll
rm ./sonarr_bin/UI/*.map ./sonarr_bin/UI/Content/*.map
chmod -R ugo-x,ugo+rwX,go-w ./sonarr_bin/*

echo Updating changelog for $BuildVersion
sed -i "s:{version}:$BuildVersion:g; s:{branch}:$BuildBranch:g;" debian/changelog
sed -i "s:{version}:$BuildVersion:g; s:{updater}:$PackageUpdater:g" debian/preinst debian/postinst debian/postrm
sed -i '/#BEGIN BUILTIN UPDATER/,/#END BUILTIN UPDATER/d' debian/preinst debian/postinst debian/postrm
echo "# Do Not Edit\nReleaseVersion=$BuildVersion\nBranch=$BuildBranch" > release_info
echo "# Do Not Edit\nPackageVersion=$BuildVersion\nReleaseVersion=$BuildVersion\nUpdateMethod=$PackageUpdater\nBranch=$BuildBranch" > package_info

echo Running debuild for $BuildVersion
debuild -b

# Restore debian directory to the original files
rm -rf ./debian
mv ./debian_backup ./debian

echo Updating changelog for $BootstrapVersion
sed -i "s:{version}:$BootstrapVersion:g; s:{branch}:$BuildBranch:g;" debian/changelog
sed -i "s:{version}:$BuildVersion:g; s:{updater}:$BootstrapUpdater:g" debian/preinst debian/postinst debian/postrm
sed -i '/#BEGIN BUILTIN UPDATER/d; /#END BUILTIN UPDATER/d' debian/preinst debian/postinst debian/postrm
echo "# Do Not Edit\nPackageVersion=$BootstrapVersion\nReleaseVersion=$BuildVersion\nUpdateMethod=$BootstrapUpdater\nBranch=$BuildBranch" > package_info

echo Running debuild for $BootstrapVersion
debuild -b

echo Moving stuff around
mv ../sonarr_*.deb ./
mv ../sonarr_*.changes ./
rm ../sonarr_*.build

echo Signing Package
dpkg-sig -k 884589CE --sign builder "sonarr_${BuildVersion}_all.deb"
dpkg-sig -k 884589CE --sign builder "sonarr_${BootstrapVersion}_all.deb"

echo running alien
alien -r -v ./*.deb
