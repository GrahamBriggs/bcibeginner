<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
          "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" xml:lang="en">
<head>
    <title>brainHat</title>
    <meta http-equiv="content-type" content="text/html; charset=iso-8859-1" />
    <link rel="stylesheet" href="style.css" type="text/css" />
</head>

<body>
    <div id="sidebar">
        <div id="logo">  
            <img src="./logo.png" alt="brainHat"  />
        </div>
        <h2>brainHat Server</h2>
        <div id="menu">
            <a class="active" href="index.php">Home</a>
            <a href="wifistatus.php">Wi-Fi Status</a>
			<a href="network.php">Network</a>
            <a href="performance.php">Processes</a>
            <a href="piinfo.php">System</a>
            <a href="admin.php">Reboot / Shutdown</a>
        </div>
    </div>
    
	<div id="content">
        <h1><?php echo gethostname();?> </h1>
		<p><h2>Page Title</h2><p>


    </div>

</body>
</html>