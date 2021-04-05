<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
          "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en">
<head>
    <title>System Info</title>
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
            </a>
        </div>
        <h2>brainHat Server</h2>
        <div id="menu">
            <a href="index.php">Home</a>
            <a href="wifistatus.php">Wi-Fi Status</a>
			<a href="network.php">Network</a>
            <a href="performance.php">Processes</a>
            <a class="active" href="piinfo.php">System</a>
            <a href="admin.php">Reboot / Shutdown</a>
        </div>
    </div>

    <div id="content">
      <h1><?php echo gethostname();?> </h1>
	  <p><h2>Services</h2></p>
	  
<?php
    echo "<h3>Wi-Fi</h3>";
    $memory = shell_exec('service hostapd status');
    echo "<pre>$memory</pre>";
    ?>

<?php
    echo "<h3>DHCP</h3>";
    $memory = shell_exec('systemctl status dhcpcd');
    echo "<pre>$memory</pre>";
    ?>

		
		
<?php
	 echo "<h3>Samba</h3>";
    $fileshare = shell_exec('systemctl status smbd');
    echo "<pre>$fileshare</pre>";
?>

<?php
	 echo "<h3>PHP</h3>";
    $version = shell_exec('php --version');
    echo "<pre>$version</pre>";

	
?>
	<ul>
		<li><u><a href="phpinfo.php">PHP info</a></u></li>
	</ul>
	
	<h2>Hardware</h2>

<?php
	echo "<h3>Memory</h3>";
$memory = shell_exec('free -htl');
    echo "<pre>$memory</pre>";

    echo "<h3>Disk Usage</h3>";
    $disk = shell_exec('df -h');
    echo "<pre>$disk</pre>"
?>



    </div>
  </body>
</html>







