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
rpmbuild -D "_topdir $(pwd)" -D "_sourcedir $(pwd)" -D "_builddir $(pwd)" -D "_rpmdir $(pwd)" -D "_specdir $(pwd)" -D "_srcrpmdir $(pwd)" -ba sonarr-3.0.4.994-develop.spec

mv */*.rpm ./

echo === Checking built RPMs with rpmlint
rpmlint *.rpm | egrep -v 'dir-or-file-in-opt'
