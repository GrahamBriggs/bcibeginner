# bci beginner

Learning about bio-sensing using OpenBCI with the brainflow libraries, and hacking on some beginner projects.


## brainHat

brainHat is a software system to provide easy plug and play Lab Streaming Layer (LSL) and brainflow broadcast of data from an OpenBCI biosensing board.

For more detailed information, see the published document here: 
**[brainHat Software System Description](https://docs.google.com/document/d/e/2PACX-1vTXQGazx1bMeUP3yy8naeh7qg_c4RRRfAiN7E3Sr6DkLWUqhxE9w7PBIjzVfpdaYlxAuEuS9O4nrpYw/pub)**

For a quick overview of what the system can do, see the videos here:
- **[brainHat Viewer](https://www.youtube.com/watch?v=tYAy1uis2tA)**
- **[brainHat Beta One Video](https://youtu.be/rwSlOQfDRk4)**

### brainHat Servers
See brainHatServer/brainHat for the C++ server program for the Raspberry Pi. Run it from the command line, or use the included scripts to configure it as a background daemon.

![brainHatDaemon](https://user-images.githubusercontent.com/5171343/112923410-1d540480-90c3-11eb-92e4-bf756578f7a7.png)

See brainHatServer/brainHatSharp for a GUI version of the server program for Windows and Raspberry Pi.

![brainHatServer](https://user-images.githubusercontent.com/5171343/112923172-a585da00-90c2-11eb-9de1-739dcb2998d3.png)


### brainHat Clients
See BrainHatClient for a C# GUI client program for Windows and Raspberry Pi.

![brainHatClient](https://user-images.githubusercontent.com/5171343/112923869-f21de500-90c3-11eb-9a00-fe50b9192685.png)


See brainHatLit for a demo program to light up pins on the Raspberry Pi in response to brainflow data processing events.

![brainHatLit](https://user-images.githubusercontent.com/5171343/112924212-943dcd00-90c4-11eb-8c3b-5eb14d6cab01.png)


###  brainHat Components
See BrainHatComponents for the shared component assemblies used by  the C# server and client programs.

##  Attribution
The brainHat software system uses:

**[Lab Streaming Layer](https://github.com/sccn/labstreaminglayer/blob/master/LICENSE)**

**[brainflow](https://github.com/brainflow-dev/brainflow/blob/master/LICENSE)**

**[EDFlib](https://gitlab.com/Teuniz/EDFlib![brainHatLit])**



### License
MIT
