<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
          "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en">
<head>
    <title>Set Wi-Fi</title>
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
            <a class="active"  href="wifistatus.php">Wi-Fi Status</a>
        </div>
    </div>

    <div id="content">

	<h1><?php echo gethostname();?> </h1>       
	<p><h2>Wi-Fi Settings</h2><p>
	



	<form method="get" action="wifisubmit.php" id="form">
	
<?php
		
		$ip_server = $_SERVER['SERVER_ADDR']; 
		$socket = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
		if(!is_resource($socket)) onSocketFailure("Failed to create socket");
		socket_connect($socket, "$ip_server", 48888)
			or onSocketFailure("Failed to connect to $ip_server:48888", $socket);
		
		socket_write($socket, "\$LBP_GET_APS");

		$res = socket_read($socket, 5024, PHP_BINARY_READ);
		$accessPoints = explode(',', $res);
		$listAccessPoints = explode("\n", $accessPoints[2]);
		
		$busy = False;
		foreach ($listAccessPoints as &$value)
		{
			if ( strcmp($value,'busy') == 0 )
			{
				print '<p>Server is busy.</p>';
				$busy = True;
				break;
			}
		}
		
	$ip_server = $_SERVER['SERVER_ADDR']; 
	$socket = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
	
	if(!is_resource($socket)) onSocketFailure("Failed to create socket");
	socket_connect($socket, "$ip_server", 48888)
		or onSocketFailure("Failed to connect to $ip_server:48888", $socket);
	
	socket_write($socket, "\$LBP_GET_MODE");
	$res2 = socket_read($socket, 5024, PHP_BINARY_READ);
	socket_close($socket);
	
	$response = explode(',', $res2);
	$mode = $response[2];
	$mode = trim($mode);
	
	
	if ( $busy == False ) 
	{
		if ( strcmp($mode,'Managed') == 0) 
	{
		print '<input type="radio" name="radioSetting" value="server" id="serverRadio" onchange="myFunction()"> Server Mode <br>';
		print '<input type="radio" name="radioSetting" value="client" id="clientRadio" checked="true" onchange="myFunction()"> Client Mode <br>';
		
		print '<div id="clientControls" style="display:block">';

	}
	else if ( strcmp($mode,'Master') == 0) 
	{

		print '<input type="radio" name="radioSetting" value="server" id="serverRadio" checked="true" onchange="myFunction()"> Server Mode <br>';
		print '<input type="radio" name="radioSetting" value="client" id="clientRadio" onchange="myFunction()"> Client Mode <br>';
		
		print '<div id="clientControls" style="display:none">';
	}
	
		print '<br>';
		print '<br>';
		print 'Access Point<br>';
	
		print '<select name="category">';
		
		

		foreach ($listAccessPoints as &$value)
		{
			if ( strlen($value) > 1 )
			{
				print '<option value="'.$value.'">'.$value.'</option>';
			}
		}
		socket_close($socket);	
	
		print '</select>';
		
		
		print '<br>';
		print '<br>';
		print '<label>Password:</label> <br>';
		print '<input type="text" name="password" id="password" /> <br> <br>';

		print '</div>';
		print '<br>';
		print '<br>';

		print '<input type="submit" name="submit" id="submit" value="Submit">';
	}

?>  

<br> 
<br>




</form>
  
</div>

 
	
	
<script>

function myFunction() {  
	if(document.getElementById("serverRadio").checked) {
		document.getElementById("clientControls").style.display = "none";
	}else if(document.getElementById("clientRadio").checked) {
		document.getElementById("clientControls").style.display = "block";
	}
}
	
 </script>
 
 
</body>
</html>
