#!/bin/bash
set -e

# Install the Microsoft package repository
apt-get update
apt-get install -y wget apt-transport-https
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install the .NET SDK
apt-get update
apt-get install -y dotnet-sdk-8.0

echo "Installed .NET SDK"
