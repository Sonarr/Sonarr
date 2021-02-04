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
  echo "# Do Not Edit"
  echo "PackageVersion=%{version}"
  echo "PackageAuthor=[Team Sonarr](https://sonarr.tv)"
  echo "ReleaseVersion=%{version}"
  echo "UpdateMethod=External"
  echo "UpdateMethodMessage=Update with: sudo yum update sonarr"
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
    useradd -r -g %{name} -d %{_sharedstatedir}/%{name} -s /sbin/nologin \
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
* Thu Feb 04 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1100-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Series Removed From TVDB wiki link
- Detect Dolby Vision as HDR and MediaInfo Update
- New: Add name field to release profiles

* Tue Feb 02 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1096-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Global scene mapping aliases disappeared from UI
- Fixed: Validation of new qbittorrent max-ratio action config
- Added searchEngine support in Newznab/Torznab caps
- Fixed: FLAC audio channels in media info

* Mon Feb 01 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1095-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- New: Disable season search if series is unmonitored
- Fixed: Handle more obfuscated names

* Sun Jan 31 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1093-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- New: On Episode File Delete For Upgrade notification option
- New: Unify series custom filter options

* Mon Jan 25 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1091-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Label for 'On Episode File Delete'
- Consistent types for on delete custom script events
- Fixed: Webhook events not sent for series deletions
- Separate event types for series and episode deletions
- Fixed: Queue refresh closing manual import from queue if items change

* Sun Jan 24 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1085-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- New: On Delete Notifications
- Fixed: Files with lower preferred word scores are imported
- Fixed: Series Type Filter
- Manual Import episode improvements
- Fixed: Improve multi-episode title squashing

* Thu Jan 21 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1083-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Update bug report template
- Lock closed issues after 90 days without activity
- New: Flood Download Client
- Typo for linux

* Mon Jan 18 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1077-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Error handling when cannot create folder in Recycling Bin

* Sun Jan 17 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1076-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- New: Treat Manual Bad in history as failed
- make HashedReleaseFixture entries generic
- Fixed: Handle more obfuscated names
- Fixed parsing (duplicate) releases for series with multiple season number mappings

* Sat Jan 16 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1073-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed accounting for zero terminator in long path limitation
- New: Require Encryption option for email
- Fixed: Managing display profiles on mobile
- Fixed: Sorting in Interactive search duplicates results
- Fixed duplicate id searches due to missing Equals on SceneSeasonMapping
- Show separate message for unknown episode/series
- Fixed: Regular Anime being caught in Chinese parser rules
- Fixed Agenda Time wrapping

* Fri Jan 15 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1069-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Updated BTN url to https
- Linting as usual
- Fixed: Unnecessary certificate validation errors on localhost/loopback
- New: Added Scene Info to Interactive Search results to show more about the applied scene/TheXEM mappings
- Fixed searching the wrong season.

* Thu Jan 14 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1066-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- New: Parsing of '[WEB]' as WebDL
- Update contributing.md
- Fix name of max NumberInput in QualityDefinition.js
- Readme updates
- New: Replace SmtpClient with Mailkit

* Wed Jan 13 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1062-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Parse standalone UHD as 2160p if no other resolution info is present

* Tue Jan 12 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1061-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Dailiezearch.

* Sun Jan 10 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1060-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Update wiki link hints for health checks

* Thu Jan 07 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1059-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- New: Allow quality size limits to be closer together
- Better task interval fetching
- Fixed: Only delete update folder if it exists

* Tue Jan 05 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1058-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed tests
- No longer need the special tvdb season number handling since it's integrated into the search.
- Fixed: Regression in searching anime by primary title
- New: Support in services for multiple scene naming/numbering exceptions
- Fixed: Backups interval being used as minutes instead of days

* Mon Jan 04 2021 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1052-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed tests
- Linting
- Fixed: Additional handling for obfuscated releases
- Fixed: Parsing of 4Kto1080p as 1080p
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Additional handling for obfuscated releases Closes #4198
- Fixed: Parsing of 4Kto1080p as 1080p Closes #4199
- Use createHandleActions for adding/removing commands so itemMap is synced properly
- New: Removing update folder from temp folder during housekeeping
- New: Renamed Quick Import to Move Automatically
- Fixed UpdatePackageProviderFixture tests
- Fixed: Don't convert series selection filter to lower case in state
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Restored robots.txt
- Fixed: Timespan over 1 month shown incorrectly
- Fixed: Missing leading 0 in minutes/seconds for media info duration
- Fixed: Backup interval is updated on change

* Thu Dec 31 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1042-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Update path before importing to ensure it hasn't changed
- Fixed: Parsing Polish language
- New: Rename Import to Library Import

* Sat Dec 26 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1039-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- eslint

* Fri Dec 25 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1035-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Small helper in UI to access Sonarr API more easily
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Series year wrong when airing January 1st.
- Fixed: OSX version detection

* Sat Dec 19 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1033-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Format Errors from AudioChannel formatter
- Fixed Migration 148 test
- Fixed: Handle 3 digit audio channels
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: Language parsing with space-delimited releases
- Fixed: Don't workaround DTS if audioChannels invalid
- Fixed: Migrate Mediainfo properties that changed names
- Fixed: Use audioChannels_Original if it exists in MI
- Fixed health check wiki link unit tests
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- New: Sorting Series List/Mass Editor by Language Profile and Tags

* Mon Dec 14 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1026-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- All Wiki links now use the consolidated Servarr wiki
- Fixed: '/series' URL Base breaking UI navigation
- New: Added Series Monitoring Toggle to Series Details

* Mon Dec 07 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1024-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Move config.yml for github
- Fixed: Using folder as scene name for season packs

* Wed Dec 02 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1023-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Update GitHub templates
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed: List Import no longer fails due to duplicates

* Mon Nov 23 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1021-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Removed unnecessary importlists warning.

* Sun Nov 22 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1020-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed binary execute permissions for osx and Radarr

* Sun Nov 22 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1019-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fixed disk permission tests

* Sat Nov 21 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1017-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Reverted temporary dev debug code change
- Fixed: Monitor 'None' won't monitor latest season
- New: Validate that naming formats don't contain illegal characters
- New: Displaying folder-based permissions in UI rather than file-based permissions and with selectable sane presets

* Wed Nov 18 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1011-1.develop
- Merge branch 'phantom-develop' of https://github.com/Sonarr/Sonarr into phantom-rpm-package
- Fix package_info file
- Update indexer category parameters for the other nyaa
- Dropping release back to 1, to prep for next Version update
- Minor typo in package_info

* Tue Nov 17 2020 Eric Eisenhart <freiheit@gmail.com> - 3.0.4.1009-4.develop
- Bump release tag to get an update
- Fix useradd
- Update changelog

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
