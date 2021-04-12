stopWiFiCtrl = "/usr/bin/sudo /usr/local/sbin/killWiFiCtrl"  
switchToRouter = "/usr/bin/sudo /usr/local/sbin/switchToRouter"  
command = "/usr/bin/sudo /sbin/poweroff"  
import subprocess  
import time  
time.sleep(5)  
process = subprocess.Popen(stopWiFiCtrl.split(), stdout=subprocess.PIPE)  
output = process.communicate()[0]  
print output  
time.sleep(5)  
process = subprocess.Popen(switchToRouter.split(), stdout=subprocess.PIPE)  
output = process.communicate()[0]  
print output  
time.sleep(5)  
process = subprocess.Popen(command.split(), stdout=subprocess.PIPE)  
output = process.communicate()[0]  
print output 
