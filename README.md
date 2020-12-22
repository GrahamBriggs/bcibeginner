# bci beginner

Learning about bio-sensing using OpenBCI with the brainflow libraries, and hacking on some beginner projects.



## brainHat

brainHat is a software system to provide easy plug and play Lab Streaming Layer (LSL) broadcast of data from the OpenBCI biosensing boards.

For more detailed information, see the published document here: 
**[brainHat Software System Description](https://docs.google.com/document/d/e/2PACX-1vTXQGazx1bMeUP3yy8naeh7qg_c4RRRfAiN7E3Sr6DkLWUqhxE9w7PBIjzVfpdaYlxAuEuS9O4nrpYw/pub)**

For a quick overview of what the system can do, see the video here:
**[brainHat Beta One Video](https://youtu.be/rwSlOQfDRk4)**

### brainHat Servers
See brainHatServer/brainHat for the C++ server program for the Raspberry Pi (stable beta).

See brainHatServer/brainHatSharp for a C# implementation of the server for Windows and Raspberry Pi (experimental alpha).

### brainHat Clients
See BrainHatClient for the C# GUI client program for Windows and Raspberry Pi (stable beta).

See brainHatLit for a demo program to light up pins on the Raspberry Pi in response to brainflow data processing events.

###  brainHat Components
See BrainHatComponents for the shared component assemblies used by  the C# server and client programs.

##  Attribution
The brainHat software system uses:

**[Lab Streaming Layer](https://github.com/sccn/labstreaminglayer/blob/master/LICENSE)**

**[brainflow](https://github.com/brainflow-dev/brainflow/blob/master/LICENSE)**



### License
MIT
