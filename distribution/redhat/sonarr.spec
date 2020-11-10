%define sonarr_url_service  http://services.sonarr.tv/v1/update/master
# default mock chroots do not provide python
#%define sonarr_url_package  %( python -c 'import requests; print(requests.get("%{sonarr_url_service}").json()["updatePackage"]["url"].replace("windows","mono").replace("zip","tar.gz"))' )
#%define sonarr_version      %( python -c 'import requests; print(requests.get("%{sonarr_url_service}").json()["updatePackage"]["version"])' )
%define sonarr_url_package  %( curl -s "%{sonarr_url_service}" | tr -d '[[:space:]]' | sed -n 's#.*"url":"\\([^"]*\\)".*#\\1#p' | sed 's#windows#mono#g;s#zip$#tar.gz#' )
%define sonarr_version      %( curl -s "%{sonarr_url_service}" | tr -d '[[:space:]]' | sed -n 's#.*"version":"\\([^"]*\\)".*#\\1#p' )


Name:           sonarr
Version:        %{sonarr_version}

Release:        1%{?dist}
Summary:        PVR for Usenet and BitTorrent users

License:        GPLv3+
URL:            https://sonarr.tv/
Source0:        %{sonarr_url_package}
Source1:        copyright
Source2:        license
Source3:        sonarr.systemd

Requires:           sqlite >= 3.7
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
%autosetup -n NzbDrone


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
* Fri Jan 02 2015 Yclept Nemo <"".join(chr(ord(c)-1) for c in "pscjtwjdjtAhnbjm/dpn")> - 2.0.0.2572-1.fc21
- Initial package
