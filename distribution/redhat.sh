#!/bin/sh

## TODO ##
# I (freiheit) did a few things to emulate my own lack of the real CI/CD
# environment that need to be done more properly in CI/CD:
# - Copy Sonarr.phantom-${BuildBranch}.${BuildVersion}.linux.tar.gz into ./redhat/
#
# Requirements:
# - mock and rpmbuild installed
# - user this is run as in mock group
#   (note: can use that to escalate to root if can change spec)
# - ran from "distribution" directory

BuildVersion=${dependent_build_number:-3.0.4.994}
BuildBranch=${dependent_build_branch:-develop}
BootstrapVersion=`echo "$BuildVersion" | cut -d. -f1,2,3`
BootstrapUpdater="BuiltIn"

echo === Checking spec with rpmlint
# Ignore failure
rpmlint redhat/sonarr.spec || true

echo === Fetch tarball if not present
if [ ! -e redhat/Sonarr.phantom-${BuildBranch}.${BuildVersion}.linux.tar.gz ]; then
  (
    cd redhat
    wget https://download.sonarr.tv/v3/phantom-${BuildBranch}/${BuildVersion}/Sonarr.phantom-${BuildBranch}.${BuildVersion}.linux.tar.gz 
  )
fi

echo === Building a .src.rpm package:
mock --buildsrpm --configdir=./redhat-mock-config -r epel-7-x86_64 --sources redhat  --spec redhat/sonarr.spec --resultdir=./ --define "BuildVersion $BuildVersion" --define "BuildBranch $BuildBranch"

echo === Building for CentOS/RHEL/EPEL 8:
mock -r epel-8-x86_64 --configdir=./redhat-mock-config sonarr-${BuildVersion}-*.src.rpm --resultdir=./  --define "BuildVersion $BuildVersion" --define "BuildBranch $BuildBranch" --define "dist .el8"

echo === Building for CentOS/RHEL/EPEL 7:
mock -r epel-7-x86_64 --configdir=./redhat-mock-config sonarr-${BuildVersion}-*.src.rpm --resultdir=./  --define "BuildVersion $BuildVersion" --define "BuildBranch $BuildBranch" --define "dist .el7"

echo === Building for CentOS/RHEL/EPEL 6:
mock -r epel-7-x86_64 --configdir=./redhat-mock-config sonarr-${BuildVersion}-*.src.rpm --resultdir=./  --define "BuildVersion $BuildVersion" --define "BuildBranch $BuildBranch" --define "dist .el6"

echo === Building for Fedora 34:
mock -r fedora-34-x86_64 --configdir=./redhat-mock-config sonarr-${BuildVersion}-*.src.rpm --resultdir=./  --define "BuildVersion $BuildVersion" --define "BuildBranch $BuildBranch" --define "dist .fc34"

echo === Building for Fedora 33:
mock -r fedora-33-x86_64 --configdir=./redhat-mock-config sonarr-${BuildVersion}-*.src.rpm --resultdir=./  --define "BuildVersion $BuildVersion" --define "BuildBranch $BuildBranch" --define "dist .fc33"

echo === Building for Fedora 32:
mock -r fedora-32-x86_64 --configdir=./redhat-mock-config sonarr-${BuildVersion}-*.src.rpm --resultdir=./  --define "BuildVersion $BuildVersion" --define "BuildBranch $BuildBranch" --define "dist .fc32"

echo === Building for Fedora Rawhide:
mock -r fedora-rawhide-x86_64 --configdir=./redhat-mock-config sonarr-${BuildVersion}-*.src.rpm --resultdir=./  --define "BuildVersion $BuildVersion" --define "BuildBranch $BuildBranch" --define "dist rawhide"

echo === Checking built RPMs with rpmlint
rpmlint *.rpm
