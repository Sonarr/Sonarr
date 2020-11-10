config_opts['chroot_setup_cmd'] = 'install patterns-devel-base-devel_rpm_build'
config_opts['dist'] = 'tumbleweed'  # only useful for --resultdir variable subst
config_opts['extra_chroot_dirs'] = [ '/run/lock', ]
config_opts['useradd'] = '/usr/sbin/useradd -o -m -u {{chrootuid}} -g {{chrootgid}} -d {{chroothome}} {{chrootuser}}'
config_opts['releasever'] = '0'
config_opts['macros']['%dist'] = '.suse.tw%(sh -c ". /etc/os-release; echo \$VERSION_ID")'
config_opts['package_manager'] = 'dnf'
config_opts['ssl_ca_bundle_path'] = '/var/lib/ca-certificates/ca-bundle.pem'

# Due to the nature of the OpenSUSE mirroring system, we can not use
# metalinks easily and also we can not rely on the fact that baseurl's
# always work (issue #553) -- by design we need to expect a one minute
# repository problems (configured four attempts means 3 periods of 20s).
config_opts['package_manager_max_attempts'] = 4
config_opts['package_manager_attempt_delay'] = 20

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
{% if target_arch == 'x86_64' %}
excludepkgs=*.i586,*.i686
{% elif target_arch == 'i586' %}
excludepkgs=*.x86_64
{% elif target_arch == 'ppc64le' %}
excludepkgs=*.ppc,*.ppc64
{% elif target_arch == 'ppc64' %}
excludepkgs=*.ppc,*.ppc64le
{% endif %}

protected_packages=
user_agent={{ user_agent }}

# repos

[opensuse-tumbleweed-oss]
name=openSUSE Tumbleweed - {{ target_arch }} - OSS
{% if target_arch in ['x86_64', 'i586'] %}
baseurl=http://download.opensuse.org/tumbleweed/repo/oss/
#metalink=http://download.opensuse.org/tumbleweed/repo/oss/repodata/repomd.xml.metalink
{% elif target_arch in ['ppc64le', 'ppc64'] %}
baseurl=http://download.opensuse.org/ports/ppc/tumbleweed/repo/oss/
#metalink=http://download.opensuse.org/ports/ppc/tumbleweed/repo/oss/repodata/repomd.xml.metalink
{% elif target_arch in ['aarch64'] %}
baseurl=http://download.opensuse.org/ports/aarch64/tumbleweed/repo/oss/
#metalink=http://download.opensuse.org/ports/aarch64/tumbleweed/repo/oss/repodata/repomd.xml.metalink
{% endif %}
gpgkey=file:///usr/share/distribution-gpg-keys/opensuse/RPM-GPG-KEY-openSUSE
gpgcheck=1

"""
