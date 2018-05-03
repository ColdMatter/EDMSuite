(* ::Package:: *)

(************************************************************************)
(* This file was generated automatically by the Mathematica front end.  *)
(* It contains Initialization cells from a Notebook file, which         *)
(* typically will have the same name as this file except ending in      *)
(* ".nb" instead of ".m".                                               *)
(*                                                                      *)
(* This file is intended to be loaded into the Mathematica kernel using *)
(* the package loading commands Get or Needs.  Doing so is equivalent   *)
(* to using the Evaluate Initialization Cells menu command in the front *)
(* end.                                                                 *)
(*                                                                      *)
(* DO NOT EDIT THIS FILE.  This entire file is regenerated              *)
(* automatically each time the parent Notebook file is saved in the     *)
(* Mathematica front end.  Any changes you make to this file will be    *)
(* overwritten.                                                         *)
(************************************************************************)



BeginPackage["SEDM3`EDMSuite`", "NETLink`","JLink`"];


initialiseSharedCode::usage="Reinstalls .NET/Link and reloads the EDMSuite dlls.";
createBlockSerializer::usage="Creates a block serializer, which is available as SEDM3`EDMSuite`$blockSerializer.";
createScanSerializer::usage="Creates a scan serializer, which is available as SEDM3`EDMSuite`$scanSerializer.";
connectToSirCachealot::usage="Creates a .NET remoting connection to SirCachealot. An instance of SirCachealots Controller object is made available as SEDM3`EDMSuite`$sirCachealot.";



$blockSerializer=Null;
$scanSerializer=Null;


Begin["`Private`"];


initialiseSharedCode[]:=Module[{},
ReinstallNET[];
LoadNETAssembly[$TopDirectory<>"\\AddOns\\Applications\\SEDM3\\Libraries\\SharedCode.dll"];
LoadNETAssembly[$TopDirectory<>"\\AddOns\\Applications\\SEDM3\\Libraries\\SirCachealot.exe"];
LoadNETType["System.Runtime.Remoting.RemotingConfiguration"];
LoadNETType["System.Type"];
];


createBlockSerializer[]:=SEDM3`EDMSuite`$blockSerializer = NETNew["Data.EDM.BlockSerializer"];


createScanSerializer[]:=SEDM3`EDMSuite`$scanSerializer = NETNew["Data.Scans.ScanSerializer"];

connectToSirCachealot[]:=Module[{},
RemotingConfiguration`RegisterWellKnownClientType[Type`GetType["SirCachealot.Controller, SirCachealot"],"tcp://localhost:1180/controller.rem"];
SEDM3`EDMSuite`$sirCachealot =NETNew["SirCachealot.Controller"]
]


End[];
EndPackage[];