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

BuildVersion=${dependent_build_number:-3.0.4.994}
BuildBranch=${dependent_build_branch:-develop}
BootstrapVersion=`echo "$BuildVersion" | cut -d. -f1,2,3`
BootstrapUpdater="BuiltIn"
SpecFile="sonarr-$BuildVersion-$BuildBranch.spec"

cd redhat || exit 1

echo === Generating spec file with BuildVersion/BuildBranch
(
  echo "%define BuildVersion $BuildVersion"
  echo "%define BuildBranch $BuildBranch"
  echo
  cat sonarr.spec
) > $SpecFile

echo === Updating RPM changelog
last_version=$(grep -A 2 "%changelog" $SpecFile | grep "^\*" | awk '{print $NF}')

current_version=$(rpmspec -P $SpecFile | grep "^Version:" | awk '{print $2}')-$(rpmspec -P $SpecFile | grep "^Release:" | awk '{print $2}')

if [ "$last_version" != "$current_version" ]; then
   echo "No changelog for current release"
   userstring=$(git log --pretty='format:%an <%ae>' -1)
   since_hash=$(git blame sonarr.spec | grep "\*" | grep "$last_version$" | awk '{print $1}')
   echo -e "* $(date +"%a %b %d %Y") $userstring - $current_version\n$(git log --pretty='format:- %s' $since_hash..HEAD)\n" > ./change
   sed -i "/%changelog/ r ./change" sonarr.spec
   echo "Added change to spec-file:"
   rm ./change
   git add sonarr.spec
   git commit -m 'Updated changelog'
   git push
fi

echo === Generating spec file with BuildVersion and updated ChangeLog
(
  echo "%define BuildVersion $BuildVersion"
  echo "%define BuildBranch $BuildBranch"
  echo
  cat sonarr.spec
) > $SpecFile

echo === Checking spec with rpmlint
# Ignore failure
rpmlint $SpecFile || true

echo === Fetch tarball if not present
if [ ! -e Sonarr.phantom-${BuildBranch}.${BuildVersion}.linux.tar.gz ]; then
  (
    wget https://download.sonarr.tv/v3/phantom-${BuildBranch}/${BuildVersion}/Sonarr.phantom-${BuildBranch}.${BuildVersion}.linux.tar.gz 
  )
fi

echo === Cleaning out old RPMS
rm -f *.rpm */*.rpm

echo === Building RPM and SRPM
rpmbuild -D "_topdir $(pwd)" -D "_sourcedir $(pwd)" -D "_builddir $(pwd)" -D "_rpmdir $(pwd)" -D "_specdir $(pwd)" -D "_srcrpmdir $(pwd)" --define "debug_package %{nil}" -bb sonarr-3.0.4.994-develop.spec

echo === Put RPMs in more predictable place
mv */*.rpm ./

echo === Throw away any empty directories that rpmbuild added
find . -mindepth 1 -type d -print0 | xargs --null rmdir

echo === Checking built RPMs with rpmlint
rpmlint *.rpm | egrep -v 'dir-or-file-in-opt'
