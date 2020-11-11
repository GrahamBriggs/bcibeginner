Open BCI brainflow Library Test Projects

MONOhatSimulator:
* Runs on Windows or Raspberry Pi using Mono
* Connects to the Cyton board using the dongle
*   or, can run as simulator, playing back data in OpenBCI_GUI .txt format
* Runs data processing engine, can detect blinks and alpha waves
* Turns on LED lights when blinks are detected (RPi version only)
* Broadcasts raw data over UDP socket for haClient to display in a user interface


brainflowsharp
* hatClient application project is GUI to view data from hatSimulator
* can log data in OpenBCI_GUI .txt format.