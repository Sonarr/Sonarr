#!/bin/bash
### Description: Sonarr .NET Debian install
### Originally written for Radarr by: DoctorArr - doctorarr@the-rowlands.co.uk on 2021-10-01 v1.0
### Updates for servarr suite made by Bakerboy448, DoctorArr, brightghost, aeramor and VP-EN
### Version v1.0.0 2023-12-29 - StevieTV - adapted from servarr script for Sonarr installs
### Version V1.0.1 2024-01-02 - StevieTV - remove UTF8-BOM
### Version V1.0.2 2024-01-03 - markus101 - Get user input from /dev/tty
### Version V1.0.3 2024-01-06 - StevieTV - exit script when it is ran from install
### Version V1.0.4 2024-04-10 - nostrusdominion - added colors, moved root check, moved architecture check, added title splash screen, improved readablity, changed app_prereq to not bother apt if they are already installed, supressed tarball extraction, add sleep timers.

### Boilerplate Warning
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
# NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
# LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
# OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
# WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

### Colors
green=$(tput setaf 2)
yellow=$(tput setaf 3)
red=$(tput setaf 1)
orange=$(tput setaf 166)
cyan=$(tput setaf 6)
reset=$(tput sgr0) # No Color

scriptversion="1.0.4"
scriptdate="2024-04-10"

set -euo pipefail

# Am I root?, need root!
if [ "$EUID" -ne 0 ]; then
    echo -e ${red}"Please run as root!"
    echo -e "Exiting script!"
    exit
fi

# Stuff the gremlins needs to know
app="sonarr"
app_port="8989"
app_prereq="curl sqlite3 wget"
app_umask="0002"
branch="main"

### Constants

# Update these variables as required for your specific instance
installdir="/opt"              # {Update me if needed} Install Location
bindir="${installdir}/${app^}" # Full Path to Install Location
datadir="/var/lib/$app/"       # {Update me if needed} AppData directory to use
app_bin=${app^}                # Binary Name of the app

# Check if architecture is correct
echo ""
ARCH=$(dpkg --print-architecture)
# get arch
dlbase="https://services.sonarr.tv/v1/download/$branch/latest?version=4&os=linux"
case "$ARCH" in
"amd64") DLURL="${dlbase}&arch=x64" ;;
"armhf") DLURL="${dlbase}&arch=arm" ;;
"arm64") DLURL="${dlbase}&arch=arm64" ;;
*)
    echo -e ${red}"Your arch is not supported!"
    echo -e "Exiting installer script!"
    exit 1
    ;;
esac

### Title Splash!

echo -e "" ${cyan}
echo -e "           _____  ____  _   _          _____  _____             "
echo -e "          / ____|/ __ \| \ | |   /\   |  __ \|  __ \            "
echo -e "         | (___ | |  | |  \| |  /  \  | |__) | |__) |           "
echo -e "          \___ \| |  | | .   | / /\ \ |  _  /|  _  /            "
echo -e "          ____) | |__| | |\  |/ ____ \| | \ \| | \ \            "
echo -e "  _____ _|_____/_\____/|_|_\_/_/   _\_\_| _\_\_| _\_\__ _____   "
echo -e " |_   _| \ | |/ ____|__   __|/\   | |    | |    |  ____|  __ \  "
echo -e "   | | |  \| | (___    | |  /  \  | |    | |    | |__  | |__) | "
echo -e "   | | | .   |\___ \   | | / /\ \ | |    | |    |  __| |  _  /  "
echo -e "  _| |_| |\  |____) |  | |/ ____ \| |____| |____| |____| | \ \  "
echo -e " |_____|_| \_|_____/___|_/_/___ \_\______|______|______|_|  \_\ "
echo -e "            / ____|/ ____|  __ \|_   _|  __ \__   __|           "
echo -e "           | (___ | |    | |__) | | | | |__) | | |              "
echo -e "            \___ \| |    |  _  /  | | |  ___/  | |              "
echo -e "            ____) | |____| | \ \ _| |_| |      | |              "
echo -e "           |_____/ \_____|_|  \_\_____|_|      |_|              " ${reset}
echo -e ""
echo -e "         Running version ${orange}[$scriptversion]${reset} as of ${orange}[$scriptdate]${reset}      "

