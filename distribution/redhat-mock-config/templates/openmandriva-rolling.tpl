config_opts['chroot_setup_cmd'] = 'install basesystem-minimal rpm-build rpm-openmandriva-setup rpm-openmandriva-setup-build'
config_opts['dist'] = 'rolling'  # only useful for --resultdir variable subst
config_opts['extra_chroot_dirs'] = [ '/run/lock', ]
config_opts['useradd'] = '/usr/sbin/useradd -o -m -u {{chrootuid}} -g {{chrootgid}} -d {{chroothome}} {{chrootuser}}'
config_opts['releasever'] = '5.0'
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

[openmandriva-rolling]
name=OpenMandriva Rolling - {{ target_arch }}
# Master repository:
# baseurl=http://abf-downloads.openmandriva.org/rolling/repository/{{ target_arch }}/main/release/
mirrorlist=http://mirrors.openmandriva.org/mirrors.php?platform=rolling&arch={{ target_arch }}&repo=main&release=release
fastestmirror=1
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/openmandriva/RPM-GPG-KEY-OpenMandriva
enabled=1
skip_if_unavailable=False

[openmandriva-rolling-debuginfo]
name=OpenMandriva Rolling - {{ target_arch }} - Debug
# Master repository:
# baseurl=http://abf-downloads.openmandriva.org/rolling/repository/{{ target_arch }}/debug_main/release/
mirrorlist=http://mirrors.openmandriva.org/mirrors.php?platform=rolling&arch={{ target_arch }}&repo=debug_main&release=release
fastestmirror=1
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/openmandriva/RPM-GPG-KEY-OpenMandriva
enabled=0
skip_if_unavailable=False
"""
