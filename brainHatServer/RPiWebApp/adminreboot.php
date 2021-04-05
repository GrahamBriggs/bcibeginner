
    
<?php  
   $action = $_GET["action"];  
  
  if ($action == "reboot")
  {  
        exec('bash -c "exec nohup setsid python /usr/share/nginx/html/reboot.py > /dev/null 2>&1 &"');
		header("Location: ./adminwaitforreboot.php?action=reboot");
  }  
  else if ($action == "shutdown")
  {
        exec('bash -c "exec nohup setsid python /usr/share/nginx/html/shutdown.py > /dev/null 2>&1 &"');
		header("Location: ./adminwaitforreboot.php?action=shutdown");
  }
  else
  {
		print 'ERROR'; 
  }
?>  
   