# Fedora Package #

## How to build ##

    rpmlint sonarr.spec # sanity check
    spectool -g sonarr.spec
    mock --buildsrpm --sources . --spec sonarr.spec --resultdir .
    mock --rebuild *src.rpm --resultdir .
    rpmlint !(*src).rpm # sanity check

The macros %{version} and %{Source0} are pulled from services.nzbdrone.com. Use the -D (--define) option of mock and rpmbuild to integrate with an external build system instead.

## In a fedora repository ##

The following fedora requirements must be met:

* Build from source, rather than a pre-built tarball
* Separate lib->/usr/lib/sonarr or /usr/lib/mono/sonarr (the GAC), data->/usr/share/sonarr
* Wait until fedora packages mono 3, most likely in F22
* Do not use /opt, which is a reserved directory

Also the runtime dependencies might be a little too inclusive and could use some refinement.

The first two most likely require upstream modifications.

See [Packaging:Mono](https://fedoraproject.org/wiki/Packaging:Mono) for more details.

## Other ##

A firewalld rule has been provided (sonarr.firewalld) but not packaged.

Before enabling automatic updates, change ownership of /opt/sonarr to user sonarr.

For the record the official [Xamarin repository](http://www.mono-project.com/docs/getting-started/install/linux/#centos-fedora-and-derivatives) works well on F21.
