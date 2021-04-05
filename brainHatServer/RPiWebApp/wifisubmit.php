


<?php
$mode = $_GET["radioSetting"];
if( $mode == "server" )
{
	$ip_server = $_SERVER['SERVER_ADDR']; 
	$socket = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
	if(!is_resource($socket)) onSocketFailure("Failed to create socket");
	socket_connect($socket, "$ip_server", 48888)
		or onSocketFailure("Failed to connect to $ip_server:48888", $socket);
		
	socket_write($socket, "\$LBP_SET_MODE,Master");
	
	$response = socket_read($socket, 5024, PHP_BINARY_READ);
	$hostName = gethostname();

	if (strpos ($response, 'ACK') >= 0 )
	{
		header("Location: ./wifiwaitforchange.php?mode=master&success=true&hostname=$hostName");
	} 
	else 
	{
		header("Location: ./wifiwaitforchange.php?mode=master&success=false&hostname=$hostName");
	}
} 
else if ( $mode == "client")
{
?> <h2>Setting client mode</h2> <br>  <?php

	$ap = $_GET["category"];
	$pw = $_GET["password"];
	$request = '$LBP_SET_AP,'.$ap.','.$pw;

	$ip_server = $_SERVER['SERVER_ADDR']; 
	$socket = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
	if(!is_resource($socket)) onSocketFailure("Failed to create socket");
	socket_connect($socket, "$ip_server", 48888)
		or onSocketFailure("Failed to connect to $ip_server:48888", $socket);
		
	socket_write($socket, $request);
	
	$response = socket_read($socket, 5024, PHP_BINARY_READ);
	if (strpos ($response, 'ACK') >= 0 ) 
	{
		header("Location: ./wifiwaitforchange.php?mode=managed&success=true&hostname=$ap");
	} else 
	{
		header("Location: ./wifiwaitforchange.php?mode=managed&success=false&hostname=$ap");
	}
}
?>


</body>
</html>