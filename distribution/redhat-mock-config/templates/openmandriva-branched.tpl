config_opts['root'] = 'openmandriva-{{ releasever }}-{{ target_arch }}'
config_opts['chroot_setup_cmd'] = 'install basesystem-build'
config_opts['extra_chroot_dirs'] = [ '/run/lock', ]
config_opts['useradd'] = '/usr/sbin/useradd -o -m -u {{chrootuid}} -g {{chrootgid}} -d {{chroothome}} {{chrootuser}}'
config_opts['macros']['%cross_compiling'] = '0' # Mock should generally be considered native builds
config_opts['package_manager'] = 'dnf'

config_opts['dnf.conf'] = """
[main]
keepcache=1
debuglevel=2
reposdir=/dev/null
logfile=/var/log/yum.log
retries=20
obsoletes=1
gpgcheck=0
assumeyes=1
syslog_ident=mock
syslog_device=
install_weak_deps=0
metadata_expire=0
best=1
protected_packages=
user_agent={{ user_agent }}

# repos

[openmandriva]
name=OpenMandriva $releasever - {{ target_arch }}
# Master repository:
# baseurl=http://abf-downloads.openmandriva.org/$releasever/repository/{{ target_arch }}/main/release/
mirrorlist=http://mirrors.openmandriva.org/mirrors.php?platform=$releasever&arch={{ target_arch }}&repo=main&release=release
fastestmirror=1
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/openmandriva/RPM-GPG-KEY-OpenMandriva
enabled=1
skip_if_unavailable=False

[openmandriva-updates]
name=OpenMandriva $releasever - {{ target_arch }} - Updates
# Master repository:
# baseurl=http://abf-downloads.openmandriva.org/$releasever/repository/{{ target_arch }}/main/updates/
mirrorlist=http://mirrors.openmandriva.org/mirrors.php?platform=$releasever&arch={{ target_arch }}&repo=main&release=updates
fastestmirror=1
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/openmandriva/RPM-GPG-KEY-OpenMandriva
enabled=1
skip_if_unavailable=False

[openmandriva-debuginfo]
name=OpenMandriva $releasever - {{ target_arch }} - Debug
# Master repository:
# baseurl=http://abf-downloads.openmandriva.org/$releasever/repository/{{ target_arch }}/debug_main/release/
mirrorlist=http://mirrors.openmandriva.org/mirrors.php?platform=$releasever&arch={{ target_arch }}&repo=debug_main&release=release
fastestmirror=1
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/openmandriva/RPM-GPG-KEY-OpenMandriva
enabled=0
skip_if_unavailable=False

[openmandriva-updates-debuginfo]
name=OpenMandriva $releasever - {{ target_arch }} - Updates - Debug
# Master repository:
# baseurl=http://abf-downloads.openmandriva.org/$releasever/repository/{{ target_arch }}/debug_main/updates/
mirrorlist=http://mirrors.openmandriva.org/mirrors.php?platform=$releasever&arch={{ target_arch }}&repo=debug_main&release=updates
fastestmirror=1
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/openmandriva/RPM-GPG-KEY-OpenMandriva
enabled=0
skip_if_unavailable=False
"""
