<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
          "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en">
<head>
    <title>Set Wi-Fi</title>
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
        </div>
    </div>

    
	<div id="content">
	
		<h1><?php echo gethostname();?> </h1>
<br>
<br>


<?php
$mode = $_GET["mode"];
if( $mode == "master" )
{
	print '<h2>Setting server mode</h2> <br>';
	
	$success = $_GET["success"];
	$hostname = $_GET["hostname"];

	
	if ($success == "true")
	{
		print '<p>Set Wi-Fi to server mode. In a few moments you should see the access point '. $hostname. '.</p>';
	} 
	else 
	{
		print '<p>Failed to set server mode. The program is busy.</p>';
	}
} 
else if ( $mode == "managed")
{
	print '<h2>Setting client mode</h2> <br>';

	$success = $_GET["success"];
	$ap = $_GET["hostname"];

	if ($success == "true")
	{
		print '<p>Set Wi-Fi to client mode. In a few moments this device should appear on the network ' . $ap . '.</p>';
	} 
	else 
	{
		print '<p>Failed to set client mode. The program is busy.</p>';
	}
}
?>


</body>
</html>