# This script should not be ran from installdir, otherwise later in the script the extracted files will be removed before they can be moved to installdir.
if [ "$installdir" == "$(dirname -- "$( readlink -f -- "$0"; )")" ] || [ "$bindir" == "$(dirname -- "$( readlink -f -- "$0"; )")" ]; then
    echo ""
    echo -e "$ {red}Error!${reset} You should not run this script from the intended install directory."
    echo " Please re-run it from another directory."
    echo " Exiting Script!"
    exit
fi

# User warning about permission conflicts
echo ""
echo -e ${red}"  WARNING!"${reset}
echo ""
echo -e "  It is ${red}CRITICAL${reset} that the ${orange}User${reset} and ${orange}Group${reset} you select"
echo -e "  to run ${orange}[${app^}]${reset} will have both ${red}READ${reset} and ${red}WRITE${reset} access"
echo -e "  to your Media Library and Download Client directories!"${reset}

# Prompt User
echo ""
read -r -p " What user should [${app^}] run as? (Default: $app): " app_uid < /dev/tty
app_uid=$(echo "$app_uid" | tr -d ' ')
app_uid=${app_uid:-$app}

# Prompt Group
echo ""
read -r -p " What group should [${app^}] run as? (Default: media): " app_guid < /dev/tty
app_guid=$(echo "$app_guid" | tr -d ' ')
app_guid=${app_guid:-media}

# Info for the user and user confirmation
echo ""
echo -e " ${orange}[${app^}]${reset} will be installed to ${orange}[$bindir]${reset} and use ${orange}[$datadir]${reset} for the AppData Directory"
echo ""
echo -e " ${orange}${app^}${reset} will run as the user ${orange}[$app_uid]${reset} and group ${orange}[$app_guid]${reset}."
echo ""
echo -e "   By continuing, you ${red}CONFIRM${reset} that user ${orange}[$app_uid]${reset} and group ${orange}[$app_guid]${reset}"
echo -e "   will have both ${red}READ${reset} and ${red}WRITE${reset} access to all required directories."
echo -e " By continuing, you ${red}CONFIRM${reset} that that ${orange}[$app_uid]${reset} and ${orange}[$app_guid]${reset}"
echo -e " will have both ${red}READ${reset} and ${red}WRITE${reset} access to all required directories."

# User confirmation for installation to continue.
echo ""
while true; do
    read -r -p " Do you want to continue with the installation? [y/N]: " response
    response_lowercase=$(echo "$response" | tr '[:upper:]' '[:lower:]')
    if [[ $response_lowercase == "y" ]]; then
        break
    elif [[ $response_lowercase == "n" ]]; then
        echo ""
        echo " Installation canceled."
        echo " Exiting script."
        exit 0
    else
        echo ""
        echo " Invalid response. Please enter 'y' to continue or 'n' to cancel."
        echo ""
    fi
done

# Create User / Group as needed
if [ "$app_guid" != "$app_uid" ]; then
    if ! getent group "$app_guid" >/dev/null; then
        groupadd "$app_guid"
    fi
fi
if ! getent passwd "$app_uid" >/dev/null; then
    adduser --system --no-create-home --ingroup "$app_guid" "$app_uid"
    echo ""
    echo -e "Created User ${yellow}$app_uid${reset}"
    echo ""
    echo -e "Created Group ${yellow}$app_guid${reset}."
    sleep 3
fi
if ! getent group "$app_guid" | grep -qw "$app_uid"; then
    echo ""
    echo -e "User ${yellow}$app_uid${reset} did not exist in Group ${yellow}$app_guid${reset}."
    usermod -a -G "$app_guid" "$app_uid"
    echo ""
    echo -e "Added User ${yellow}$app_uid${reset} to Group ${yellow}$app_guid${reset}."
    sleep 3
fi

# Stop the App if running
if service --status-all | grep -Fq "$app"; then
    systemctl stop "$app"
    systemctl disable "$app".service
    echo ""
    echo -e ${yellow}"Stopped existing $app service."${reset}
fi
sleep 1

# Create Appdata Directory
mkdir -p "$datadir"
chown -R "$app_uid":"$app_guid" "$datadir"
chmod 775 "$datadir"
echo ""
echo -e "${yellow}$datadir${reset} was successfully created."
sleep 1

