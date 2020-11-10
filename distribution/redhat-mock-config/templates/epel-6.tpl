config_opts['chroot_setup_cmd'] = 'install @buildsys-build'
config_opts['dist'] = 'el6'  # only useful for --resultdir variable subst
# beware RHEL uses 6Server or 6Client
config_opts['releasever'] = '6'
config_opts['isolation'] = 'simple'
config_opts['bootstrap_image'] = 'centos:6'
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

# repos
[base]
name=BaseOS
enabled=1
mirrorlist=http://mirrorlist.centos.org/?release=6&arch=$basearch&repo=os
failovermethod=priority
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-6
gpgcheck=1
skip_if_unavailable=False

[updates]
name=updates
enabled=1
mirrorlist=http://mirrorlist.centos.org/?release=6&arch=$basearch&repo=updates
failovermethod=priority
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-6
gpgcheck=1
skip_if_unavailable=False

[epel]
name=epel
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=epel-6&arch=$basearch
failovermethod=priority
gpgkey=file:///usr/share/distribution-gpg-keys/epel/RPM-GPG-KEY-EPEL-6
gpgcheck=1
skip_if_unavailable=False

{% if target_arch == "x86_64" %}
[sclo]
name=sclo
baseurl=http://mirror.centos.org/centos/6/sclo/$basearch/sclo/
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-SIG-SCLo
gpgcheck=1
includepkgs=devtoolset*
skip_if_unavailable=False

[sclo-rh]
name=sclo-rh
baseurl=http://mirror.centos.org/centos/6/sclo/$basearch/rh/
gpgkey=file:///usr/share/distribution-gpg-keys/centos/RPM-GPG-KEY-CentOS-SIG-SCLo
gpgcheck=1
includepkgs=devtoolset*
skip_if_unavailable=False
{% endif %}

[testing]
name=epel-testing
enabled=0
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=testing-epel6&arch=$basearch
failovermethod=priority
skip_if_unavailable=False

[local]
name=local
baseurl=https://kojipkgs.fedoraproject.org/repos/dist-6E-epel-build/latest/$basearch/
cost=2000
enabled=0
skip_if_unavailable=False

[epel-debuginfo]
name=epel-debug
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=epel-debug-6&arch=$basearch
failovermethod=priority
enabled=0
skip_if_unavailable=False
"""
