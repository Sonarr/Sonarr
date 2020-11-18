Name:           sonarr-bootstrap
Version:        %{BuildVersion}

Release:        1.%{?BuildBranch}
BuildArch:      noarch
Summary:        PVR for Usenet and BitTorrent users; self-updating package

License:        GPLv3+
URL:            https://sonarr.tv/
Source0:        https://download.sonarr.tv/v3/phantom-%{BuildBranch}/%{BuildVersion}/Sonarr.phantom-%{BuildBranch}.%{version}.linux.tar.gz
Source3:        sonarr.systemd
Source4:        sonarr.firewalld
Source5:        sonarr-secure.firewalld

BuildRequires:      systemd

Requires:           sqlite-libs >= 3.7
Requires:           mediainfo >= 0.7.52
Requires:           mono-complete

Requires(pre):      shadow-utils
Requires(postun):   shadow-utils

Requires(post):     systemd
Requires(preun):    systemd
Requires(postun):   systemd

Provides: /opt/sonarr/Sonarr.exe
Provides: sonarr
Conflicts: sonarr

# These prevent Sonarr's DLLs from auto-creating requires and provides
# Doing that because RH's mono require/provide detection isn't working
# right here 
# (thinks it requires a different version of a library than it provides type problems)
%global __provides_exclude_from ^/opt/sonarr/.*$
%global __requires_exclude_from ^/opt/sonarr/.*$

%description
Sonarr is a PVR for Usenet and BitTorrent users. It can monitor multiple RSS
feeds for new episodes of your favorite shows and will grab, sorts and renames
them. It can also be configured to automatically upgrade the quality of files
already downloaded when a better quality format becomes available.


%prep
%autosetup -n Sonarr

%build
# Empty build just to make rpmlint happier.
# This spec uses binaries built on windows by Sonarr team instead of
# attempting to build on Linux.

%install

# systemd service
install -m 0755 -d %{buildroot}%{_unitdir}
install -m 0644 %{SOURCE3} %{buildroot}%{_unitdir}/sonarr.service

# firewalld
install -m 0755 -d %{buildroot}%{_prefix}/lib/firewalld/services/
install -m 0644 %{SOURCE4} %{buildroot}%{_prefix}/lib/firewalld/services/sonarr.xml
install -m 0644 %{SOURCE5} %{buildroot}%{_prefix}/lib/firewalld/services/sonarr-secure.xml

# sonarr user in /var
install -m 0755 -d %{buildroot}%{_sharedstatedir}/sonarr

# sonarr software itself
install -m 0755 -d %{buildroot}/opt/sonarr


mv * %{buildroot}/opt/sonarr

find %{buildroot}/opt/sonarr -type f -exec chmod 644 '{}' \;
find %{buildroot}/opt/sonarr -type d -exec chmod 755 '{}' \;


%files
%defattr(0644,root,root,0755)
%dir %{_unitdir}
%{_unitdir}/sonarr.service

%dir %{_prefix}/lib/firewalld
%dir %{_prefix}/lib/firewalld/services
%{_prefix}/lib/firewalld/services/*.xml

%attr(0755,sonnar,sonnar) %dir /opt/sonarr
%verify(not md5 mode size mtime) %attr(-,sonarr,sonarr) /opt/sonarr/*

%attr(-,sonarr,sonarr)%{_sharedstatedir}/sonarr

%pre
getent group sonarr >/dev/null || groupadd -r sonarr
getent passwd sonarr >/dev/null || \
    useradd -r -g sonarr -d d %{_sharedstatedir}/sonarr -s /sbin/nologin \
    -c "Sonarr PVR for Usenet and BitTorrent Users " sonarr
exit 0

%post
%systemd_post sonarr.service
%firewalld_reload
systemctl enable --now sonarr.service
firewall-cmd --add-service=sonarr --permanent

%preun
%systemd_preun sonarr.service
firewall-cmd --remove-service=sonarr --permanent

%postun
%systemd_postun_with_restart sonarr.service

## This is dangerous, rpmlint doesn't like it,
## and could break things if somebody uninstalls
## and reinstalls (instead of upgrade)
#if (($1==0)); then
#    if getent passwd sonarr &>/dev/null; then
#        userdel sonarr
#    fi
#    if getent group sonarr &>/dev/null; then
#        groupdel sonarr
#    fi
#fi

%changelog
* Tue Nov 17 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1009-1.develop


* Fri Nov 13 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.994-10.develop
- RPM redone for Sonarr v3 beta
- auto-maintain the rpm changelog
- If tarball isn't there already, download from download.sonarr.tv
- Merge from orbisvicis/develop

* Fri Jan 02 2015 Yclept Nemo <"".join(chr(ord(c)-1) for c in "pscjtwjdjtAhnbjm/dpn")> - 2.0.0.2572-1.fc21
- Initial package
