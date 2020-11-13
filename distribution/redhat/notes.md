# Fedora Package #

## How to build ##

    Copy Sonarr.phantom-develop.3.0.4.994.linux.tar.gz into distribution/redhat
    export dependent_build_number=3.0.4.994
    export dependent_build_branch=develop
    cd distribution
    ./redhat.sh

## Prep work to build ##

Doesn't have to be "builduser". Any non-root user in the "mock" group will do.

### Prep work to build on Debian ###

I started with a basic barebones Debian 10.6 system, and here's full steps I
did:
1. apt install git wget
2. apt install mock
3. apt install creatrerepo # Only for yumrepo, not package builds
4. useradd -m -G mock builduser # Name not important, but must be in mock group
5. su -s /bin/bash --login builduser
6. git clone https://github.com/freiheit/Sonarr.git

## Prep work to build on Ubuntu ##

Still figuring this out...  No mock or createrepo packages readily available
here. Is there a ppa or snap? Use container on Ubuntu?

### Prep work to build on RedHat (tested on CentOS8) ###
1. yum install mock wget
2. useradd -m -G mock builduser
3. su -s /bin/bash --login builduser
4. git clone https://github.com/freiheit/Sonarr.git

## Other ##

A firewalld rule has been provided (sonarr.firewalld) but not packaged.
