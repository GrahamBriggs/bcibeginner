<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
          "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en">
<head>
    <title>Processes</title>
    <meta http-equiv="content-type" content="text/html; charset=iso-8859-1" />
    <link rel="stylesheet" href="style.css" type="text/css" />
    <!--[if IE 6]>
    <link rel="stylesheet" href="fix.css" type="text/css" />
    <![endif]-->
</head>
<body>
    <div id="sidebar">
        <div id="logo">  
                <img src="./logo.png" alt="brainHat"  />
        </div>
        <h2>brainHat Server</h2>
        <div id="menu">
            <a  href="index.php">Home</a>
            <a href="wifistatus.php">Wi-Fi Status</a>
			<a href="network.php">Network</a>
            <a class="active" href="performance.php">Processes</a>
            <a href="piinfo.php">System</a>
            <a href="admin.php">Reboot / Shutdown</a>
        </div>
    </div>

    <div id="content">
      
	  <h1><?php echo gethostname();?> </h1>	  
	  <p><h2>Top Processes</h2></p>
	  
	  <meta http-equiv="refresh" content="5">    
	  <div class="content">
<?php
	$output = shell_exec('top -b -n 1 | head -n 15');
	echo "<pre>$output</pre>";
?>

</body>
</html>

