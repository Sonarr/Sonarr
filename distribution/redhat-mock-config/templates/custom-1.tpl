config_opts['chroot_setup_cmd'] = ''
config_opts['chroot_additional_packages'] = ''
config_opts['extra_chroot_dirs'] = [ '/run/lock', ]
config_opts['package_manager'] = 'dnf'
config_opts['module_enable'] = []
config_opts['module_install'] = []
# DNF may not be available in this chroot
config_opts['use_bootstrap'] = False

config_opts['dnf.conf'] = """
[main]
keepcache=1
debuglevel=2
reposdir=/dev/null
logfile=/var/log/dnf.log
retries=20
obsoletes=1
gpgcheck=0
assumeyes=1
syslog_ident=mock
syslog_device=
install_weak_deps=0
metadata_expire=0
mdpolicy=group:primary
best=1
protected_packages=
user_agent={{ user_agent }}

"""
