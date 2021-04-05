<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
          "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en">
<head>
    <title>Reboot</title>
    <meta http-equiv="content-type" content="text/html; charset=iso-8859-1" />
    <link rel="stylesheet" href="style.css" type="text/css" />
</head>
<body>
    <div id="sidebar">
        <div id="logo">  
                <img src="./logo.png" alt="brainHat"  />
            </a>
        </div>
        <h2>brainHat Server</h2>
        <div id="menu">
            <a href="index.php">Home</a>
        </div>
    </div>

    <div id="content">
         <h1><?php echo gethostname();?> </h1>
        <p><h2>Reboot / Shut Down</h2></p>
        <br>
        <br>
   
		
<?php  
	$action = $_GET["action"];  
  
	if ($action == "reboot")
	{  
        print '<h3>Server is rebooting ...</h3>';  
	}  
	else if ($action == "shutdown")
	{
        print '<h3>Server is shutting down ...</h3>';
	}
	else
	{
		print 'ERROR'; 
	}
?>  
   

   
    </div>
</body>
</html>
