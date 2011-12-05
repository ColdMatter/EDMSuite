This is a very incomplete readme file. At the moment it just has some notes about source code organisation and the build system.

Source code organisation and build system
=========================================

The source is organised in to projects. Each of these projects has its own top level folder, and a .csproj file inside that folder. All of the projects are grouped together in a single Visual Studio solution.

There are a couple of problems we face with drivers: 
1) each experiment has its own specialised hardware, complete with specialised drivers. We want to avoid having to install these drivers on every computer just to support one experiment. [For example, the edm experiment has an rf AWG that requires NI-RFSG, and the buffer gas experiment has a camera that requires NI-VISION. It would be better if the decelerator didn't have to install these drivers too.]
2) the analysis code should be able to run anywhere, so we want to be able to build the analysis/data handling part of EDMSuite without installing any hardware (or, more generally non-free) drivers [think parallel analysis on many machines - we don't want to depend on licensed libraries.]

To solve these problems, the source code is organised in a particular way. There is a project called SharedCode that contains the data definitions and basic analysis code. This project has no dependencies on non-free or hardware libraries. The DAQ library contains the bulk of the low-level hardware control code. It is only allowed to reference the "standard" NI drivers: NI-DAQ, and GPIB. This covers the bulk of most of the experimental control. The project ScanMaster is used by all experiments and should only depend on SharedCode and GPIB. It should not depend on any fancy hardware libraries or other projects (this is not possible at the moment for a technical reason, but it's _almost_ true. There's a workaround, described below, that takes care of the problem for the moment.) Other projects can depend on anything they like.

Within Visual Studio there are a number of build configurations. Each build configuration has a separate project configuration for each project. The way it works is that if you select the "Decelerator" configuration, it will only build the projects that are relevant to the decelerator. In particular this implies that only the library dependencies of the projects that are built need to be met on the build computer. [The other projects will show with broken references in Visual Studio, but this isn't a problem, just a reminder that you can't build them.]

To hammer the point home, consider the build configuration "EDMAnalysis" which only builds SharedCode and SirCachealot. These projects have no dependence on any external installed libraries, so this configuration will build on a machine with only Visual Studio installed.

Fly in the ointment (advanced)
------------------------------

There's one fly in this otherwise elegant ointment, the DecelerationHardwareController. ScanMaster has a specialised scan mode where it signals the DHC to move the laser, rather than actually moving the laser itself. This breaks the above picture, because it means SM has to know about the DHC at build time, in order to be able to call its methods. So to build SM for the decelerator there must be a reference to DHC in the SM code.

In the longer term, we should fix this. The problem is that we're using .NET remoting, which is typesafe. This means that the compiler needs to be able to see the assembly of the remoting target to check that the remote method invocations are valid. We'd be better off using a non-typesafe RPC mechanism - one of the many standard web mechanisms would do. This, combined with changing the data storage method to JSON would solve many other problems, and is something that one day I'll get around to doing.

In the meantime there's a workaround. When the "Decelerator" build configuration is active, the projects all switch to the "Decelerator" project configurations. The "Decelerator" project configuration for SM defines a pre-processor variable, Decelerator. This is used build-configuration-selectively enable the code in SM that talks to DHC. The other trick is to disable the reference to DHC in SM for every other build configuration. This is done by manually adding a condition to the .csproj xml file that only activates the DHC reference if the Decelerator build configuration is active.

This mechanism is pretty messy so we should avoid using it if at all possible.
