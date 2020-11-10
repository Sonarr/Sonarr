config_opts['chroot_setup_cmd'] = 'install @buildsys-build yum'
config_opts['dist'] = 'amzn2' # only useful for --resultdir variable subst
config_opts['plugin_conf']['ccache_enable'] = False
config_opts['package_manager'] = 'yum'
config_opts['releasever'] = '2'

config_opts['bootstrap_image'] = 'amazonlinux:2'

config_opts['yum.conf'] = """
[main]
cachedir=/var/cache/yum
debuglevel=1
logfile=/var/log/yum.log
reposdir=/dev/null
retries=20
obsoletes=1
gpgcheck=0
assumeyes=1

[amzn2]
name=AMZN2
mirrorlist=http://amazonlinux.default.amazonaws.com/$releasever/core/latest/$basearch/mirror.list
gpgcheck=1
gpgkey=file:///usr/share/distribution-gpg-keys/amazon-linux/RPM-GPG-KEY-amazon-linux-2
enabled=1
"""
