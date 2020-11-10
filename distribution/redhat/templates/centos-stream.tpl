config_opts['chroot_setup_cmd'] = 'install tar gcc-c++ redhat-rpm-config redhat-release which xz sed make bzip2 gzip gcc coreutils unzip shadow-utils diffutils cpio bash gawk rpm-build info patch util-linux findutils grep'
config_opts['dist'] = 'el8'  # only useful for --resultdir variable subst
config_opts['releasever'] = '8'
config_opts['package_manager'] = 'dnf'
config_opts['extra_chroot_dirs'] = [ '/run/lock', ]
config_opts['bootstrap_image'] = 'centos:8'
config_opts['dnf_vars'] = { 'stream': '8-stream',
                            'contentdir': 'centos',
                          }

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
mdpolicy=group:primary
best=1
protected_packages=
module_platform_id=platform:el8
user_agent={{ user_agent }}

[Stream-BaseOS]
name=CentOS-Stream - Base
baseurl=http://mirror.centos.org/centos/8-stream/BaseOS/$basearch/os/
failovermethod=priority
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official
gpgcheck=1
skip_if_unavailable=False

[Stream-AppStream]
name=CentOS-Stream - AppStream
baseurl=http://mirror.centos.org/centos/8-stream/AppStream/$basearch/os/
gpgcheck=1
enabled=1
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official

[Stream-centosplus]
name=CentOS-Stream - Plus
baseurl=http://mirror.centos.org/centos/8-stream/centosplus/$basearch/os/
gpgcheck=1
enabled=0
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official

[cr]
name=CentOS-$releasever - cr
baseurl=http://mirror.centos.org/centos/8/cr/$basearch/os/
gpgcheck=1
enabled=0
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official

[Stream-base-debuginfo]
name=CentOS-Stream - Debuginfo
baseurl=http://debuginfo.centos.org/8-stream/$basearch/
gpgcheck=1
enabled=0
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official

[Stream-extras]
name=CentOS-Stream - Extras
baseurl=http://mirror.centos.org/centos/8-stream/extras/$basearch/os/
gpgcheck=1
enabled=1
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official

[Stream-PowerTools]
name=CentOS-Stream - PowerTools
baseurl=http://mirror.centos.org/centos/8-stream/PowerTools/$basearch/os/
gpgcheck=1
enabled=1
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official

[Stream-Devel]
name=CentOS-Stream - Devel WARNING! FOR BUILDROOT USE ONLY!
baseurl=http://mirror.centos.org/centos/8-stream/Devel/$basearch/os/
gpgcheck=1
enabled=0
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official

[Stream-BaseOS-source]
name=CentOS-Stream - BaseOS Sources
baseurl=http://vault.centos.org/centos/8-stream/BaseOS/Source/
gpgcheck=1
enabled=0
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official

[Stream-AppStream-source]
name=CentOS-Stream - AppStream Sources
baseurl=http://vault.centos.org/centos/8-stream/AppStream/Source/
gpgcheck=1
enabled=0
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official

[Stream-PowerTools-source]
name=CentOS-Stream - PowerTools Sources
baseurl=http://vault.centos.org/centos/8-stream/PowerTools/Source/
gpgcheck=1
enabled=0
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official

[Stream-extras-source]
name=CentOS-Stream - Extras Sources
baseurl=http://vault.centos.org/centos/8-stream/extras/Source/
gpgcheck=1
enabled=0
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official

[Stream-centosplus-source]
name=CentOS-Stream - Plus Sources
baseurl=http://vault.centos.org/centos/8-stream/centosplus/Source/
gpgcheck=1
enabled=0
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-Official
"""
