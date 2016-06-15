EDMSuite
========

This project includes the control code for the molecular beam based experiments at the Centre for Cold Matter, Imperial College London. It is currently used to drive experiments including the YbF electron EDM measurement, laser cooling of CaF and SrF, buffer gas cooling of YbF, Li/LiH sympathetic cooling, and precsion measurement of CH transitions to test the stability of fundamental constants.

For historical reasons this project also includes the low-level analysis code for the electron EDM experiment.

General
=======

The code is not terribly pretty, but it does the job. There is minimal documentation, and what is there should be treated with suspicion!

Building
========

The code should build in Visual Studio 2012. There are several build profiles: the EDMAnalysis profile just builds the EDM analysis code, and has no external dependencies. All of the other projects will require National Instruments NI-DAQ, and the camera based projects need NI-IMAQ.

Contributors
============

The code that makes up the trunk today has seen contributions from:

* Aki Matsushima
* Anne Cournol
* Chris Sinclair
* Devin Dunseith
* Dhiren Kara
* Jack Devlin
* Joe Smallman
* Jony Hudson
* Michael Tarbutt
* Nick Bulleid
* Sean Tokunaga

In addition, early versions of this code, that pre-date this repository, or never made it to the trunk, were contributed to by:

* Henry Ashworth
* Suresh Doravari
* Jalani Kanem

License
=======

The code in this repository is licensed to you under the MIT License. Please see LICENSE.txt for details.
