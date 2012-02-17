What is it?
===========
Luces is a .NET Windows service for setting up a build light, using standard Windows API calls via [NUSB](https://github.com/thenathanjones/nusb).

Why do this?
============
Borne out of frustration from trying to get [blinky](https://github.com/perryn/blinky) (which I truly love the simplicity of) running with libusb on Windows.  I figured a pure .NET version might be simpler and gave me something to code.

Surely this exists?
===================
It does, it's called [blinky](https://github.com/perryn/blinky).  I gave after having trouble getting it to work on Windows though.  By all means though, on a UNIX-y platform, use blinky, it's great.  I may also get around to trying it again on Windows down the track.

How do I use this?
==================
Pre-requisites
--------------
I've built Luces as a .NET 4 project, so the runtime will need to be installed.
Installation
------------
Simplest way to get started is to grab the installer from [HERE](https://github.com/downloads/thenathanjones/luces/Luces-0.2.msi) which is the latest one I've bothered to produce.  
Or, if you're feeling up for it, grab the whole lot and build it yourself.  The dependencies are managed by NuGet, and for the moment it's a .NET 4 project, although it's quite possible it will work on earlier versions of the framework.
Configuration
-------------
On startup, Luces will look for a configuration file in the installed directory called "luces.yml".  If one isn't present, it will create one with a sample configuration and close.  You may supply the full path to an alternative config file as an argument to the service. 
Here is an example config:

     # Configuration file for Luces build light tool
     -
       servertype: CruiseControl
       url: http://10.1.1.2:8153/go/cctray.xml
       username: 
       password: 
       pipelines:
         -
           name: "Trunk :: spec"
           
At this point in time it's the standard [Burro](https://github.com/thenathanjones/burro) configuration.  Check that project for details.
It will only read the configuration file on startup, so if you make any changes to the file, you will need to restart the service to pick them up.
Running Luces
-------------
The intention is to run Luces as a Windows service which is how it is installed.  It can also run as a standalone file just by running the executable.  If any errors occur when running it, they will appear in the Windows Event log.

Limitations
===========
At this point in time it only supports USB devices, in particular the Delcom Engineering V2 light because that's all I had access to.  If you would like to contribute alternate lights, by all means, the underlying [NUSB](https://github.com/thenathanjones/nusb) library provides numerous ways of communicating with USB devices.  
The types of build servers supported are again limited by whatever [Burro](https://github.com/thenathanjones/burro) is capable of, and as such check that project for what you can use.
