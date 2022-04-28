#!/bin/sh

## TODO ##
# I (freiheit) did a few things to emulate my own lack of the real CI/CD
# environment that need to be done more properly in CI/CD:
# - Copy Sonarr.phantom-${BuildBranch}.${BuildVersion}.linux.tar.gz into ./redhat/
#
# Requirements:
# - rpmbuild installed
# - user this is run as in mock group
#   (note: can use that to escalate to root if can change spec)
# - ran from "distribution" directory

# Take BuildVersion from environment, then try for pulling latest version from services.sonarr.tv, then fallback to a default
echo === Working out version to build
if [ -n "${dependent_build_number}" ]; then
  echo === dependent_build_number=${dependent_build_number}
  BuildVersion=${dependent_build_number}
else
  echo === Attempting to ask services.sonarr.tv latest version
  BuildVersion=$(python3 -c 'import requests; print(requests.get("http://services.sonarr.tv/v1/update/main?version=3.0").json()["updatePackage"]["version"])')
  if [ -z "${BuildVersion}" ]; then
    echo === Falling back to default
    BuildVersion='3.0.4.1002'
  else
    echo === services.sonarr.tv said ${BuildVersion}
  fi
fi


COPR="true"
BuildBranch=${dependent_build_branch:-main}
BootstrapVersion=`echo "$BuildVersion" | cut -d. -f1,2,3`
BootstrapUpdater="BuiltIn"
SpecFile="sonarr-$BuildVersion-$BuildBranch.spec"
BootStrapSpecFile="sonarr-bootstrap-$BuildVersion-$BuildBranch.spec"

cd redhat || exit 1

echo === Generating spec file with BuildVersion/BuildBranch
(
  echo "%define BuildVersion $BuildVersion"
  echo "%define BuildBranch $BuildBranch"
  echo
  cat sonarr.spec
) > $SpecFile

echo === Generating bootstrap spec file with BuildVersion/BuildBranch
(
  echo "%define BuildVersion $BuildVersion"
  echo "%define BuildBranch $BuildBranch"
  echo
  cat sonarr-bootstrap.spec
) > $BootStrapSpecFile

echo === Updating RPM changelog
last_version=$(grep -A 2 "%changelog" $SpecFile | grep "^\*" | awk '{print $NF}')

current_version=$(rpmspec -P $SpecFile | grep "^Version:" | awk '{print $2}')-$(rpmspec -P $SpecFile | grep "^Release:" | awk '{print $2}')

if [ "$last_version" != "$current_version" ]; then
   echo "No changelog for current release"
   userstring=$(git log --pretty='format:%an <%ae>' -1)
   since_hash=$(git blame sonarr.spec | grep "\*" | grep "$last_version$" | awk '{print $1}')
   echo -e "* $(date +"%a %b %d %Y") $userstring - $current_version\n$(git log --pretty='format:- %s' $since_hash..HEAD)\n" | grep -v 'Updated changelog' > ./change
   sed -i "/%changelog/ r ./change" sonarr.spec
   echo "Added change to spec-file:"
   rm ./change
   git add sonarr.spec
   git commit -m 'Updated changelog'
   git push
fi

echo === Updating bootstrap RPM changelog
bootstrap_last_version=$(grep -A 2 "%changelog" $BootStrapSpecFile | grep "^\*" | awk '{print $NF}')

bootstrap_current_version=$(rpmspec -P $BootStrapSpecFile | grep "^Version:" | awk '{print $2}')-$(rpmspec -P $BootStrapSpecFile | grep "^Release:" | awk '{print $2}')

if [ "$last_version" != "$current_version" ]; then
   echo "No changelog for current release"
   userstring=$(git log --pretty='format:%an <%ae>' -1)
   since_hash=$(git blame sonarr-bootstrap.spec | grep "\*" | grep "$last_version$" | awk '{print $1}')
   echo -e "* $(date +"%a %b %d %Y") $userstring - $current_version\n$(git log --pretty='format:- %s' $since_hash..HEAD)\n" | grep -v 'Updated changelog' > ./change
   sed -i "/%changelog/ r ./change" sonarr-bootstrap.spec
   echo "Added change to spec-file:"
   rm ./change
   git add sonarr-bootstrap.spec
   git commit -m 'Updated changelog'
   git push
fi

echo === Re-Generating spec file with BuildVersion and updated ChangeLog
(
  echo "%define BuildVersion $BuildVersion"
  echo "%define BuildBranch $BuildBranch"
  echo
  cat sonarr.spec
) > $SpecFile

echo === Re-Generating bootstrap spec file with BuildVersion and updated ChangeLog
(
  echo "%define BuildVersion $BuildVersion"
  echo "%define BuildBranch $BuildBranch"
  echo
  cat sonarr-bootstrap.spec
) > $BootStrapSpecFile

echo === Checking spec with rpmlint
# Ignore failure
rpmlint $SpecFile || true

echo === Checking bootstrap spec with rpmlint
# Ignore failure
rpmlint $BootStrapSpecFile || true

echo === Fetch tarball if not present
if [ ! -e Sonarr.${BuildBranch}.${BuildVersion}.linux.tar.gz ]; then
  (
    wget https://download.sonarr.tv/v3/${BuildBranch}/${BuildVersion}/Sonarr.${BuildBranch}.${BuildVersion}.linux.tar.gz 
  )
fi

echo === Cleaning out old RPMS
rm -f *.rpm */*.rpm

if [ ".$COPR" == ".true" ]; then
  echo === Building SRPM and RPM
  rpmbuild -D "_topdir $(pwd)" -D "_sourcedir $(pwd)" -D "_builddir $(pwd)" -D "_rpmdir $(pwd)" -D "_specdir $(pwd)" -D "_srcrpmdir $(pwd)" --define "debug_package %{nil}" -ba $SpecFile

  echo === Uploading to copr to ask them to build for us
  copr build --nowait sonarr-v3-test sonarr-${BuildVersion}-*.src.rpm

  echo === Building bootstrap SRPM and RPM
  rpmbuild -D "_topdir $(pwd)" -D "_sourcedir $(pwd)" -D "_builddir $(pwd)" -D "_rpmdir $(pwd)" -D "_specdir $(pwd)" -D "_srcrpmdir $(pwd)" --define "debug_package %{nil}" -ba $BootStrapSpecFile

  echo === Uploading bootstrap to copr to ask them to build for us
  copr build --nowait sonarr-v3-test sonarr-bootstrap-${BuildVersion}-*.src.rpm
else
  echo === Building RPM
  rpmbuild -D "_topdir $(pwd)" -D "_sourcedir $(pwd)" -D "_builddir $(pwd)" -D "_rpmdir $(pwd)" -D "_specdir $(pwd)" -D "_srcrpmdir $(pwd)" --define "debug_package %{nil}" -bb $SpecFile

  echo === Building bootstrap RPM
  rpmbuild -D "_topdir $(pwd)" -D "_sourcedir $(pwd)" -D "_builddir $(pwd)" -D "_rpmdir $(pwd)" -D "_specdir $(pwd)" -D "_srcrpmdir $(pwd)" --define "debug_package %{nil}" -bb $BootStrapSpecFile
fi

echo === Put RPMs in more predictable place
mv */*.rpm ./

echo === Throw away any empty directories that rpmbuild added
find . -mindepth 1 -type d -print0 | xargs --null rmdir

echo === Checking built RPMs with rpmlint
rpmlint *.rpm | egrep -v 'dir-or-file-in-opt'
