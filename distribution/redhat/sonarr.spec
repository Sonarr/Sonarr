Name:           sonarr
Version:        %{BuildVersion}

Release:        1.%{?BuildBranch}
BuildArch:      noarch
Summary:        PVR for Usenet and BitTorrent users

License:        GPLv3+
URL:            https://sonarr.tv/
Source0:        https://download.sonarr.tv/v3/phantom-%{BuildBranch}/%{BuildVersion}/Sonarr.phantom-%{BuildBranch}.%{version}.linux.tar.gz
Source3:        %{name}.systemd
Source4:        %{name}.firewalld
Source5:        %{name}-secure.firewalld

BuildRequires:      systemd

Requires:           sqlite-libs >= 3.7
Requires:           mediainfo >= 0.7.52
Requires:           mono-complete

Requires(pre):      shadow-utils
Requires(postun):   shadow-utils

Requires(post):     systemd
Requires(preun):    systemd
Requires(postun):   systemd

Provides: /opt/%{name}/Sonarr.exe

Conflicts: sonarr-bootstrap

# These prevent Sonarr's DLLs from auto-creating requires and provides
# Doing that because RH's mono require/provide detection isn't working
# right here 
# (thinks it requires a different version of a library than it provides type problems)
%global __provides_exclude_from ^/opt/%{name}/.*$
%global __requires_exclude_from ^/opt/%{name}/.*$

%description
Sonarr is a PVR for Usenet and BitTorrent users. It can monitor multiple RSS
feeds for new episodes of your favorite shows and will grab, sorts and renames
them. It can also be configured to automatically upgrade the quality of files
already downloaded when a better quality format becomes available.


%prep
%autosetup -n Sonarr

# Remove Updater
rm -rf Sonarr.Update

%build
# Empty build just to make rpmlint happier.
# This spec uses binaries built on windows by Sonarr team instead of
# attempting to build on Linux.

%install

# systemd service
install -m 0755 -d %{buildroot}%{_unitdir}
install -m 0644 %{SOURCE3} %{buildroot}%{_unitdir}/%{name}.service

# firewalld
install -m 0755 -d %{buildroot}%{_prefix}/lib/firewalld/services/
install -m 0644 %{SOURCE4} %{buildroot}%{_prefix}/lib/firewalld/services/%{name}.xml
install -m 0644 %{SOURCE5} %{buildroot}%{_prefix}/lib/firewalld/services/%{name}-secure.xml

# sonarr user in /var
install -m 0755 -d %{buildroot}%{_sharedstatedir}/%{name}

# sonarr software itself
install -m 0755 -d %{buildroot}/opt/%{name}



(
  echo "# Do Not Edit\n"
  echo "PackageVersion=%{version}"
  echo "PackageAuthor=[Team Sonarr](https://sonarr.tv)"
  echo "ReleaseVersion=%{version}"
  echo "UpdateMethod=yum"
  echo "Branch=phantom-%{BuildBranch}" 
) > %{buildroot}/opt/%{name}/package_info

mv * %{buildroot}/opt/%{name}

find %{buildroot}/opt/%{name} -type f -exec chmod 644 '{}' \;
find %{buildroot}/opt/%{name} -type d -exec chmod 755 '{}' \;


%files
%dir %{_unitdir}
%attr(0644,root,root) %{_unitdir}/%{name}.service

%dir %{_prefix}/lib/firewalld
%dir %{_prefix}/lib/firewalld/services
%attr(0644,root,root) %{_prefix}/lib/firewalld/services/*.xml

%dir /opt/%{name}
%attr(-,root,root) /opt/%{name}/*

%attr(-,sonarr,sonarr)%{_sharedstatedir}/%{name}


%pre
getent group %{name} >/dev/null || groupadd -r %{name}
getent passwd %{name} >/dev/null || \
    useradd -r -g %{name} -d d %{_sharedstatedir}/%{name} -s /sbin/nologin \
    -c "Sonarr PVR for Usenet and BitTorrent Users " %{name}
exit 0

%post
%systemd_post %{name}.service
%firewalld_reload
systemctl enable --now %{name}.service
firewall-cmd --add-service=%{name} --permanent

%preun
%systemd_preun %{name}.service
firewall-cmd --remove-service=%{name} --permanent

%postun
%systemd_postun_with_restart %{name}.service

## This is dangerous, rpmlint doesn't like it,
## and could break things if somebody uninstalls
## and reinstalls (instead of upgrade)
#if (($1==0)); then
#    if getent passwd %{name} &>/dev/null; then
#        userdel %{name}
#    fi
#    if getent group %{name} &>/dev/null; then
#        groupdel %{name}
#    fi
#fi

%changelog
* Tue Nov 17 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1009-1.develop
- Update changelog
- Fix bootstrap changelog logic
- Drop release back down to 1, to prep for next version
- Tweaking the changelog stuff
- Bump release tag
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Update some provides/conflicts stuff so that it's clear you can't install both sonarr and sonnar-bootstrap
- Build a bootstrap rpm (allows self-update), and do smarter detection of what version to build
- More carefully select which spec files to ignore
- Fix systemd requirement for EL7
- Added tests for using folder name as scene name
- Credit where credit is due
- Give systemd a bit more time to restart sonarr after update
- Fixed: Import single file torrents with a folder from QBittorrent
- Fixed unit tests
- Fixed: Scene Name not being stored properly during import if not linked to a download client item and filename is obfuscated
- New: Don't process files during Manual Import if there are more than 100 items
- Fixed duplicate UpdateHistory items
- Fixed: Tags in tag editor in SafarIE
- Readded Movies cat to the end of the Newznab list
- Fixed: Truncating too long filenames with unicode characters
- Protect against Qbittorrent edgecase if users add torrents manually with Keep top-level folder disabled
- Return max tooltip width
- Fixed: Reprocessing Manual Import items resetting season number if no episodes were selected
- Fixed: Language chosen in manual import overridden during import
- Fixed: Example file names for Daily Series
- Spelling confidence
- Fixed: Manual Import breaking if quality is selected before series
- And forgot test of course
- Fix for QBittorrent directory for specific torrents
- Bumped Sabnzbd default history request size from 30 to 60
- Fixed: QBittorrent imports when torrent name and folder name differ

* Sun Nov 15 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1002-1.develop
- Accidentally hard-coded "994" in one spot, fixed that. And since Version increased, bring Release back down to 1
- Clean up after ourselves
- Saner changelog
- Attempt to auto-maintain the rpm changelog?

* Fri Nov 13 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.994-10.develop
- RPM redone for Sonarr v3 beta
- auto-maintain the rpm changelog
- If tarball isn't there already, download from download.sonarr.tv
- Merge from orbisvicis/develop

* Fri Jan 02 2015 Yclept Nemo <"".join(chr(ord(c)-1) for c in "pscjtwjdjtAhnbjm/dpn")> - 2.0.0.2572-1.fc21
- Initial package
