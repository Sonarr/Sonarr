Name:           sonarr
Version:        %{BuildVersion}

Release:        1%{?dist}.%{?BuildBranch}
BuildArch:      noarch
Summary:        PVR for Usenet and BitTorrent users

License:        GPLv3+
URL:            https://sonarr.tv/
Source0:        https://download.sonarr.tv/v3/phantom-%{BuildBranc}/%{BuildVersion}/Sonarr.phantom-%{BuildBranch}.%{version}.linux.tar.gz
Source1:        copyright
Source2:        license
Source3:        sonarr.systemd

Requires:           sqlite-libs >= 3.7
Requires:           mediainfo >= 0.7.52
Requires:           mono-complete >= 3.10

Requires(pre):      shadow-utils
Requires(postun):   shadow-utils

Requires(post):     systemd
Requires(preun):    systemd
Requires(postun):   systemd
BuildRequires:      systemd

%description
Sonarr is a PVR for Usenet and BitTorrent users. It can monitor multiple RSS
feeds for new episodes of your favorite shows and will grab, sorts and renames
them. It can also be configured to automatically upgrade the quality of files
already downloaded when a better quality format becomes available.


%prep
%autosetup -n Sonarr


%install
# documentation
install -m 0755 -d %{buildroot}%{_defaultdocdir}/%{name}
install -m 0644 %{SOURCE1} %{buildroot}%{_defaultdocdir}/%{name}
install -m 0644 %{SOURCE2} %{buildroot}%{_defaultdocdir}/%{name}
# systemd service
install -m 0755 -d %{buildroot}%{_unitdir}
install -m 0644 %{SOURCE3} %{buildroot}%{_unitdir}/%{name}.service
# sonarr user
install -m 0755 -d %{buildroot}%{_sharedstatedir}/sonarr
# sonarr
install -m 0755 -d %{buildroot}/opt/%{name}
# Remove updater:
rm -rf %{buildroot}/opt/%{name}/Sonarr.Update

mv * %{buildroot}/opt/%{name}
find %{buildroot}/opt/%{name} -type f -exec chmod 644 '{}' \;
find %{buildroot}/opt/%{name} -type d -exec chmod 755 '{}' \;


%files
%{_docdir}/%{name}*
%{_unitdir}/%{name}*
/opt/%{name}*
%attr(-,sonarr,sonarr)%{_sharedstatedir}/sonarr


%pre
if ! getent group sonarr &>/dev/null; then
    groupadd -r sonarr
fi
if ! getent passwd sonarr &>/dev/null; then
    useradd -c "Sonarr user" -d %{_sharedstatedir}/sonarr -g sonarr -G users -r -s /sbin/nologin sonarr
fi


%post
%systemd_post %{name}.service


%preun
%systemd_preun %{name}.service


%postun
%systemd_postun_with_restart %{name}.service
if (($1==0)); then
    if getent passwd sonarr &>/dev/null; then
        userdel sonarr
    fi
    if getent group sonarr &>/dev/null; then
        groupdel sonarr
    fi
fi


%changelog
* Mon Nov 09 2020 Eric Eisenhart <freiheit at gmail dot com>'
- Updating for Sonarr v3

* Fri Jan 02 2015 Yclept Nemo <"".join(chr(ord(c)-1) for c in "pscjtwjdjtAhnbjm/dpn")> - 2.0.0.2572-1.fc21
- Initial package
