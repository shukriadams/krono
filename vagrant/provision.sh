#!/usr/bin/env bash
set -e

# force non-interactive setup
export DEBIAN_FRONTEND=noninteractive

sudo apt-get update

# dotnetcore
sudo apt install dotnet-sdk-6.0 -y

# altecover report generator
dotnet tool install --global dotnet-reportgenerator-globaltool --version 4.1.5

# set up mail send
sudo apt-get install ssmtp -y

# porter
sudo wget https://github.com/shukriadams/porter/releases/download/0.0.2/porter_linux-x64 -O /usr/bin/porter
sudo chmod +x /usr/bin/porter

# force startup folder to vagrant project
echo "cd /vagrant/src" >> /home/vagrant/.bashrc
