<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
          "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en">
<head>
    <title>Wi-Fi Status</title>
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
            <a class="active"  href="wifistatus.php">Wi-Fi Status</a>
			<a href="network.php">Network</a>
            <a href="performance.php">Processes</a>
            <a href="piinfo.php">System</a>
            <a href="admin.php">Reboot / Shutdown</a>
        </div>
    </div>

    <div id="content">
		<h1><?php echo gethostname();?> </h1>
		<p><h2>Wi-Fi Status</h2><p>

    
<?php 
	$ip_server = $_SERVER['SERVER_ADDR']; 
    echo "Server IP Address is: $ip_server"; 
	echo "<br>";
	echo "<br>";


	$socket = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);

	if(!is_resource($socket)) onSocketFailure("Failed to create socket");

	socket_connect($socket, "$ip_server", 48888)
        or onSocketFailure("Failed to connect to $ip_server:48888", $socket);
	
	socket_write($socket, "\$LBP_GET_STATUS");	
	$line = socket_read($socket, 5024, PHP_NORMAL_READ);
	$response = explode(',', $line);
	echo $response[2];
	
	echo "<br>";
	echo "<br>";
	
	socket_write($socket, "\$LBP_GET_APS");
	
	$line2 = socket_read($socket, 5024, PHP_BINARY_READ);
	$response2 = explode("\n", $line2);

	foreach ($response2 as &$value) {
		echo $value;
		echo "<br>";
	}
	
	echo "<br>";

	socket_close($socket);
?> 
    

	<u><h3>
<a href="wifisettings.php">Configure Wi-Fi</a></h3></u>
    
</div>





</body>
</html>
