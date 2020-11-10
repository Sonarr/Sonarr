config_opts['chroot_setup_cmd'] = 'install @buildsys-build'
config_opts['dist'] = 'el7'  # only useful for --resultdir variable subst
config_opts['releasever'] = '7'
config_opts['yum_install_command'] += ' --disablerepo=sclo*'
config_opts['bootstrap_image'] = 'centos:7'
config_opts['package_manager'] = 'yum'

config_opts['yum.conf'] = """
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
metadata_expire=0
mdpolicy=group:primary
best=1
protected_packages=
user_agent={{ user_agent }}

{% set epel_7_gpg_keys = 'file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-7' %}
{% if target_arch in ['ppc64le', 'ppc64'] %}
{%   set epel_7_gpg_keys = epel_7_gpg_keys + ',file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-SIG-AltArch-7-' + target_arch %}
{% elif target_arch in ['aarch64'] %}
{%   set epel_7_gpg_keys = epel_7_gpg_keys + ',file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-7-aarch64' %}
{% endif %}

# repos
[base]
name=BaseOS
mirrorlist=http://mirrorlist.centos.org/?release=7&arch=$basearch&repo=os
failovermethod=priority
gpgkey={{ epel_7_gpg_keys }}
gpgcheck=1
skip_if_unavailable=False

[updates]
name=updates
enabled=1
mirrorlist=http://mirrorlist.centos.org/?release=7&arch=$basearch&repo=updates
failovermethod=priority
gpgkey={{ epel_7_gpg_keys }}
gpgcheck=1
skip_if_unavailable=False

[epel]
name=epel
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=epel-7&arch=$basearch
failovermethod=priority
gpgkey=file:///usr/share/distribution-gpg-keys/epel/RPM-GPG-KEY-EPEL-7
gpgcheck=1
skip_if_unavailable=False

[extras]
name=extras
mirrorlist=http://mirrorlist.centos.org/?release=7&arch=$basearch&repo=extras
failovermethod=priority
gpgkey={{ epel_7_gpg_keys }}
gpgcheck=1
skip_if_unavailable=False

{% if target_arch == 'x86_64' %}
[sclo]
name=sclo
baseurl=http://mirror.centos.org/centos/7/sclo/$basearch/sclo/
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-SIG-SCLo
gpgcheck=1
includepkgs=devtoolset*
skip_if_unavailable=False
{% endif %}

{% if target_arch in ['x86_64', 'ppc64le', 'aarch64'] %}
[sclo-rh]
name=sclo-rh
mirrorlist=http://mirrorlist.centos.org/?release=7&arch=$basearch&repo=sclo-rh
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-SIG-SCLo
gpgcheck=1
includepkgs=devtoolset*
skip_if_unavailable=False
{% endif %}

[testing]
name=epel-testing
enabled=0
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=testing-epel7&arch=$basearch
failovermethod=priority
skip_if_unavailable=False

[local]
name=local
baseurl=https://kojipkgs.fedoraproject.org/repos/epel7-build/latest/$basearch/
cost=2000
enabled=0
skip_if_unavailable=False

[epel-debuginfo]
name=epel-debug
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=epel-debug-7&arch=$basearch
failovermethod=priority
enabled=0
skip_if_unavailable=False
"""
