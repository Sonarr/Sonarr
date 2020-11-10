config_opts['chroot_setup_cmd'] += " epel-release epel-rpm-macros fedpkg-minimal"

config_opts['dnf.conf'] += """

[epel]
name=epel
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=epel-8&arch=$basearch
failovermethod=priority
gpgkey=file:///usr/share/distribution-gpg-keys/epel/RPM-GPG-KEY-EPEL-8
gpgcheck=1
skip_if_unavailable=False

[testing]
name=epel-testing
enabled=0
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=testing-epel8&arch=$basearch
failovermethod=priority
skip_if_unavailable=False

[local]
name=local
baseurl=https://kojipkgs.fedoraproject.org/repos/epel8-build/latest/$basearch/
cost=2000
enabled=0
skip_if_unavailable=False

[epel-debuginfo]
name=epel-debug
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=epel-debug-8&arch=$basearch
failovermethod=priority
enabled=0
skip_if_unavailable=False

[epel-source]
name=epel-source
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=epel-source-8&arch=$basearch
failovermethod=priority
enabled=0
skip_if_unavailable=False

[epel-modular]
name=epel-modular
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=epel-modular-8&arch=$basearch
failovermethod=priority
enabled=0
skip_if_unavailable=False

[epel-modular-debuginfo]
name=epel-modular-debug
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=epel-modular-debug-8&arch=$basearch
failovermethod=priority
enabled=0
skip_if_unavailable=False

[epel-modular-source]
name=epel-modular-source
mirrorlist=http://mirrors.fedoraproject.org/mirrorlist?repo=epel-modular-source-8&arch=$basearch
failovermethod=priority
enabled=0
skip_if_unavailable=False
"""
