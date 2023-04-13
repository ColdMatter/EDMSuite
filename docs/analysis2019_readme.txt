============================================================
Instructions and general information for analysing EDM data
		      dated: 30/10/2019
============================================================


**********************
DATA STRUCTURE
**********************

A Block (I'm going to capitalise all .NET objects) consists of an ArrayList of EDMPoints. This list is as long as the number of points in a block, that is 4096 for now. It also contains a BlockConfig which contains the waveform codes for the different switches (called Modulations).

Each EDMPoint contains a Shot object and a SinglePointData hashtable. The SinglePointData hashtable just returns the value of an analog input that is sampled once per shot (to use: SinglePointData["analogInputName"]). 

Each Shot contains an ArrayList of TOFs, and the length of this list is given by the number of detectors that is sampled in time in every shot. To find the detector you want, you need to first find the index of the detector via b.detectors.IndexOf("detector"). Then TOFs[index] will return the TOF for the detector.

Each TOF contains a list of times: Times, and a list of values: Data.

Additionally, you may come across a TOFWithError object, which is just a derived class of TOF and has an extra list of errors on each value: Errors.

After BlockHead acquires one block of data, some extra "detectors" are added in order to make the analysis easier. These are: background-subtracted PMT detectors, the subtracted laser background for each PMT detector, the bottom detector signal scaled in time to match the signal in the top detector, and the point-by-point calculation of the asymmetry. The block is then serialised and saved as an Xml file (block.xml) and compressed into a zip file with the name given by the cluster name and cluster index. These files can be found in Data\sedm\v3 in the edm Box directory.

**********************
BLOCK DEMODULATION
**********************

Blocks can be demodulated into its channels by the DemodulateBlock function in the BlockDemodulator class, returning a DemodulatedBlock object. Demodulation is the process of extracting the signal from a detector that switches according to a certain set of switches, averaging over the other switches. This is done by multiplying the signal of each point by the combined switch state of the switches we are interested in, and averaging over all the points in the block. This gives a channel that is identified by its unique set of switches. Since there is some redundancy in the number of points we take (number of unique states < number of points in a block), this allows us to associate an error to each channel value.

To demodulate a block, we need to supply a DemodulationConfig, which consists of lists of detectors classified by its type:

A "TOF detector" (not a class/object in this case) is a detector that will be demodulated point-by-point into its channels, returning a TOFWithError object. 
A "gated detector" is a detector that is sampled as a TOF, but will be gated prior to demodulation. The gating information is stored in a Gate object, which just has the start and end times of the gate, as well as a flag to indicate whether the signal will be integrated or averaged over the gating period. After demodulation, we obtain a PointWithError object which is, literally, a point value and its error.
A "point detector" is a single point detector that is demodulated to give a PointWithError object.

By changing the DemodulationConfig, we can change which detectors are included in the demodulation and how we demodulate them (gate information for example). Currently, the way to create such a config is to edit the DemodulationConfig class and include a function that returns a DemodulationConfig object based on what we want. There are some examples in the code.

Additionally, the demodulation also picks out some detectors (currently hard-coded in: the asymmetry, scaled bottom probe (bg-subtracted) and top probe (bg-subtracted)) and appends these detectors with special channels to ease the analysis later on. These channels are combinations of channels such as the EDM channel {E.B}/{DB} that are useful for later analysis.

The DemodulatedBlock object that is returned contains a TimeStamp for the block, its BlockConfig, its DemodulationConfig, a hashtable of DetectorCalibrations, and hashtables for "TOFChannelSets" and "PointChannelSets" for each detector. A ChannelSet is a collection of channels for all the different switch states in a block. These channels can either be in the form of TOFWithErrors or PointWithErrors. TOF detectors give TOF channel sets, and gated/point detectors gives point channel sets. These can be extracted from the demodulated block via the convenience functions GetTOFChannel and GetPointChannel.

**********************
EDM DATABASE
**********************

For ease of retrieval and identification of blocks, the demodulated blocks are stored in a MySQL database. There should only be one EDM database that everyone has access to. The database consists of three tables:

"dblocks" which describes each demodulated block entry in the database by its UID (unique identifier), cluster name, cluster index, analysis tag (name of the demodulation config used to demodulate the block), the manual states, block timestamp and the voltages on the E-field plates.
"dblockdata" which stores the demodulated block object in binary, identified by its UID.
"tags" which stores tags for each block, described by its cluster name and index.

The database will ideally be hosted on the EDM analysis computer in the lab. For the moment, it's hosted on my computer in the level 7 office. Everyone will receive a username with a password to access this database.

**********************
ANALYSIS TOOLS
**********************

The first tool you will need is SirCachealot - I'll explain how to install all of this below. SirCachealot allows you to interact with the database via exposed .NET functions (so you don't have to learn MySQL or remember its syntax). It allows you to take a block file (zipped Xml), demodulate it based on some demodulation configuration, and add it to the database. It allows you to extract a demodulated block from the database based on certain criteria (manual states, cluster names, tags, etc.).

