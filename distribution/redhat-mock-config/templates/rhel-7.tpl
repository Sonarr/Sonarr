config_opts['chroot_setup_cmd'] = 'install bash bzip2 coreutils cpio diffutils findutils gawk gcc gcc-c++ grep gzip info make patch redhat-rpm-config rpm-build sed shadow-utils tar unzip util-linux which xz'
config_opts['dist'] = 'el7'  # only useful for --resultdir variable subst
config_opts['releasever'] = '7Server'
config_opts['package_manager'] = 'yum'
config_opts['bootstrap_image'] = 'ubi7/ubi'

config_opts['dnf_install_command'] += ' subscription-manager'
config_opts['yum_install_command'] += ' subscription-manager'

config_opts['root'] = 'rhel-7-{{ target_arch }}'

config_opts['redhat_subscription_required'] = True

config_opts['yum.conf'] = """
[main]
keepcache=1
debuglevel=2
reposdir=/dev/null
logfile=/var/log/yum.log
retries=20
obsoletes=1
gpgcheck=1
assumeyes=1
syslog_ident=mock
syslog_device=
best=1
protected_packages=

# repos

[rhel]
name = Red Hat Enterprise Linux
{% if target_arch == 'aarch64' %}
baseurl = https://cdn.redhat.com/content/dist/rhel-alt/{{ rhel_product }}/7/$releasever/armv8-a/$basearch/os
{% else %}
baseurl = https://cdn.redhat.com/content/dist/rhel/{{ rhel_product }}/7/$releasever/$basearch/os
{% endif %}
sslverify = 1
sslcacert = /etc/rhsm/ca/redhat-uep.pem
sslclientkey = /etc/pki/entitlement/{{ redhat_subscription_key_id }}-key.pem
sslclientcert = /etc/pki/entitlement/{{ redhat_subscription_key_id }}.pem
gpgkey=file:///usr/share/distribution-gpg-keys/redhat/RPM-GPG-KEY-redhat7-release
skip_if_unavailable=False


[rhel-optional]
name = Red Hat Enterprise Linux - Optional
{% if target_arch == 'aarch64' %}
baseurl = https://cdn.redhat.com/content/dist/rhel-alt/{{ rhel_product }}/7/$releasever/armv8-a/$basearch/optional/os
{% else %}
baseurl = https://cdn.redhat.com/content/dist/rhel/{{ rhel_product }}/7/$releasever/$basearch/optional/os
{% endif %}
sslverify = 1
sslcacert = /etc/rhsm/ca/redhat-uep.pem
sslclientkey = /etc/pki/entitlement/{{ redhat_subscription_key_id }}-key.pem
sslclientcert = /etc/pki/entitlement/{{ redhat_subscription_key_id }}.pem
gpgkey=file:///usr/share/distribution-gpg-keys/redhat/RPM-GPG-KEY-redhat7-release
skip_if_unavailable=False

[rhel-extras]
name = Red Hat Enterprise Linux - Extras
{% if target_arch == 'aarch64' %}
baseurl = https://cdn.redhat.com/content/dist/rhel-alt/{{ rhel_product }}/7/$releasever/armv8-a/$basearch/extras/os
{% else %}
baseurl = https://cdn.redhat.com/content/dist/rhel/{{ rhel_product }}/7/7Server/$basearch/extras/os
{% endif %}
enabled=0
sslverify = 1
sslcacert = /etc/rhsm/ca/redhat-uep.pem
sslclientkey = /etc/pki/entitlement/{{ redhat_subscription_key_id }}-key.pem
sslclientcert = /etc/pki/entitlement/{{ redhat_subscription_key_id }}.pem
gpgkey=file:///usr/share/distribution-gpg-keys/redhat/RPM-GPG-KEY-redhat7-release
skip_if_unavailable=False
"""
