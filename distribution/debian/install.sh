#!/bin/bash
### Description: Sonarr .NET Debian install
### Originally written for Radarr by: DoctorArr - doctorarr@the-rowlands.co.uk on 2021-10-01 v1.0
### Updates for servarr suite made by Bakerboy448, DoctorArr, brightghost, aeramor and VP-EN
### Version v1.0.0 2023-12-29 - StevieTV - adapted from servarr script for Sonarr installs

### Boilerplate Warning
#THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
#EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
#MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
#NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
#LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
#OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
#WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

scriptversion="1.0.0"
scriptdate="2023-12-29"

set -euo pipefail

echo "Running Sonarr Install Script - Version [$scriptversion] as of [$scriptdate]"

# Am I root?, need root!

if [ "$EUID" -ne 0 ]; then
    echo "Please run as root."
    exit
fi

app="sonarr"
app_port="8989"
app_prereq="curl sqlite3 wget"
app_umask="0002"
branch="main"

# Constants
### Update these variables as required for your specific instance
installdir="/opt"              # {Update me if needed} Install Location
bindir="${installdir}/${app^}" # Full Path to Install Location
datadir="/var/lib/$app/"       # {Update me if needed} AppData directory to use
app_bin=${app^}                # Binary Name of the app

# Prompt User
read -r -p "What user should ${app^} run as? (Default: $app): " app_uid
app_uid=$(echo "$app_uid" | tr -d ' ')
app_uid=${app_uid:-$app}
# Prompt Group
read -r -p "What group should ${app^} run as? (Default: media): " app_guid
app_guid=$(echo "$app_guid" | tr -d ' ')
app_guid=${app_guid:-media}

echo "This will install [${app^}] to [$bindir] and use [$datadir] for the AppData Directory"
echo "${app^} will run as the user [$app_uid] and group [$app_guid]. By continuing, you've confirmed that that user and group will have READ and WRITE access to your Media Library and Download Client Completed Download directories"
echo "Continue with the installation [Yes/No]?"
select yn in "Yes" "No"; do
    case $yn in
    Yes) break ;;
    No) exit 0 ;;
    esac
done

# Create User / Group as needed
if [ "$app_guid" != "$app_uid" ]; then
    if ! getent group "$app_guid" >/dev/null; then
        groupadd "$app_guid"
    fi
fi
if ! getent passwd "$app_uid" >/dev/null; then
    adduser --system --no-create-home --ingroup "$app_guid" "$app_uid"
    echo "Created and added User [$app_uid] to Group [$app_guid]"
fi
if ! getent group "$app_guid" | grep -qw "$app_uid"; then
    echo "User [$app_uid] did not exist in Group [$app_guid]"
    usermod -a -G "$app_guid" "$app_uid"
    echo "Added User [$app_uid] to Group [$app_guid]"
fi

# Stop the App if running
if service --status-all | grep -Fq "$app"; then
    systemctl stop "$app"
    systemctl disable "$app".service
    echo "Stopped existing $app"
fi

# Create Appdata Directory

# AppData
mkdir -p "$datadir"
chown -R "$app_uid":"$app_guid" "$datadir"
chmod 775 "$datadir"
echo "Directories created"
# Download and install the App

# prerequisite packages
echo ""
echo "Installing pre-requisite Packages"
# shellcheck disable=SC2086
apt update && apt install $app_prereq
echo ""
ARCH=$(dpkg --print-architecture)
# get arch
dlbase="https://services.sonarr.tv/v1/download/$branch/latest?version=4&os=linux"
case "$ARCH" in
"amd64") DLURL="${dlbase}&arch=x64" ;;
"armhf") DLURL="${dlbase}&arch=arm" ;;
"arm64") DLURL="${dlbase}&arch=arm64" ;;
*)
    echo "Arch not supported"
    exit 1
    ;;
esac
echo ""
echo "Removing previous tarballs"
# -f to Force so we fail if it doesnt exist
rm -f "${app^}".*.tar.gz
echo ""
echo "Downloading..."
wget --content-disposition "$DLURL"
tar -xvzf "${app^}".*.tar.gz
echo ""
echo "Installation files downloaded and extracted"

# remove existing installs
echo "Removing existing installation"
# If you happen to run this script in the installdir the line below will delete the extracted files and cause the mv some lines below to fail.
rm -rf "$bindir"
echo "Installing..."
mv "${app^}" $installdir
chown "$app_uid":"$app_guid" -R "$bindir"
chmod 775 "$bindir"
rm -rf "${app^}.*.tar.gz"
# Ensure we check for an update in case user installs older version or different branch
touch "$datadir"/update_required
chown "$app_uid":"$app_guid" "$datadir"/update_required
echo "App Installed"
# Configure Autostart

# Remove any previous app .service
echo "Removing old service file"
rm -rf /etc/systemd/system/"$app".service

# Create app .service with correct user startup
echo "Creating service file"
cat <<EOF | tee /etc/systemd/system/"$app".service >/dev/null
[Unit]
Description=${app^} Daemon
After=syslog.target network.target
[Service]
User=$app_uid
Group=$app_guid
UMask=$app_umask
Type=simple
ExecStart=$bindir/$app_bin -nobrowser -data=$datadir
TimeoutStopSec=20
KillMode=process
Restart=on-failure
[Install]
WantedBy=multi-user.target
EOF

# Start the App
echo "Service file created. Attempting to start the app"
systemctl -q daemon-reload
systemctl enable --now -q "$app"

# Finish Update/Installation
host=$(hostname -I)
ip_local=$(grep -oP '^\S*' <<<"$host")
echo ""
echo "Install complete"
sleep 10
STATUS="$(systemctl is-active "$app")"
if [ "${STATUS}" = "active" ]; then
    echo "Browse to http://$ip_local:$app_port for the ${app^} GUI"
else
    echo "${app^} failed to start"
fi

# Exit
exit 0
