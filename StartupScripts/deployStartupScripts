#!/bin/bash  

echo "Copying brainHatStart to /etc/init.d"
cp ./etc/init.d/brainHatStart /etc/init.d/brainHatStart
chmod +x /etc/init.d/brainHatStart

echo "Copying brainHat.service to /etc/systemd/system"
cp ./etc/systemd/system/brainHat.service /etc/systemd/system/brainHat.service

echo "Don't forget to edit /etc/systemd/system/brainHat.service to specify your board type."
