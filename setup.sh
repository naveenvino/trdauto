#!/usr/bin/env bash
set -e

REQUIRED_MAJOR_VERSION=8

if command -v dotnet >/dev/null 2>&1; then
    INSTALLED_VERSION=$(dotnet --version | cut -d. -f1)
    if [ "$INSTALLED_VERSION" = "$REQUIRED_MAJOR_VERSION" ]; then
        echo ".NET SDK $REQUIRED_MAJOR_VERSION is already installed."
        exit 0
    fi
fi

# Install .NET SDK 8.0 on Debian/Ubuntu
sudo apt-get update
sudo apt-get install -y wget apt-transport-https
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

