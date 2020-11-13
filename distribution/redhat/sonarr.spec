Name:           sonarr
Version:        %{BuildVersion}

Release:        9.%{?BuildBranch}
BuildArch:      noarch
Summary:        PVR for Usenet and BitTorrent users

License:        GPLv3+
URL:            https://sonarr.tv/
Source0:        https://download.sonarr.tv/v3/phantom-%{BuildBranch}/%{BuildVersion}/Sonarr.phantom-%{BuildBranch}.%{version}.linux.tar.gz
Source3:        %{name}.systemd
Source4:        %{name}.firewalld
Source5:        %{name}-secure.firewalld

BuildRequires:      systemd-rpm-macros

Requires:           sqlite-libs >= 3.7
Requires:           mediainfo >= 0.7.52
Requires:           mono-complete

Requires(pre):      shadow-utils
Requires(postun):   shadow-utils

Requires(post):     systemd
Requires(preun):    systemd
Requires(postun):   systemd

Provides: /opt/%{name}/Sonarr.exe

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
* Mon Nov 09 2020 Eric Eisenhart <freiheit at gmail dot com>' - 3.0.4.994-9.develop
- Updating for Sonarr v3

* Fri Jan 02 2015 Yclept Nemo <"".join(chr(ord(c)-1) for c in "pscjtwjdjtAhnbjm/dpn")> - 2.0.0.2572-1.fc21
- Initial package
