config_opts['releasever'] = 'eln'

config_opts['root'] = 'fedora-eln-{{ target_arch }}'

config_opts['chroot_setup_cmd'] = 'install @buildsys-build'

config_opts['dist'] = 'eln'  # only useful for --resultdir variable subst
config_opts['extra_chroot_dirs'] = [ '/run/lock', ]
config_opts['package_manager'] = 'dnf'
config_opts['bootstrap_image'] = 'fedora:latest'

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
user_agent={{ user_agent }}

# TODO
module_platform_id=platform:eln
protected_packages=

# repos
[local]
name=local
baseurl=https://kojipkgs.fedoraproject.org/repos/eln-build/latest/$basearch/
cost=2000
enabled=0
skip_if_unavailable=False

[eln]
name=Fedora ELN - Developmental modular packages for the next Enterprise Linux release
#metalink=https://mirrors.fedoraproject.org/metalink?repo=eln&arch=$basearch
baseurl=https://odcs.fedoraproject.org/composes/production/latest-Fedora-ELN/compose/Everything/$basearch/os/
enabled=1
repo_gpgcheck=0
type=rpm
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/fedora/RPM-GPG-KEY-fedora-rawhide-primary
skip_if_unavailable=False

[eln-debuginfo]
name=Fedora ELN - Debug
metalink=https://mirrors.fedoraproject.org/metalink?repo=eln-debug&arch=$basearch
#baseurl=https://odcs.fedoraproject.org/composes/production/latest-Fedora-ELN/compose/Everything/$basearch/debug/tree
enabled=0
repo_gpgcheck=0
type=rpm
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/fedora/RPM-GPG-KEY-fedora-rawhide-primary
skip_if_unavailable=False

[eln-source]
name=Fedora ELN - Source
metalink=https://mirrors.fedoraproject.org/metalink?repo=eln-source&arch=$basearch
#baseurl=https://odcs.fedoraproject.org/composes/production/latest-Fedora-ELN/compose/Everything/source/tree/
enabled=0
repo_gpgcheck=0
type=rpm
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/fedora/RPM-GPG-KEY-fedora-rawhide-primary
skip_if_unavailable=False
"""