The next tool is the set of analysis packages written in Mathematica - I've called it SEDM4 (it probably has gone through enough of a change to warrant a version upgrade from SEDM3). These contain a considerable amount of helper functions to make life easier when analysing blocks. I've tried to write useful usage messages so you don't have to keep referring to the notebooks themselves when running analyses.

Finally, there are Mathematica notebooks to analyse the data. The most important ones are:

Database control: Adds blocks and clusters to the database via SirCachealot. Can add tags to individual blocks/entire clusters. Provides a good record of all the data taken for the experimental run.
Block spy: Peek into a block and look at various channels and TOF shapes. Can be used to help decide whether to include/exclude a block from analysis.
Cluster view: Look at cluster-level statistics and trends. Can be used to make sure servos are working properly, and to spot bad blocks in a cluster.
Run analysis: Takes any number of clusters and looks at the edm as well as other channels. Clusters going into here should have already been properly tagged.

**********************
INSTALLATION NOTES
**********************

1. Install Visual Studio Community 2019. It's free!

2. Install Mathematica 12 (no need for ErrorListPlot anymore in this version).

3. Install MySQL 8.0 (I installed MySQL Server, MySQL Shell, MySQL for Visual Studio and Connector/NET).

4. Configure MySQL so that it can receive/send large messages (in this case our demodulated blocks). To do this:
	* Navigate to "my.ini" - on my computer this is in C:\ProgramData\MySQL\my.ini (it's a hidden folder).
	* Open my.ini in Notepad++/Atom/Visual Studio Code (Notepad adds hex characters to the beginning of the file and messes up the file when you save it).
	* Find max_allowed_packet and change its value to 32M (default is 4M).
	* Find innodb_buffer_pool_size and change its value to 64M (default is 8M).

5. Get EDMSuite on your computer. Navigate to the folder you want to put EDMSuite in and run Git Bash here. Note that we're on the SEDM4-development branch for now. The commands you need are:
	* git clone https://github.com/ColdMatter/EDMSuite.git
	* cd EDMSuite
	* git checkout -b SEDM4-development remotes/origin/SEDM4-development

6. Run EDMSuite in Visual Studio and select the EDMAnalysis build configuration. Build the solution (which should only include SharedCode and SirCachealot, which are both NI-free).

7. Update the .NET libraries that Mathematica will use by running "..\EDMSuite\SEDM4\Tools\RefreshLibraries.bat".

8. Add EDMSuite to Mathematica's $Path by running the command in "..\EDMSuite\SEDM4\Tools\IncludePath.nb".

9. Analyse away! 

Note: SirCachealot needs to be running in order for Mathematica to use its functions. It can be run from "..\EDMSuite\SEDM4\Tools\RunSirCachealot.bat". When you run SirCachealot, a prompt window will appear asking for your username/password.