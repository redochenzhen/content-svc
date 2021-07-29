#!/bin/bash

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

chmod a+x "${SCRIPT_DIR}/mc"

#####################
#TODO: necessary?
#yum install -y cabextract fontconfig xorg-x11-font-utils || true
yum install -y  https://downloads.sourceforge.net/project/mscorefonts2/rpms/msttcore-fonts-installer-2.6-1.noarch.rpm || true