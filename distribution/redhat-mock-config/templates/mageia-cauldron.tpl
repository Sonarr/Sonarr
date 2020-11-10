config_opts['chroot_setup_cmd'] = 'install basesystem-minimal rpm-build rpm-mageia-setup rpm-mageia-setup-build'
config_opts['dist'] = 'cauldron'  # only useful for --resultdir variable subst
config_opts['extra_chroot_dirs'] = [ '/run/lock', ]
config_opts['useradd'] = '/usr/sbin/useradd -o -m -u {{chrootuid}} -g {{chrootgid}} -d {{chroothome}} {{chrootuser}}'
config_opts['releasever'] = '8'
config_opts['macros']['%distro_section'] = 'core'
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

[mageia-cauldron]
name=Mageia Cauldron - {{ target_arch }}
#baseurl=http://mirrors.kernel.org/mageia/distrib/cauldron/{{ target_arch }}/media/core/release/
#metalink=https://mirrors.mageia.org/metalink?distrib=cauldron&arch={{ target_arch }}@&section=core&repo=release
mirrorlist=https://www.mageia.org/mirrorlist/?release=cauldron&arch={{ target_arch }}&section=core&repo=release
fastestmirror=1
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/mageia/RPM-GPG-KEY-Mageia
enabled=1
skip_if_unavailable=False

[mageia-cauldron-debuginfo]
name=Mageia Cauldron - {{ target_arch }} - Debug
#baseurl=http://mirrors.kernel.org/mageia/distrib/cauldron/{{ target_arch }}/media/debug/core/release/
#metalink=https://mirrors.mageia.org/metalink?distrib=cauldron&arch={{ target_arch }}@&section=core&repo=release&debug=true
mirrorlist=https://www.mageia.org/mirrorlist/?release=cauldron&arch={{ target_arch }}&section=core&repo=release&debug=1
fastestmirror=1
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/mageia/RPM-GPG-KEY-Mageia
enabled=0
skip_if_unavailable=False
"""