# Download and install the App

# Check if prerequisite packages are already installed and install them if needed
echo ""
echo -e ${yellow}"Checking Pre-Requisite Packages..."${reset}
sleep 3

missing_packages=()
for pkg in $app_prereq; do
    if ! dpkg -s "$pkg" >/dev/null 2>&1; then
        missing_packages+=("$pkg")
    fi
done

if [ ${#missing_packages[@]} -eq 0 ]; then
    echo ""
    echo -e ${green}"All prerequisite packages are already installed!"${reset}
else
    echo ""
    echo -e "Installing missing prerequisite packages: ${orange}${missing_packages[*]}${reset}"
    # Install missing prerequisite packages
    apt update && apt install "${missing_packages[@]}"
fi

# Remove old tarballs, then download and install sonarr tarball for installation
echo ""
echo -e ${yellow}"Removing previous installation files..."${reset}
sleep 2
# -f to Force so we do not fail if it doesn't exist
rm -f "${app^}".*.tar.gz
echo ""
echo -e ${yellow}"Downloading required installation files..."${reset}
echo ""
wget --content-disposition "$DLURL"
echo ""
echo -e ${yellow}"Download complete!"${reset}
echo ""
echo -e ${yellow}"Extracting installation files..."${reset}
tar -xvzf "${app^}".*.tar.gz >/dev/null 2>&1
echo ""
echo -e ${yellow}"Installation files downloaded and extracted."${reset}

# Remove existing installs
echo ""
echo -e "Removing existing installation files from ${orange}[$bindir]..."${reset}
rm -rf "$bindir"
sleep 2
echo ""
echo -e "Attempting to install ${orange}[${app^}]${reset}..."
sleep 2
mv "${app^}" $installdir
chown "$app_uid":"$app_guid" -R "$bindir"
chmod 775 "$bindir"
# Ensure we check for an update in case user installs older version or different branch
touch "$datadir"/update_required
chown "$app_uid":"$app_guid" "$datadir"/update_required
echo ""
echo -e "Successfully installed ${cyan}[${app^}]${reset}!!"
rm -rf "${app^}.*.tar.gz"
sleep 2

# Configure Autostart

# Remove any previous app .service
echo ""
echo "Removing old service file..."
rm -rf /etc/systemd/system/"$app".service
sleep 2

# Create app .service with correct user startup
echo ""
echo "Creating new service file..."
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
sleep 2
echo ""
echo -e "New service file has been created."

# Start the App
echo ""
echo -e "${orange}[${app^}]${reset} is attempting to start, this may take a few seconds..."
systemctl -q daemon-reload
systemctl enable --now -q "$app"
sleep 3

# Check if the service is up and running
echo ""
echo "Checking if the service is up and running... again this might take a few seconds"
# Loop to wait until the service is active
timeout=30
start_time=$(date +%s) #current time in seconds
while ! systemctl is-active --quiet "$app"; do
    current_time=$(date +%s)
    elapsed_time=$((current_time - start_time))
    if (( elsaped_time >= timeout )); then
        echo -e "${red}ERROR!${reset} Service failed to start within $timeout seconds."
        echo -e "${red} EXITING SCRIPT!"
        break
    fi
done
echo ""
echo -e "${orange}[${app^}]${reset} installation and service start up is complete!"

# Finish Installation
host=$(hostname -I)
ip_local=$(grep -oP '^\S*' <<<"$host")
echo ""
echo -e "Attempting to check for a connection at http://$ip_local:$app_port..."
sleep 3

# Use curl to check for connection
response_code=$(curl -s -o /dev/null -w "%{http_code}" "http://$ip_local:$app_port")

if [ "$response_code" = "200" ]; then
    echo ""
    echo "Successful connection!"
    echo ""
    echo -e "Browse to ${green}http://$ip_local:$app_port${reset} for the GUI."
    echo ""
    echo "Script complete! Exiting now!"
    echo ""
else
    echo ""
    echo -e "${red}${app^} failed to start.${reset}"
    echo ""
    echo "Please try again. Exiting script."
    echo
fi

# Exit
exit 0
