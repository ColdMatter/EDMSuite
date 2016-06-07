[1mdiff --git a/BuffergasHardwareControl/BuffergasHardwareControl.csproj b/BuffergasHardwareControl/BuffergasHardwareControl.csproj[m
[1mindex e83ec73..8f5bbd9 100644[m
[1m--- a/BuffergasHardwareControl/BuffergasHardwareControl.csproj[m
[1m+++ b/BuffergasHardwareControl/BuffergasHardwareControl.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m
[32m+[m[32m<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m[41m[m
   <PropertyGroup>[m
     <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>[m
     <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>[m
[36m@@ -10,7 +10,7 @@[m
     <AppDesignerFolder>Properties</AppDesignerFolder>[m
     <RootNamespace>BuffergasHardwareControl</RootNamespace>[m
     <AssemblyName>BuffergasHardwareControl</AssemblyName>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5.1</TargetFrameworkVersion>[m[41m[m
     <FileAlignment>512</FileAlignment>[m
     <FileUpgradeFlags>[m
     </FileUpgradeFlags>[m
[36m@@ -26,6 +26,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -34,9 +35,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -45,6 +48,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -53,6 +57,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -61,6 +66,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -69,14 +75,15 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.DAQmx, Version=9.2.40.82, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
[31m-      <SpecificVersion>False</SpecificVersion>[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.DAQmx, Version=15.5.45.109, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=x86">[m[41m[m
       <HintPath>C:\Program Files (x86)\National Instruments\MeasurementStudioVS2010\DotNET\Assemblies\Current\NationalInstruments.DAQmx.dll</HintPath>[m
     </Reference>[m
     <Reference Include="NationalInstruments.UI, Version=9.0.40.292, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL">[m
[1mdiff --git a/BuffergasHardwareControl/Properties/Resources.Designer.cs b/BuffergasHardwareControl/Properties/Resources.Designer.cs[m
[1mindex 17bfd1a..a809317 100644[m
[1m--- a/BuffergasHardwareControl/Properties/Resources.Designer.cs[m
[1m+++ b/BuffergasHardwareControl/Properties/Resources.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.239[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/BuffergasHardwareControl/Properties/Settings.Designer.cs b/BuffergasHardwareControl/Properties/Settings.Designer.cs[m
[1mindex 1416465..4de6994 100644[m
[1m--- a/BuffergasHardwareControl/Properties/Settings.Designer.cs[m
[1m+++ b/BuffergasHardwareControl/Properties/Settings.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.239[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[36m@@ -12,7 +12,7 @@[m [mnamespace BuffergasHardwareControl.Properties {[m
     [m
     [m
     [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()][m
[31m-    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")][m
[32m+[m[32m    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")][m[41m[m
     internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {[m
         [m
         private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));[m
[1mdiff --git a/BuffergasHardwareControl/app.config b/BuffergasHardwareControl/app.config[m
[1mindex 27d3fab..1e39b5c 100644[m
[1m--- a/BuffergasHardwareControl/app.config[m
[1m+++ b/BuffergasHardwareControl/app.config[m
[36m@@ -12,4 +12,4 @@[m
             </setting>[m
         </BuffergasHardwareControl.Properties.Settings>[m
     </userSettings>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup></configuration>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5.1"/></startup></configuration>[m[41m[m
[1mdiff --git a/DAQ/DAQ.csproj b/DAQ/DAQ.csproj[m
[1mindex 0c2568c..629b428 100644[m
[1m--- a/DAQ/DAQ.csproj[m
[1m+++ b/DAQ/DAQ.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">[m
[32m+[m[32m<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="12.0">[m[41m[m
   <PropertyGroup>[m
     <ProjectType>Local</ProjectType>[m
     <ProductVersion>9.0.21022</ProductVersion>[m
[36m@@ -21,13 +21,14 @@[m
     <OutputType>Library</OutputType>[m
     <RootNamespace>DAQ</RootNamespace>[m
     <RunPostBuildEvent>OnBuildSuccess</RunPostBuildEvent>[m
[31m-    <StartupObject>NIDAQ.Pattern.Test.PatternBuilderTest</StartupObject>[m
[32m+[m[32m    <StartupObject>[m[41m[m
[32m+[m[32m    </StartupObject>[m[41m[m
     <FileUpgradeFlags>[m
     </FileUpgradeFlags>[m
     <UpgradeBackupLocation>[m
     </UpgradeBackupLocation>[m
     <OldToolsVersion>3.5</OldToolsVersion>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <TargetFrameworkProfile />[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Decelerator|AnyCPU' ">[m
[36m@@ -40,6 +41,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -51,9 +53,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -65,6 +69,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -76,6 +81,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -87,6 +93,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -101,19 +108,19 @@[m
     <CodeAnalysisIgnoreBuiltInRuleSets>false</CodeAnalysisIgnoreBuiltInRuleSets>[m
     <CodeAnalysisIgnoreBuiltInRules>false</CodeAnalysisIgnoreBuiltInRules>[m
     <CodeAnalysisFailOnMissingRules>false</CodeAnalysisFailOnMissingRules>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
     <Reference Include="alglibnet2">[m
       <HintPath>.\alglibnet2.dll</HintPath>[m
     </Reference>[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.DAQmx, Version=9.6.40.57, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
[31m-      <Private>False</Private>[m
[31m-    </Reference>[m
[31m-    <Reference Include="NationalInstruments.VisaNS, Version=13.0.40.167, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.DAQmx, Version=15.5.45.109, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=x86" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.VisaNS, Version=13.0.45.167, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="System">[m
       <Name>System</Name>[m
     </Reference>[m
[1mdiff --git a/DAQ/PXIEDMHardware.cs b/DAQ/PXIEDMHardware.cs[m
[1mindex 7b128b9..a943357 100644[m
[1m--- a/DAQ/PXIEDMHardware.cs[m
[1m+++ b/DAQ/PXIEDMHardware.cs[m
[36m@@ -75,6 +75,7 @@[m [mnamespace DAQ.HAL[m
 [m
             // add the GPIB/RS232 instruments[m
             Instruments.Add("green", new HP8657ASynth("GPIB0::7::INSTR"));[m
[32m+[m[32m            Instruments.Add("gigatronix", new Gigatronics7100Synth("GPIB0::19::INSTR"));[m[41m[m
             Instruments.Add("red", new HP3325BSynth("GPIB0::12::INSTR"));[m
             Instruments.Add("4861", new ICS4861A("GPIB0::4::INSTR"));[m
             Instruments.Add("bCurrentMeter", new HP34401A("GPIB0::22::INSTR"));[m
[36m@@ -252,8 +253,15 @@[m [mnamespace DAQ.HAL[m
             AddAnalogOutputChannel("I2LockBias", aoBoard + "/ao5", 0, 5);[m
 [m
             //Microwave Control Channels[m
[31m-            AddAnalogOutputChannel("uWaveDCFM", aoBoard + "/a011", -2.5, 2.5);[m
[32m+[m[32m            AddAnalogOutputChannel("uWaveDCFM", aoBoard + "/ao11", -2.5, 2.5);[m[41m[m
             AddAnalogOutputChannel("uWaveMixerV", aoBoard + "/ao12", 0, 10);[m
[32m+[m[32m            AddAnalogOutputChannel("VCO161Amp", aoBoard + "/ao13", 0, 10);[m[41m[m
[32m+[m[32m            AddAnalogOutputChannel("VCO161Freq", aoBoard + "/ao14", 0, 10);[m[41m[m
[32m+[m[32m            AddAnalogOutputChannel("VCO30Amp", aoBoard + "/ao15", 0, 10);[m[41m[m
[32m+[m[32m            AddAnalogOutputChannel("VCO30Freq", aoBoard + "/ao16", 0, 10);[m[41m[m
[32m+[m[32m            AddAnalogOutputChannel("VCO155Amp", aoBoard + "/ao17", 0, 10);[m[41m[m
[32m+[m[32m            AddAnalogOutputChannel("VCO155Freq", aoBoard + "/ao18", 0, 10);[m[41m[m
[32m+[m[41m[m
         }[m
 [m
     }[m
[1mdiff --git a/DAQ/SerialDAQ.cs b/DAQ/SerialDAQ.cs[m
[1mindex 5f39bf6..f8e5889 100644[m
[1m--- a/DAQ/SerialDAQ.cs[m
[1m+++ b/DAQ/SerialDAQ.cs[m
[36m@@ -8,7 +8,7 @@[m [musing DAQ.Environment;[m
 namespace DAQ.HAL[m
 {[m
 	/// <summary>[m
[31m-	/// This is is the interface to the serial DAQ board[m
[32m+[m	[32m/// This is is the interface to the serial DAQ boardm[m[41m   [m
 	/// </summary>[m
 	public class SerialDAQ : DAQ.HAL.RS232Instrument[m
 	{[m
[1mdiff --git a/DecelerationHardwareControl/Controller.cs b/DecelerationHardwareControl/Controller.cs[m
[1mindex d9ee189..44018ea 100644[m
[1m--- a/DecelerationHardwareControl/Controller.cs[m
[1m+++ b/DecelerationHardwareControl/Controller.cs[m
[36m@@ -6,10 +6,10 @@[m [musing System.Windows.Forms;[m
 using DAQ.Environment;[m
 using DAQ.HAL;[m
 using DAQ.TransferCavityLock;[m
[31m-using NationalInstruments.DAQmx;[m
 using NationalInstruments;[m
 using NationalInstruments.DAQmx;[m
 [m
[32m+[m[41m[m
 namespace DecelerationHardwareControl[m
 {[m
     public class Controller : MarshalByRefObject, TransferCavityLockable[m
[1mdiff --git a/DecelerationHardwareControl/DecelerationHardwareControl.csproj b/DecelerationHardwareControl/DecelerationHardwareControl.csproj[m
[1mindex 64b2be9..0742348 100644[m
[1m--- a/DecelerationHardwareControl/DecelerationHardwareControl.csproj[m
[1m+++ b/DecelerationHardwareControl/DecelerationHardwareControl.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">[m
[32m+[m[32m<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="12.0">[m[41m[m
   <PropertyGroup>[m
     <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>[m
     <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>[m
[36m@@ -15,7 +15,7 @@[m
     <OldToolsVersion>3.5</OldToolsVersion>[m
     <UpgradeBackupLocation>[m
     </UpgradeBackupLocation>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <TargetFrameworkProfile />[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Decelerator|AnyCPU' ">[m
[36m@@ -25,6 +25,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -33,9 +34,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -44,6 +47,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -52,6 +56,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -60,6 +65,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -68,17 +74,16 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.DAQmx, Version=9.6.40.57, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
[31m-      <Private>False</Private>[m
[31m-    </Reference>[m
[31m-    <Reference Include="NationalInstruments.UI, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.UI.WindowsForms, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI.WindowsForms, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="System" />[m
     <Reference Include="System.Core">[m
       <RequiredTargetFramework>3.5</RequiredTargetFramework>[m
[1mdiff --git a/DecelerationHardwareControl/Properties/Resources.Designer.cs b/DecelerationHardwareControl/Properties/Resources.Designer.cs[m
[1mindex 53b5ed7..536175b 100644[m
[1m--- a/DecelerationHardwareControl/Properties/Resources.Designer.cs[m
[1m+++ b/DecelerationHardwareControl/Properties/Resources.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.225[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/DecelerationHardwareControl/Properties/Settings.Designer.cs b/DecelerationHardwareControl/Properties/Settings.Designer.cs[m
[1mindex 8b6a3a1..4ce2833 100644[m
[1m--- a/DecelerationHardwareControl/Properties/Settings.Designer.cs[m
[1m+++ b/DecelerationHardwareControl/Properties/Settings.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.225[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[36m@@ -12,7 +12,7 @@[m [mnamespace DecelerationHardwareControl.Properties {[m
     [m
     [m
     [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()][m
[31m-    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")][m
[32m+[m[32m    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")][m[41m[m
     internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {[m
         [m
         private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));[m
[1mdiff --git a/DecelerationHardwareControl/app.config b/DecelerationHardwareControl/app.config[m
[1mindex 7f9ebee..78ad81b 100644[m
[1m--- a/DecelerationHardwareControl/app.config[m
[1m+++ b/DecelerationHardwareControl/app.config[m
[36m@@ -1,6 +1,6 @@[m
 <?xml version="1.0"?>[m
 <configuration>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup>	<runtime>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup>	<runtime>[m[41m[m
 		<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">[m
 			<dependentAssembly>[m
 				<assemblyIdentity name="NationalInstruments.Common.Native" publicKeyToken="18CBAE0F9955702A" culture="neutral"/>[m
[1mdiff --git a/DecelerationLaserLock/DecelerationLaserLock.csproj b/DecelerationLaserLock/DecelerationLaserLock.csproj[m
[1mindex 6554f44..fd71ee1 100644[m
[1m--- a/DecelerationLaserLock/DecelerationLaserLock.csproj[m
[1m+++ b/DecelerationLaserLock/DecelerationLaserLock.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">[m
[32m+[m[32m<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="12.0">[m[41m[m
   <PropertyGroup>[m
     <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>[m
     <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>[m
[36m@@ -17,7 +17,7 @@[m
     <ApplicationRevision>0</ApplicationRevision>[m
     <UpgradeBackupLocation>[m
     </UpgradeBackupLocation>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <TargetFrameworkProfile />[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Decelerator|AnyCPU' ">[m
[36m@@ -27,6 +27,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -35,9 +36,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -46,6 +49,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -54,6 +58,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -62,6 +67,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -70,18 +76,19 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.DAQmx, Version=9.2.40.82, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
[31m-      <SpecificVersion>False</SpecificVersion>[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.DAQmx, Version=15.5.45.109, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=x86">[m[41m[m
       <HintPath>..\..\..\..\..\..\..\..\Program Files (x86)\National Instruments\MeasurementStudioVS2010\DotNET\Assemblies\Current\NationalInstruments.DAQmx.dll</HintPath>[m
     </Reference>[m
[31m-    <Reference Include="NationalInstruments.UI, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.UI.WindowsForms, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI.WindowsForms, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="System" />[m
     <Reference Include="System.Core">[m
       <RequiredTargetFramework>3.5</RequiredTargetFramework>[m
[1mdiff --git a/DecelerationLaserLock/Properties/Resources.Designer.cs b/DecelerationLaserLock/Properties/Resources.Designer.cs[m
[1mindex 9527078..ee8e0ee 100644[m
[1m--- a/DecelerationLaserLock/Properties/Resources.Designer.cs[m
[1m+++ b/DecelerationLaserLock/Properties/Resources.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.225[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/DecelerationLaserLock/Properties/Settings.Designer.cs b/DecelerationLaserLock/Properties/Settings.Designer.cs[m
[1mindex d135f36..c378ccf 100644[m
[1m--- a/DecelerationLaserLock/Properties/Settings.Designer.cs[m
[1m+++ b/DecelerationLaserLock/Properties/Settings.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.225[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[36m@@ -12,7 +12,7 @@[m [mnamespace DecelerationLaserLock.Properties {[m
     [m
     [m
     [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()][m
[31m-    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")][m
[32m+[m[32m    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")][m[41m[m
     internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {[m
         [m
         private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));[m
[1mdiff --git a/DecelerationLaserLock/app.config b/DecelerationLaserLock/app.config[m
[1mindex b3b8311..195b885 100644[m
[1m--- a/DecelerationLaserLock/app.config[m
[1m+++ b/DecelerationLaserLock/app.config[m
[36m@@ -1,4 +1,4 @@[m
 <?xml version="1.0"?>[m
 <configuration>[m
 [m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup></configuration>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup></configuration>[m[41m[m
[1mdiff --git a/EDMBlockHead/EDMBlockHead.csproj b/EDMBlockHead/EDMBlockHead.csproj[m
[1mindex de10e4e..6f25f84 100644[m
[1m--- a/EDMBlockHead/EDMBlockHead.csproj[m
[1m+++ b/EDMBlockHead/EDMBlockHead.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">[m
[32m+[m[32m<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="12.0">[m[41m[m
   <PropertyGroup>[m
     <ProjectType>Local</ProjectType>[m
     <ProductVersion>9.0.21022</ProductVersion>[m
[36m@@ -26,7 +26,7 @@[m
     <UpgradeBackupLocation>[m
     </UpgradeBackupLocation>[m
     <OldToolsVersion>3.5</OldToolsVersion>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5.1</TargetFrameworkVersion>[m[41m[m
     <TargetFrameworkProfile />[m
     <PublishUrl>publish\</PublishUrl>[m
     <Install>true</Install>[m
[36m@@ -53,6 +53,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -63,9 +64,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -76,6 +79,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -86,6 +90,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -96,6 +101,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -106,18 +112,19 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.DAQmx, Version=9.2.40.82, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
[31m-      <SpecificVersion>False</SpecificVersion>[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.DAQmx, Version=15.5.45.109, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=x86">[m[41m[m
       <HintPath>..\..\..\..\..\..\..\..\Program Files (x86)\National Instruments\MeasurementStudioVS2010\DotNET\Assemblies\Current\NationalInstruments.DAQmx.dll</HintPath>[m
     </Reference>[m
[31m-    <Reference Include="NationalInstruments.UI, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.UI.WindowsForms, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI.WindowsForms, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="System">[m
       <Name>System</Name>[m
     </Reference>[m
[1mdiff --git a/EDMBlockHead/Properties/Resources.Designer.cs b/EDMBlockHead/Properties/Resources.Designer.cs[m
[1mindex b13e320..b90e165 100644[m
[1m--- a/EDMBlockHead/Properties/Resources.Designer.cs[m
[1m+++ b/EDMBlockHead/Properties/Resources.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.225[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/EDMBlockHead/app.config b/EDMBlockHead/app.config[m
[1mindex 1855d24..6cbce2a 100644[m
[1m--- a/EDMBlockHead/app.config[m
[1m+++ b/EDMBlockHead/app.config[m
[36m@@ -17,4 +17,4 @@[m
 			</dependentAssembly>[m
 		</assemblyBinding>[m
 	</runtime>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup></configuration>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5.1"/></startup></configuration>[m[41m[m
[1mdiff --git a/EDMHardwareControl/ControlWindow.cs b/EDMHardwareControl/ControlWindow.cs[m
[1mindex c35f99f..dfb0f01 100644[m
[1m--- a/EDMHardwareControl/ControlWindow.cs[m
[1m+++ b/EDMHardwareControl/ControlWindow.cs[m
[36m@@ -486,10 +486,83 @@[m [mnamespace EDMHardwareControl[m
         private Button UpdateProbeAOMButton;[m
         private Label label68;[m
         //uWave Control[m
[31m-    //    private TabPage tabPage12;[m
[31m-      //  private GroupBox groupBox121;[m
[31m-    //    public System.Windows.Forms.TextBox uWaveDCFMBox;[m
[31m-     //   private System.Windows.Forms.Label label140;[m
[32m+[m[32m        private TabPage tabPage12;[m[41m[m
[32m+[m[32m        private GroupBox groupBox161MHzVCO;[m[41m[m
[32m+[m[32m        public TrackBar VCO161AmpTrackBar;[m[41m[m
[32m+[m[32m        public TextBox VCO161FreqTextBox;[m[41m[m
[32m+[m[32m        private Label label137;[m[41m[m
[32m+[m[32m        public TextBox VCO161AmpVoltageTextBox;[m[41m[m
[32m+[m[32m        private Button VCO161UpdateButton;[m[41m[m
[32m+[m[32m        private Label label138;[m[41m[m
[32m+[m[32m        private GroupBox groupBox40;[m[41m[m
[32m+[m[32m        public TrackBar VCO155AmpTrackBar;[m[41m[m
[32m+[m[32m        public TextBox VCO155FreqVoltageTextBox;[m[41m[m
[32m+[m[32m        private Label label135;[m[41m[m
[32m+[m[32m        public TextBox VCO155AmpVoltageTextBox;[m[41m[m
[32m+[m[32m        private Button VCO155UpdateButton;[m[41m[m
[32m+[m[32m        private Label label136;[m[41m[m
[32m+[m[32m        private GroupBox groupBox39;[m[41m[m
[32m+[m[32m        public TrackBar VCO30AmpTrackBar;[m[41m[m
[32m+[m[32m        public TextBox VCO30FreqVoltageTextBox;[m[41m[m
[32m+[m[32m        private Label label83;[m[41m[m
[32m+[m[32m        public TextBox VCO30AmpVoltageTextBox;[m[41m[m
[32m+[m[32m        private Button VCO30UpdateButton;[m[41m[m
[32m+[m[32m        private Label label134;[m[41m[m
[32m+[m[32m        public TrackBar VCO161FreqTrackBar;[m[41m[m
[32m+[m[32m        private Label label140;[m[41m[m
[32m+[m[32m        private Label label139;[m[41m[m
[32m+[m[32m        public TrackBar VCO30FreqTrackBar;[m[41m[m
[32m+[m[32m        private Label label142;[m[41m[m
[32m+[m[32m        private Label label141;[m[41m[m
[32m+[m[32m        private Label label144;[m[41m[m
[32m+[m[32m        private Label label143;[m[41m[m
[32m+[m[32m        public TrackBar VCO155FreqTrackBar;[m[41m[m
[32m+[m[32m        private Button VCO161AmpStepMinusButton;[m[41m[m
[32m+[m[32m        public TextBox VCO161AmpStepTextBox;[m[41m[m
[32m+[m[32m        private Label label145;[m[41m[m
[32m+[m[32m        private Button VCO161AmpStepPlusButton;[m[41m[m
[32m+[m[32m        private Button VCO161FreqStepPlusButton;[m[41m[m
[32m+[m[32m        public TextBox VCO161FreqStepTextBox;[m[41m[m
[32m+[m[32m        private Label label146;[m[41m[m
[32m+[m[32m        private Button VCO161FreqStepMinusButton;[m[41m[m
[32m+[m[32m        private Button VCO30FreqStepMinusButton;[m[41m[m
[32m+[m[32m        private Button VCO30FreqStepPlusButton;[m[41m[m
[32m+[m[32m        public TextBox VCO30FreqStepTextBox;[m[41m[m
[32m+[m[32m        private Label label148;[m[41m[m
[32m+[m[32m        private Button VCO30AmpStepMinusButton;[m[41m[m
[32m+[m[32m        private Button VCO30AmpStepPlusButton;[m[41m[m
[32m+[m[32m        public TextBox VCO30AmpStepTextBox;[m[41m[m
[32m+[m[32m        private Label label147;[m[41m[m
[32m+[m[32m        private Button VCO155FreqStepMinusButton;[m[41m[m
[32m+[m[32m        private Button VCO155FreqStepPlusButton;[m[41m[m
[32m+[m[32m        private Button VCO155AmpStepMinusButton;[m[41m[m
[32m+[m[32m        private Button VCO155AmpStepPlusButton;[m[41m[m
[32m+[m[32m        public TextBox VCO155FreqStepTextBox;[m[41m[m
[32m+[m[32m        private Label label150;[m[41m[m
[32m+[m[32m        public TextBox VCO155AmpStepTextBox;[m[41m[m
[32m+[m[32m        private Label label149;[m[41m[m
[32m+[m[32m        //uWave control[m[41m[m
[32m+[m[32m        private TabPage tabPage13;[m[41m[m
[32m+[m[32m        private GroupBox groupBox41;[m[41m[m
[32m+[m[32m        private Button mixerVoltageMinusButton;[m[41m[m
[32m+[m[32m        private Button mixerVoltagePlusButton;[m[41m[m
[32m+[m[32m        public TextBox stepMixerVoltageTextBox;[m[41m[m
[32m+[m[32m        private Label label151;[m[41m[m
[32m+[m[32m        private Button uWaveDCFMMinusButton;[m[41m[m
[32m+[m[32m        public TextBox uWaveDCFMStepTextBox;[m[41m[m
[32m+[m[32m        private Label label152;[m[41m[m
[32m+[m[32m        private Button uWaveDCFMPlusButton;[m[41m[m
[32m+[m[32m        public TrackBar mixerVoltageTrackBar;[m[41m[m
[32m+[m[32m        private Label label153;[m[41m[m
[32m+[m[32m        private Label label154;[m[41m[m
[32m+[m[32m        public TrackBar uWaveDCFMTrackBar;[m[41m[m
[32m+[m[32m        public TextBox mixerVoltageTextBox;[m[41m[m
[32m+[m[32m        private Label label155;[m[41m[m
[32m+[m[32m        public TextBox uWaveDCFMTextBox;[m[41m[m
[32m+[m[32m        private Button uWaveUpdateButton;[m[41m[m
[32m+[m[32m        private Label label156;[m[41m[m
[32m+[m[41m[m
[32m+[m[41m [m
 [m
 [m
 		public Controller controller;[m
[36m@@ -824,67 +897,6 @@[m [mnamespace EDMHardwareControl[m
             this.probeAOMVTextBox = new System.Windows.Forms.TextBox();[m
             this.UpdateProbeAOMButton = new System.Windows.Forms.Button();[m
             this.label68 = new System.Windows.Forms.Label();[m
[31m-            this.tabPage6 = new System.Windows.Forms.TabPage();[m
[31m-            this.groupBox34 = new System.Windows.Forms.GroupBox();[m
[31m-            this.label108 = new System.Windows.Forms.Label();[m
[31m-            this.label109 = new System.Windows.Forms.Label();[m
[31m-            this.pumpPolMesAngle = new System.Windows.Forms.TextBox();[m
[31m-            this.updatePumpPolMesAngle = new System.Windows.Forms.Button();[m
[31m-            this.zeroPumpPol = new System.Windows.Forms.Button();[m
[31m-            this.label110 = new System.Windows.Forms.Label();[m
[31m-            this.groupBox35 = new System.Windows.Forms.GroupBox();[m
[31m-            this.label124 = new System.Windows.Forms.Label();[m
[31m-            this.pumpBacklashTextBox = new System.Windows.Forms.TextBox();[m
[31m-            this.pumpPolVoltStopButton = new System.Windows.Forms.Button();[m
[31m-            this.pumpPolVoltTrackBar = new System.Windows.Forms.TrackBar();[m
[31m-            this.label111 = new System.Windows.Forms.Label();[m
[31m-            this.label112 = new System.Windows.Forms.Label();[m
[31m-            this.pumpPolSetAngle = new System.Windows.Forms.TextBox();[m
[31m-            this.label113 = new System.Windows.Forms.Label();[m
[31m-            this.label114 = new System.Windows.Forms.Label();[m
[31m-            this.setPumpPolAngle = new System.Windows.Forms.Button();[m
[31m-            this.pumpPolModeSelectSwitch = new NationalInstruments.UI.WindowsForms.Switch();[m
[31m-            this.groupBox32 = new System.Windows.Forms.GroupBox();[m
[31m-            this.label106 = new System.Windows.Forms.Label();[m
[31m-            this.label105 = new System.Windows.Forms.Label();[m
[31m-            this.probePolMesAngle = new System.Windows.Forms.TextBox();[m
[31m-            this.updateProbePolMesAngle = new System.Windows.Forms.Button();[m
[31m-            this.zeroProbePol = new System.Windows.Forms.Button();[m
[31m-            this.label101 = new System.Windows.Forms.Label();[m
[31m-            this.groupBox33 = new System.Windows.Forms.GroupBox();[m
[31m-            this.label123 = new System.Windows.Forms.Label();[m
[31m-            this.probeBacklashTextBox = new System.Windows.Forms.TextBox();[m
[31m-            this.probePolVoltStopButton = new System.Windows.Forms.Button();[m
[31m-            this.probePolVoltTrackBar = new System.Windows.Forms.TrackBar();[m
[31m-            this.label107 = new System.Windows.Forms.Label();[m
[31m-            this.label102 = new System.Windows.Forms.Label();[m
[31m-            this.probePolSetAngle = new System.Windows.Forms.TextBox();[m
[31m-            this.label103 = new System.Windows.Forms.Label();[m
[31m-            this.label104 = new System.Windows.Forms.Label();[m
[31m-            this.setProbePolAngle = new System.Windows.Forms.Button();[m
[31m-            this.probePolModeSelectSwitch = new NationalInstruments.UI.WindowsForms.Switch();[m
[31m-            this.tabPage5 = new System.Windows.Forms.TabPage();[m
[31m-            this.groupBox17 = new System.Windows.Forms.GroupBox();[m
[31m-            this.TargetStepButton = new System.Windows.Forms.Button();[m
[31m-            this.label66 = new System.Windows.Forms.Label();[m
[31m-            this.TargetNumStepsTextBox = new System.Windows.Forms.TextBox();[m
[31m-            this.groupBox15 = new System.Windows.Forms.GroupBox();[m
[31m-            this.label33 = new System.Windows.Forms.Label();[m
[31m-            this.checkYagInterlockButton = new System.Windows.Forms.Button();[m
[31m-            this.yagFlashlampVTextBox = new System.Windows.Forms.TextBox();[m
[31m-            this.interlockStatusTextBox = new System.Windows.Forms.TextBox();[m
[31m-            this.updateFlashlampVButton = new System.Windows.Forms.Button();[m
[31m-            this.label34 = new System.Windows.Forms.Label();[m
[31m-            this.startYAGFlashlampsButton = new System.Windows.Forms.Button();[m
[31m-            this.yagQDisableButton = new System.Windows.Forms.Button();[m
[31m-            this.stopYagFlashlampsButton = new System.Windows.Forms.Button();[m
[31m-            this.yagQEnableButton = new System.Windows.Forms.Button();[m
[31m-            this.tabPage9 = new System.Windows.Forms.TabPage();[m
[31m-            this.switchScanTTLSwitch = new NationalInstruments.UI.WindowsForms.Switch();[m
[31m-            this.label97 = new System.Windows.Forms.Label();[m
[31m-            this.tabPage7 = new System.Windows.Forms.TabPage();[m
[31m-            this.clearAlertButton = new System.Windows.Forms.Button();[m
[31m-            this.alertTextBox = new System.Windows.Forms.TextBox();[m
             this.tabPage8 = new System.Windows.Forms.TabPage();[m
             this.groupBox36 = new System.Windows.Forms.GroupBox();[m
             this.flAOMFreqStepTextBox = new System.Windows.Forms.TextBox();[m
[36m@@ -952,6 +964,141 @@[m [mnamespace EDMHardwareControl[m
             this.diodeCurrentPlot = new NationalInstruments.UI.WaveformPlot();[m
             this.xAxis2 = new NationalInstruments.UI.XAxis();[m
             this.yAxis2 = new NationalInstruments.UI.YAxis();[m
[32m+[m[32m            this.tabPage13 = new System.Windows.Forms.TabPage();[m[41m[m
[32m+[m[32m            this.groupBox41 = new System.Windows.Forms.GroupBox();[m[41m[m
[32m+[m[32m            this.mixerVoltageMinusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.mixerVoltagePlusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.stepMixerVoltageTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label151 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.uWaveDCFMMinusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.uWaveDCFMStepTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label152 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.uWaveDCFMPlusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.mixerVoltageTrackBar = new System.Windows.Forms.TrackBar();[m[41m[m
[32m+[m[32m            this.label153 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.label154 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.uWaveDCFMTrackBar = new System.Windows.Forms.TrackBar();[m[41m[m
[32m+[m[32m            this.mixerVoltageTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label155 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.uWaveDCFMTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.uWaveUpdateButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.label156 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.tabPage12 = new System.Windows.Forms.TabPage();[m[41m[m
[32m+[m[32m            this.groupBox40 = new System.Windows.Forms.GroupBox();[m[41m[m
[32m+[m[32m            this.VCO155FreqStepMinusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO155FreqStepPlusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO155AmpStepMinusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO155AmpStepPlusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO155FreqStepTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label150 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.VCO155AmpStepTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label149 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.label144 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.label143 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.VCO155FreqTrackBar = new System.Windows.Forms.TrackBar();[m[41m[m
[32m+[m[32m            this.VCO155AmpTrackBar = new System.Windows.Forms.TrackBar();[m[41m[m
[32m+[m[32m            this.VCO155FreqVoltageTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label135 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.VCO155AmpVoltageTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.VCO155UpdateButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.label136 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.groupBox39 = new System.Windows.Forms.GroupBox();[m[41m[m
[32m+[m[32m            this.VCO30FreqStepMinusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO30FreqStepPlusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO30FreqStepTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label148 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.VCO30AmpStepMinusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO30AmpStepPlusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO30AmpStepTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label147 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.VCO30FreqTrackBar = new System.Windows.Forms.TrackBar();[m[41m[m
[32m+[m[32m            this.label142 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.label141 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.VCO30AmpTrackBar = new System.Windows.Forms.TrackBar();[m[41m[m
[32m+[m[32m            this.VCO30FreqVoltageTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label83 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.VCO30AmpVoltageTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.VCO30UpdateButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.label134 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO = new System.Windows.Forms.GroupBox();[m[41m[m
[32m+[m[32m            this.VCO161FreqStepMinusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO161FreqStepPlusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO161FreqStepTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label146 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.VCO161AmpStepMinusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO161AmpStepTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label145 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.VCO161AmpStepPlusButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.VCO161FreqTrackBar = new System.Windows.Forms.TrackBar();[m[41m[m
[32m+[m[32m            this.label140 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.label139 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.VCO161AmpTrackBar = new System.Windows.Forms.TrackBar();[m[41m[m
[32m+[m[32m            this.VCO161FreqTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label137 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.VCO161AmpVoltageTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.VCO161UpdateButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.label138 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.tabPage6 = new System.Windows.Forms.TabPage();[m[41m[m
[32m+[m[32m            this.groupBox34 = new System.Windows.Forms.GroupBox();[m[41m[m
[32m+[m[32m            this.label108 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.label109 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.pumpPolMesAngle = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.updatePumpPolMesAngle = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.zeroPumpPol = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.label110 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.groupBox35 = new System.Windows.Forms.GroupBox();[m[41m[m
[32m+[m[32m            this.label124 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.pumpBacklashTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.pumpPolVoltStopButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.pumpPolVoltTrackBar = new System.Windows.Forms.TrackBar();[m[41m[m
[32m+[m[32m            this.label111 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.label112 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.pumpPolSetAngle = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label113 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.label114 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.setPumpPolAngle = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.pumpPolModeSelectSwitch = new NationalInstruments.UI.WindowsForms.Switch();[m[41m[m
[32m+[m[32m            this.groupBox32 = new System.Windows.Forms.GroupBox();[m[41m[m
[32m+[m[32m            this.label106 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.label105 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.probePolMesAngle = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.updateProbePolMesAngle = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.zeroProbePol = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.label101 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.groupBox33 = new System.Windows.Forms.GroupBox();[m[41m[m
[32m+[m[32m            this.label123 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.probeBacklashTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.probePolVoltStopButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.probePolVoltTrackBar = new System.Windows.Forms.TrackBar();[m[41m[m
[32m+[m[32m            this.label107 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.label102 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.probePolSetAngle = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.label103 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.label104 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.setProbePolAngle = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.probePolModeSelectSwitch = new NationalInstruments.UI.WindowsForms.Switch();[m[41m[m
[32m+[m[32m            this.tabPage5 = new System.Windows.Forms.TabPage();[m[41m[m
[32m+[m[32m            this.groupBox17 = new System.Windows.Forms.GroupBox();[m[41m[m
[32m+[m[32m            this.TargetStepButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.label66 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.TargetNumStepsTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.groupBox15 = new System.Windows.Forms.GroupBox();[m[41m[m
[32m+[m[32m            this.label33 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.checkYagInterlockButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.yagFlashlampVTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.interlockStatusTextBox = new System.Windows.Forms.TextBox();[m[41m[m
[32m+[m[32m            this.updateFlashlampVButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.label34 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.startYAGFlashlampsButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.yagQDisableButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.stopYagFlashlampsButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.yagQEnableButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.tabPage9 = new System.Windows.Forms.TabPage();[m[41m[m
[32m+[m[32m            this.switchScanTTLSwitch = new NationalInstruments.UI.WindowsForms.Switch();[m[41m[m
[32m+[m[32m            this.label97 = new System.Windows.Forms.Label();[m[41m[m
[32m+[m[32m            this.tabPage7 = new System.Windows.Forms.TabPage();[m[41m[m
[32m+[m[32m            this.clearAlertButton = new System.Windows.Forms.Button();[m[41m[m
[32m+[m[32m            this.alertTextBox = new System.Windows.Forms.TextBox();[m[41m[m
             this.tabPage10 = new System.Windows.Forms.TabPage();[m
             this.groupBox19 = new System.Windows.Forms.GroupBox();[m
             this.UpdateI2BiasVoltage = new System.Windows.Forms.Button();[m
[36m@@ -986,10 +1133,6 @@[m [mnamespace EDMHardwareControl[m
             this.radioButton4 = new System.Windows.Forms.RadioButton();[m
             this.radioButton5 = new System.Windows.Forms.RadioButton();[m
             this.radioButton6 = new System.Windows.Forms.RadioButton();[m
[31m-         //   this.tabPage12 = new System.Windows.Forms.TabPage();[m
[31m-         //   this.groupBox121 = new System.Windows.Forms.GroupBox();[m
[31m-          //  this.uWaveDCFMBox = new System.Windows.Forms.TextBox();[m
[31m-         //   this.label140 = new System.Windows.Forms.Label();[m
             this.groupBox2.SuspendLayout();[m
             ((System.ComponentModel.ISupportInitialize)(this.switchingLED)).BeginInit();[m
             ((System.ComponentModel.ISupportInitialize)(this.rampLED)).BeginInit();[m
[36m@@ -1029,21 +1172,6 @@[m [mnamespace EDMHardwareControl[m
             this.groupBox18.SuspendLayout();[m
             ((System.ComponentModel.ISupportInitialize)(this.probeAOMtrackBar)).BeginInit();[m
             this.panel5.SuspendLayout();[m
[31m-            this.tabPage6.SuspendLayout();[m
[31m-            this.groupBox34.SuspendLayout();[m
[31m-            this.groupBox35.SuspendLayout();[m
[31m-            ((System.ComponentModel.ISupportInitialize)(this.pumpPolVoltTrackBar)).BeginInit();[m
[31m-            ((System.ComponentModel.ISupportInitialize)(this.pumpPolModeSelectSwitch)).BeginInit();[m
[31m-            this.groupBox32.SuspendLayout();[m
[31m-            this.groupBox33.SuspendLayout();[m
[31m-            ((System.ComponentModel.ISupportInitialize)(this.probePolVoltTrackBar)).BeginInit();[m
[31m-            ((System.ComponentModel.ISupportInitialize)(this.probePolModeSelectSwitch)).BeginInit();[m
[31m-            this.tabPage5.SuspendLayout();[m
[31m-            this.groupBox17.SuspendLayout();[m
[31m-            this.groupBox15.SuspendLayout();[m
[31m-            this.tabPage9.SuspendLayout();[m
[31m-            ((System.ComponentModel.ISupportInitialize)(this.switchScanTTLSwitch)).BeginInit();[m
[31m-            this.tabPage7.SuspendLayout();[m
             this.tabPage8.SuspendLayout();[m
             this.groupBox36.SuspendLayout();[m
             this.panel8.SuspendLayout();[m
[36m@@ -1063,6 +1191,35 @@[m [mnamespace EDMHardwareControl[m
             this.panel6.SuspendLayout();[m
             this.groupBox26.SuspendLayout();[m
             ((System.ComponentModel.ISupportInitialize)(this.diodeCurrentGraph)).BeginInit();[m
[32m+[m[32m            this.tabPage13.SuspendLayout();[m[41m[m
[32m+[m[32m            this.groupBox41.SuspendLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.mixerVoltageTrackBar)).BeginInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.uWaveDCFMTrackBar)).BeginInit();[m[41m[m
[32m+[m[32m            this.tabPage12.SuspendLayout();[m[41m[m
[32m+[m[32m            this.groupBox40.SuspendLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO155FreqTrackBar)).BeginInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO155AmpTrackBar)).BeginInit();[m[41m[m
[32m+[m[32m            this.groupBox39.SuspendLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO30FreqTrackBar)).BeginInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO30AmpTrackBar)).BeginInit();[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.SuspendLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO161FreqTrackBar)).BeginInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO161AmpTrackBar)).BeginInit();[m[41m[m
[32m+[m[32m            this.tabPage6.SuspendLayout();[m[41m[m
[32m+[m[32m            this.groupBox34.SuspendLayout();[m[41m[m
[32m+[m[32m            this.groupBox35.SuspendLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.pumpPolVoltTrackBar)).BeginInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.pumpPolModeSelectSwitch)).BeginInit();[m[41m[m
[32m+[m[32m            this.groupBox32.SuspendLayout();[m[41m[m
[32m+[m[32m            this.groupBox33.SuspendLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.probePolVoltTrackBar)).BeginInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.probePolModeSelectSwitch)).BeginInit();[m[41m[m
[32m+[m[32m            this.tabPage5.SuspendLayout();[m[41m[m
[32m+[m[32m            this.groupBox17.SuspendLayout();[m[41m[m
[32m+[m[32m            this.groupBox15.SuspendLayout();[m[41m[m
[32m+[m[32m            this.tabPage9.SuspendLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.switchScanTTLSwitch)).BeginInit();[m[41m[m
[32m+[m[32m            this.tabPage7.SuspendLayout();[m[41m[m
             this.tabPage10.SuspendLayout();[m
             this.groupBox19.SuspendLayout();[m
             ((System.ComponentModel.ISupportInitialize)(this.I2BiasVoltageTrackBar)).BeginInit();[m
[36m@@ -1071,7 +1228,6 @@[m [mnamespace EDMHardwareControl[m
             ((System.ComponentModel.ISupportInitialize)(this.I2ErrorSigGraph)).BeginInit();[m
             this.menuStrip1.SuspendLayout();[m
             this.SuspendLayout();[m
[31m-[m
             // [m
             // groupBox2[m
             // [m
[36m@@ -1497,7 +1653,8 @@[m [mnamespace EDMHardwareControl[m
             this.tabControl.Controls.Add(this.tabPage3);[m
             this.tabControl.Controls.Add(this.tabPage11);[m
             this.tabControl.Controls.Add(this.tabPage8);[m
[31m-         //   this.tabControl.Controls.Add(this.tabPage12);[m
[32m+[m[32m            this.tabControl.Controls.Add(this.tabPage13);[m[41m[m
[32m+[m[32m            this.tabControl.Controls.Add(this.tabPage12);[m[41m[m
             this.tabControl.Controls.Add(this.tabPage6);[m
             this.tabControl.Controls.Add(this.tabPage5);[m
             this.tabControl.Controls.Add(this.tabPage9);[m
[36m@@ -4089,1307 +4246,2023 @@[m [mnamespace EDMHardwareControl[m
             this.label68.TabIndex = 36;[m
             this.label68.Text = "Voltage (V)";[m
             // [m
[31m-            // tabPage6[m
[31m-            // [m
[31m-            this.tabPage6.BackColor = System.Drawing.Color.Transparent;[m
[31m-            this.tabPage6.Controls.Add(this.groupBox34);[m
[31m-            this.tabPage6.Controls.Add(this.groupBox32);[m
[31m-            this.tabPage6.Location = new System.Drawing.Point(4, 22);[m
[31m-            this.tabPage6.Name = "tabPage6";[m
[31m-            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);[m
[31m-            this.tabPage6.Size = new System.Drawing.Size(697, 575);[m
[31m-            this.tabPage6.TabIndex = 5;[m
[31m-            this.tabPage6.Text = "Polarizer";[m
[31m-            // [m
[31m-            // groupBox34[m
[32m+[m[32m            // tabPage8[m[41m[m
             // [m
[31m-            this.groupBox34.Controls.Add(this.label108);[m
[31m-            this.groupBox34.Controls.Add(this.label109);[m
[31m-            this.groupBox34.Controls.Add(this.pumpPolMesAngle);[m
[31m-            this.groupBox34.Controls.Add(this.updatePumpPolMesAngle);[m
[31m-            this.groupBox34.Controls.Add(this.zeroPumpPol);[m
[31m-            this.groupBox34.Controls.Add(this.label110);[m
[31m-            this.groupBox34.Controls.Add(this.groupBox35);[m
[31m-            this.groupBox34.Location = new System.Drawing.Point(349, 6);[m
[31m-            this.groupBox34.Name = "groupBox34";[m
[31m-            this.groupBox34.Size = new System.Drawing.Size(345, 229);[m
[31m-            this.groupBox34.TabIndex = 13;[m
[31m-            this.groupBox34.TabStop = false;[m
[31m-            this.groupBox34.Text = "Pump Polariser";[m
[32m+[m[32m            this.tabPage8.BackColor = System.Drawing.Color.Transparent;[m[41m[m
[32m+[m[32m            this.tabPage8.Controls.Add(this.groupBox36);[m[41m[m
[32m+[m[32m            this.tabPage8.Controls.Add(this.groupBox28);[m[41m[m
[32m+[m[32m            this.tabPage8.Controls.Add(this.groupBox27);[m[41m[m
[32m+[m[32m            this.tabPage8.Controls.Add(this.groupBox26);[m[41m[m
[32m+[m[32m            this.tabPage8.Location = new System.Drawing.Point(4, 22);[m[41m[m
[32m+[m[32m            this.tabPage8.Name = "tabPage8";[m[41m[m
[32m+[m[32m            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);[m[41m[m
[32m+[m[32m            this.tabPage8.Size = new System.Drawing.Size(697, 575);[m[41m[m
[32m+[m[32m            this.tabPage8.TabIndex = 7;[m[41m[m
[32m+[m[32m            this.tabPage8.Text = "N=2 Lasers";[m[41m[m
             // [m
[31m-            // label108[m
[32m+[m[32m            // groupBox36[m[41m[m
             // [m
[31m-            this.label108.AutoSize = true;[m
[31m-            this.label108.Location = new System.Drawing.Point(271, 30);[m
[31m-            this.label108.Name = "label108";[m
[31m-            this.label108.Size = new System.Drawing.Size(0, 13);[m
[31m-            this.label108.TabIndex = 48;[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.flAOMFreqStepTextBox);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.label119);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.flAOMFreqPlusTextBox);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.flAOMFreqCentreTextBox);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.label120);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.flAOMFreqMinusTextBox);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.label121);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.flAOMFreqUpdateButton);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.label122);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.panel8);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.flAOMStepTextBox);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.label117);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.flAOMVoltageTextBox);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.UpdateFLAOMButton);[m[41m[m
[32m+[m[32m            this.groupBox36.Controls.Add(this.label118);[m[41m[m
[32m+[m[32m            this.groupBox36.Location = new System.Drawing.Point(9, 409);[m[41m[m
[32m+[m[32m            this.groupBox36.Name = "groupBox36";[m[41m[m
[32m+[m[32m            this.groupBox36.Size = new System.Drawing.Size(393, 148);[m[41m[m
[32m+[m[32m            this.groupBox36.TabIndex = 49;[m[41m[m
[32m+[m[32m            this.groupBox36.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox36.Text = "Stabilising AOM";[m[41m[m
             // [m
[31m-            // label109[m
[32m+[m[32m            // flAOMFreqStepTextBox[m[41m[m
             // [m
[31m-            this.label109.AutoSize = true;[m
[31m-            this.label109.Location = new System.Drawing.Point(15, 35);[m
[31m-            this.label109.Name = "label109";[m
[31m-            this.label109.Size = new System.Drawing.Size(74, 13);[m
[31m-            this.label109.TabIndex = 47;[m
[31m-            this.label109.Text = "Position Mode";[m
[32m+[m[32m            this.flAOMFreqStepTextBox.BackColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.flAOMFreqStepTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m[41m[m
[32m+[m[32m            this.flAOMFreqStepTextBox.Location = new System.Drawing.Point(255, 95);[m[41m[m
[32m+[m[32m            this.flAOMFreqStepTextBox.Name = "flAOMFreqStepTextBox";[m[41m[m
[32m+[m[32m            this.flAOMFreqStepTextBox.ReadOnly = true;[m[41m[m
[32m+[m[32m            this.flAOMFreqStepTextBox.Size = new System.Drawing.Size(126, 20);[m[41m[m
[32m+[m[32m            this.flAOMFreqStepTextBox.TabIndex = 74;[m[41m[m
[32m+[m[32m            this.flAOMFreqStepTextBox.Text = "0";[m[41m[m
             // [m
[31m-            // pumpPolMesAngle[m
[32m+[m[32m            // label119[m[41m[m
             // [m
[31m-            this.pumpPolMesAngle.BackColor = System.Drawing.Color.Black;[m
[31m-            this.pumpPolMesAngle.ForeColor = System.Drawing.Color.Chartreuse;[m
[31m-            this.pumpPolMesAngle.Location = new System.Drawing.Point(111, 180);[m
[31m-            this.pumpPolMesAngle.Name = "pumpPolMesAngle";[m
[31m-            this.pumpPolMesAngle.ReadOnly = true;[m
[31m-            this.pumpPolMesAngle.Size = new System.Drawing.Size(82, 20);[m
[31m-            this.pumpPolMesAngle.TabIndex = 43;[m
[31m-            this.pumpPolMesAngle.Text = "0";[m
[32m+[m[32m            this.label119.Location = new System.Drawing.Point(151, 98);[m[41m[m
[32m+[m[32m            this.label119.Name = "label119";[m[41m[m
[32m+[m[32m            this.label119.Size = new System.Drawing.Size(96, 23);[m[41m[m
[32m+[m[32m            this.label119.TabIndex = 72;[m[41m[m
[32m+[m[32m            this.label119.Text = "Step (Hz)";[m[41m[m
             // [m
[31m-            // updatePumpPolMesAngle[m
[32m+[m[32m            // flAOMFreqPlusTextBox[m[41m[m
             // [m
[31m-            this.updatePumpPolMesAngle.Location = new System.Drawing.Point(199, 178);[m
[31m-            this.updatePumpPolMesAngle.Name = "updatePumpPolMesAngle";[m
[31m-            this.updatePumpPolMesAngle.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.updatePumpPolMesAngle.TabIndex = 6;[m
[31m-            this.updatePumpPolMesAngle.Text = "Update";[m
[31m-            this.updatePumpPolMesAngle.UseVisualStyleBackColor = true;[m
[31m-            this.updatePumpPolMesAngle.Click += new System.EventHandler(this.updatePumpPolMesAngle_Click);[m
[31m-            // [m
[31m-            // zeroPumpPol[m
[32m+[m[32m            this.flAOMFreqPlusTextBox.BackColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.flAOMFreqPlusTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m[41m[m
[32m+[m[32m            this.flAOMFreqPlusTextBox.Location = new System.Drawing.Point(255, 41);[m[41m[m
[32m+[m[32m            this.flAOMFreqPlusTextBox.Name = "flAOMFreqPlusTextBox";[m[41m[m
[32m+[m[32m            this.flAOMFreqPlusTextBox.ReadOnly = true;[m[41m[m
[32m+[m[32m            this.flAOMFreqPlusTextBox.Size = new System.Drawing.Size(126, 20);[m[41m[m
[32m+[m[32m            this.flAOMFreqPlusTextBox.TabIndex = 75;[m[41m[m
[32m+[m[32m            this.flAOMFreqPlusTextBox.Text = "0";[m[41m[m
             // [m
[31m-            this.zeroPumpPol.Location = new System.Drawing.Point(280, 177);[m
[31m-            this.zeroPumpPol.Name = "zeroPumpPol";[m
[31m-            this.zeroPumpPol.Size = new System.Drawing.Size(44, 23);[m
[31m-            this.zeroPumpPol.TabIndex = 2;[m
[31m-            this.zeroPumpPol.Text = "Zero";[m
[31m-            this.zeroPumpPol.UseVisualStyleBackColor = true;[m
[31m-            this.zeroPumpPol.Click += new System.EventHandler(this.zeroPumpPol_Click);[m
[32m+[m[32m            // flAOMFreqCentreTextBox[m[41m[m
             // [m
[31m-            // label110[m
[32m+[m[32m            this.flAOMFreqCentreTextBox.BackColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.flAOMFreqCentreTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m[41m[m
[32m+[m[32m            this.flAOMFreqCentreTextBox.Location = new System.Drawing.Point(255, 69);[m[41m[m
[32m+[m[32m            this.flAOMFreqCentreTextBox.Name = "flAOMFreqCentreTextBox";[m[41m[m
[32m+[m[32m            this.flAOMFreqCentreTextBox.ReadOnly = true;[m[41m[m
[32m+[m[32m            this.flAOMFreqCentreTextBox.Size = new System.Drawing.Size(126, 20);[m[41m[m
[32m+[m[32m            this.flAOMFreqCentreTextBox.TabIndex = 71;[m[41m[m
[32m+[m[32m            this.flAOMFreqCentreTextBox.Text = "0";[m[41m[m
             // [m
[31m-            this.label110.AutoSize = true;[m
[31m-            this.label110.Location = new System.Drawing.Point(12, 183);[m
[31m-            this.label110.Name = "label110";[m
[31m-            this.label110.Size = new System.Drawing.Size(84, 13);[m
[31m-            this.label110.TabIndex = 7;[m
[31m-            this.label110.Text = "Measured Angle";[m
[32m+[m[32m            // label120[m[41m[m
             // [m
[31m-            // groupBox35[m
[32m+[m[32m            this.label120.Location = new System.Drawing.Point(151, 44);[m[41m[m
[32m+[m[32m            this.label120.Name = "label120";[m[41m[m
[32m+[m[32m            this.label120.Size = new System.Drawing.Size(98, 23);[m[41m[m
[32m+[m[32m            this.label120.TabIndex = 73;[m[41m[m
[32m+[m[32m            this.label120.Text = "AOM freq high (Hz)";[m[41m[m
             // [m
[31m-            this.groupBox35.Controls.Add(this.label124);[m
[31m-            this.groupBox35.Controls.Add(this.pumpBacklashTextBox);[m
[31m-            this.groupBox35.Controls.Add(this.pumpPolVoltStopButton);[m
[31m-            this.groupBox35.Controls.Add(this.pumpPolVoltTrackBar);[m
[31m-            this.groupBox35.Controls.Add(this.label111);[m
[31m-            this.groupBox35.Controls.Add(this.label112);[m
[31m-            this.groupBox35.Controls.Add(this.pumpPolSetAngle);[m
[31m-            this.groupBox35.Controls.Add(this.label113);[m
[31m-            this.groupBox35.Controls.Add(this.label114);[m
[31m-            this.groupBox35.Controls.Add(this.setPumpPolAngle);[m
[31m-            this.groupBox35.Controls.Add(this.pumpPolModeSelectSwitch);[m
[31m-            this.groupBox35.Location = new System.Drawing.Point(6, 11);[m
[31m-            this.groupBox35.Name = "groupBox35";[m
[31m-            this.groupBox35.Size = new System.Drawing.Size(332, 153);[m
[31m-            this.groupBox35.TabIndex = 50;[m
[31m-            this.groupBox35.TabStop = false;[m
[32m+[m[32m            // flAOMFreqMinusTextBox[m[41m[m
             // [m
[31m-            // label124[m
[32m+[m[32m            this.flAOMFreqMinusTextBox.BackColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.flAOMFreqMinusTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m[41m[m
[32m+[m[32m            this.flAOMFreqMinusTextBox.Location = new System.Drawing.Point(255, 15);[m[41m[m
[32m+[m[32m            this.flAOMFreqMinusTextBox.Name = "flAOMFreqMinusTextBox";[m[41m[m
[32m+[m[32m            this.flAOMFreqMinusTextBox.ReadOnly = true;[m[41m[m
[32m+[m[32m            this.flAOMFreqMinusTextBox.Size = new System.Drawing.Size(126, 20);[m[41m[m
[32m+[m[32m            this.flAOMFreqMinusTextBox.TabIndex = 70;[m[41m[m
[32m+[m[32m            this.flAOMFreqMinusTextBox.Text = "0";[m[41m[m
             // [m
[31m-            this.label124.AutoSize = true;[m
[31m-            this.label124.Location = new System.Drawing.Point(118, 55);[m
[31m-            this.label124.Name = "label124";[m
[31m-            this.label124.Size = new System.Drawing.Size(114, 13);[m
[31m-            this.label124.TabIndex = 54;[m
[31m-            this.label124.Text = "-ve overshoot ( 0 = off)";[m
[32m+[m[32m            // label121[m[41m[m
             // [m
[31m-            // pumpBacklashTextBox[m
[32m+[m[32m            this.label121.Location = new System.Drawing.Point(151, 72);[m[41m[m
[32m+[m[32m            this.label121.Name = "label121";[m[41m[m
[32m+[m[32m            this.label121.Size = new System.Drawing.Size(96, 23);[m[41m[m
[32m+[m[32m            this.label121.TabIndex = 67;[m[41m[m
[32m+[m[32m            this.label121.Text = "Centre (Hz)";[m[41m[m
             // [m
[31m-            this.pumpBacklashTextBox.Location = new System.Drawing.Point(244, 52);[m
[31m-            this.pumpBacklashTextBox.Name = "pumpBacklashTextBox";[m
[31m-            this.pumpBacklashTextBox.Size = new System.Drawing.Size(75, 20);[m
[31m-            this.pumpBacklashTextBox.TabIndex = 53;[m
[31m-            this.pumpBacklashTextBox.Text = "0";[m
[32m+[m[32m            // flAOMFreqUpdateButton[m[41m[m
             // [m
[31m-            // pumpPolVoltStopButton[m
[32m+[m[32m            this.flAOMFreqUpdateButton.Location = new System.Drawing.Point(305, 121);[m[41m[m
[32m+[m[32m            this.flAOMFreqUpdateButton.Name = "flAOMFreqUpdateButton";[m[41m[m
[32m+[m[32m            this.flAOMFreqUpdateButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.flAOMFreqUpdateButton.TabIndex = 69;[m[41m[m
[32m+[m[32m            this.flAOMFreqUpdateButton.Text = "Update";[m[41m[m
[32m+[m[32m            this.flAOMFreqUpdateButton.Click += new System.EventHandler(this.flAOMFreqUpdateButton_Click);[m[41m[m
             // [m
[31m-            this.pumpPolVoltStopButton.Enabled = false;[m
[31m-            this.pumpPolVoltStopButton.Location = new System.Drawing.Point(243, 106);[m
[31m-            this.pumpPolVoltStopButton.Name = "pumpPolVoltStopButton";[m
[31m-            this.pumpPolVoltStopButton.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.pumpPolVoltStopButton.TabIndex = 51;[m
[31m-            this.pumpPolVoltStopButton.Text = "Stop";[m
[31m-            this.pumpPolVoltStopButton.UseVisualStyleBackColor = true;[m
[31m-            this.pumpPolVoltStopButton.Click += new System.EventHandler(this.pumpPolVoltStopButton_Click);[m
[32m+[m[32m            // label122[m[41m[m
             // [m
[31m-            // pumpPolVoltTrackBar[m
[32m+[m[32m            this.label122.Location = new System.Drawing.Point(150, 18);[m[41m[m
[32m+[m[32m            this.label122.Name = "label122";[m[41m[m
[32m+[m[32m            this.label122.Size = new System.Drawing.Size(99, 23);[m[41m[m
[32m+[m[32m            this.label122.TabIndex = 68;[m[41m[m
[32m+[m[32m            this.label122.Text = "AOM freq low (Hz)";[m[41m[m
             // [m
[31m-            this.pumpPolVoltTrackBar.Enabled = false;[m
[31m-            this.pumpPolVoltTrackBar.Location = new System.Drawing.Point(88, 102);[m
[31m-            this.pumpPolVoltTrackBar.Maximum = 100;[m
[31m-            this.pumpPolVoltTrackBar.Minimum = -100;[m
[31m-            this.pumpPolVoltTrackBar.Name = "pumpPolVoltTrackBar";[m
[31m-            this.pumpPolVoltTrackBar.Size = new System.Drawing.Size(149, 45);[m
[31m-            this.pumpPolVoltTrackBar.TabIndex = 51;[m
[31m-            this.pumpPolVoltTrackBar.Scroll += new System.EventHandler(this.pumpPolVoltTrackBar_Scroll);[m
[32m+[m[32m            // panel8[m[41m[m
             // [m
[31m-            // label111[m
[32m+[m[32m            this.panel8.Controls.Add(this.flAOMStepZeroButton);[m[41m[m
[32m+[m[32m            this.panel8.Controls.Add(this.flAOMStepPlusButton);[m[41m[m
[32m+[m[32m            this.panel8.Controls.Add(this.flAOMStepMinusButton);[m[41m[m
[32m+[m[32m            this.panel8.Location = new System.Drawing.Point(9, 67);[m[41m[m
[32m+[m[32m            this.panel8.Name = "panel8";[m[41m[m
[32m+[m[32m            this.panel8.Size = new System.Drawing.Size(111, 32);[m[41m[m
[32m+[m[32m            this.panel8.TabIndex = 48;[m[41m[m
             // [m
[31m-            this.label111.AutoSize = true;[m
[31m-            this.label111.Location = new System.Drawing.Point(9, 126);[m
[31m-            this.label111.Name = "label111";[m
[31m-            this.label111.Size = new System.Drawing.Size(73, 13);[m
[31m-            this.label111.TabIndex = 49;[m
[31m-            this.label111.Text = "Voltage Mode";[m
[32m+[m[32m            // flAOMStepZeroButton[m[41m[m
             // [m
[31m-            // label112[m
[32m+[m[32m            this.flAOMStepZeroButton.AutoSize = true;[m[41m[m
[32m+[m[32m            this.flAOMStepZeroButton.Checked = true;[m[41m[m
[32m+[m[32m            this.flAOMStepZeroButton.Location = new System.Drawing.Point(77, 7);[m[41m[m
[32m+[m[32m            this.flAOMStepZeroButton.Name = "flAOMStepZeroButton";[m[41m[m
[32m+[m[32m            this.flAOMStepZeroButton.Size = new System.Drawing.Size(31, 17);[m[41m[m
[32m+[m[32m            this.flAOMStepZeroButton.TabIndex = 32;[m[41m[m
[32m+[m[32m            this.flAOMStepZeroButton.TabStop = true;[m[41m[m
[32m+[m[32m            this.flAOMStepZeroButton.Text = "0";[m[41m[m
[32m+[m[32m            this.flAOMStepZeroButton.UseVisualStyleBackColor = true;[m[41m[m
             // [m
[31m-            this.label112.AutoSize = true;[m
[31m-            this.label112.Location = new System.Drawing.Point(102, 24);[m
[31m-            this.label112.Name = "label112";[m
[31m-            this.label112.Size = new System.Drawing.Size(53, 13);[m
[31m-            this.label112.TabIndex = 8;[m
[31m-            this.label112.Text = "Set Angle";[m
[32m+[m[32m            // flAOMStepPlusButton[m[41m[m
             // [m
[31m-            // pumpPolSetAngle[m
[32m+[m[32m            this.flAOMStepPlusButton.AutoSize = true;[m[41m[m
[32m+[m[32m            this.flAOMStepPlusButton.Location = new System.Drawing.Point(3, 6);[m[41m[m
[32m+[m[32m            this.flAOMStepPlusButton.Name = "flAOMStepPlusButton";[m[41m[m
[32m+[m[32m            this.flAOMStepPlusButton.Size = new System.Drawing.Size(31, 17);[m[41m[m
[32m+[m[32m            this.flAOMStepPlusButton.TabIndex = 32;[m[41m[m
[32m+[m[32m            this.flAOMStepPlusButton.Text = "+";[m[41m[m
[32m+[m[32m            this.flAOMStepPlusButton.UseVisualStyleBackColor = true;[m[41m[m
             // [m
[31m-            this.pumpPolSetAngle.Location = new System.Drawing.Point(161, 19);[m
[31m-            this.pumpPolSetAngle.Name = "pumpPolSetAngle";[m
[31m-            this.pumpPolSetAngle.Size = new System.Drawing.Size(66, 20);[m
[31m-            this.pumpPolSetAngle.TabIndex = 13;[m
[31m-            this.pumpPolSetAngle.Text = "0";[m
[32m+[m[32m            // flAOMStepMinusButton[m[41m[m
             // [m
[31m-            // label113[m
[32m+[m[32m            this.flAOMStepMinusButton.AutoSize = true;[m[41m[m
[32m+[m[32m            this.flAOMStepMinusButton.Location = new System.Drawing.Point(42, 7);[m[41m[m
[32m+[m[32m            this.flAOMStepMinusButton.Name = "flAOMStepMinusButton";[m[41m[m
[32m+[m[32m            this.flAOMStepMinusButton.Size = new System.Drawing.Size(28, 17);[m[41m[m
[32m+[m[32m            this.flAOMStepMinusButton.TabIndex = 32;[m[41m[m
[32m+[m[32m            this.flAOMStepMinusButton.Text = "-";[m[41m[m
[32m+[m[32m            this.flAOMStepMinusButton.UseVisualStyleBackColor = true;[m[41m[m
             // [m
[31m-            this.label113.AutoSize = true;[m
[31m-            this.label113.Location = new System.Drawing.Point(172, 78);[m
[31m-            this.label113.Name = "label113";[m
[31m-            this.label113.Size = new System.Drawing.Size(55, 13);[m
[31m-            this.label113.TabIndex = 44;[m
[31m-            this.label113.Text = "Clockwise";[m
[32m+[m[32m            // flAOMStepTextBox[m[41m[m
             // [m
[31m-            // label114[m
[32m+[m[32m            this.flAOMStepTextBox.Location = new System.Drawing.Point(68, 41);[m[41m[m
[32m+[m[32m            this.flAOMStepTextBox.Name = "flAOMStepTextBox";[m[41m[m
[32m+[m[32m            this.flAOMStepTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.flAOMStepTextBox.TabIndex = 47;[m[41m[m
[32m+[m[32m            this.flAOMStepTextBox.Text = "0";[m[41m[m
             // [m
[31m-            this.label114.AutoSize = true;[m
[31m-            this.label114.Location = new System.Drawing.Point(85, 78);[m
[31m-            this.label114.Name = "label114";[m
[31m-            this.label114.Size = new System.Drawing.Size(75, 13);[m
[31m-            this.label114.TabIndex = 45;[m
[31m-            this.label114.Text = "Anti-clockwise";[m
[32m+[m[32m            // label117[m[41m[m
             // [m
[31m-            // setPumpPolAngle[m
[32m+[m[32m            this.label117.Location = new System.Drawing.Point(6, 44);[m[41m[m
[32m+[m[32m            this.label117.Name = "label117";[m[41m[m
[32m+[m[32m            this.label117.Size = new System.Drawing.Size(80, 23);[m[41m[m
[32m+[m[32m            this.label117.TabIndex = 46;[m[41m[m
[32m+[m[32m            this.label117.Text = "Step (V)";[m[41m[m
             // [m
[31m-            this.setPumpPolAngle.Location = new System.Drawing.Point(243, 17);[m
[31m-            this.setPumpPolAngle.Name = "setPumpPolAngle";[m
[31m-            this.setPumpPolAngle.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.setPumpPolAngle.TabIndex = 5;[m
[31m-            this.setPumpPolAngle.Text = "Set";[m
[31m-            this.setPumpPolAngle.UseVisualStyleBackColor = true;[m
[31m-            this.setPumpPolAngle.Click += new System.EventHandler(this.setPumpPolAngle_Click);[m
[32m+[m[32m            // flAOMVoltageTextBox[m[41m[m
             // [m
[31m-            // pumpPolModeSelectSwitch[m
[32m+[m[32m            this.flAOMVoltageTextBox.Location = new System.Drawing.Point(68, 21);[m[41m[m
[32m+[m[32m            this.flAOMVoltageTextBox.Name = "flAOMVoltageTextBox";[m[41m[m
[32m+[m[32m            this.flAOMVoltageTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.flAOMVoltageTextBox.TabIndex = 45;[m[41m[m
[32m+[m[32m            this.flAOMVoltageTextBox.Text = "0";[m[41m[m
             // [m
[31m-            this.pumpPolModeSelectSwitch.Location = new System.Drawing.Point(12, 33);[m
[31m-            this.pumpPolModeSelectSwitch.Name = "pumpPolModeSelectSwitch";[m
[31m-            this.pumpPolModeSelectSwitch.Size = new System.Drawing.Size(64, 96);[m
[31m-            this.pumpPolModeSelectSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;[m
[31m-            this.pumpPolModeSelectSwitch.TabIndex = 51;[m
[31m-            this.pumpPolModeSelectSwitch.Value = true;[m
[31m-            this.pumpPolModeSelectSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.pumpPolModeSelectSwitch_StateChanged);[m
[32m+[m[32m            // UpdateFLAOMButton[m[41m[m
             // [m
[31m-            // groupBox32[m
[32m+[m[32m            this.UpdateFLAOMButton.Location = new System.Drawing.Point(24, 121);[m[41m[m
[32m+[m[32m            this.UpdateFLAOMButton.Name = "UpdateFLAOMButton";[m[41m[m
[32m+[m[32m            this.UpdateFLAOMButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.UpdateFLAOMButton.TabIndex = 40;[m[41m[m
[32m+[m[32m            this.UpdateFLAOMButton.Text = "Update";[m[41m[m
[32m+[m[32m            this.UpdateFLAOMButton.Click += new System.EventHandler(this.UpdateFLAOMButton_Click);[m[41m[m
             // [m
[31m-            this.groupBox32.Controls.Add(this.label106);[m
[31m-            this.groupBox32.Controls.Add(this.label105);[m
[31m-            this.groupBox32.Controls.Add(this.probePolMesAngle);[m
[31m-            this.groupBox32.Controls.Add(this.updateProbePolMesAngle);[m
[31m-            this.groupBox32.Controls.Add(this.zeroProbePol);[m
[31m-            this.groupBox32.Controls.Add(this.label101);[m
[31m-            this.groupBox32.Controls.Add(this.groupBox33);[m
[31m-            this.groupBox32.Location = new System.Drawing.Point(3, 6);[m
[31m-            this.groupBox32.Name = "groupBox32";[m
[31m-            this.groupBox32.Size = new System.Drawing.Size(345, 229);[m
[31m-            this.groupBox32.TabIndex = 12;[m
[31m-            this.groupBox32.TabStop = false;[m
[31m-            this.groupBox32.Text = "Probe Polariser";[m
[32m+[m[32m            // label118[m[41m[m
             // [m
[31m-            // label106[m
[32m+[m[32m            this.label118.Location = new System.Drawing.Point(6, 23);[m[41m[m
[32m+[m[32m            this.label118.Name = "label118";[m[41m[m
[32m+[m[32m            this.label118.Size = new System.Drawing.Size(80, 23);[m[41m[m
[32m+[m[32m            this.label118.TabIndex = 36;[m[41m[m
[32m+[m[32m            this.label118.Text = "Voltage (V)";[m[41m[m
             // [m
[31m-            this.label106.AutoSize = true;[m
[31m-            this.label106.Location = new System.Drawing.Point(271, 30);[m
[31m-            this.label106.Name = "label106";[m
[31m-            this.label106.Size = new System.Drawing.Size(0, 13);[m
[31m-            this.label106.TabIndex = 48;[m
[32m+[m[32m            // groupBox28[m[41m[m
             // [m
[31m-            // label105[m
[32m+[m[32m            this.groupBox28.Controls.Add(this.groupBox30);[m[41m[m
[32m+[m[32m            this.groupBox28.Controls.Add(this.groupBox31);[m[41m[m
[32m+[m[32m            this.groupBox28.Controls.Add(this.groupBox29);[m[41m[m
[32m+[m[32m            this.groupBox28.Location = new System.Drawing.Point(408, 218);[m[41m[m
[32m+[m[32m            this.groupBox28.Name = "groupBox28";[m[41m[m
[32m+[m[32m            this.groupBox28.Size = new System.Drawing.Size(283, 252);[m[41m[m
[32m+[m[32m            this.groupBox28.TabIndex = 2;[m[41m[m
[32m+[m[32m            this.groupBox28.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox28.Text = "Fibre Amplifier";[m[41m[m
             // [m
[31m-            this.label105.AutoSize = true;[m
[31m-            this.label105.Location = new System.Drawing.Point(15, 35);[m
[31m-            this.label105.Name = "label105";[m
[31m-            this.label105.Size = new System.Drawing.Size(74, 13);[m
[31m-            this.label105.TabIndex = 47;[m
[31m-            this.label105.Text = "Position Mode";[m
[32m+[m[32m            // groupBox30[m[41m[m
             // [m
[31m-            // probePolMesAngle[m
[32m+[m[32m            this.groupBox30.Controls.Add(this.fibreAmpEnableLED);[m[41m[m
[32m+[m[32m            this.groupBox30.Controls.Add(this.fibreAmpEnableSwitch);[m[41m[m
[32m+[m[32m            this.groupBox30.Location = new System.Drawing.Point(9, 18);[m[41m[m
[32m+[m[32m            this.groupBox30.Name = "groupBox30";[m[41m[m
[32m+[m[32m            this.groupBox30.Size = new System.Drawing.Size(124, 79);[m[41m[m
[32m+[m[32m            this.groupBox30.TabIndex = 50;[m[41m[m
[32m+[m[32m            this.groupBox30.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox30.Text = "On/Off";[m[41m[m
             // [m
[31m-            this.probePolMesAngle.BackColor = System.Drawing.Color.Black;[m
[31m-            this.probePolMesAngle.ForeColor = System.Drawing.Color.Chartreuse;[m
[31m-            this.probePolMesAngle.Location = new System.Drawing.Point(111, 180);[m
[31m-            this.probePolMesAngle.Name = "probePolMesAngle";[m
[31m-            this.probePolMesAngle.ReadOnly = true;[m
[31m-            this.probePolMesAngle.Size = new System.Drawing.Size(82, 20);[m
[31m-            this.probePolMesAngle.TabIndex = 43;[m
[31m-            this.probePolMesAngle.Text = "0";[m
[32m+[m[32m            // fibreAmpEnableLED[m[41m[m
             // [m
[31m-            // updateProbePolMesAngle[m
[32m+[m[32m            this.fibreAmpEnableLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m[41m[m
[32m+[m[32m            this.fibreAmpEnableLED.Location = new System.Drawing.Point(9, 20);[m[41m[m
[32m+[m[32m            this.fibreAmpEnableLED.Name = "fibreAmpEnableLED";[m[41m[m
[32m+[m[32m            this.fibreAmpEnableLED.OffColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.fibreAmpEnableLED.Size = new System.Drawing.Size(47, 49);[m[41m[m
[32m+[m[32m            this.fibreAmpEnableLED.TabIndex = 51;[m[41m[m
             // [m
[31m-            this.updateProbePolMesAngle.Location = new System.Drawing.Point(199, 178);[m
[31m-            this.updateProbePolMesAngle.Name = "updateProbePolMesAngle";[m
[31m-            this.updateProbePolMesAngle.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.updateProbePolMesAngle.TabIndex = 6;[m
[31m-            this.updateProbePolMesAngle.Text = "Update";[m
[31m-            this.updateProbePolMesAngle.UseVisualStyleBackColor = true;[m
[31m-            this.updateProbePolMesAngle.Click += new System.EventHandler(this.updateProbePolMesAngle_Click);[m
[32m+[m[32m            // fibreAmpEnableSwitch[m[41m[m
             // [m
[31m-            // zeroProbePol[m
[32m+[m[32m            this.fibreAmpEnableSwitch.Location = new System.Drawing.Point(60, -4);[m[41m[m
[32m+[m[32m            this.fibreAmpEnableSwitch.Name = "fibreAmpEnableSwitch";[m[41m[m
[32m+[m[32m            this.fibreAmpEnableSwitch.Size = new System.Drawing.Size(64, 96);[m[41m[m
[32m+[m[32m            this.fibreAmpEnableSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;[m[41m[m
[32m+[m[32m            this.fibreAmpEnableSwitch.TabIndex = 50;[m[41m[m
[32m+[m[32m            this.fibreAmpEnableSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.fibreAmpEnableSwitch_StateChanged);[m[41m[m
             // [m
[31m-            this.zeroProbePol.Location = new System.Drawing.Point(280, 177);[m
[31m-            this.zeroProbePol.Name = "zeroProbePol";[m
[31m-            this.zeroProbePol.Size = new System.Drawing.Size(44, 23);[m
[31m-            this.zeroProbePol.TabIndex = 2;[m
[31m-            this.zeroProbePol.Text = "Zero";[m
[31m-            this.zeroProbePol.UseVisualStyleBackColor = true;[m
[31m-            this.zeroProbePol.Click += new System.EventHandler(this.zeroProbePol_Click);[m
[32m+[m[32m            // groupBox31[m[41m[m
             // [m
[31m-            // label101[m
[32m+[m[32m            this.groupBox31.Controls.Add(this.updateFibreAmpPwrButton);[m[41m[m
[32m+[m[32m            this.groupBox31.Controls.Add(this.fibreAmpPwrTextBox);[m[41m[m
[32m+[m[32m            this.groupBox31.Location = new System.Drawing.Point(149, 19);[m[41m[m
[32m+[m[32m            this.groupBox31.Name = "groupBox31";[m[41m[m
[32m+[m[32m            this.groupBox31.Size = new System.Drawing.Size(124, 76);[m[41m[m
[32m+[m[32m            this.groupBox31.TabIndex = 48;[m[41m[m
[32m+[m[32m            this.groupBox31.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox31.Text = "Power";[m[41m[m
             // [m
[31m-            this.label101.AutoSize = true;[m
[31m-            this.label101.Location = new System.Drawing.Point(12, 183);[m
[31m-            this.label101.Name = "label101";[m
[31m-            this.label101.Size = new System.Drawing.Size(84, 13);[m
[31m-            this.label101.TabIndex = 7;[m
[31m-            this.label101.Text = "Measured Angle";[m
[32m+[m[32m            // updateFibreAmpPwrButton[m[41m[m
             // [m
[31m-            // groupBox33[m
[32m+[m[32m            this.updateFibreAmpPwrButton.Location = new System.Drawing.Point(6, 45);[m[41m[m
[32m+[m[32m            this.updateFibreAmpPwrButton.Name = "updateFibreAmpPwrButton";[m[41m[m
[32m+[m[32m            this.updateFibreAmpPwrButton.Size = new System.Drawing.Size(100, 23);[m[41m[m
[32m+[m[32m            this.updateFibreAmpPwrButton.TabIndex = 60;[m[41m[m
[32m+[m[32m            this.updateFibreAmpPwrButton.Text = "Update";[m[41m[m
[32m+[m[32m            this.updateFibreAmpPwrButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.updateFibreAmpPwrButton.Click += new System.EventHandler(this.updateFibreAmpPwrButton_Click);[m[41m[m
             // [m
[31m-            this.groupBox33.Controls.Add(this.label123);[m
[31m-            this.groupBox33.Controls.Add(this.probeBacklashTextBox);[m
[31m-            this.groupBox33.Controls.Add(this.probePolVoltStopButton);[m
[31m-            this.groupBox33.Controls.Add(this.probePolVoltTrackBar);[m
[31m-            this.groupBox33.Controls.Add(this.label107);[m
[31m-            this.groupBox33.Controls.Add(this.label102);[m
[31m-            this.groupBox33.Controls.Add(this.probePolSetAngle);[m
[31m-            this.groupBox33.Controls.Add(this.label103);[m
[31m-            this.groupBox33.Controls.Add(this.label104);[m
[31m-            this.groupBox33.Controls.Add(this.setProbePolAngle);[m
[31m-            this.groupBox33.Controls.Add(this.probePolModeSelectSwitch);[m
[31m-            this.groupBox33.Location = new System.Drawing.Point(6, 11);[m
[31m-            this.groupBox33.Name = "groupBox33";[m
[31m-            this.groupBox33.Size = new System.Drawing.Size(332, 153);[m
[31m-            this.groupBox33.TabIndex = 50;[m
[31m-            this.groupBox33.TabStop = false;[m
[32m+[m[32m            // fibreAmpPwrTextBox[m[41m[m
             // [m
[31m-            // label123[m
[32m+[m[32m            this.fibreAmpPwrTextBox.BackColor = System.Drawing.Color.LimeGreen;[m[41m[m
[32m+[m[32m            this.fibreAmpPwrTextBox.Location = new System.Drawing.Point(6, 19);[m[41m[m
[32m+[m[32m            this.fibreAmpPwrTextBox.Name = "fibreAmpPwrTextBox";[m[41m[m
[32m+[m[32m            this.fibreAmpPwrTextBox.Size = new System.Drawing.Size(100, 20);[m[41m[m
[32m+[m[32m            this.fibreAmpPwrTextBox.TabIndex = 49;[m[41m[m
[32m+[m[32m            this.fibreAmpPwrTextBox.Text = "0";[m[41m[m
             // [m
[31m-            this.label123.AutoSize = true;[m
[31m-            this.label123.Location = new System.Drawing.Point(117, 55);[m
[31m-            this.label123.Name = "label123";[m
[31m-            this.label123.Size = new System.Drawing.Size(114, 13);[m
[31m-            this.label123.TabIndex = 52;[m
[31m-            this.label123.Text = "-ve overshoot ( 0 = off)";[m
[32m+[m[32m            // groupBox29[m[41m[m
             // [m
[31m-            // probeBacklashTextBox[m
[32m+[m[32m            this.groupBox29.Controls.Add(this.fibreAmpPowerFaultLED);[m[41m[m
[32m+[m[32m            this.groupBox29.Controls.Add(this.fibreAmpTempFaultLED);[m[41m[m
[32m+[m[32m            this.groupBox29.Controls.Add(this.fibreAmpBackReflectFaultLED);[m[41m[m
[32m+[m[32m            this.groupBox29.Controls.Add(this.fibreAmpSeedFaultLED);[m[41m[m
[32m+[m[32m            this.groupBox29.Controls.Add(this.fibreAmpMasterFaultLED);[m[41m[m
[32m+[m[32m            this.groupBox29.Controls.Add(this.faultCheckButton);[m[41m[m
[32m+[m[32m            this.groupBox29.Controls.Add(this.label93);[m[41m[m
[32m+[m[32m            this.groupBox29.Controls.Add(this.label92);[m[41m[m
[32m+[m[32m            this.groupBox29.Controls.Add(this.label91);[m[41m[m
[32m+[m[32m            this.groupBox29.Controls.Add(this.label90);[m[41m[m
[32m+[m[32m            this.groupBox29.Controls.Add(this.label89);[m[41m[m
[32m+[m[32m            this.groupBox29.Location = new System.Drawing.Point(9, 103);[m[41m[m
[32m+[m[32m            this.groupBox29.Name = "groupBox29";[m[41m[m
[32m+[m[32m            this.groupBox29.Size = new System.Drawing.Size(264, 142);[m[41m[m
[32m+[m[32m            this.groupBox29.TabIndex = 1;[m[41m[m
[32m+[m[32m            this.groupBox29.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox29.Text = "Faults";[m[41m[m
             // [m
[31m-            this.probeBacklashTextBox.Location = new System.Drawing.Point(243, 52);[m
[31m-            this.probeBacklashTextBox.Name = "probeBacklashTextBox";[m
[31m-            this.probeBacklashTextBox.Size = new System.Drawing.Size(75, 20);[m
[31m-            this.probeBacklashTextBox.TabIndex = 14;[m
[31m-            this.probeBacklashTextBox.Text = "0";[m
[32m+[m[32m            // fibreAmpPowerFaultLED[m[41m[m
             // [m
[31m-            // probePolVoltStopButton[m
[32m+[m[32m            this.fibreAmpPowerFaultLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m[41m[m
[32m+[m[32m            this.fibreAmpPowerFaultLED.Location = new System.Drawing.Point(214, 34);[m[41m[m
[32m+[m[32m            this.fibreAmpPowerFaultLED.Name = "fibreAmpPowerFaultLED";[m[41m[m
[32m+[m[32m            this.fibreAmpPowerFaultLED.OffColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.fibreAmpPowerFaultLED.OnColor = System.Drawing.Color.Red;[m[41m[m
[32m+[m[32m            this.fibreAmpPowerFaultLED.Size = new System.Drawing.Size(38, 40);[m[41m[m
[32m+[m[32m            this.fibreAmpPowerFaultLED.TabIndex = 66;[m[41m[m
             // [m
[31m-            this.probePolVoltStopButton.Enabled = false;[m
[31m-            this.probePolVoltStopButton.Location = new System.Drawing.Point(243, 106);[m
[31m-            this.probePolVoltStopButton.Name = "probePolVoltStopButton";[m
[31m-            this.probePolVoltStopButton.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.probePolVoltStopButton.TabIndex = 51;[m
[31m-            this.probePolVoltStopButton.Text = "Stop";[m
[31m-            this.probePolVoltStopButton.UseVisualStyleBackColor = true;[m
[31m-            this.probePolVoltStopButton.Click += new System.EventHandler(this.probePolVoltStopButton_Click);[m
[32m+[m[32m            // fibreAmpTempFaultLED[m[41m[m
             // [m
[31m-            // probePolVoltTrackBar[m
[32m+[m[32m            this.fibreAmpTempFaultLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m[41m[m
[32m+[m[32m            this.fibreAmpTempFaultLED.Location = new System.Drawing.Point(166, 34);[m[41m[m
[32m+[m[32m            this.fibreAmpTempFaultLED.Name = "fibreAmpTempFaultLED";[m[41m[m
[32m+[m[32m            this.fibreAmpTempFaultLED.OffColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.fibreAmpTempFaultLED.OnColor = System.Drawing.Color.Red;[m[41m[m
[32m+[m[32m            this.fibreAmpTempFaultLED.Size = new System.Drawing.Size(38, 40);[m[41m[m
[32m+[m[32m            this.fibreAmpTempFaultLED.TabIndex = 65;[m[41m[m
             // [m
[31m-            this.probePolVoltTrackBar.Enabled = false;[m
[31m-            this.probePolVoltTrackBar.Location = new System.Drawing.Point(88, 102);[m
[31m-            this.probePolVoltTrackBar.Maximum = 100;[m
[31m-            this.probePolVoltTrackBar.Minimum = -100;[m
[31m-            this.probePolVoltTrackBar.Name = "probePolVoltTrackBar";[m
[31m-            this.probePolVoltTrackBar.Size = new System.Drawing.Size(149, 45);[m
[31m-            this.probePolVoltTrackBar.TabIndex = 51;[m
[31m-            this.probePolVoltTrackBar.Scroll += new System.EventHandler(this.probePolVoltTrackBar_Scroll);[m
[32m+[m[32m            // fibreAmpBackReflectFaultLED[m[41m[m
             // [m
[31m-            // label107[m
[32m+[m[32m            this.fibreAmpBackReflectFaultLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m[41m[m
[32m+[m[32m            this.fibreAmpBackReflectFaultLED.Location = new System.Drawing.Point(112, 34);[m[41m[m
[32m+[m[32m            this.fibreAmpBackReflectFaultLED.Name = "fibreAmpBackReflectFaultLED";[m[41m[m
[32m+[m[32m            this.fibreAmpBackReflectFaultLED.OffColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.fibreAmpBackReflectFaultLED.OnColor = System.Drawing.Color.Red;[m[41m[m
[32m+[m[32m            this.fibreAmpBackReflectFaultLED.Size = new System.Drawing.Size(38, 40);[m[41m[m
[32m+[m[32m            this.fibreAmpBackReflectFaultLED.TabIndex = 64;[m[41m[m
             // [m
[31m-            this.label107.AutoSize = true;[m
[31m-            this.label107.Location = new System.Drawing.Point(9, 126);[m
[31m-            this.label107.Name = "label107";[m
[31m-            this.label107.Size = new System.Drawing.Size(73, 13);[m
[31m-            this.label107.TabIndex = 49;[m
[31m-            this.label107.Text = "Voltage Mode";[m
[32m+[m[32m            // fibreAmpSeedFaultLED[m[41m[m
             // [m
[31m-            // label102[m
[32m+[m[32m            this.fibreAmpSeedFaultLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m[41m[m
[32m+[m[32m            this.fibreAmpSeedFaultLED.Location = new System.Drawing.Point(60, 34);[m[41m[m
[32m+[m[32m            this.fibreAmpSeedFaultLED.Name = "fibreAmpSeedFaultLED";[m[41m[m
[32m+[m[32m            this.fibreAmpSeedFaultLED.OffColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.fibreAmpSeedFaultLED.OnColor = System.Drawing.Color.Red;[m[41m[m
[32m+[m[32m            this.fibreAmpSeedFaultLED.Size = new System.Drawing.Size(38, 40);[m[41m[m
[32m+[m[32m            this.fibreAmpSeedFaultLED.TabIndex = 63;[m[41m[m
             // [m
[31m-            this.label102.AutoSize = true;[m
[31m-            this.label102.Location = new System.Drawing.Point(102, 24);[m
[31m-            this.label102.Name = "label102";[m
[31m-            this.label102.Size = new System.Drawing.Size(53, 13);[m
[31m-            this.label102.TabIndex = 8;[m
[31m-            this.label102.Text = "Set Angle";[m
[32m+[m[32m            // fibreAmpMasterFaultLED[m[41m[m
             // [m
[31m-            // probePolSetAngle[m
[32m+[m[32m            this.fibreAmpMasterFaultLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m[41m[m
[32m+[m[32m            this.fibreAmpMasterFaultLED.Location = new System.Drawing.Point(16, 34);[m[41m[m
[32m+[m[32m            this.fibreAmpMasterFaultLED.Name = "fibreAmpMasterFaultLED";[m[41m[m
[32m+[m[32m            this.fibreAmpMasterFaultLED.OffColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.fibreAmpMasterFaultLED.OnColor = System.Drawing.Color.Red;[m[41m[m
[32m+[m[32m            this.fibreAmpMasterFaultLED.Size = new System.Drawing.Size(38, 40);[m[41m[m
[32m+[m[32m            this.fibreAmpMasterFaultLED.TabIndex = 62;[m[41m[m
             // [m
[31m-            this.probePolSetAngle.Location = new System.Drawing.Point(161, 19);[m
[31m-            this.probePolSetAngle.Name = "probePolSetAngle";[m
[31m-            this.probePolSetAngle.Size = new System.Drawing.Size(66, 20);[m
[31m-            this.probePolSetAngle.TabIndex = 13;[m
[31m-            this.probePolSetAngle.Text = "0";[m
[32m+[m[32m            // faultCheckButton[m[41m[m
             // [m
[31m-            // label103[m
[32m+[m[32m            this.faultCheckButton.Location = new System.Drawing.Point(169, 111);[m[41m[m
[32m+[m[32m            this.faultCheckButton.Name = "faultCheckButton";[m[41m[m
[32m+[m[32m            this.faultCheckButton.Size = new System.Drawing.Size(89, 23);[m[41m[m
[32m+[m[32m            this.faultCheckButton.TabIndex = 61;[m[41m[m
[32m+[m[32m            this.faultCheckButton.Text = "Check for faults";[m[41m[m
[32m+[m[32m            this.faultCheckButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.faultCheckButton.Click += new System.EventHandler(this.faultCheckButton_Click);[m[41m[m
             // [m
[31m-            this.label103.AutoSize = true;[m
[31m-            this.label103.Location = new System.Drawing.Point(172, 78);[m
[31m-            this.label103.Name = "label103";[m
[31m-            this.label103.Size = new System.Drawing.Size(55, 13);[m
[31m-            this.label103.TabIndex = 44;[m
[31m-            this.label103.Text = "Clockwise";[m
[32m+[m[32m            // label93[m[41m[m
             // [m
[31m-            // label104[m
[32m+[m[32m            this.label93.Location = new System.Drawing.Point(218, 77);[m[41m[m
[32m+[m[32m            this.label93.Name = "label93";[m[41m[m
[32m+[m[32m            this.label93.Size = new System.Drawing.Size(47, 31);[m[41m[m
[32m+[m[32m            this.label93.TabIndex = 49;[m[41m[m
[32m+[m[32m            this.label93.Text = "Power supply";[m[41m[m
             // [m
[31m-            this.label104.AutoSize = true;[m
[31m-            this.label104.Location = new System.Drawing.Point(85, 78);[m
[31m-            this.label104.Name = "label104";[m
[31m-            this.label104.Size = new System.Drawing.Size(75, 13);[m
[31m-            this.label104.TabIndex = 45;[m
[31m-            this.label104.Text = "Anti-clockwise";[m
[32m+[m[32m            // label92[m[41m[m
             // [m
[31m-            // setProbePolAngle[m
[32m+[m[32m            this.label92.Location = new System.Drawing.Point(166, 77);[m[41m[m
[32m+[m[32m            this.label92.Name = "label92";[m[41m[m
[32m+[m[32m            this.label92.Size = new System.Drawing.Size(38, 18);[m[41m[m
[32m+[m[32m            this.label92.TabIndex = 48;[m[41m[m
[32m+[m[32m            this.label92.Text = "Temp";[m[41m[m
             // [m
[31m-            this.setProbePolAngle.Location = new System.Drawing.Point(243, 17);[m
[31m-            this.setProbePolAngle.Name = "setProbePolAngle";[m
[31m-            this.setProbePolAngle.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.setProbePolAngle.TabIndex = 5;[m
[31m-            this.setProbePolAngle.Text = "Set";[m
[31m-            this.setProbePolAngle.UseVisualStyleBackColor = true;[m
[31m-            this.setProbePolAngle.Click += new System.EventHandler(this.setProbePolAngle_Click);[m
[32m+[m[32m            // label91[m[41m[m
             // [m
[31m-            // probePolModeSelectSwitch[m
[32m+[m[32m            this.label91.Location = new System.Drawing.Point(112, 77);[m[41m[m
[32m+[m[32m            this.label91.Name = "label91";[m[41m[m
[32m+[m[32m            this.label91.Size = new System.Drawing.Size(59, 31);[m[41m[m
[32m+[m[32m            this.label91.TabIndex = 47;[m[41m[m
[32m+[m[32m            this.label91.Text = "Back reflection";[m[41m[m
             // [m
[31m-            this.probePolModeSelectSwitch.Location = new System.Drawing.Point(12, 33);[m
[31m-            this.probePolModeSelectSwitch.Name = "probePolModeSelectSwitch";[m
[31m-            this.probePolModeSelectSwitch.Size = new System.Drawing.Size(64, 96);[m
[31m-            this.probePolModeSelectSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;[m
[31m-            this.probePolModeSelectSwitch.TabIndex = 51;[m
[31m-            this.probePolModeSelectSwitch.Value = true;[m
[31m-            this.probePolModeSelectSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.probePolModeSelectSwitch_StateChanged_1);[m
[32m+[m[32m            // label90[m[41m[m
             // [m
[31m-            // tabPage5[m
[32m+[m[32m            this.label90.Location = new System.Drawing.Point(67, 77);[m[41m[m
[32m+[m[32m            this.label90.Name = "label90";[m[41m[m
[32m+[m[32m            this.label90.Size = new System.Drawing.Size(39, 18);[m[41m[m
[32m+[m[32m            this.label90.TabIndex = 46;[m[41m[m
[32m+[m[32m            this.label90.Text = "Seed";[m[41m[m
             // [m
[31m-            this.tabPage5.BackColor = System.Drawing.Color.Transparent;[m
[31m-            this.tabPage5.Controls.Add(this.groupBox17);[m
[31m-            this.tabPage5.Controls.Add(this.groupBox15);[m
[31m-            this.tabPage5.Location = new System.Drawing.Point(4, 22);[m
[31m-            this.tabPage5.Name = "tabPage5";[m
[31m-            this.tabPage5.Size = new System.Drawing.Size(697, 575);[m
[31m-            this.tabPage5.TabIndex = 4;[m
[31m-            this.tabPage5.Text = "Source";[m
[32m+[m[32m            // label89[m[41m[m
             // [m
[31m-            // groupBox17[m
[32m+[m[32m            this.label89.Location = new System.Drawing.Point(13, 77);[m[41m[m
[32m+[m[32m            this.label89.Name = "label89";[m[41m[m
[32m+[m[32m            this.label89.Size = new System.Drawing.Size(41, 18);[m[41m[m
[32m+[m[32m            this.label89.TabIndex = 45;[m[41m[m
[32m+[m[32m            this.label89.Text = "Master";[m[41m[m
             // [m
[31m-            this.groupBox17.Controls.Add(this.TargetStepButton);[m
[31m-            this.groupBox17.Controls.Add(this.label66);[m
[31m-            this.groupBox17.Controls.Add(this.TargetNumStepsTextBox);[m
[31m-            this.groupBox17.Location = new System.Drawing.Point(13, 165);[m
[31m-            this.groupBox17.Name = "groupBox17";[m
[31m-            this.groupBox17.Size = new System.Drawing.Size(351, 64);[m
[31m-            this.groupBox17.TabIndex = 47;[m
[31m-            this.groupBox17.TabStop = false;[m
[31m-            this.groupBox17.Text = "Target stepper";[m
[32m+[m[32m            // groupBox27[m[41m[m
             // [m
[31m-            // TargetStepButton[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.flPZT2TempCurButton);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.flPZT2CurTextBox);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.flPZT2TempUpdateButton);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.label116);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.flPZT2TempTextBox);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.label115);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.MenloPZTTrackBar);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.label94);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.MenloPZTStepTextBox);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.panel6);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.updateflPZTButton);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.MenloPZTTextBox);[m[41m[m
[32m+[m[32m            this.groupBox27.Controls.Add(this.label87);[m[41m[m
[32m+[m[32m            this.groupBox27.Location = new System.Drawing.Point(9, 218);[m[41m[m
[32m+[m[32m            this.groupBox27.Name = "groupBox27";[m[41m[m
[32m+[m[32m            this.groupBox27.Size = new System.Drawing.Size(393, 185);[m[41m[m
[32m+[m[32m            this.groupBox27.TabIndex = 1;[m[41m[m
[32m+[m[32m            this.groupBox27.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox27.Text = "Fibre Laser";[m[41m[m
             // [m
[31m-            this.TargetStepButton.Location = new System.Drawing.Point(256, 20);[m
[31m-            this.TargetStepButton.Name = "TargetStepButton";[m
[31m-            this.TargetStepButton.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.TargetStepButton.TabIndex = 2;[m
[31m-            this.TargetStepButton.Text = "Step!";[m
[31m-            this.TargetStepButton.UseVisualStyleBackColor = true;[m
[31m-            this.TargetStepButton.Click += new System.EventHandler(this.TargetStepButton_Click);[m
[32m+[m[32m            // flPZT2TempCurButton[m[41m[m
             // [m
[31m-            // label66[m
[32m+[m[32m            this.flPZT2TempCurButton.Location = new System.Drawing.Point(208, 153);[m[41m[m
[32m+[m[32m            this.flPZT2TempCurButton.Name = "flPZT2TempCurButton";[m[41m[m
[32m+[m[32m            this.flPZT2TempCurButton.Size = new System.Drawing.Size(72, 23);[m[41m[m
[32m+[m[32m            this.flPZT2TempCurButton.TabIndex = 79;[m[41m[m
[32m+[m[32m            this.flPZT2TempCurButton.Text = "Update";[m[41m[m
[32m+[m[32m            this.flPZT2TempCurButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.flPZT2TempCurButton.Click += new System.EventHandler(this.flPZT2TempCurButton_Click);[m[41m[m
             // [m
[31m-            this.label66.AutoSize = true;[m
[31m-            this.label66.Location = new System.Drawing.Point(19, 25);[m
[31m-            this.label66.Name = "label66";[m
[31m-            this.label66.Size = new System.Drawing.Size(89, 13);[m
[31m-            this.label66.TabIndex = 1;[m
[31m-            this.label66.Text = "Number of pulses";[m
[32m+[m[32m            // flPZT2CurTextBox[m[41m[m
             // [m
[31m-            // TargetNumStepsTextBox[m
[32m+[m[32m            this.flPZT2CurTextBox.BackColor = System.Drawing.Color.White;[m[41m[m
[32m+[m[32m            this.flPZT2CurTextBox.Location = new System.Drawing.Point(138, 156);[m[41m[m
[32m+[m[32m            this.flPZT2CurTextBox.Name = "flPZT2CurTextBox";[m[41m[m
[32m+[m[32m            this.flPZT2CurTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.flPZT2CurTextBox.TabIndex = 78;[m[41m[m
[32m+[m[32m            this.flPZT2CurTextBox.Text = "0";[m[41m[m
             // [m
[31m-            this.TargetNumStepsTextBox.Location = new System.Drawing.Point(158, 22);[m
[31m-            this.TargetNumStepsTextBox.Name = "TargetNumStepsTextBox";[m
[31m-            this.TargetNumStepsTextBox.Size = new System.Drawing.Size(66, 20);[m
[31m-            this.TargetNumStepsTextBox.TabIndex = 0;[m
[31m-            this.TargetNumStepsTextBox.Text = "10";[m
[32m+[m[32m            // flPZT2TempUpdateButton[m[41m[m
             // [m
[31m-            // groupBox15[m
[32m+[m[32m            this.flPZT2TempUpdateButton.Location = new System.Drawing.Point(208, 131);[m[41m[m
[32m+[m[32m            this.flPZT2TempUpdateButton.Name = "flPZT2TempUpdateButton";[m[41m[m
[32m+[m[32m            this.flPZT2TempUpdateButton.Size = new System.Drawing.Size(72, 23);[m[41m[m
[32m+[m[32m            this.flPZT2TempUpdateButton.TabIndex = 77;[m[41m[m
[32m+[m[32m            this.flPZT2TempUpdateButton.Text = "Update";[m[41m[m
[32m+[m[32m            this.flPZT2TempUpdateButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.flPZT2TempUpdateButton.Click += new System.EventHandler(this.flPZT2TempUpdateButton_Click);[m[41m[m
             // [m
[31m-            this.groupBox15.Controls.Add(this.label33);[m
[31m-            this.groupBox15.Controls.Add(this.checkYagInterlockButton);[m
[31m-            this.groupBox15.Controls.Add(this.yagFlashlampVTextBox);[m
[31m-            this.groupBox15.Controls.Add(this.interlockStatusTextBox);[m
[31m-            this.groupBox15.Controls.Add(this.updateFlashlampVButton);[m
[31m-            this.groupBox15.Controls.Add(this.label34);[m
[31m-            this.groupBox15.Controls.Add(this.startYAGFlashlampsButton);[m
[31m-            this.groupBox15.Controls.Add(this.yagQDisableButton);[m
[31m-            this.groupBox15.Controls.Add(this.stopYagFlashlampsButton);[m
[31m-            this.groupBox15.Controls.Add(this.yagQEnableButton);[m
[31m-            this.groupBox15.Location = new System.Drawing.Point(13, 14);[m
[31m-            this.groupBox15.Name = "groupBox15";[m
[31m-            this.groupBox15.Size = new System.Drawing.Size(528, 145);[m
[31m-            this.groupBox15.TabIndex = 46;[m
[31m-            this.groupBox15.TabStop = false;[m
[31m-            this.groupBox15.Text = "YAG";[m
[32m+[m[32m            // label116[m[41m[m
             // [m
[31m-            // label33[m
[32m+[m[32m            this.label116.Location = new System.Drawing.Point(6, 159);[m[41m[m
[32m+[m[32m            this.label116.Name = "label116";[m[41m[m
[32m+[m[32m            this.label116.Size = new System.Drawing.Size(126, 18);[m[41m[m
[32m+[m[32m            this.label116.TabIndex = 76;[m[41m[m
[32m+[m[32m            this.label116.Text = "Current Control (V)";[m[41m[m
             // [m
[31m-            this.label33.Location = new System.Drawing.Point(16, 31);[m
[31m-            this.label33.Name = "label33";[m
[31m-            this.label33.Size = new System.Drawing.Size(144, 23);[m
[31m-            this.label33.TabIndex = 13;[m
[31m-            this.label33.Text = "Flashlamp voltage (V)";[m
[32m+[m[32m            // flPZT2TempTextBox[m[41m[m
             // [m
[31m-            // checkYagInterlockButton[m
[32m+[m[32m            this.flPZT2TempTextBox.BackColor = System.Drawing.Color.White;[m[41m[m
[32m+[m[32m            this.flPZT2TempTextBox.Location = new System.Drawing.Point(138, 134);[m[41m[m
[32m+[m[32m            this.flPZT2TempTextBox.Name = "flPZT2TempTextBox";[m[41m[m
[32m+[m[32m            this.flPZT2TempTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.flPZT2TempTextBox.TabIndex = 75;[m[41m[m
[32m+[m[32m            this.flPZT2TempTextBox.Text = "0";[m[41m[m
             // [m
[31m-            this.checkYagInterlockButton.Location = new System.Drawing.Point(256, 63);[m
[31m-            this.checkYagInterlockButton.Name = "checkYagInterlockButton";[m
[31m-            this.checkYagInterlockButton.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.checkYagInterlockButton.TabIndex = 45;[m
[31m-            this.checkYagInterlockButton.Text = "Check";[m
[31m-            this.checkYagInterlockButton.Click += new System.EventHandler(this.checkYagInterlockButton_Click);[m
[32m+[m[32m            // label115[m[41m[m
             // [m
[31m-            // yagFlashlampVTextBox[m
[32m+[m[32m            this.label115.Location = new System.Drawing.Point(6, 137);[m[41m[m
[32m+[m[32m            this.label115.Name = "label115";[m[41m[m
[32m+[m[32m            this.label115.Size = new System.Drawing.Size(126, 18);[m[41m[m
[32m+[m[32m            this.label115.TabIndex = 74;[m[41m[m
[32m+[m[32m            this.label115.Text = "Temp Control (V)";[m[41m[m
             // [m
[31m-            this.yagFlashlampVTextBox.Location = new System.Drawing.Point(160, 31);[m
[31m-            this.yagFlashlampVTextBox.Name = "yagFlashlampVTextBox";[m
[31m-            this.yagFlashlampVTextBox.Size = new System.Drawing.Size(64, 20);[m
[31m-            this.yagFlashlampVTextBox.TabIndex = 12;[m
[31m-            this.yagFlashlampVTextBox.Text = "1220";[m
[32m+[m[32m            // MenloPZTTrackBar[m[41m[m
             // [m
[31m-            // interlockStatusTextBox[m
[32m+[m[32m            this.MenloPZTTrackBar.Location = new System.Drawing.Point(7, 51);[m[41m[m
[32m+[m[32m            this.MenloPZTTrackBar.Maximum = 1000;[m[41m[m
[32m+[m[32m            this.MenloPZTTrackBar.Name = "MenloPZTTrackBar";[m[41m[m
[32m+[m[32m            this.MenloPZTTrackBar.Size = new System.Drawing.Size(373, 45);[m[41m[m
[32m+[m[32m            this.MenloPZTTrackBar.TabIndex = 50;[m[41m[m
[32m+[m[32m            this.MenloPZTTrackBar.Scroll += new System.EventHandler(this.diodeRefCavtrackBar_Scroll);[m[41m[m
             // [m
[31m-            this.interlockStatusTextBox.BackColor = System.Drawing.Color.Black;[m
[31m-            this.interlockStatusTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m
[31m-            this.interlockStatusTextBox.Location = new System.Drawing.Point(160, 63);[m
[31m-            this.interlockStatusTextBox.Name = "interlockStatusTextBox";[m
[31m-            this.interlockStatusTextBox.ReadOnly = true;[m
[31m-            this.interlockStatusTextBox.Size = new System.Drawing.Size(64, 20);[m
[31m-            this.interlockStatusTextBox.TabIndex = 44;[m
[31m-            this.interlockStatusTextBox.Text = "0";[m
[32m+[m[32m            // label94[m[41m[m
             // [m
[31m-            // updateFlashlampVButton[m
[32m+[m[32m            this.label94.Location = new System.Drawing.Point(6, 102);[m[41m[m
[32m+[m[32m            this.label94.Name = "label94";[m[41m[m
[32m+[m[32m            this.label94.Size = new System.Drawing.Size(126, 18);[m[41m[m
[32m+[m[32m            this.label94.TabIndex = 73;[m[41m[m
[32m+[m[32m            this.label94.Text = "Piezo Control Step (V)";[m[41m[m
             // [m
[31m-            this.updateFlashlampVButton.Location = new System.Drawing.Point(256, 31);[m
[31m-            this.updateFlashlampVButton.Name = "updateFlashlampVButton";[m
[31m-            this.updateFlashlampVButton.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.updateFlashlampVButton.TabIndex = 14;[m
[31m-            this.updateFlashlampVButton.Text = "Update V";[m
[31m-            this.updateFlashlampVButton.Click += new System.EventHandler(this.updateFlashlampVButton_Click);[m
[32m+[m[32m            // MenloPZTStepTextBox[m[41m[m
             // [m
[31m-            // label34[m
[32m+[m[32m            this.MenloPZTStepTextBox.BackColor = System.Drawing.Color.White;[m[41m[m
[32m+[m[32m            this.MenloPZTStepTextBox.Location = new System.Drawing.Point(138, 100);[m[41m[m
[32m+[m[32m            this.MenloPZTStepTextBox.Name = "MenloPZTStepTextBox";[m[41m[m
[32m+[m[32m            this.MenloPZTStepTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.MenloPZTStepTextBox.TabIndex = 72;[m[41m[m
[32m+[m[32m            this.MenloPZTStepTextBox.Text = "0";[m[41m[m
             // [m
[31m-            this.label34.Location = new System.Drawing.Point(16, 63);[m
[31m-            this.label34.Name = "label34";[m
[31m-            this.label34.Size = new System.Drawing.Size(104, 23);[m
[31m-            this.label34.TabIndex = 43;[m
[31m-            this.label34.Text = "Interlock failed";[m
[32m+[m[32m            // panel6[m[41m[m
             // [m
[31m-            // startYAGFlashlampsButton[m
[32m+[m[32m            this.panel6.Controls.Add(this.flPZT2StepZeroButton);[m[41m[m
[32m+[m[32m            this.panel6.Controls.Add(this.MenloPZTStepPlusButton);[m[41m[m
[32m+[m[32m            this.panel6.Controls.Add(this.MenloPZTStepMinusButton);[m[41m[m
[32m+[m[32m            this.panel6.Location = new System.Drawing.Point(194, 16);[m[41m[m
[32m+[m[32m            this.panel6.Name = "panel6";[m[41m[m
[32m+[m[32m            this.panel6.Size = new System.Drawing.Size(108, 29);[m[41m[m
[32m+[m[32m            this.panel6.TabIndex = 71;[m[41m[m
             // [m
[31m-            this.startYAGFlashlampsButton.Location = new System.Drawing.Point(16, 103);[m
[31m-            this.startYAGFlashlampsButton.Name = "startYAGFlashlampsButton";[m
[31m-            this.startYAGFlashlampsButton.Size = new System.Drawing.Size(112, 23);[m
[31m-            this.startYAGFlashlampsButton.TabIndex = 15;[m
[31m-            this.startYAGFlashlampsButton.Text = "Start Flashlamps";[m
[31m-            this.startYAGFlashlampsButton.Click += new System.EventHandler(this.startYAGFlashlampsButton_Click);[m
[32m+[m[32m            // flPZT2StepZeroButton[m[41m[m
             // [m
[31m-            // yagQDisableButton[m
[32m+[m[32m            this.flPZT2StepZeroButton.AutoSize = true;[m[41m[m
[32m+[m[32m            this.flPZT2StepZeroButton.Checked = true;[m[41m[m
[32m+[m[32m            this.flPZT2StepZeroButton.Location = new System.Drawing.Point(74, 7);[m[41m[m
[32m+[m[32m            this.flPZT2StepZeroButton.Name = "flPZT2StepZeroButton";[m[41m[m
[32m+[m[32m            this.flPZT2StepZeroButton.Size = new System.Drawing.Size(31, 17);[m[41m[m
[32m+[m[32m            this.flPZT2StepZeroButton.TabIndex = 32;[m[41m[m
[32m+[m[32m            this.flPZT2StepZeroButton.TabStop = true;[m[41m[m
[32m+[m[32m            this.flPZT2StepZeroButton.Text = "0";[m[41m[m
[32m+[m[32m            this.flPZT2StepZeroButton.UseVisualStyleBackColor = true;[m[41m[m
             // [m
[31m-            this.yagQDisableButton.Enabled = false;[m
[31m-            this.yagQDisableButton.Location = new System.Drawing.Point(400, 103);[m
[31m-            this.yagQDisableButton.Name = "yagQDisableButton";[m
[31m-            this.yagQDisableButton.Size = new System.Drawing.Size(112, 23);[m
[31m-            this.yagQDisableButton.TabIndex = 18;[m
[31m-            this.yagQDisableButton.Text = "Q-switch Disable";[m
[31m-            this.yagQDisableButton.Click += new System.EventHandler(this.yagQDisableButton_Click);[m
[32m+[m[32m            // MenloPZTStepPlusButton[m[41m[m
             // [m
[31m-            // stopYagFlashlampsButton[m
[32m+[m[32m            this.MenloPZTStepPlusButton.AutoSize = true;[m[41m[m
[32m+[m[32m            this.MenloPZTStepPlusButton.Location = new System.Drawing.Point(3, 6);[m[41m[m
[32m+[m[32m            this.MenloPZTStepPlusButton.Name = "MenloPZTStepPlusButton";[m[41m[m
[32m+[m[32m            this.MenloPZTStepPlusButton.Size = new System.Drawing.Size(31, 17);[m[41m[m
[32m+[m[32m            this.MenloPZTStepPlusButton.TabIndex = 32;[m[41m[m
[32m+[m[32m            this.MenloPZTStepPlusButton.Text = "+";[m[41m[m
[32m+[m[32m            this.MenloPZTStepPlusButton.UseVisualStyleBackColor = true;[m[41m[m
             // [m
[31m-            this.stopYagFlashlampsButton.Enabled = false;[m
[31m-            this.stopYagFlashlampsButton.Location = new System.Drawing.Point(144, 103);[m
[31m-            this.stopYagFlashlampsButton.Name = "stopYagFlashlampsButton";[m
[31m-            this.stopYagFlashlampsButton.Size = new System.Drawing.Size(112, 23);[m
[31m-            this.stopYagFlashlampsButton.TabIndex = 16;[m
[31m-            this.stopYagFlashlampsButton.Text = "Stop Flashlamps";[m
[31m-            this.stopYagFlashlampsButton.Click += new System.EventHandler(this.stopYagFlashlampsButton_Click);[m
[32m+[m[32m            // MenloPZTStepMinusButton[m[41m[m
             // [m
[31m-            // yagQEnableButton[m
[32m+[m[32m            this.MenloPZTStepMinusButton.AutoSize = true;[m[41m[m
[32m+[m[32m            this.MenloPZTStepMinusButton.Location = new System.Drawing.Point(40, 7);[m[41m[m
[32m+[m[32m            this.MenloPZTStepMinusButton.Name = "MenloPZTStepMinusButton";[m[41m[m
[32m+[m[32m            this.MenloPZTStepMinusButton.Size = new System.Drawing.Size(28, 17);[m[41m[m
[32m+[m[32m            this.MenloPZTStepMinusButton.TabIndex = 32;[m[41m[m
[32m+[m[32m            this.MenloPZTStepMinusButton.Text = "-";[m[41m[m
[32m+[m[32m            this.MenloPZTStepMinusButton.UseVisualStyleBackColor = true;[m[41m[m
             // [m
[31m-            this.yagQEnableButton.Location = new System.Drawing.Point(272, 103);[m
[31m-            this.yagQEnableButton.Name = "yagQEnableButton";[m
[31m-            this.yagQEnableButton.Size = new System.Drawing.Size(112, 23);[m
[31m-            this.yagQEnableButton.TabIndex = 17;[m
[31m-            this.yagQEnableButton.Text = "Q-switch Enable";[m
[31m-            this.yagQEnableButton.Click += new System.EventHandler(this.yagQEnableButton_Click);[m
[32m+[m[32m            // updateflPZTButton[m[41m[m
             // [m
[31m-            // tabPage9[m
[32m+[m[32m            this.updateflPZTButton.Location = new System.Drawing.Point(308, 19);[m[41m[m
[32m+[m[32m            this.updateflPZTButton.Name = "updateflPZTButton";[m[41m[m
[32m+[m[32m            this.updateflPZTButton.Size = new System.Drawing.Size(72, 23);[m[41m[m
[32m+[m[32m            this.updateflPZTButton.TabIndex = 64;[m[41m[m
[32m+[m[32m            this.updateflPZTButton.Text = "Update";[m[41m[m
[32m+[m[32m            this.updateflPZTButton.UseVisualStyleBackColor = true;[m[41m[m
             // [m
[31m-            this.tabPage9.BackColor = System.Drawing.Color.Transparent;[m
[31m-            this.tabPage9.Controls.Add(this.switchScanTTLSwitch);[m
[31m-            this.tabPage9.Controls.Add(this.label97);[m
[31m-            this.tabPage9.Location = new System.Drawing.Point(4, 22);[m
[31m-            this.tabPage9.Name = "tabPage9";[m
[31m-            this.tabPage9.Padding = new System.Windows.Forms.Padding(3);[m
[31m-            this.tabPage9.Size = new System.Drawing.Size(697, 575);[m
[31m-            this.tabPage9.TabIndex = 8;[m
[31m-            this.tabPage9.Text = "Misc";[m
[32m+[m[32m            // MenloPZTTextBox[m[41m[m
             // [m
[31m-            // switchScanTTLSwitch[m
[32m+[m[32m            this.MenloPZTTextBox.BackColor = System.Drawing.Color.LimeGreen;[m[41m[m
[32m+[m[32m            this.MenloPZTTextBox.Location = new System.Drawing.Point(127, 22);[m[41m[m
[32m+[m[32m            this.MenloPZTTextBox.Name = "MenloPZTTextBox";[m[41m[m
[32m+[m[32m            this.MenloPZTTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.MenloPZTTextBox.TabIndex = 45;[m[41m[m
[32m+[m[32m            this.MenloPZTTextBox.Text = "0";[m[41m[m
             // [m
[31m-            this.switchScanTTLSwitch.Location = new System.Drawing.Point(6, 6);[m
[31m-            this.switchScanTTLSwitch.Name = "switchScanTTLSwitch";[m
[31m-            this.switchScanTTLSwitch.Size = new System.Drawing.Size(64, 96);[m
[31m-            this.switchScanTTLSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;[m
[31m-            this.switchScanTTLSwitch.TabIndex = 2;[m
[31m-            this.switchScanTTLSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.switch1_StateChanged);[m
[32m+[m[32m            // label87[m[41m[m
             // [m
[31m-            // label97[m
[32m+[m[32m            this.label87.Location = new System.Drawing.Point(6, 25);[m[41m[m
[32m+[m[32m            this.label87.Name = "label87";[m[41m[m
[32m+[m[32m            this.label87.Size = new System.Drawing.Size(93, 18);[m[41m[m
[32m+[m[32m            this.label87.TabIndex = 44;[m[41m[m
[32m+[m[32m            this.label87.Text = "Piezo Control (V)";[m[41m[m
             // [m
[31m-            this.label97.AutoSize = true;[m
[31m-            this.label97.Location = new System.Drawing.Point(76, 53);[m
[31m-            this.label97.Name = "label97";[m
[31m-            this.label97.Size = new System.Drawing.Size(90, 13);[m
[31m-            this.label97.TabIndex = 1;[m
[31m-            this.label97.Text = "Switch Scan TTL";[m
[32m+[m[32m            // groupBox26[m[41m[m
             // [m
[31m-            // tabPage7[m
[32m+[m[32m            this.groupBox26.Controls.Add(this.updateDiodeCurrentMonButton);[m[41m[m
[32m+[m[32m            this.groupBox26.Controls.Add(this.diodeCurrentTextBox);[m[41m[m
[32m+[m[32m            this.groupBox26.Controls.Add(this.stopDiodeCurrentPollButton);[m[41m[m
[32m+[m[32m            this.groupBox26.Controls.Add(this.startDiodeCurrentPollButton);[m[41m[m
[32m+[m[32m            this.groupBox26.Controls.Add(this.diodeCurrentPollTextBox);[m[41m[m
[32m+[m[32m            this.groupBox26.Controls.Add(this.label86);[m[41m[m
[32m+[m[32m            this.groupBox26.Controls.Add(this.diodeCurrentGraph);[m[41m[m
[32m+[m[32m            this.groupBox26.Location = new System.Drawing.Point(6, 6);[m[41m[m
[32m+[m[32m            this.groupBox26.Name = "groupBox26";[m[41m[m
[32m+[m[32m            this.groupBox26.Size = new System.Drawing.Size(685, 206);[m[41m[m
[32m+[m[32m            this.groupBox26.TabIndex = 0;[m[41m[m
[32m+[m[32m            this.groupBox26.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox26.Text = "Current Supply";[m[41m[m
             // [m
[31m-            this.tabPage7.BackColor = System.Drawing.Color.Transparent;[m
[31m-            this.tabPage7.Controls.Add(this.clearAlertButton);[m
[31m-            this.tabPage7.Controls.Add(this.alertTextBox);[m
[31m-            this.tabPage7.ImageKey = "(none)";[m
[31m-            this.tabPage7.Location = new System.Drawing.Point(4, 22);[m
[31m-            this.tabPage7.Name = "tabPage7";[m
[31m-            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);[m
[31m-            this.tabPage7.Size = new System.Drawing.Size(697, 575);[m
[31m-            this.tabPage7.TabIndex = 6;[m
[31m-            this.tabPage7.Text = "Alerts";[m
[32m+[m[32m            // updateDiodeCurrentMonButton[m[41m[m
             // [m
[31m-            // clearAlertButton[m
[32m+[m[32m            this.updateDiodeCurrentMonButton.Location = new System.Drawing.Point(178, 176);[m[41m[m
[32m+[m[32m            this.updateDiodeCurrentMonButton.Name = "updateDiodeCurrentMonButton";[m[41m[m
[32m+[m[32m            this.updateDiodeCurrentMonButton.Size = new System.Drawing.Size(72, 23);[m[41m[m
[32m+[m[32m            this.updateDiodeCurrentMonButton.TabIndex = 62;[m[41m[m
[32m+[m[32m            this.updateDiodeCurrentMonButton.Text = "Update";[m[41m[m
[32m+[m[32m            this.updateDiodeCurrentMonButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.updateDiodeCurrentMonButton.Click += new System.EventHandler(this.updateDiodeCurrentMonButton_Click);[m[41m[m
             // [m
[31m-            this.clearAlertButton.Location = new System.Drawing.Point(18, 540);[m
[31m-            this.clearAlertButton.Name = "clearAlertButton";[m
[31m-            this.clearAlertButton.Size = new System.Drawing.Size(140, 23);[m
[31m-            this.clearAlertButton.TabIndex = 1;[m
[31m-            this.clearAlertButton.Text = "Clear alert status";[m
[31m-            this.clearAlertButton.UseVisualStyleBackColor = true;[m
[31m-            this.clearAlertButton.Click += new System.EventHandler(this.clearAlertButton_Click);[m
[32m+[m[32m            // diodeCurrentTextBox[m[41m[m
             // [m
[31m-            // alertTextBox[m
[32m+[m[32m            this.diodeCurrentTextBox.BackColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.diodeCurrentTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m[41m[m
[32m+[m[32m            this.diodeCurrentTextBox.Location = new System.Drawing.Point(35, 178);[m[41m[m
[32m+[m[32m            this.diodeCurrentTextBox.Name = "diodeCurrentTextBox";[m[41m[m
[32m+[m[32m            this.diodeCurrentTextBox.ReadOnly = true;[m[41m[m
[32m+[m[32m            this.diodeCurrentTextBox.Size = new System.Drawing.Size(137, 20);[m[41m[m
[32m+[m[32m            this.diodeCurrentTextBox.TabIndex = 61;[m[41m[m
[32m+[m[32m            this.diodeCurrentTextBox.Text = "0";[m[41m[m
             // [m
[31m-            this.alertTextBox.Location = new System.Drawing.Point(18, 22);[m
[31m-            this.alertTextBox.Multiline = true;[m
[31m-            this.alertTextBox.Name = "alertTextBox";[m
[31m-            this.alertTextBox.Size = new System.Drawing.Size(654, 512);[m
[31m-            this.alertTextBox.TabIndex = 0;[m
[32m+[m[32m            // stopDiodeCurrentPollButton[m[41m[m
             // [m
[31m-            // tabPage8[m
[32m+[m[32m            this.stopDiodeCurrentPollButton.Enabled = false;[m[41m[m
[32m+[m[32m            this.stopDiodeCurrentPollButton.Location = new System.Drawing.Point(604, 176);[m[41m[m
[32m+[m[32m            this.stopDiodeCurrentPollButton.Name = "stopDiodeCurrentPollButton";[m[41m[m
[32m+[m[32m            this.stopDiodeCurrentPollButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.stopDiodeCurrentPollButton.TabIndex = 60;[m[41m[m
[32m+[m[32m            this.stopDiodeCurrentPollButton.Text = "Stop poll";[m[41m[m
[32m+[m[32m            this.stopDiodeCurrentPollButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.stopDiodeCurrentPollButton.Click += new System.EventHandler(this.stopDiodeCurrentPollButton_Click);[m[41m[m
             // [m
[31m-            this.tabPage8.BackColor = System.Drawing.Color.Transparent;[m
[31m-            this.tabPage8.Controls.Add(this.groupBox36);[m
[31m-            this.tabPage8.Controls.Add(this.groupBox28);[m
[31m-            this.tabPage8.Controls.Add(this.groupBox27);[m
[31m-            this.tabPage8.Controls.Add(this.groupBox26);[m
[31m-            this.tabPage8.Location = new System.Drawing.Point(4, 22);[m
[31m-            this.tabPage8.Name = "tabPage8";[m
[31m-            this.tabPage8.Padding = new System.Windows.Forms.Padding(3);[m
[31m-            this.tabPage8.Size = new System.Drawing.Size(697, 575);[m
[31m-            this.tabPage8.TabIndex = 7;[m
[31m-            this.tabPage8.Text = "N=2 Lasers";[m
[32m+[m[32m            // startDiodeCurrentPollButton[m[41m[m
             // [m
[31m-            // groupBox36[m
[32m+[m[32m            this.startDiodeCurrentPollButton.Location = new System.Drawing.Point(523, 176);[m[41m[m
[32m+[m[32m            this.startDiodeCurrentPollButton.Name = "startDiodeCurrentPollButton";[m[41m[m
[32m+[m[32m            this.startDiodeCurrentPollButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.startDiodeCurrentPollButton.TabIndex = 59;[m[41m[m
[32m+[m[32m            this.startDiodeCurrentPollButton.Text = "Start poll";[m[41m[m
[32m+[m[32m            this.startDiodeCurrentPollButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.startDiodeCurrentPollButton.Click += new System.EventHandler(this.startDiodeCurrentPollButton_Click);[m[41m[m
             // [m
[31m-            this.groupBox36.Controls.Add(this.flAOMFreqStepTextBox);[m
[31m-            this.groupBox36.Controls.Add(this.label119);[m
[31m-            this.groupBox36.Controls.Add(this.flAOMFreqPlusTextBox);[m
[31m-            this.groupBox36.Controls.Add(this.flAOMFreqCentreTextBox);[m
[31m-            this.groupBox36.Controls.Add(this.label120);[m
[31m-            this.groupBox36.Controls.Add(this.flAOMFreqMinusTextBox);[m
[31m-            this.groupBox36.Controls.Add(this.label121);[m
[31m-            this.groupBox36.Controls.Add(this.flAOMFreqUpdateButton);[m
[31m-            this.groupBox36.Controls.Add(this.label122);[m
[31m-            this.groupBox36.Controls.Add(this.panel8);[m
[31m-            this.groupBox36.Controls.Add(this.flAOMStepTextBox);[m
[31m-            this.groupBox36.Controls.Add(this.label117);[m
[31m-            this.groupBox36.Controls.Add(this.flAOMVoltageTextBox);[m
[31m-            this.groupBox36.Controls.Add(this.UpdateFLAOMButton);[m
[31m-            this.groupBox36.Controls.Add(this.label118);[m
[31m-            this.groupBox36.Location = new System.Drawing.Point(9, 409);[m
[31m-            this.groupBox36.Name = "groupBox36";[m
[31m-            this.groupBox36.Size = new System.Drawing.Size(393, 148);[m
[31m-            this.groupBox36.TabIndex = 49;[m
[31m-            this.groupBox36.TabStop = false;[m
[31m-            this.groupBox36.Text = "Stabilising AOM";[m
[32m+[m[32m            // diodeCurrentPollTextBox[m[41m[m
             // [m
[31m-            // flAOMFreqStepTextBox[m
[32m+[m[32m            this.diodeCurrentPollTextBox.Location = new System.Drawing.Point(453, 178);[m[41m[m
[32m+[m[32m            this.diodeCurrentPollTextBox.Name = "diodeCurrentPollTextBox";[m[41m[m
[32m+[m[32m            this.diodeCurrentPollTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.diodeCurrentPollTextBox.TabIndex = 58;[m[41m[m
[32m+[m[32m            this.diodeCurrentPollTextBox.Text = "100";[m[41m[m
             // [m
[31m-            this.flAOMFreqStepTextBox.BackColor = System.Drawing.Color.Black;[m
[31m-            this.flAOMFreqStepTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m
[31m-            this.flAOMFreqStepTextBox.Location = new System.Drawing.Point(255, 95);[m
[31m-            this.flAOMFreqStepTextBox.Name = "flAOMFreqStepTextBox";[m
[31m-            this.flAOMFreqStepTextBox.ReadOnly = true;[m
[31m-            this.flAOMFreqStepTextBox.Size = new System.Drawing.Size(126, 20);[m
[31m-            this.flAOMFreqStepTextBox.TabIndex = 74;[m
[31m-            this.flAOMFreqStepTextBox.Text = "0";[m
[32m+[m[32m            // label86[m[41m[m
             // [m
[31m-            // label119[m
[32m+[m[32m            this.label86.Location = new System.Drawing.Point(366, 181);[m[41m[m
[32m+[m[32m            this.label86.Name = "label86";[m[41m[m
[32m+[m[32m            this.label86.Size = new System.Drawing.Size(101, 23);[m[41m[m
[32m+[m[32m            this.label86.TabIndex = 57;[m[41m[m
[32m+[m[32m            this.label86.Text = "Poll period (ms)";[m[41m[m
             // [m
[31m-            this.label119.Location = new System.Drawing.Point(151, 98);[m
[31m-            this.label119.Name = "label119";[m
[31m-            this.label119.Size = new System.Drawing.Size(96, 23);[m
[31m-            this.label119.TabIndex = 72;[m
[31m-            this.label119.Text = "Step (Hz)";[m
[32m+[m[32m            // diodeCurrentGraph[m[41m[m
             // [m
[31m-            // flAOMFreqPlusTextBox[m
[32m+[m[32m            this.diodeCurrentGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY)[m[41m [m
[32m+[m[32m            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint)[m[41m [m
[32m+[m[32m            | NationalInstruments.UI.GraphInteractionModes.PanX)[m[41m [m
[32m+[m[32m            | NationalInstruments.UI.GraphInteractionModes.PanY)[m[41m [m
[32m+[m[32m            | NationalInstruments.UI.GraphInteractionModes.DragCursor)[m[41m [m
[32m+[m[32m            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption)[m[41m [m
[32m+[m[32m            | NationalInstruments.UI.GraphInteractionModes.EditRange)));[m[41m[m
[32m+[m[32m            this.diodeCurrentGraph.Location = new System.Drawing.Point(6, 19);[m[41m[m
[32m+[m[32m            this.diodeCurrentGraph.Name = "diodeCurrentGraph";[m[41m[m
[32m+[m[32m            this.diodeCurrentGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {[m[41m[m
[32m+[m[32m            this.diodeCurrentPlot});[m[41m[m
[32m+[m[32m            this.diodeCurrentGraph.Size = new System.Drawing.Size(673, 153);[m[41m[m
[32m+[m[32m            this.diodeCurrentGraph.TabIndex = 46;[m[41m[m
[32m+[m[32m            this.diodeCurrentGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {[m[41m[m
[32m+[m[32m            this.xAxis2});[m[41m[m
[32m+[m[32m            this.diodeCurrentGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {[m[41m[m
[32m+[m[32m            this.yAxis2});[m[41m[m
             // [m
[31m-            this.flAOMFreqPlusTextBox.BackColor = System.Drawing.Color.Black;[m
[31m-            this.flAOMFreqPlusTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m
[31m-            this.flAOMFreqPlusTextBox.Location = new System.Drawing.Point(255, 41);[m
[31m-            this.flAOMFreqPlusTextBox.Name = "flAOMFreqPlusTextBox";[m
[31m-            this.flAOMFreqPlusTextBox.ReadOnly = true;[m
[31m-            this.flAOMFreqPlusTextBox.Size = new System.Drawing.Size(126, 20);[m
[31m-            this.flAOMFreqPlusTextBox.TabIndex = 75;[m
[31m-            this.flAOMFreqPlusTextBox.Text = "0";[m
[32m+[m[32m            // diodeCurrentPlot[m[41m[m
             // [m
[31m-            // flAOMFreqCentreTextBox[m
[32m+[m[32m            this.diodeCurrentPlot.AntiAliased = true;[m[41m[m
[32m+[m[32m            this.diodeCurrentPlot.HistoryCapacity = 10000;[m[41m[m
[32m+[m[32m            this.diodeCurrentPlot.LineWidth = 2F;[m[41m[m
[32m+[m[32m            this.diodeCurrentPlot.XAxis = this.xAxis2;[m[41m[m
[32m+[m[32m            this.diodeCurrentPlot.YAxis = this.yAxis2;[m[41m[m
             // [m
[31m-            this.flAOMFreqCentreTextBox.BackColor = System.Drawing.Color.Black;[m
[31m-            this.flAOMFreqCentreTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m
[31m-            this.flAOMFreqCentreTextBox.Location = new System.Drawing.Point(255, 69);[m
[31m-            this.flAOMFreqCentreTextBox.Name = "flAOMFreqCentreTextBox";[m
[31m-            this.flAOMFreqCentreTextBox.ReadOnly = true;[m
[31m-            this.flAOMFreqCentreTextBox.Size = new System.Drawing.Size(126, 20);[m
[31m-            this.flAOMFreqCentreTextBox.TabIndex = 71;[m
[31m-            this.flAOMFreqCentreTextBox.Text = "0";[m
[32m+[m[32m            // xAxis2[m[41m[m
             // [m
[31m-            // label120[m
[32m+[m[32m            this.xAxis2.Mode = NationalInstruments.UI.AxisMode.StripChart;[m[41m[m
[32m+[m[32m            this.xAxis2.Range = new NationalInstruments.UI.Range(0D, 500D);[m[41m[m
             // [m
[31m-            this.label120.Location = new System.Drawing.Point(151, 44);[m
[31m-            this.label120.Name = "label120";[m
[31m-            this.label120.Size = new System.Drawing.Size(98, 23);[m
[31m-            this.label120.TabIndex = 73;[m
[31m-            this.label120.Text = "AOM freq high (Hz)";[m
[32m+[m[32m            // yAxis2[m[41m[m
             // [m
[31m-            // flAOMFreqMinusTextBox[m
[32m+[m[32m            this.yAxis2.Mode = NationalInstruments.UI.AxisMode.Fixed;[m[41m[m
[32m+[m[32m            this.yAxis2.OriginLineVisible = true;[m[41m[m
             // [m
[31m-            this.flAOMFreqMinusTextBox.BackColor = System.Drawing.Color.Black;[m
[31m-            this.flAOMFreqMinusTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m
[31m-            this.flAOMFreqMinusTextBox.Location = new System.Drawing.Point(255, 15);[m
[31m-            this.flAOMFreqMinusTextBox.Name = "flAOMFreqMinusTextBox";[m
[31m-            this.flAOMFreqMinusTextBox.ReadOnly = true;[m
[31m-            this.flAOMFreqMinusTextBox.Size = new System.Drawing.Size(126, 20);[m
[31m-            this.flAOMFreqMinusTextBox.TabIndex = 70;[m
[31m-            this.flAOMFreqMinusTextBox.Text = "0";[m
[32m+[m[32m            // tabPage13[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.tabPage13.BackColor = System.Drawing.SystemColors.Control;[m[41m[m
[32m+[m[32m            this.tabPage13.Controls.Add(this.groupBox41);[m[41m[m
[32m+[m[32m            this.tabPage13.Location = new System.Drawing.Point(4, 22);[m[41m[m
[32m+[m[32m            this.tabPage13.Name = "tabPage13";[m[41m[m
[32m+[m[32m            this.tabPage13.Size = new System.Drawing.Size(697, 575);[m[41m[m
[32m+[m[32m            this.tabPage13.TabIndex = 10;[m[41m[m
[32m+[m[32m            this.tabPage13.Text = "Microwaves";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // groupBox41[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.groupBox41.Controls.Add(this.mixerVoltageMinusButton);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.mixerVoltagePlusButton);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.stepMixerVoltageTextBox);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.label151);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.uWaveDCFMMinusButton);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.uWaveDCFMStepTextBox);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.label152);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.uWaveDCFMPlusButton);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.mixerVoltageTrackBar);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.label153);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.label154);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.uWaveDCFMTrackBar);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.mixerVoltageTextBox);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.label155);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.uWaveDCFMTextBox);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.uWaveUpdateButton);[m[41m[m
[32m+[m[32m            this.groupBox41.Controls.Add(this.label156);[m[41m[m
[32m+[m[32m            this.groupBox41.Location = new System.Drawing.Point(14, 18);[m[41m[m
[32m+[m[32m            this.groupBox41.Name = "groupBox41";[m[41m[m
[32m+[m[32m            this.groupBox41.Size = new System.Drawing.Size(661, 175);[m[41m[m
[32m+[m[32m            this.groupBox41.TabIndex = 70;[m[41m[m
[32m+[m[32m            this.groupBox41.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox41.Text = "Anapico Synth";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // mixerVoltageMinusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.mixerVoltageMinusButton.Location = new System.Drawing.Point(551, 19);[m[41m[m
[32m+[m[32m            this.mixerVoltageMinusButton.Name = "mixerVoltageMinusButton";[m[41m[m
[32m+[m[32m            this.mixerVoltageMinusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.mixerVoltageMinusButton.TabIndex = 60;[m[41m[m
[32m+[m[32m            this.mixerVoltageMinusButton.Text = "-";[m[41m[m
[32m+[m[32m            this.mixerVoltageMinusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.mixerVoltageMinusButton.Click += new System.EventHandler(this.mixerVoltageMinusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // mixerVoltagePlusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.mixerVoltagePlusButton.Location = new System.Drawing.Point(508, 19);[m[41m[m
[32m+[m[32m            this.mixerVoltagePlusButton.Name = "mixerVoltagePlusButton";[m[41m[m
[32m+[m[32m            this.mixerVoltagePlusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.mixerVoltagePlusButton.TabIndex = 59;[m[41m[m
[32m+[m[32m            this.mixerVoltagePlusButton.Text = "+";[m[41m[m
[32m+[m[32m            this.mixerVoltagePlusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.mixerVoltagePlusButton.Click += new System.EventHandler(this.mixerVoltagePlusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // stepMixerVoltageTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.stepMixerVoltageTextBox.Location = new System.Drawing.Point(438, 48);[m[41m[m
[32m+[m[32m            this.stepMixerVoltageTextBox.Name = "stepMixerVoltageTextBox";[m[41m[m
[32m+[m[32m            this.stepMixerVoltageTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.stepMixerVoltageTextBox.TabIndex = 58;[m[41m[m
[32m+[m[32m            this.stepMixerVoltageTextBox.Text = "0.1";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label151[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label151.Location = new System.Drawing.Point(342, 51);[m[41m[m
[32m+[m[32m            this.label151.Name = "label151";[m[41m[m
[32m+[m[32m            this.label151.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label151.TabIndex = 57;[m[41m[m
[32m+[m[32m            this.label151.Text = "Step Mixer (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // uWaveDCFMMinusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.uWaveDCFMMinusButton.Location = new System.Drawing.Point(225, 19);[m[41m[m
[32m+[m[32m            this.uWaveDCFMMinusButton.Name = "uWaveDCFMMinusButton";[m[41m[m
[32m+[m[32m            this.uWaveDCFMMinusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.uWaveDCFMMinusButton.TabIndex = 56;[m[41m[m
[32m+[m[32m            this.uWaveDCFMMinusButton.Text = "-";[m[41m[m
[32m+[m[32m            this.uWaveDCFMMinusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.uWaveDCFMMinusButton.Click += new System.EventHandler(this.uWaveDCFMMinusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // uWaveDCFMStepTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.uWaveDCFMStepTextBox.Location = new System.Drawing.Point(112, 48);[m[41m[m
[32m+[m[32m            this.uWaveDCFMStepTextBox.Name = "uWaveDCFMStepTextBox";[m[41m[m
[32m+[m[32m            this.uWaveDCFMStepTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.uWaveDCFMStepTextBox.TabIndex = 55;[m[41m[m
[32m+[m[32m            this.uWaveDCFMStepTextBox.Text = "0.1";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label152[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label152.Location = new System.Drawing.Point(16, 51);[m[41m[m
[32m+[m[32m            this.label152.Name = "label152";[m[41m[m
[32m+[m[32m            this.label152.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label152.TabIndex = 54;[m[41m[m
[32m+[m[32m            this.label152.Text = "Step DCFM (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // uWaveDCFMPlusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.uWaveDCFMPlusButton.Location = new System.Drawing.Point(182, 19);[m[41m[m
[32m+[m[32m            this.uWaveDCFMPlusButton.Name = "uWaveDCFMPlusButton";[m[41m[m
[32m+[m[32m            this.uWaveDCFMPlusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.uWaveDCFMPlusButton.TabIndex = 53;[m[41m[m
[32m+[m[32m            this.uWaveDCFMPlusButton.Text = "+";[m[41m[m
[32m+[m[32m            this.uWaveDCFMPlusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.uWaveDCFMPlusButton.Click += new System.EventHandler(this.uWaveDCFMPlusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // mixerVoltageTrackBar[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.mixerVoltageTrackBar.Location = new System.Drawing.Point(345, 99);[m[41m[m
[32m+[m[32m            this.mixerVoltageTrackBar.Maximum = 1000;[m[41m[m
[32m+[m[32m            this.mixerVoltageTrackBar.Name = "mixerVoltageTrackBar";[m[41m[m
[32m+[m[32m            this.mixerVoltageTrackBar.Size = new System.Drawing.Size(287, 45);[m[41m[m
[32m+[m[32m            this.mixerVoltageTrackBar.TabIndex = 52;[m[41m[m
[32m+[m[32m            this.mixerVoltageTrackBar.Scroll += new System.EventHandler(this.mixerVoltageTrackBar_Scroll);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label153[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label153.Location = new System.Drawing.Point(342, 78);[m[41m[m
[32m+[m[32m            this.label153.Name = "label153";[m[41m[m
[32m+[m[32m            this.label153.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label153.TabIndex = 51;[m[41m[m
[32m+[m[32m            this.label153.Text = "Mixer Voltage";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label154[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label154.Location = new System.Drawing.Point(16, 78);[m[41m[m
[32m+[m[32m            this.label154.Name = "label154";[m[41m[m
[32m+[m[32m            this.label154.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label154.TabIndex = 50;[m[41m[m
[32m+[m[32m            this.label154.Text = "DCFM Voltage";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // uWaveDCFMTrackBar[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.uWaveDCFMTrackBar.Location = new System.Drawing.Point(6, 99);[m[41m[m
[32m+[m[32m            this.uWaveDCFMTrackBar.Maximum = 250;[m[41m[m
[32m+[m[32m            this.uWaveDCFMTrackBar.Minimum = -250;[m[41m[m
[32m+[m[32m            this.uWaveDCFMTrackBar.Name = "uWaveDCFMTrackBar";[m[41m[m
[32m+[m[32m            this.uWaveDCFMTrackBar.Size = new System.Drawing.Size(287, 45);[m[41m[m
[32m+[m[32m            this.uWaveDCFMTrackBar.TabIndex = 49;[m[41m[m
[32m+[m[32m            this.uWaveDCFMTrackBar.Scroll += new System.EventHandler(this.uWaveDCFMTrackBar_Scroll);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // mixerVoltageTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.mixerVoltageTextBox.Location = new System.Drawing.Point(438, 21);[m[41m[m
[32m+[m[32m            this.mixerVoltageTextBox.Name = "mixerVoltageTextBox";[m[41m[m
[32m+[m[32m            this.mixerVoltageTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.mixerVoltageTextBox.TabIndex = 47;[m[41m[m
[32m+[m[32m            this.mixerVoltageTextBox.Text = "0";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label155[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label155.Location = new System.Drawing.Point(342, 24);[m[41m[m
[32m+[m[32m            this.label155.Name = "label155";[m[41m[m
[32m+[m[32m            this.label155.Size = new System.Drawing.Size(101, 23);[m[41m[m
[32m+[m[32m            this.label155.TabIndex = 46;[m[41m[m
[32m+[m[32m            this.label155.Text = "Mixer Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // uWaveDCFMTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.uWaveDCFMTextBox.Location = new System.Drawing.Point(112, 21);[m[41m[m
[32m+[m[32m            this.uWaveDCFMTextBox.Name = "uWaveDCFMTextBox";[m[41m[m
[32m+[m[32m            this.uWaveDCFMTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.uWaveDCFMTextBox.TabIndex = 45;[m[41m[m
[32m+[m[32m            this.uWaveDCFMTextBox.Text = "0";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // uWaveUpdateButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.uWaveUpdateButton.Location = new System.Drawing.Point(283, 146);[m[41m[m
[32m+[m[32m            this.uWaveUpdateButton.Name = "uWaveUpdateButton";[m[41m[m
[32m+[m[32m            this.uWaveUpdateButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.uWaveUpdateButton.TabIndex = 40;[m[41m[m
[32m+[m[32m            this.uWaveUpdateButton.Text = "Update";[m[41m[m
[32m+[m[32m            this.uWaveUpdateButton.Click += new System.EventHandler(this.uWaveUpdateButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label156[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label156.Location = new System.Drawing.Point(16, 24);[m[41m[m
[32m+[m[32m            this.label156.Name = "label156";[m[41m[m
[32m+[m[32m            this.label156.Size = new System.Drawing.Size(103, 23);[m[41m[m
[32m+[m[32m            this.label156.TabIndex = 36;[m[41m[m
[32m+[m[32m            this.label156.Text = "DCFM Voltage (V)";[m[41m[m
             // [m
[31m-            // label121[m
[32m+[m[32m            // tabPage12[m[41m[m
             // [m
[31m-            this.label121.Location = new System.Drawing.Point(151, 72);[m
[31m-            this.label121.Name = "label121";[m
[31m-            this.label121.Size = new System.Drawing.Size(96, 23);[m
[31m-            this.label121.TabIndex = 67;[m
[31m-            this.label121.Text = "Centre (Hz)";[m
[32m+[m[32m            this.tabPage12.BackColor = System.Drawing.SystemColors.Control;[m[41m[m
[32m+[m[32m            this.tabPage12.Controls.Add(this.groupBox40);[m[41m[m
[32m+[m[32m            this.tabPage12.Controls.Add(this.groupBox39);[m[41m[m
[32m+[m[32m            this.tabPage12.Controls.Add(this.groupBox161MHzVCO);[m[41m[m
[32m+[m[32m            this.tabPage12.Location = new System.Drawing.Point(4, 22);[m[41m[m
[32m+[m[32m            this.tabPage12.Name = "tabPage12";[m[41m[m
[32m+[m[32m            this.tabPage12.Size = new System.Drawing.Size(697, 575);[m[41m[m
[32m+[m[32m            this.tabPage12.TabIndex = 10;[m[41m[m
[32m+[m[32m            this.tabPage12.Text = "RF VCOs";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // groupBox40[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.groupBox40.Controls.Add(this.VCO155FreqStepMinusButton);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.VCO155FreqStepPlusButton);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.VCO155AmpStepMinusButton);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.VCO155AmpStepPlusButton);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.VCO155FreqStepTextBox);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.label150);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.VCO155AmpStepTextBox);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.label149);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.label144);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.label143);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.VCO155FreqTrackBar);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.VCO155AmpTrackBar);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.VCO155FreqVoltageTextBox);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.label135);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.VCO155AmpVoltageTextBox);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.VCO155UpdateButton);[m[41m[m
[32m+[m[32m            this.groupBox40.Controls.Add(this.label136);[m[41m[m
[32m+[m[32m            this.groupBox40.Location = new System.Drawing.Point(17, 381);[m[41m[m
[32m+[m[32m            this.groupBox40.Name = "groupBox40";[m[41m[m
[32m+[m[32m            this.groupBox40.Size = new System.Drawing.Size(661, 184);[m[41m[m
[32m+[m[32m            this.groupBox40.TabIndex = 71;[m[41m[m
[32m+[m[32m            this.groupBox40.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox40.Text = "155 MHz VCO";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO155FreqStepMinusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO155FreqStepMinusButton.Location = new System.Drawing.Point(551, 20);[m[41m[m
[32m+[m[32m            this.VCO155FreqStepMinusButton.Name = "VCO155FreqStepMinusButton";[m[41m[m
[32m+[m[32m            this.VCO155FreqStepMinusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO155FreqStepMinusButton.TabIndex = 71;[m[41m[m
[32m+[m[32m            this.VCO155FreqStepMinusButton.Text = "-";[m[41m[m
[32m+[m[32m            this.VCO155FreqStepMinusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO155FreqStepMinusButton.Click += new System.EventHandler(this.VCO155FreqStepMinusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO155FreqStepPlusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO155FreqStepPlusButton.Location = new System.Drawing.Point(508, 20);[m[41m[m
[32m+[m[32m            this.VCO155FreqStepPlusButton.Name = "VCO155FreqStepPlusButton";[m[41m[m
[32m+[m[32m            this.VCO155FreqStepPlusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO155FreqStepPlusButton.TabIndex = 70;[m[41m[m
[32m+[m[32m            this.VCO155FreqStepPlusButton.Text = "+";[m[41m[m
[32m+[m[32m            this.VCO155FreqStepPlusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO155FreqStepPlusButton.Click += new System.EventHandler(this.VCO155FreqStepPlusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO155AmpStepMinusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO155AmpStepMinusButton.Location = new System.Drawing.Point(225, 20);[m[41m[m
[32m+[m[32m            this.VCO155AmpStepMinusButton.Name = "VCO155AmpStepMinusButton";[m[41m[m
[32m+[m[32m            this.VCO155AmpStepMinusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO155AmpStepMinusButton.TabIndex = 67;[m[41m[m
[32m+[m[32m            this.VCO155AmpStepMinusButton.Text = "-";[m[41m[m
[32m+[m[32m            this.VCO155AmpStepMinusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO155AmpStepMinusButton.Click += new System.EventHandler(this.VCO155AmpStepMinusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO155AmpStepPlusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO155AmpStepPlusButton.Location = new System.Drawing.Point(182, 19);[m[41m[m
[32m+[m[32m            this.VCO155AmpStepPlusButton.Name = "VCO155AmpStepPlusButton";[m[41m[m
[32m+[m[32m            this.VCO155AmpStepPlusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO155AmpStepPlusButton.TabIndex = 67;[m[41m[m
[32m+[m[32m            this.VCO155AmpStepPlusButton.Text = "+";[m[41m[m
[32m+[m[32m            this.VCO155AmpStepPlusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO155AmpStepPlusButton.Click += new System.EventHandler(this.VCO155AmpStepPlusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO155FreqStepTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO155FreqStepTextBox.Location = new System.Drawing.Point(438, 50);[m[41m[m
[32m+[m[32m            this.VCO155FreqStepTextBox.Name = "VCO155FreqStepTextBox";[m[41m[m
[32m+[m[32m            this.VCO155FreqStepTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO155FreqStepTextBox.TabIndex = 69;[m[41m[m
[32m+[m[32m            this.VCO155FreqStepTextBox.Text = "0.1";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label150[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label150.Location = new System.Drawing.Point(342, 53);[m[41m[m
[32m+[m[32m            this.label150.Name = "label150";[m[41m[m
[32m+[m[32m            this.label150.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label150.TabIndex = 68;[m[41m[m
[32m+[m[32m            this.label150.Text = "Step Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO155AmpStepTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO155AmpStepTextBox.Location = new System.Drawing.Point(112, 50);[m[41m[m
[32m+[m[32m            this.VCO155AmpStepTextBox.Name = "VCO155AmpStepTextBox";[m[41m[m
[32m+[m[32m            this.VCO155AmpStepTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO155AmpStepTextBox.TabIndex = 67;[m[41m[m
[32m+[m[32m            this.VCO155AmpStepTextBox.Text = "0.1";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label149[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label149.Location = new System.Drawing.Point(16, 53);[m[41m[m
[32m+[m[32m            this.label149.Name = "label149";[m[41m[m
[32m+[m[32m            this.label149.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label149.TabIndex = 67;[m[41m[m
[32m+[m[32m            this.label149.Text = "Step Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label144[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label144.Location = new System.Drawing.Point(342, 85);[m[41m[m
[32m+[m[32m            this.label144.Name = "label144";[m[41m[m
[32m+[m[32m            this.label144.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label144.TabIndex = 56;[m[41m[m
[32m+[m[32m            this.label144.Text = "VCO Frequency";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label143[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label143.Location = new System.Drawing.Point(16, 85);[m[41m[m
[32m+[m[32m            this.label143.Name = "label143";[m[41m[m
[32m+[m[32m            this.label143.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label143.TabIndex = 56;[m[41m[m
[32m+[m[32m            this.label143.Text = "VCO Amplitude";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO155FreqTrackBar[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO155FreqTrackBar.Location = new System.Drawing.Point(345, 108);[m[41m[m
[32m+[m[32m            this.VCO155FreqTrackBar.Maximum = 1000;[m[41m[m
[32m+[m[32m            this.VCO155FreqTrackBar.Name = "VCO155FreqTrackBar";[m[41m[m
[32m+[m[32m            this.VCO155FreqTrackBar.Size = new System.Drawing.Size(287, 45);[m[41m[m
[32m+[m[32m            this.VCO155FreqTrackBar.TabIndex = 50;[m[41m[m
[32m+[m[32m            this.VCO155FreqTrackBar.Scroll += new System.EventHandler(this.VCO155FreqTrackBar_Scroll);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO155AmpTrackBar[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO155AmpTrackBar.Location = new System.Drawing.Point(6, 108);[m[41m[m
[32m+[m[32m            this.VCO155AmpTrackBar.Maximum = 1000;[m[41m[m
[32m+[m[32m            this.VCO155AmpTrackBar.Name = "VCO155AmpTrackBar";[m[41m[m
[32m+[m[32m            this.VCO155AmpTrackBar.Size = new System.Drawing.Size(287, 45);[m[41m[m
[32m+[m[32m            this.VCO155AmpTrackBar.TabIndex = 49;[m[41m[m
[32m+[m[32m            this.VCO155AmpTrackBar.Scroll += new System.EventHandler(this.VCO155AmpTrackBar_Scroll);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO155FreqVoltageTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO155FreqVoltageTextBox.Location = new System.Drawing.Point(438, 21);[m[41m[m
[32m+[m[32m            this.VCO155FreqVoltageTextBox.Name = "VCO155FreqVoltageTextBox";[m[41m[m
[32m+[m[32m            this.VCO155FreqVoltageTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO155FreqVoltageTextBox.TabIndex = 47;[m[41m[m
[32m+[m[32m            this.VCO155FreqVoltageTextBox.Text = "0";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label135[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label135.Location = new System.Drawing.Point(342, 23);[m[41m[m
[32m+[m[32m            this.label135.Name = "label135";[m[41m[m
[32m+[m[32m            this.label135.Size = new System.Drawing.Size(99, 23);[m[41m[m
[32m+[m[32m            this.label135.TabIndex = 46;[m[41m[m
[32m+[m[32m            this.label135.Text = "Freq Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO155AmpVoltageTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO155AmpVoltageTextBox.Location = new System.Drawing.Point(112, 22);[m[41m[m
[32m+[m[32m            this.VCO155AmpVoltageTextBox.Name = "VCO155AmpVoltageTextBox";[m[41m[m
[32m+[m[32m            this.VCO155AmpVoltageTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO155AmpVoltageTextBox.TabIndex = 45;[m[41m[m
[32m+[m[32m            this.VCO155AmpVoltageTextBox.Text = "0";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO155UpdateButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO155UpdateButton.Location = new System.Drawing.Point(283, 152);[m[41m[m
[32m+[m[32m            this.VCO155UpdateButton.Name = "VCO155UpdateButton";[m[41m[m
[32m+[m[32m            this.VCO155UpdateButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.VCO155UpdateButton.TabIndex = 40;[m[41m[m
[32m+[m[32m            this.VCO155UpdateButton.Text = "Update";[m[41m[m
[32m+[m[32m            this.VCO155UpdateButton.Click += new System.EventHandler(this.VCO155UpdateButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label136[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label136.Location = new System.Drawing.Point(16, 24);[m[41m[m
[32m+[m[32m            this.label136.Name = "label136";[m[41m[m
[32m+[m[32m            this.label136.Size = new System.Drawing.Size(98, 23);[m[41m[m
[32m+[m[32m            this.label136.TabIndex = 36;[m[41m[m
[32m+[m[32m            this.label136.Text = "Amp Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // groupBox39[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.groupBox39.Controls.Add(this.VCO30FreqStepMinusButton);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.VCO30FreqStepPlusButton);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.VCO30FreqStepTextBox);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.label148);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.VCO30AmpStepMinusButton);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.VCO30AmpStepPlusButton);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.VCO30AmpStepTextBox);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.label147);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.VCO30FreqTrackBar);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.label142);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.label141);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.VCO30AmpTrackBar);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.VCO30FreqVoltageTextBox);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.label83);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.VCO30AmpVoltageTextBox);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.VCO30UpdateButton);[m[41m[m
[32m+[m[32m            this.groupBox39.Controls.Add(this.label134);[m[41m[m
[32m+[m[32m            this.groupBox39.Location = new System.Drawing.Point(17, 193);[m[41m[m
[32m+[m[32m            this.groupBox39.Name = "groupBox39";[m[41m[m
[32m+[m[32m            this.groupBox39.Size = new System.Drawing.Size(661, 182);[m[41m[m
[32m+[m[32m            this.groupBox39.TabIndex = 70;[m[41m[m
[32m+[m[32m            this.groupBox39.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox39.Text = "30 MHz VCO";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO30FreqStepMinusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO30FreqStepMinusButton.Location = new System.Drawing.Point(551, 22);[m[41m[m
[32m+[m[32m            this.VCO30FreqStepMinusButton.Name = "VCO30FreqStepMinusButton";[m[41m[m
[32m+[m[32m            this.VCO30FreqStepMinusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO30FreqStepMinusButton.TabIndex = 66;[m[41m[m
[32m+[m[32m            this.VCO30FreqStepMinusButton.Text = "-";[m[41m[m
[32m+[m[32m            this.VCO30FreqStepMinusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO30FreqStepMinusButton.Click += new System.EventHandler(this.VCO30FreqStepMinusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO30FreqStepPlusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO30FreqStepPlusButton.Location = new System.Drawing.Point(508, 22);[m[41m[m
[32m+[m[32m            this.VCO30FreqStepPlusButton.Name = "VCO30FreqStepPlusButton";[m[41m[m
[32m+[m[32m            this.VCO30FreqStepPlusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO30FreqStepPlusButton.TabIndex = 65;[m[41m[m
[32m+[m[32m            this.VCO30FreqStepPlusButton.Text = "+";[m[41m[m
[32m+[m[32m            this.VCO30FreqStepPlusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO30FreqStepPlusButton.Click += new System.EventHandler(this.VCO30FreqStepPlusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO30FreqStepTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO30FreqStepTextBox.Location = new System.Drawing.Point(438, 50);[m[41m[m
[32m+[m[32m            this.VCO30FreqStepTextBox.Name = "VCO30FreqStepTextBox";[m[41m[m
[32m+[m[32m            this.VCO30FreqStepTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO30FreqStepTextBox.TabIndex = 64;[m[41m[m
[32m+[m[32m            this.VCO30FreqStepTextBox.Text = "0.1";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label148[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label148.Location = new System.Drawing.Point(342, 53);[m[41m[m
[32m+[m[32m            this.label148.Name = "label148";[m[41m[m
[32m+[m[32m            this.label148.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label148.TabIndex = 63;[m[41m[m
[32m+[m[32m            this.label148.Text = "Step Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO30AmpStepMinusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO30AmpStepMinusButton.Location = new System.Drawing.Point(225, 22);[m[41m[m
[32m+[m[32m            this.VCO30AmpStepMinusButton.Name = "VCO30AmpStepMinusButton";[m[41m[m
[32m+[m[32m            this.VCO30AmpStepMinusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO30AmpStepMinusButton.TabIndex = 61;[m[41m[m
[32m+[m[32m            this.VCO30AmpStepMinusButton.Text = "-";[m[41m[m
[32m+[m[32m            this.VCO30AmpStepMinusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO30AmpStepMinusButton.Click += new System.EventHandler(this.VCO30AmpStepMinusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO30AmpStepPlusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO30AmpStepPlusButton.Location = new System.Drawing.Point(182, 22);[m[41m[m
[32m+[m[32m            this.VCO30AmpStepPlusButton.Name = "VCO30AmpStepPlusButton";[m[41m[m
[32m+[m[32m            this.VCO30AmpStepPlusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO30AmpStepPlusButton.TabIndex = 62;[m[41m[m
[32m+[m[32m            this.VCO30AmpStepPlusButton.Text = "+";[m[41m[m
[32m+[m[32m            this.VCO30AmpStepPlusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO30AmpStepPlusButton.Click += new System.EventHandler(this.VCO30AmpStepPlusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO30AmpStepTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO30AmpStepTextBox.Location = new System.Drawing.Point(112, 50);[m[41m[m
[32m+[m[32m            this.VCO30AmpStepTextBox.Name = "VCO30AmpStepTextBox";[m[41m[m
[32m+[m[32m            this.VCO30AmpStepTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO30AmpStepTextBox.TabIndex = 61;[m[41m[m
[32m+[m[32m            this.VCO30AmpStepTextBox.Text = "0.1";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label147[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label147.Location = new System.Drawing.Point(16, 53);[m[41m[m
[32m+[m[32m            this.label147.Name = "label147";[m[41m[m
[32m+[m[32m            this.label147.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label147.TabIndex = 61;[m[41m[m
[32m+[m[32m            this.label147.Text = "Step Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO30FreqTrackBar[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO30FreqTrackBar.Location = new System.Drawing.Point(345, 105);[m[41m[m
[32m+[m[32m            this.VCO30FreqTrackBar.Maximum = 1000;[m[41m[m
[32m+[m[32m            this.VCO30FreqTrackBar.Name = "VCO30FreqTrackBar";[m[41m[m
[32m+[m[32m            this.VCO30FreqTrackBar.Size = new System.Drawing.Size(287, 45);[m[41m[m
[32m+[m[32m            this.VCO30FreqTrackBar.TabIndex = 55;[m[41m[m
[32m+[m[32m            this.VCO30FreqTrackBar.Scroll += new System.EventHandler(this.VCO30FreqTrackBar_Scroll);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label142[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label142.Location = new System.Drawing.Point(342, 79);[m[41m[m
[32m+[m[32m            this.label142.Name = "label142";[m[41m[m
[32m+[m[32m            this.label142.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label142.TabIndex = 54;[m[41m[m
[32m+[m[32m            this.label142.Text = "VCO Frequency";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label141[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label141.Location = new System.Drawing.Point(16, 79);[m[41m[m
[32m+[m[32m            this.label141.Name = "label141";[m[41m[m
[32m+[m[32m            this.label141.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label141.TabIndex = 53;[m[41m[m
[32m+[m[32m            this.label141.Text = "VCO Amplitude";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO30AmpTrackBar[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO30AmpTrackBar.Location = new System.Drawing.Point(6, 105);[m[41m[m
[32m+[m[32m            this.VCO30AmpTrackBar.Maximum = 1000;[m[41m[m
[32m+[m[32m            this.VCO30AmpTrackBar.Name = "VCO30AmpTrackBar";[m[41m[m
[32m+[m[32m            this.VCO30AmpTrackBar.Size = new System.Drawing.Size(287, 45);[m[41m[m
[32m+[m[32m            this.VCO30AmpTrackBar.TabIndex = 49;[m[41m[m
[32m+[m[32m            this.VCO30AmpTrackBar.Scroll += new System.EventHandler(this.VCO30AmpTrackBar_Scroll);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO30FreqVoltageTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO30FreqVoltageTextBox.Location = new System.Drawing.Point(438, 24);[m[41m[m
[32m+[m[32m            this.VCO30FreqVoltageTextBox.Name = "VCO30FreqVoltageTextBox";[m[41m[m
[32m+[m[32m            this.VCO30FreqVoltageTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO30FreqVoltageTextBox.TabIndex = 47;[m[41m[m
[32m+[m[32m            this.VCO30FreqVoltageTextBox.Text = "0";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label83[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label83.Location = new System.Drawing.Point(342, 24);[m[41m[m
[32m+[m[32m            this.label83.Name = "label83";[m[41m[m
[32m+[m[32m            this.label83.Size = new System.Drawing.Size(99, 23);[m[41m[m
[32m+[m[32m            this.label83.TabIndex = 46;[m[41m[m
[32m+[m[32m            this.label83.Text = "Freq Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO30AmpVoltageTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO30AmpVoltageTextBox.Location = new System.Drawing.Point(112, 24);[m[41m[m
[32m+[m[32m            this.VCO30AmpVoltageTextBox.Name = "VCO30AmpVoltageTextBox";[m[41m[m
[32m+[m[32m            this.VCO30AmpVoltageTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO30AmpVoltageTextBox.TabIndex = 45;[m[41m[m
[32m+[m[32m            this.VCO30AmpVoltageTextBox.Text = "0";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO30UpdateButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO30UpdateButton.Location = new System.Drawing.Point(283, 149);[m[41m[m
[32m+[m[32m            this.VCO30UpdateButton.Name = "VCO30UpdateButton";[m[41m[m
[32m+[m[32m            this.VCO30UpdateButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.VCO30UpdateButton.TabIndex = 40;[m[41m[m
[32m+[m[32m            this.VCO30UpdateButton.Text = "Update";[m[41m[m
[32m+[m[32m            this.VCO30UpdateButton.Click += new System.EventHandler(this.VCO30UpdateButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label134[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label134.Location = new System.Drawing.Point(16, 24);[m[41m[m
[32m+[m[32m            this.label134.Name = "label134";[m[41m[m
[32m+[m[32m            this.label134.Size = new System.Drawing.Size(98, 23);[m[41m[m
[32m+[m[32m            this.label134.TabIndex = 36;[m[41m[m
[32m+[m[32m            this.label134.Text = "Amp Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // groupBox161MHzVCO[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.VCO161FreqStepMinusButton);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.VCO161FreqStepPlusButton);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.VCO161FreqStepTextBox);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.label146);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.VCO161AmpStepMinusButton);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.VCO161AmpStepTextBox);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.label145);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.VCO161AmpStepPlusButton);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.VCO161FreqTrackBar);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.label140);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.label139);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.VCO161AmpTrackBar);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.VCO161FreqTextBox);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.label137);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.VCO161AmpVoltageTextBox);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.VCO161UpdateButton);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Controls.Add(this.label138);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Location = new System.Drawing.Point(17, 12);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Name = "groupBox161MHzVCO";[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Size = new System.Drawing.Size(661, 175);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.TabIndex = 69;[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Text = "161 MHz VCO";[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.Enter += new System.EventHandler(this.groupBox39_Enter);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO161FreqStepMinusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO161FreqStepMinusButton.Location = new System.Drawing.Point(551, 19);[m[41m[m
[32m+[m[32m            this.VCO161FreqStepMinusButton.Name = "VCO161FreqStepMinusButton";[m[41m[m
[32m+[m[32m            this.VCO161FreqStepMinusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO161FreqStepMinusButton.TabIndex = 60;[m[41m[m
[32m+[m[32m            this.VCO161FreqStepMinusButton.Text = "-";[m[41m[m
[32m+[m[32m            this.VCO161FreqStepMinusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO161FreqStepMinusButton.Click += new System.EventHandler(this.VCO161FreqStepMinusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO161FreqStepPlusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO161FreqStepPlusButton.Location = new System.Drawing.Point(508, 19);[m[41m[m
[32m+[m[32m            this.VCO161FreqStepPlusButton.Name = "VCO161FreqStepPlusButton";[m[41m[m
[32m+[m[32m            this.VCO161FreqStepPlusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO161FreqStepPlusButton.TabIndex = 59;[m[41m[m
[32m+[m[32m            this.VCO161FreqStepPlusButton.Text = "+";[m[41m[m
[32m+[m[32m            this.VCO161FreqStepPlusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO161FreqStepPlusButton.Click += new System.EventHandler(this.VCO161FreqStepPlusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO161FreqStepTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO161FreqStepTextBox.Location = new System.Drawing.Point(438, 48);[m[41m[m
[32m+[m[32m            this.VCO161FreqStepTextBox.Name = "VCO161FreqStepTextBox";[m[41m[m
[32m+[m[32m            this.VCO161FreqStepTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO161FreqStepTextBox.TabIndex = 58;[m[41m[m
[32m+[m[32m            this.VCO161FreqStepTextBox.Text = "0.1";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label146[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label146.Location = new System.Drawing.Point(342, 51);[m[41m[m
[32m+[m[32m            this.label146.Name = "label146";[m[41m[m
[32m+[m[32m            this.label146.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label146.TabIndex = 57;[m[41m[m
[32m+[m[32m            this.label146.Text = "Step Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO161AmpStepMinusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO161AmpStepMinusButton.Location = new System.Drawing.Point(225, 19);[m[41m[m
[32m+[m[32m            this.VCO161AmpStepMinusButton.Name = "VCO161AmpStepMinusButton";[m[41m[m
[32m+[m[32m            this.VCO161AmpStepMinusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO161AmpStepMinusButton.TabIndex = 56;[m[41m[m
[32m+[m[32m            this.VCO161AmpStepMinusButton.Text = "-";[m[41m[m
[32m+[m[32m            this.VCO161AmpStepMinusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO161AmpStepMinusButton.Click += new System.EventHandler(this.VCO161StepMinusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO161AmpStepTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO161AmpStepTextBox.Location = new System.Drawing.Point(112, 48);[m[41m[m
[32m+[m[32m            this.VCO161AmpStepTextBox.Name = "VCO161AmpStepTextBox";[m[41m[m
[32m+[m[32m            this.VCO161AmpStepTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO161AmpStepTextBox.TabIndex = 55;[m[41m[m
[32m+[m[32m            this.VCO161AmpStepTextBox.Text = "0.1";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label145[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label145.Location = new System.Drawing.Point(16, 51);[m[41m[m
[32m+[m[32m            this.label145.Name = "label145";[m[41m[m
[32m+[m[32m            this.label145.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label145.TabIndex = 54;[m[41m[m
[32m+[m[32m            this.label145.Text = "Step Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO161AmpStepPlusButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO161AmpStepPlusButton.Location = new System.Drawing.Point(182, 19);[m[41m[m
[32m+[m[32m            this.VCO161AmpStepPlusButton.Name = "VCO161AmpStepPlusButton";[m[41m[m
[32m+[m[32m            this.VCO161AmpStepPlusButton.Size = new System.Drawing.Size(37, 23);[m[41m[m
[32m+[m[32m            this.VCO161AmpStepPlusButton.TabIndex = 53;[m[41m[m
[32m+[m[32m            this.VCO161AmpStepPlusButton.Text = "+";[m[41m[m
[32m+[m[32m            this.VCO161AmpStepPlusButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.VCO161AmpStepPlusButton.Click += new System.EventHandler(this.VCO161StepPlusButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO161FreqTrackBar[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO161FreqTrackBar.Location = new System.Drawing.Point(345, 99);[m[41m[m
[32m+[m[32m            this.VCO161FreqTrackBar.Maximum = 1000;[m[41m[m
[32m+[m[32m            this.VCO161FreqTrackBar.Name = "VCO161FreqTrackBar";[m[41m[m
[32m+[m[32m            this.VCO161FreqTrackBar.Size = new System.Drawing.Size(287, 45);[m[41m[m
[32m+[m[32m            this.VCO161FreqTrackBar.TabIndex = 52;[m[41m[m
[32m+[m[32m            this.VCO161FreqTrackBar.Scroll += new System.EventHandler(this.VCO161FreqTrackBar_Scroll);[m[41m[m
             // [m
[31m-            // flAOMFreqUpdateButton[m
[32m+[m[32m            // label140[m[41m[m
             // [m
[31m-            this.flAOMFreqUpdateButton.Location = new System.Drawing.Point(305, 121);[m
[31m-            this.flAOMFreqUpdateButton.Name = "flAOMFreqUpdateButton";[m
[31m-            this.flAOMFreqUpdateButton.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.flAOMFreqUpdateButton.TabIndex = 69;[m
[31m-            this.flAOMFreqUpdateButton.Text = "Update";[m
[31m-            this.flAOMFreqUpdateButton.Click += new System.EventHandler(this.flAOMFreqUpdateButton_Click);[m
[32m+[m[32m            this.label140.Location = new System.Drawing.Point(342, 78);[m[41m[m
[32m+[m[32m            this.label140.Name = "label140";[m[41m[m
[32m+[m[32m            this.label140.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label140.TabIndex = 51;[m[41m[m
[32m+[m[32m            this.label140.Text = "VCO Frequency";[m[41m[m
             // [m
[31m-            // label122[m
[32m+[m[32m            // label139[m[41m[m
             // [m
[31m-            this.label122.Location = new System.Drawing.Point(150, 18);[m
[31m-            this.label122.Name = "label122";[m
[31m-            this.label122.Size = new System.Drawing.Size(99, 23);[m
[31m-            this.label122.TabIndex = 68;[m
[31m-            this.label122.Text = "AOM freq low (Hz)";[m
[32m+[m[32m            this.label139.Location = new System.Drawing.Point(16, 78);[m[41m[m
[32m+[m[32m            this.label139.Name = "label139";[m[41m[m
[32m+[m[32m            this.label139.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label139.TabIndex = 50;[m[41m[m
[32m+[m[32m            this.label139.Text = "VCO Amplitude";[m[41m[m
             // [m
[31m-            // panel8[m
[32m+[m[32m            // VCO161AmpTrackBar[m[41m[m
             // [m
[31m-            this.panel8.Controls.Add(this.flAOMStepZeroButton);[m
[31m-            this.panel8.Controls.Add(this.flAOMStepPlusButton);[m
[31m-            this.panel8.Controls.Add(this.flAOMStepMinusButton);[m
[31m-            this.panel8.Location = new System.Drawing.Point(9, 67);[m
[31m-            this.panel8.Name = "panel8";[m
[31m-            this.panel8.Size = new System.Drawing.Size(111, 32);[m
[31m-            this.panel8.TabIndex = 48;[m
[32m+[m[32m            this.VCO161AmpTrackBar.Location = new System.Drawing.Point(6, 99);[m[41m[m
[32m+[m[32m            this.VCO161AmpTrackBar.Maximum = 1000;[m[41m[m
[32m+[m[32m            this.VCO161AmpTrackBar.Name = "VCO161AmpTrackBar";[m[41m[m
[32m+[m[32m            this.VCO161AmpTrackBar.Size = new System.Drawing.Size(287, 45);[m[41m[m
[32m+[m[32m            this.VCO161AmpTrackBar.TabIndex = 49;[m[41m[m
[32m+[m[32m            this.VCO161AmpTrackBar.Scroll += new System.EventHandler(this.VCO161AmpTrackBar_Scroll);[m[41m[m
             // [m
[31m-            // flAOMStepZeroButton[m
[32m+[m[32m            // VCO161FreqTextBox[m[41m[m
             // [m
[31m-            this.flAOMStepZeroButton.AutoSize = true;[m
[31m-            this.flAOMStepZeroButton.Checked = true;[m
[31m-            this.flAOMStepZeroButton.Location = new System.Drawing.Point(77, 7);[m
[31m-            this.flAOMStepZeroButton.Name = "flAOMStepZeroButton";[m
[31m-            this.flAOMStepZeroButton.Size = new System.Drawing.Size(31, 17);[m
[31m-            this.flAOMStepZeroButton.TabIndex = 32;[m
[31m-            this.flAOMStepZeroButton.TabStop = true;[m
[31m-            this.flAOMStepZeroButton.Text = "0";[m
[31m-            this.flAOMStepZeroButton.UseVisualStyleBackColor = true;[m
[32m+[m[32m            this.VCO161FreqTextBox.Location = new System.Drawing.Point(438, 21);[m[41m[m
[32m+[m[32m            this.VCO161FreqTextBox.Name = "VCO161FreqTextBox";[m[41m[m
[32m+[m[32m            this.VCO161FreqTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO161FreqTextBox.TabIndex = 47;[m[41m[m
[32m+[m[32m            this.VCO161FreqTextBox.Text = "0";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label137[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label137.Location = new System.Drawing.Point(342, 24);[m[41m[m
[32m+[m[32m            this.label137.Name = "label137";[m[41m[m
[32m+[m[32m            this.label137.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label137.TabIndex = 46;[m[41m[m
[32m+[m[32m            this.label137.Text = "Freq Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO161AmpVoltageTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO161AmpVoltageTextBox.Location = new System.Drawing.Point(112, 21);[m[41m[m
[32m+[m[32m            this.VCO161AmpVoltageTextBox.Name = "VCO161AmpVoltageTextBox";[m[41m[m
[32m+[m[32m            this.VCO161AmpVoltageTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.VCO161AmpVoltageTextBox.TabIndex = 45;[m[41m[m
[32m+[m[32m            this.VCO161AmpVoltageTextBox.Text = "0";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // VCO161UpdateButton[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.VCO161UpdateButton.Location = new System.Drawing.Point(283, 146);[m[41m[m
[32m+[m[32m            this.VCO161UpdateButton.Name = "VCO161UpdateButton";[m[41m[m
[32m+[m[32m            this.VCO161UpdateButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.VCO161UpdateButton.TabIndex = 40;[m[41m[m
[32m+[m[32m            this.VCO161UpdateButton.Text = "Update";[m[41m[m
[32m+[m[32m            this.VCO161UpdateButton.Click += new System.EventHandler(this.VCO161UpdateButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label138[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label138.Location = new System.Drawing.Point(16, 24);[m[41m[m
[32m+[m[32m            this.label138.Name = "label138";[m[41m[m
[32m+[m[32m            this.label138.Size = new System.Drawing.Size(90, 23);[m[41m[m
[32m+[m[32m            this.label138.TabIndex = 36;[m[41m[m
[32m+[m[32m            this.label138.Text = "Amp Voltage (V)";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // tabPage6[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.tabPage6.BackColor = System.Drawing.Color.Transparent;[m[41m[m
[32m+[m[32m            this.tabPage6.Controls.Add(this.groupBox34);[m[41m[m
[32m+[m[32m            this.tabPage6.Controls.Add(this.groupBox32);[m[41m[m
[32m+[m[32m            this.tabPage6.Location = new System.Drawing.Point(4, 22);[m[41m[m
[32m+[m[32m            this.tabPage6.Name = "tabPage6";[m[41m[m
[32m+[m[32m            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);[m[41m[m
[32m+[m[32m            this.tabPage6.Size = new System.Drawing.Size(697, 575);[m[41m[m
[32m+[m[32m            this.tabPage6.TabIndex = 5;[m[41m[m
[32m+[m[32m            this.tabPage6.Text = "Polarizer";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // groupBox34[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.groupBox34.Controls.Add(this.label108);[m[41m[m
[32m+[m[32m            this.groupBox34.Controls.Add(this.label109);[m[41m[m
[32m+[m[32m            this.groupBox34.Controls.Add(this.pumpPolMesAngle);[m[41m[m
[32m+[m[32m            this.groupBox34.Controls.Add(this.updatePumpPolMesAngle);[m[41m[m
[32m+[m[32m            this.groupBox34.Controls.Add(this.zeroPumpPol);[m[41m[m
[32m+[m[32m            this.groupBox34.Controls.Add(this.label110);[m[41m[m
[32m+[m[32m            this.groupBox34.Controls.Add(this.groupBox35);[m[41m[m
[32m+[m[32m            this.groupBox34.Location = new System.Drawing.Point(349, 6);[m[41m[m
[32m+[m[32m            this.groupBox34.Name = "groupBox34";[m[41m[m
[32m+[m[32m            this.groupBox34.Size = new System.Drawing.Size(345, 229);[m[41m[m
[32m+[m[32m            this.groupBox34.TabIndex = 13;[m[41m[m
[32m+[m[32m            this.groupBox34.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox34.Text = "Pump Polariser";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label108[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label108.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label108.Location = new System.Drawing.Point(271, 30);[m[41m[m
[32m+[m[32m            this.label108.Name = "label108";[m[41m[m
[32m+[m[32m            this.label108.Size = new System.Drawing.Size(0, 13);[m[41m[m
[32m+[m[32m            this.label108.TabIndex = 48;[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label109[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label109.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label109.Location = new System.Drawing.Point(15, 35);[m[41m[m
[32m+[m[32m            this.label109.Name = "label109";[m[41m[m
[32m+[m[32m            this.label109.Size = new System.Drawing.Size(74, 13);[m[41m[m
[32m+[m[32m            this.label109.TabIndex = 47;[m[41m[m
[32m+[m[32m            this.label109.Text = "Position Mode";[m[41m[m
             // [m
[31m-            // flAOMStepPlusButton[m
[32m+[m[32m            // pumpPolMesAngle[m[41m[m
             // [m
[31m-            this.flAOMStepPlusButton.AutoSize = true;[m
[31m-            this.flAOMStepPlusButton.Location = new System.Drawing.Point(3, 6);[m
[31m-            this.flAOMStepPlusButton.Name = "flAOMStepPlusButton";[m
[31m-            this.flAOMStepPlusButton.Size = new System.Drawing.Size(31, 17);[m
[31m-            this.flAOMStepPlusButton.TabIndex = 32;[m
[31m-            this.flAOMStepPlusButton.Text = "+";[m
[31m-            this.flAOMStepPlusButton.UseVisualStyleBackColor = true;[m
[32m+[m[32m            this.pumpPolMesAngle.BackColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.pumpPolMesAngle.ForeColor = System.Drawing.Color.Chartreuse;[m[41m[m
[32m+[m[32m            this.pumpPolMesAngle.Location = new System.Drawing.Point(111, 180);[m[41m[m
[32m+[m[32m            this.pumpPolMesAngle.Name = "pumpPolMesAngle";[m[41m[m
[32m+[m[32m            this.pumpPolMesAngle.ReadOnly = true;[m[41m[m
[32m+[m[32m            this.pumpPolMesAngle.Size = new System.Drawing.Size(82, 20);[m[41m[m
[32m+[m[32m            this.pumpPolMesAngle.TabIndex = 43;[m[41m[m
[32m+[m[32m            this.pumpPolMesAngle.Text = "0";[m[41m[m
             // [m
[31m-            // flAOMStepMinusButton[m
[32m+[m[32m            // updatePumpPolMesAngle[m[41m[m
             // [m
[31m-            this.flAOMStepMinusButton.AutoSize = true;[m
[31m-            this.flAOMStepMinusButton.Location = new System.Drawing.Point(42, 7);[m
[31m-            this.flAOMStepMinusButton.Name = "flAOMStepMinusButton";[m
[31m-            this.flAOMStepMinusButton.Size = new System.Drawing.Size(28, 17);[m
[31m-            this.flAOMStepMinusButton.TabIndex = 32;[m
[31m-            this.flAOMStepMinusButton.Text = "-";[m
[31m-            this.flAOMStepMinusButton.UseVisualStyleBackColor = true;[m
[32m+[m[32m            this.updatePumpPolMesAngle.Location = new System.Drawing.Point(199, 178);[m[41m[m
[32m+[m[32m            this.updatePumpPolMesAngle.Name = "updatePumpPolMesAngle";[m[41m[m
[32m+[m[32m            this.updatePumpPolMesAngle.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.updatePumpPolMesAngle.TabIndex = 6;[m[41m[m
[32m+[m[32m            this.updatePumpPolMesAngle.Text = "Update";[m[41m[m
[32m+[m[32m            this.updatePumpPolMesAngle.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.updatePumpPolMesAngle.Click += new System.EventHandler(this.updatePumpPolMesAngle_Click);[m[41m[m
             // [m
[31m-            // flAOMStepTextBox[m
[32m+[m[32m            // zeroPumpPol[m[41m[m
             // [m
[31m-            this.flAOMStepTextBox.Location = new System.Drawing.Point(68, 41);[m
[31m-            this.flAOMStepTextBox.Name = "flAOMStepTextBox";[m
[31m-            this.flAOMStepTextBox.Size = new System.Drawing.Size(64, 20);[m
[31m-            this.flAOMStepTextBox.TabIndex = 47;[m
[31m-            this.flAOMStepTextBox.Text = "0";[m
[32m+[m[32m            this.zeroPumpPol.Location = new System.Drawing.Point(280, 177);[m[41m[m
[32m+[m[32m            this.zeroPumpPol.Name = "zeroPumpPol";[m[41m[m
[32m+[m[32m            this.zeroPumpPol.Size = new System.Drawing.Size(44, 23);[m[41m[m
[32m+[m[32m            this.zeroPumpPol.TabIndex = 2;[m[41m[m
[32m+[m[32m            this.zeroPumpPol.Text = "Zero";[m[41m[m
[32m+[m[32m            this.zeroPumpPol.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.zeroPumpPol.Click += new System.EventHandler(this.zeroPumpPol_Click);[m[41m[m
             // [m
[31m-            // label117[m
[32m+[m[32m            // label110[m[41m[m
             // [m
[31m-            this.label117.Location = new System.Drawing.Point(6, 44);[m
[31m-            this.label117.Name = "label117";[m
[31m-            this.label117.Size = new System.Drawing.Size(80, 23);[m
[31m-            this.label117.TabIndex = 46;[m
[31m-            this.label117.Text = "Step (V)";[m
[32m+[m[32m            this.label110.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label110.Location = new System.Drawing.Point(12, 183);[m[41m[m
[32m+[m[32m            this.label110.Name = "label110";[m[41m[m
[32m+[m[32m            this.label110.Size = new System.Drawing.Size(84, 13);[m[41m[m
[32m+[m[32m            this.label110.TabIndex = 7;[m[41m[m
[32m+[m[32m            this.label110.Text = "Measured Angle";[m[41m[m
             // [m
[31m-            // flAOMVoltageTextBox[m
[32m+[m[32m            // groupBox35[m[41m[m
             // [m
[31m-            this.flAOMVoltageTextBox.Location = new System.Drawing.Point(68, 21);[m
[31m-            this.flAOMVoltageTextBox.Name = "flAOMVoltageTextBox";[m
[31m-            this.flAOMVoltageTextBox.Size = new System.Drawing.Size(64, 20);[m
[31m-            this.flAOMVoltageTextBox.TabIndex = 45;[m
[31m-            this.flAOMVoltageTextBox.Text = "0";[m
[32m+[m[32m            this.groupBox35.Controls.Add(this.label124);[m[41m[m
[32m+[m[32m            this.groupBox35.Controls.Add(this.pumpBacklashTextBox);[m[41m[m
[32m+[m[32m            this.groupBox35.Controls.Add(this.pumpPolVoltStopButton);[m[41m[m
[32m+[m[32m            this.groupBox35.Controls.Add(this.pumpPolVoltTrackBar);[m[41m[m
[32m+[m[32m            this.groupBox35.Controls.Add(this.label111);[m[41m[m
[32m+[m[32m            this.groupBox35.Controls.Add(this.label112);[m[41m[m
[32m+[m[32m            this.groupBox35.Controls.Add(this.pumpPolSetAngle);[m[41m[m
[32m+[m[32m            this.groupBox35.Controls.Add(this.label113);[m[41m[m
[32m+[m[32m            this.groupBox35.Controls.Add(this.label114);[m[41m[m
[32m+[m[32m            this.groupBox35.Controls.Add(this.setPumpPolAngle);[m[41m[m
[32m+[m[32m            this.groupBox35.Controls.Add(this.pumpPolModeSelectSwitch);[m[41m[m
[32m+[m[32m            this.groupBox35.Location = new System.Drawing.Point(6, 11);[m[41m[m
[32m+[m[32m            this.groupBox35.Name = "groupBox35";[m[41m[m
[32m+[m[32m            this.groupBox35.Size = new System.Drawing.Size(332, 153);[m[41m[m
[32m+[m[32m            this.groupBox35.TabIndex = 50;[m[41m[m
[32m+[m[32m            this.groupBox35.TabStop = false;[m[41m[m
             // [m
[31m-            // UpdateFLAOMButton[m
[32m+[m[32m            // label124[m[41m[m
             // [m
[31m-            this.UpdateFLAOMButton.Location = new System.Drawing.Point(24, 121);[m
[31m-            this.UpdateFLAOMButton.Name = "UpdateFLAOMButton";[m
[31m-            this.UpdateFLAOMButton.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.UpdateFLAOMButton.TabIndex = 40;[m
[31m-            this.UpdateFLAOMButton.Text = "Update";[m
[31m-            this.UpdateFLAOMButton.Click += new System.EventHandler(this.UpdateFLAOMButton_Click);[m
[32m+[m[32m            this.label124.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label124.Location = new System.Drawing.Point(118, 55);[m[41m[m
[32m+[m[32m            this.label124.Name = "label124";[m[41m[m
[32m+[m[32m            this.label124.Size = new System.Drawing.Size(114, 13);[m[41m[m
[32m+[m[32m            this.label124.TabIndex = 54;[m[41m[m
[32m+[m[32m            this.label124.Text = "-ve overshoot ( 0 = off)";[m[41m[m
             // [m
[31m-            // label118[m
[32m+[m[32m            // pumpBacklashTextBox[m[41m[m
             // [m
[31m-            this.label118.Location = new System.Drawing.Point(6, 23);[m
[31m-            this.label118.Name = "label118";[m
[31m-            this.label118.Size = new System.Drawing.Size(80, 23);[m
[31m-            this.label118.TabIndex = 36;[m
[31m-            this.label118.Text = "Voltage (V)";[m
[32m+[m[32m            this.pumpBacklashTextBox.Location = new System.Drawing.Point(244, 52);[m[41m[m
[32m+[m[32m            this.pumpBacklashTextBox.Name = "pumpBacklashTextBox";[m[41m[m
[32m+[m[32m            this.pumpBacklashTextBox.Size = new System.Drawing.Size(75, 20);[m[41m[m
[32m+[m[32m            this.pumpBacklashTextBox.TabIndex = 53;[m[41m[m
[32m+[m[32m            this.pumpBacklashTextBox.Text = "0";[m[41m[m
             // [m
[31m-            // groupBox28[m
[32m+[m[32m            // pumpPolVoltStopButton[m[41m[m
             // [m
[31m-            this.groupBox28.Controls.Add(this.groupBox30);[m
[31m-            this.groupBox28.Controls.Add(this.groupBox31);[m
[31m-            this.groupBox28.Controls.Add(this.groupBox29);[m
[31m-            this.groupBox28.Location = new System.Drawing.Point(408, 218);[m
[31m-            this.groupBox28.Name = "groupBox28";[m
[31m-            this.groupBox28.Size = new System.Drawing.Size(283, 252);[m
[31m-            this.groupBox28.TabIndex = 2;[m
[31m-            this.groupBox28.TabStop = false;[m
[31m-            this.groupBox28.Text = "Fibre Amplifier";[m
[32m+[m[32m            this.pumpPolVoltStopButton.Enabled = false;[m[41m[m
[32m+[m[32m            this.pumpPolVoltStopButton.Location = new System.Drawing.Point(243, 106);[m[41m[m
[32m+[m[32m            this.pumpPolVoltStopButton.Name = "pumpPolVoltStopButton";[m[41m[m
[32m+[m[32m            this.pumpPolVoltStopButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.pumpPolVoltStopButton.TabIndex = 51;[m[41m[m
[32m+[m[32m            this.pumpPolVoltStopButton.Text = "Stop";[m[41m[m
[32m+[m[32m            this.pumpPolVoltStopButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.pumpPolVoltStopButton.Click += new System.EventHandler(this.pumpPolVoltStopButton_Click);[m[41m[m
             // [m
[31m-            // groupBox30[m
[32m+[m[32m            // pumpPolVoltTrackBar[m[41m[m
             // [m
[31m-            this.groupBox30.Controls.Add(this.fibreAmpEnableLED);[m
[31m-            this.groupBox30.Controls.Add(this.fibreAmpEnableSwitch);[m
[31m-            this.groupBox30.Location = new System.Drawing.Point(9, 18);[m
[31m-            this.groupBox30.Name = "groupBox30";[m
[31m-            this.groupBox30.Size = new System.Drawing.Size(124, 79);[m
[31m-            this.groupBox30.TabIndex = 50;[m
[31m-            this.groupBox30.TabStop = false;[m
[31m-            this.groupBox30.Text = "On/Off";[m
[32m+[m[32m            this.pumpPolVoltTrackBar.Enabled = false;[m[41m[m
[32m+[m[32m            this.pumpPolVoltTrackBar.Location = new System.Drawing.Point(88, 102);[m[41m[m
[32m+[m[32m            this.pumpPolVoltTrackBar.Maximum = 100;[m[41m[m
[32m+[m[32m            this.pumpPolVoltTrackBar.Minimum = -100;[m[41m[m
[32m+[m[32m            this.pumpPolVoltTrackBar.Name = "pumpPolVoltTrackBar";[m[41m[m
[32m+[m[32m            this.pumpPolVoltTrackBar.Size = new System.Drawing.Size(149, 45);[m[41m[m
[32m+[m[32m            this.pumpPolVoltTrackBar.TabIndex = 51;[m[41m[m
[32m+[m[32m            this.pumpPolVoltTrackBar.Scroll += new System.EventHandler(this.pumpPolVoltTrackBar_Scroll);[m[41m[m
             // [m
[31m-            // fibreAmpEnableLED[m
[32m+[m[32m            // label111[m[41m[m
             // [m
[31m-            this.fibreAmpEnableLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m
[31m-            this.fibreAmpEnableLED.Location = new System.Drawing.Point(9, 20);[m
[31m-            this.fibreAmpEnableLED.Name = "fibreAmpEnableLED";[m
[31m-            this.fibreAmpEnableLED.OffColor = System.Drawing.Color.Black;[m
[31m-            this.fibreAmpEnableLED.Size = new System.Drawing.Size(47, 49);[m
[31m-            this.fibreAmpEnableLED.TabIndex = 51;[m
[32m+[m[32m            this.label111.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label111.Location = new System.Drawing.Point(9, 126);[m[41m[m
[32m+[m[32m            this.label111.Name = "label111";[m[41m[m
[32m+[m[32m            this.label111.Size = new System.Drawing.Size(73, 13);[m[41m[m
[32m+[m[32m            this.label111.TabIndex = 49;[m[41m[m
[32m+[m[32m            this.label111.Text = "Voltage Mode";[m[41m[m
             // [m
[31m-            // fibreAmpEnableSwitch[m
[32m+[m[32m            // label112[m[41m[m
             // [m
[31m-            this.fibreAmpEnableSwitch.Location = new System.Drawing.Point(60, -4);[m
[31m-            this.fibreAmpEnableSwitch.Name = "fibreAmpEnableSwitch";[m
[31m-            this.fibreAmpEnableSwitch.Size = new System.Drawing.Size(64, 96);[m
[31m-            this.fibreAmpEnableSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;[m
[31m-            this.fibreAmpEnableSwitch.TabIndex = 50;[m
[31m-            this.fibreAmpEnableSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.fibreAmpEnableSwitch_StateChanged);[m
[32m+[m[32m            this.label112.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label112.Location = new System.Drawing.Point(102, 24);[m[41m[m
[32m+[m[32m            this.label112.Name = "label112";[m[41m[m
[32m+[m[32m            this.label112.Size = new System.Drawing.Size(53, 13);[m[41m[m
[32m+[m[32m            this.label112.TabIndex = 8;[m[41m[m
[32m+[m[32m            this.label112.Text = "Set Angle";[m[41m[m
             // [m
[31m-            // groupBox31[m
[32m+[m[32m            // pumpPolSetAngle[m[41m[m
             // [m
[31m-            this.groupBox31.Controls.Add(this.updateFibreAmpPwrButton);[m
[31m-            this.groupBox31.Controls.Add(this.fibreAmpPwrTextBox);[m
[31m-            this.groupBox31.Location = new System.Drawing.Point(149, 19);[m
[31m-            this.groupBox31.Name = "groupBox31";[m
[31m-            this.groupBox31.Size = new System.Drawing.Size(124, 76);[m
[31m-            this.groupBox31.TabIndex = 48;[m
[31m-            this.groupBox31.TabStop = false;[m
[31m-            this.groupBox31.Text = "Power";[m
[32m+[m[32m            this.pumpPolSetAngle.Location = new System.Drawing.Point(161, 19);[m[41m[m
[32m+[m[32m            this.pumpPolSetAngle.Name = "pumpPolSetAngle";[m[41m[m
[32m+[m[32m            this.pumpPolSetAngle.Size = new System.Drawing.Size(66, 20);[m[41m[m
[32m+[m[32m            this.pumpPolSetAngle.TabIndex = 13;[m[41m[m
[32m+[m[32m            this.pumpPolSetAngle.Text = "0";[m[41m[m
             // [m
[31m-            // updateFibreAmpPwrButton[m
[32m+[m[32m            // label113[m[41m[m
             // [m
[31m-            this.updateFibreAmpPwrButton.Location = new System.Drawing.Point(6, 45);[m
[31m-            this.updateFibreAmpPwrButton.Name = "updateFibreAmpPwrButton";[m
[31m-            this.updateFibreAmpPwrButton.Size = new System.Drawing.Size(100, 23);[m
[31m-            this.updateFibreAmpPwrButton.TabIndex = 60;[m
[31m-            this.updateFibreAmpPwrButton.Text = "Update";[m
[31m-            this.updateFibreAmpPwrButton.UseVisualStyleBackColor = true;[m
[31m-            this.updateFibreAmpPwrButton.Click += new System.EventHandler(this.updateFibreAmpPwrButton_Click);[m
[32m+[m[32m            this.label113.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label113.Location = new System.Drawing.Point(172, 78);[m[41m[m
[32m+[m[32m            this.label113.Name = "label113";[m[41m[m
[32m+[m[32m            this.label113.Size = new System.Drawing.Size(55, 13);[m[41m[m
[32m+[m[32m            this.label113.TabIndex = 44;[m[41m[m
[32m+[m[32m            this.label113.Text = "Clockwise";[m[41m[m
             // [m
[31m-            // fibreAmpPwrTextBox[m
[32m+[m[32m            // label114[m[41m[m
             // [m
[31m-            this.fibreAmpPwrTextBox.BackColor = System.Drawing.Color.LimeGreen;[m
[31m-            this.fibreAmpPwrTextBox.Location = new System.Drawing.Point(6, 19);[m
[31m-            this.fibreAmpPwrTextBox.Name = "fibreAmpPwrTextBox";[m
[31m-            this.fibreAmpPwrTextBox.Size = new System.Drawing.Size(100, 20);[m
[31m-            this.fibreAmpPwrTextBox.TabIndex = 49;[m
[31m-            this.fibreAmpPwrTextBox.Text = "0";[m
[32m+[m[32m            this.label114.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label114.Location = new System.Drawing.Point(85, 78);[m[41m[m
[32m+[m[32m            this.label114.Name = "label114";[m[41m[m
[32m+[m[32m            this.label114.Size = new System.Drawing.Size(75, 13);[m[41m[m
[32m+[m[32m            this.label114.TabIndex = 45;[m[41m[m
[32m+[m[32m            this.label114.Text = "Anti-clockwise";[m[41m[m
             // [m
[31m-            // groupBox29[m
[32m+[m[32m            // setPumpPolAngle[m[41m[m
             // [m
[31m-            this.groupBox29.Controls.Add(this.fibreAmpPowerFaultLED);[m
[31m-            this.groupBox29.Controls.Add(this.fibreAmpTempFaultLED);[m
[31m-            this.groupBox29.Controls.Add(this.fibreAmpBackReflectFaultLED);[m
[31m-            this.groupBox29.Controls.Add(this.fibreAmpSeedFaultLED);[m
[31m-            this.groupBox29.Controls.Add(this.fibreAmpMasterFaultLED);[m
[31m-            this.groupBox29.Controls.Add(this.faultCheckButton);[m
[31m-            this.groupBox29.Controls.Add(this.label93);[m
[31m-            this.groupBox29.Controls.Add(this.label92);[m
[31m-            this.groupBox29.Controls.Add(this.label91);[m
[31m-            this.groupBox29.Controls.Add(this.label90);[m
[31m-            this.groupBox29.Controls.Add(this.label89);[m
[31m-            this.groupBox29.Location = new System.Drawing.Point(9, 103);[m
[31m-            this.groupBox29.Name = "groupBox29";[m
[31m-            this.groupBox29.Size = new System.Drawing.Size(264, 142);[m
[31m-            this.groupBox29.TabIndex = 1;[m
[31m-            this.groupBox29.TabStop = false;[m
[31m-            this.groupBox29.Text = "Faults";[m
[32m+[m[32m            this.setPumpPolAngle.Location = new System.Drawing.Point(243, 17);[m[41m[m
[32m+[m[32m            this.setPumpPolAngle.Name = "setPumpPolAngle";[m[41m[m
[32m+[m[32m            this.setPumpPolAngle.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.setPumpPolAngle.TabIndex = 5;[m[41m[m
[32m+[m[32m            this.setPumpPolAngle.Text = "Set";[m[41m[m
[32m+[m[32m            this.setPumpPolAngle.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.setPumpPolAngle.Click += new System.EventHandler(this.setPumpPolAngle_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // pumpPolModeSelectSwitch[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.pumpPolModeSelectSwitch.Location = new System.Drawing.Point(12, 33);[m[41m[m
[32m+[m[32m            this.pumpPolModeSelectSwitch.Name = "pumpPolModeSelectSwitch";[m[41m[m
[32m+[m[32m            this.pumpPolModeSelectSwitch.Size = new System.Drawing.Size(64, 96);[m[41m[m
[32m+[m[32m            this.pumpPolModeSelectSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;[m[41m[m
[32m+[m[32m            this.pumpPolModeSelectSwitch.TabIndex = 51;[m[41m[m
[32m+[m[32m            this.pumpPolModeSelectSwitch.Value = true;[m[41m[m
[32m+[m[32m            this.pumpPolModeSelectSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.pumpPolModeSelectSwitch_StateChanged);[m[41m[m
             // [m
[31m-            // fibreAmpPowerFaultLED[m
[32m+[m[32m            // groupBox32[m[41m[m
             // [m
[31m-            this.fibreAmpPowerFaultLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m
[31m-            this.fibreAmpPowerFaultLED.Location = new System.Drawing.Point(214, 34);[m
[31m-            this.fibreAmpPowerFaultLED.Name = "fibreAmpPowerFaultLED";[m
[31m-            this.fibreAmpPowerFaultLED.OffColor = System.Drawing.Color.Black;[m
[31m-            this.fibreAmpPowerFaultLED.OnColor = System.Drawing.Color.Red;[m
[31m-            this.fibreAmpPowerFaultLED.Size = new System.Drawing.Size(38, 40);[m
[31m-            this.fibreAmpPowerFaultLED.TabIndex = 66;[m
[32m+[m[32m            this.groupBox32.Controls.Add(this.label106);[m[41m[m
[32m+[m[32m            this.groupBox32.Controls.Add(this.label105);[m[41m[m
[32m+[m[32m            this.groupBox32.Controls.Add(this.probePolMesAngle);[m[41m[m
[32m+[m[32m            this.groupBox32.Controls.Add(this.updateProbePolMesAngle);[m[41m[m
[32m+[m[32m            this.groupBox32.Controls.Add(this.zeroProbePol);[m[41m[m
[32m+[m[32m            this.groupBox32.Controls.Add(this.label101);[m[41m[m
[32m+[m[32m            this.groupBox32.Controls.Add(this.groupBox33);[m[41m[m
[32m+[m[32m            this.groupBox32.Location = new System.Drawing.Point(3, 6);[m[41m[m
[32m+[m[32m            this.groupBox32.Name = "groupBox32";[m[41m[m
[32m+[m[32m            this.groupBox32.Size = new System.Drawing.Size(345, 229);[m[41m[m
[32m+[m[32m            this.groupBox32.TabIndex = 12;[m[41m[m
[32m+[m[32m            this.groupBox32.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox32.Text = "Probe Polariser";[m[41m[m
             // [m
[31m-            // fibreAmpTempFaultLED[m
[32m+[m[32m            // label106[m[41m[m
             // [m
[31m-            this.fibreAmpTempFaultLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m
[31m-            this.fibreAmpTempFaultLED.Location = new System.Drawing.Point(166, 34);[m
[31m-            this.fibreAmpTempFaultLED.Name = "fibreAmpTempFaultLED";[m
[31m-            this.fibreAmpTempFaultLED.OffColor = System.Drawing.Color.Black;[m
[31m-            this.fibreAmpTempFaultLED.OnColor = System.Drawing.Color.Red;[m
[31m-            this.fibreAmpTempFaultLED.Size = new System.Drawing.Size(38, 40);[m
[31m-            this.fibreAmpTempFaultLED.TabIndex = 65;[m
[32m+[m[32m            this.label106.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label106.Location = new System.Drawing.Point(271, 30);[m[41m[m
[32m+[m[32m            this.label106.Name = "label106";[m[41m[m
[32m+[m[32m            this.label106.Size = new System.Drawing.Size(0, 13);[m[41m[m
[32m+[m[32m            this.label106.TabIndex = 48;[m[41m[m
             // [m
[31m-            // fibreAmpBackReflectFaultLED[m
[32m+[m[32m            // label105[m[41m[m
             // [m
[31m-            this.fibreAmpBackReflectFaultLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m
[31m-            this.fibreAmpBackReflectFaultLED.Location = new System.Drawing.Point(112, 34);[m
[31m-            this.fibreAmpBackReflectFaultLED.Name = "fibreAmpBackReflectFaultLED";[m
[31m-            this.fibreAmpBackReflectFaultLED.OffColor = System.Drawing.Color.Black;[m
[31m-            this.fibreAmpBackReflectFaultLED.OnColor = System.Drawing.Color.Red;[m
[31m-            this.fibreAmpBackReflectFaultLED.Size = new System.Drawing.Size(38, 40);[m
[31m-            this.fibreAmpBackReflectFaultLED.TabIndex = 64;[m
[32m+[m[32m            this.label105.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label105.Location = new System.Drawing.Point(15, 35);[m[41m[m
[32m+[m[32m            this.label105.Name = "label105";[m[41m[m
[32m+[m[32m            this.label105.Size = new System.Drawing.Size(74, 13);[m[41m[m
[32m+[m[32m            this.label105.TabIndex = 47;[m[41m[m
[32m+[m[32m            this.label105.Text = "Position Mode";[m[41m[m
             // [m
[31m-            // fibreAmpSeedFaultLED[m
[32m+[m[32m            // probePolMesAngle[m[41m[m
             // [m
[31m-            this.fibreAmpSeedFaultLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m
[31m-            this.fibreAmpSeedFaultLED.Location = new System.Drawing.Point(60, 34);[m
[31m-            this.fibreAmpSeedFaultLED.Name = "fibreAmpSeedFaultLED";[m
[31m-            this.fibreAmpSeedFaultLED.OffColor = System.Drawing.Color.Black;[m
[31m-            this.fibreAmpSeedFaultLED.OnColor = System.Drawing.Color.Red;[m
[31m-            this.fibreAmpSeedFaultLED.Size = new System.Drawing.Size(38, 40);[m
[31m-            this.fibreAmpSeedFaultLED.TabIndex = 63;[m
[32m+[m[32m            this.probePolMesAngle.BackColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.probePolMesAngle.ForeColor = System.Drawing.Color.Chartreuse;[m[41m[m
[32m+[m[32m            this.probePolMesAngle.Location = new System.Drawing.Point(111, 180);[m[41m[m
[32m+[m[32m            this.probePolMesAngle.Name = "probePolMesAngle";[m[41m[m
[32m+[m[32m            this.probePolMesAngle.ReadOnly = true;[m[41m[m
[32m+[m[32m            this.probePolMesAngle.Size = new System.Drawing.Size(82, 20);[m[41m[m
[32m+[m[32m            this.probePolMesAngle.TabIndex = 43;[m[41m[m
[32m+[m[32m            this.probePolMesAngle.Text = "0";[m[41m[m
             // [m
[31m-            // fibreAmpMasterFaultLED[m
[32m+[m[32m            // updateProbePolMesAngle[m[41m[m
             // [m
[31m-            this.fibreAmpMasterFaultLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;[m
[31m-            this.fibreAmpMasterFaultLED.Location = new System.Drawing.Point(16, 34);[m
[31m-            this.fibreAmpMasterFaultLED.Name = "fibreAmpMasterFaultLED";[m
[31m-            this.fibreAmpMasterFaultLED.OffColor = System.Drawing.Color.Black;[m
[31m-            this.fibreAmpMasterFaultLED.OnColor = System.Drawing.Color.Red;[m
[31m-            this.fibreAmpMasterFaultLED.Size = new System.Drawing.Size(38, 40);[m
[31m-            this.fibreAmpMasterFaultLED.TabIndex = 62;[m
[32m+[m[32m            this.updateProbePolMesAngle.Location = new System.Drawing.Point(199, 178);[m[41m[m
[32m+[m[32m            this.updateProbePolMesAngle.Name = "updateProbePolMesAngle";[m[41m[m
[32m+[m[32m            this.updateProbePolMesAngle.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.updateProbePolMesAngle.TabIndex = 6;[m[41m[m
[32m+[m[32m            this.updateProbePolMesAngle.Text = "Update";[m[41m[m
[32m+[m[32m            this.updateProbePolMesAngle.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.updateProbePolMesAngle.Click += new System.EventHandler(this.updateProbePolMesAngle_Click);[m[41m[m
             // [m
[31m-            // faultCheckButton[m
[32m+[m[32m            // zeroProbePol[m[41m[m
             // [m
[31m-            this.faultCheckButton.Location = new System.Drawing.Point(169, 111);[m
[31m-            this.faultCheckButton.Name = "faultCheckButton";[m
[31m-            this.faultCheckButton.Size = new System.Drawing.Size(89, 23);[m
[31m-            this.faultCheckButton.TabIndex = 61;[m
[31m-            this.faultCheckButton.Text = "Check for faults";[m
[31m-            this.faultCheckButton.UseVisualStyleBackColor = true;[m
[31m-            this.faultCheckButton.Click += new System.EventHandler(this.faultCheckButton_Click);[m
[32m+[m[32m            this.zeroProbePol.Location = new System.Drawing.Point(280, 177);[m[41m[m
[32m+[m[32m            this.zeroProbePol.Name = "zeroProbePol";[m[41m[m
[32m+[m[32m            this.zeroProbePol.Size = new System.Drawing.Size(44, 23);[m[41m[m
[32m+[m[32m            this.zeroProbePol.TabIndex = 2;[m[41m[m
[32m+[m[32m            this.zeroProbePol.Text = "Zero";[m[41m[m
[32m+[m[32m            this.zeroProbePol.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.zeroProbePol.Click += new System.EventHandler(this.zeroProbePol_Click);[m[41m[m
             // [m
[31m-            // label93[m
[32m+[m[32m            // label101[m[41m[m
             // [m
[31m-            this.label93.Location = new System.Drawing.Point(218, 77);[m
[31m-            this.label93.Name = "label93";[m
[31m-            this.label93.Size = new System.Drawing.Size(47, 31);[m
[31m-            this.label93.TabIndex = 49;[m
[31m-            this.label93.Text = "Power supply";[m
[32m+[m[32m            this.label101.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label101.Location = new System.Drawing.Point(12, 183);[m[41m[m
[32m+[m[32m            this.label101.Name = "label101";[m[41m[m
[32m+[m[32m            this.label101.Size = new System.Drawing.Size(84, 13);[m[41m[m
[32m+[m[32m            this.label101.TabIndex = 7;[m[41m[m
[32m+[m[32m            this.label101.Text = "Measured Angle";[m[41m[m
             // [m
[31m-            // label92[m
[32m+[m[32m            // groupBox33[m[41m[m
             // [m
[31m-            this.label92.Location = new System.Drawing.Point(166, 77);[m
[31m-            this.label92.Name = "label92";[m
[31m-            this.label92.Size = new System.Drawing.Size(38, 18);[m
[31m-            this.label92.TabIndex = 48;[m
[31m-            this.label92.Text = "Temp";[m
[32m+[m[32m            this.groupBox33.Controls.Add(this.label123);[m[41m[m
[32m+[m[32m            this.groupBox33.Controls.Add(this.probeBacklashTextBox);[m[41m[m
[32m+[m[32m            this.groupBox33.Controls.Add(this.probePolVoltStopButton);[m[41m[m
[32m+[m[32m            this.groupBox33.Controls.Add(this.probePolVoltTrackBar);[m[41m[m
[32m+[m[32m            this.groupBox33.Controls.Add(this.label107);[m[41m[m
[32m+[m[32m            this.groupBox33.Controls.Add(this.label102);[m[41m[m
[32m+[m[32m            this.groupBox33.Controls.Add(this.probePolSetAngle);[m[41m[m
[32m+[m[32m            this.groupBox33.Controls.Add(this.label103);[m[41m[m
[32m+[m[32m            this.groupBox33.Controls.Add(this.label104);[m[41m[m
[32m+[m[32m            this.groupBox33.Controls.Add(this.setProbePolAngle);[m[41m[m
[32m+[m[32m            this.groupBox33.Controls.Add(this.probePolModeSelectSwitch);[m[41m[m
[32m+[m[32m            this.groupBox33.Location = new System.Drawing.Point(6, 11);[m[41m[m
[32m+[m[32m            this.groupBox33.Name = "groupBox33";[m[41m[m
[32m+[m[32m            this.groupBox33.Size = new System.Drawing.Size(332, 153);[m[41m[m
[32m+[m[32m            this.groupBox33.TabIndex = 50;[m[41m[m
[32m+[m[32m            this.groupBox33.TabStop = false;[m[41m[m
             // [m
[31m-            // label91[m
[32m+[m[32m            // label123[m[41m[m
             // [m
[31m-            this.label91.Location = new System.Drawing.Point(112, 77);[m
[31m-            this.label91.Name = "label91";[m
[31m-            this.label91.Size = new System.Drawing.Size(59, 31);[m
[31m-            this.label91.TabIndex = 47;[m
[31m-            this.label91.Text = "Back reflection";[m
[32m+[m[32m            this.label123.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label123.Location = new System.Drawing.Point(117, 55);[m[41m[m
[32m+[m[32m            this.label123.Name = "label123";[m[41m[m
[32m+[m[32m            this.label123.Size = new System.Drawing.Size(114, 13);[m[41m[m
[32m+[m[32m            this.label123.TabIndex = 52;[m[41m[m
[32m+[m[32m            this.label123.Text = "-ve overshoot ( 0 = off)";[m[41m[m
             // [m
[31m-            // label90[m
[32m+[m[32m            // probeBacklashTextBox[m[41m[m
             // [m
[31m-            this.label90.Location = new System.Drawing.Point(67, 77);[m
[31m-            this.label90.Name = "label90";[m
[31m-            this.label90.Size = new System.Drawing.Size(39, 18);[m
[31m-            this.label90.TabIndex = 46;[m
[31m-            this.label90.Text = "Seed";[m
[32m+[m[32m            this.probeBacklashTextBox.Location = new System.Drawing.Point(243, 52);[m[41m[m
[32m+[m[32m            this.probeBacklashTextBox.Name = "probeBacklashTextBox";[m[41m[m
[32m+[m[32m            this.probeBacklashTextBox.Size = new System.Drawing.Size(75, 20);[m[41m[m
[32m+[m[32m            this.probeBacklashTextBox.TabIndex = 14;[m[41m[m
[32m+[m[32m            this.probeBacklashTextBox.Text = "0";[m[41m[m
             // [m
[31m-            // label89[m
[32m+[m[32m            // probePolVoltStopButton[m[41m[m
             // [m
[31m-            this.label89.Location = new System.Drawing.Point(13, 77);[m
[31m-            this.label89.Name = "label89";[m
[31m-            this.label89.Size = new System.Drawing.Size(41, 18);[m
[31m-            this.label89.TabIndex = 45;[m
[31m-            this.label89.Text = "Master";[m
[32m+[m[32m            this.probePolVoltStopButton.Enabled = false;[m[41m[m
[32m+[m[32m            this.probePolVoltStopButton.Location = new System.Drawing.Point(243, 106);[m[41m[m
[32m+[m[32m            this.probePolVoltStopButton.Name = "probePolVoltStopButton";[m[41m[m
[32m+[m[32m            this.probePolVoltStopButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.probePolVoltStopButton.TabIndex = 51;[m[41m[m
[32m+[m[32m            this.probePolVoltStopButton.Text = "Stop";[m[41m[m
[32m+[m[32m            this.probePolVoltStopButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.probePolVoltStopButton.Click += new System.EventHandler(this.probePolVoltStopButton_Click);[m[41m[m
             // [m
[31m-            // groupBox27[m
[32m+[m[32m            // probePolVoltTrackBar[m[41m[m
             // [m
[31m-            this.groupBox27.Controls.Add(this.flPZT2TempCurButton);[m
[31m-            this.groupBox27.Controls.Add(this.flPZT2CurTextBox);[m
[31m-            this.groupBox27.Controls.Add(this.flPZT2TempUpdateButton);[m
[31m-            this.groupBox27.Controls.Add(this.label116);[m
[31m-            this.groupBox27.Controls.Add(this.flPZT2TempTextBox);[m
[31m-            this.groupBox27.Controls.Add(this.label115);[m
[31m-            this.groupBox27.Controls.Add(this.MenloPZTTrackBar);[m
[31m-            this.groupBox27.Controls.Add(this.label94);[m
[31m-            this.groupBox27.Controls.Add(this.MenloPZTStepTextBox);[m
[31m-            this.groupBox27.Controls.Add(this.panel6);[m
[31m-            this.groupBox27.Controls.Add(this.updateflPZTButton);[m
[31m-            this.groupBox27.Controls.Add(this.MenloPZTTextBox);[m
[31m-            this.groupBox27.Controls.Add(this.label87);[m
[31m-            this.groupBox27.Location = new System.Drawing.Point(9, 218);[m
[31m-            this.groupBox27.Name = "groupBox27";[m
[31m-            this.groupBox27.Size = new System.Drawing.Size(393, 185);[m
[31m-            this.groupBox27.TabIndex = 1;[m
[31m-            this.groupBox27.TabStop = false;[m
[31m-            this.groupBox27.Text = "Fibre Laser";[m
[32m+[m[32m            this.probePolVoltTrackBar.Enabled = false;[m[41m[m
[32m+[m[32m            this.probePolVoltTrackBar.Location = new System.Drawing.Point(88, 102);[m[41m[m
[32m+[m[32m            this.probePolVoltTrackBar.Maximum = 100;[m[41m[m
[32m+[m[32m            this.probePolVoltTrackBar.Minimum = -100;[m[41m[m
[32m+[m[32m            this.probePolVoltTrackBar.Name = "probePolVoltTrackBar";[m[41m[m
[32m+[m[32m            this.probePolVoltTrackBar.Size = new System.Drawing.Size(149, 45);[m[41m[m
[32m+[m[32m            this.probePolVoltTrackBar.TabIndex = 51;[m[41m[m
[32m+[m[32m            this.probePolVoltTrackBar.Scroll += new System.EventHandler(this.probePolVoltTrackBar_Scroll);[m[41m[m
             // [m
[31m-            // flPZT2TempCurButton[m
[32m+[m[32m            // label107[m[41m[m
             // [m
[31m-            this.flPZT2TempCurButton.Location = new System.Drawing.Point(208, 153);[m
[31m-            this.flPZT2TempCurButton.Name = "flPZT2TempCurButton";[m
[31m-            this.flPZT2TempCurButton.Size = new System.Drawing.Size(72, 23);[m
[31m-            this.flPZT2TempCurButton.TabIndex = 79;[m
[31m-            this.flPZT2TempCurButton.Text = "Update";[m
[31m-            this.flPZT2TempCurButton.UseVisualStyleBackColor = true;[m
[31m-            this.flPZT2TempCurButton.Click += new System.EventHandler(this.flPZT2TempCurButton_Click);[m
[32m+[m[32m            this.label107.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label107.Location = new System.Drawing.Point(9, 126);[m[41m[m
[32m+[m[32m            this.label107.Name = "label107";[m[41m[m
[32m+[m[32m            this.label107.Size = new System.Drawing.Size(73, 13);[m[41m[m
[32m+[m[32m            this.label107.TabIndex = 49;[m[41m[m
[32m+[m[32m            this.label107.Text = "Voltage Mode";[m[41m[m
             // [m
[31m-            // flPZT2CurTextBox[m
[32m+[m[32m            // label102[m[41m[m
             // [m
[31m-            this.flPZT2CurTextBox.BackColor = System.Drawing.Color.White;[m
[31m-            this.flPZT2CurTextBox.Location = new System.Drawing.Point(138, 156);[m
[31m-            this.flPZT2CurTextBox.Name = "flPZT2CurTextBox";[m
[31m-            this.flPZT2CurTextBox.Size = new System.Drawing.Size(64, 20);[m
[31m-            this.flPZT2CurTextBox.TabIndex = 78;[m
[31m-            this.flPZT2CurTextBox.Text = "0";[m
[32m+[m[32m            this.label102.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label102.Location = new System.Drawing.Point(102, 24);[m[41m[m
[32m+[m[32m            this.label102.Name = "label102";[m[41m[m
[32m+[m[32m            this.label102.Size = new System.Drawing.Size(53, 13);[m[41m[m
[32m+[m[32m            this.label102.TabIndex = 8;[m[41m[m
[32m+[m[32m            this.label102.Text = "Set Angle";[m[41m[m
             // [m
[31m-            // flPZT2TempUpdateButton[m
[32m+[m[32m            // probePolSetAngle[m[41m[m
             // [m
[31m-            this.flPZT2TempUpdateButton.Location = new System.Drawing.Point(208, 131);[m
[31m-            this.flPZT2TempUpdateButton.Name = "flPZT2TempUpdateButton";[m
[31m-            this.flPZT2TempUpdateButton.Size = new System.Drawing.Size(72, 23);[m
[31m-            this.flPZT2TempUpdateButton.TabIndex = 77;[m
[31m-            this.flPZT2TempUpdateButton.Text = "Update";[m
[31m-            this.flPZT2TempUpdateButton.UseVisualStyleBackColor = true;[m
[31m-            this.flPZT2TempUpdateButton.Click += new System.EventHandler(this.flPZT2TempUpdateButton_Click);[m
[32m+[m[32m            this.probePolSetAngle.Location = new System.Drawing.Point(161, 19);[m[41m[m
[32m+[m[32m            this.probePolSetAngle.Name = "probePolSetAngle";[m[41m[m
[32m+[m[32m            this.probePolSetAngle.Size = new System.Drawing.Size(66, 20);[m[41m[m
[32m+[m[32m            this.probePolSetAngle.TabIndex = 13;[m[41m[m
[32m+[m[32m            this.probePolSetAngle.Text = "0";[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // label103[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.label103.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label103.Location = new System.Drawing.Point(172, 78);[m[41m[m
[32m+[m[32m            this.label103.Name = "label103";[m[41m[m
[32m+[m[32m            this.label103.Size = new System.Drawing.Size(55, 13);[m[41m[m
[32m+[m[32m            this.label103.TabIndex = 44;[m[41m[m
[32m+[m[32m            this.label103.Text = "Clockwise";[m[41m[m
             // [m
[31m-            // label116[m
[32m+[m[32m            // label104[m[41m[m
             // [m
[31m-            this.label116.Location = new System.Drawing.Point(6, 159);[m
[31m-            this.label116.Name = "label116";[m
[31m-            this.label116.Size = new System.Drawing.Size(126, 18);[m
[31m-            this.label116.TabIndex = 76;[m
[31m-            this.label116.Text = "Current Control (V)";[m
[32m+[m[32m            this.label104.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label104.Location = new System.Drawing.Point(85, 78);[m[41m[m
[32m+[m[32m            this.label104.Name = "label104";[m[41m[m
[32m+[m[32m            this.label104.Size = new System.Drawing.Size(75, 13);[m[41m[m
[32m+[m[32m            this.label104.TabIndex = 45;[m[41m[m
[32m+[m[32m            this.label104.Text = "Anti-clockwise";[m[41m[m
             // [m
[31m-            // flPZT2TempTextBox[m
[32m+[m[32m            // setProbePolAngle[m[41m[m
             // [m
[31m-            this.flPZT2TempTextBox.BackColor = System.Drawing.Color.White;[m
[31m-            this.flPZT2TempTextBox.Location = new System.Drawing.Point(138, 134);[m
[31m-            this.flPZT2TempTextBox.Name = "flPZT2TempTextBox";[m
[31m-            this.flPZT2TempTextBox.Size = new System.Drawing.Size(64, 20);[m
[31m-            this.flPZT2TempTextBox.TabIndex = 75;[m
[31m-            this.flPZT2TempTextBox.Text = "0";[m
[32m+[m[32m            this.setProbePolAngle.Location = new System.Drawing.Point(243, 17);[m[41m[m
[32m+[m[32m            this.setProbePolAngle.Name = "setProbePolAngle";[m[41m[m
[32m+[m[32m            this.setProbePolAngle.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.setProbePolAngle.TabIndex = 5;[m[41m[m
[32m+[m[32m            this.setProbePolAngle.Text = "Set";[m[41m[m
[32m+[m[32m            this.setProbePolAngle.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.setProbePolAngle.Click += new System.EventHandler(this.setProbePolAngle_Click);[m[41m[m
             // [m
[31m-            // label115[m
[32m+[m[32m            // probePolModeSelectSwitch[m[41m[m
             // [m
[31m-            this.label115.Location = new System.Drawing.Point(6, 137);[m
[31m-            this.label115.Name = "label115";[m
[31m-            this.label115.Size = new System.Drawing.Size(126, 18);[m
[31m-            this.label115.TabIndex = 74;[m
[31m-            this.label115.Text = "Temp Control (V)";[m
[32m+[m[32m            this.probePolModeSelectSwitch.Location = new System.Drawing.Point(12, 33);[m[41m[m
[32m+[m[32m            this.probePolModeSelectSwitch.Name = "probePolModeSelectSwitch";[m[41m[m
[32m+[m[32m            this.probePolModeSelectSwitch.Size = new System.Drawing.Size(64, 96);[m[41m[m
[32m+[m[32m            this.probePolModeSelectSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;[m[41m[m
[32m+[m[32m            this.probePolModeSelectSwitch.TabIndex = 51;[m[41m[m
[32m+[m[32m            this.probePolModeSelectSwitch.Value = true;[m[41m[m
[32m+[m[32m            this.probePolModeSelectSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.probePolModeSelectSwitch_StateChanged_1);[m[41m[m
             // [m
[31m-            // MenloPZTTrackBar[m
[32m+[m[32m            // tabPage5[m[41m[m
             // [m
[31m-            this.MenloPZTTrackBar.Location = new System.Drawing.Point(7, 51);[m
[31m-            this.MenloPZTTrackBar.Maximum = 1000;[m
[31m-            this.MenloPZTTrackBar.Name = "MenloPZTTrackBar";[m
[31m-            this.MenloPZTTrackBar.Size = new System.Drawing.Size(373, 45);[m
[31m-            this.MenloPZTTrackBar.TabIndex = 50;[m
[31m-            this.MenloPZTTrackBar.Scroll += new System.EventHandler(this.diodeRefCavtrackBar_Scroll);[m
[32m+[m[32m            this.tabPage5.BackColor = System.Drawing.Color.Transparent;[m[41m[m
[32m+[m[32m            this.tabPage5.Controls.Add(this.groupBox17);[m[41m[m
[32m+[m[32m            this.tabPage5.Controls.Add(this.groupBox15);[m[41m[m
[32m+[m[32m            this.tabPage5.Location = new System.Drawing.Point(4, 22);[m[41m[m
[32m+[m[32m            this.tabPage5.Name = "tabPage5";[m[41m[m
[32m+[m[32m            this.tabPage5.Size = new System.Drawing.Size(697, 575);[m[41m[m
[32m+[m[32m            this.tabPage5.TabIndex = 4;[m[41m[m
[32m+[m[32m            this.tabPage5.Text = "Source";[m[41m[m
             // [m
[31m-            // label94[m
[32m+[m[32m            // groupBox17[m[41m[m
             // [m
[31m-            this.label94.Location = new System.Drawing.Point(6, 102);[m
[31m-            this.label94.Name = "label94";[m
[31m-            this.label94.Size = new System.Drawing.Size(126, 18);[m
[31m-            this.label94.TabIndex = 73;[m
[31m-            this.label94.Text = "Piezo Control Step (V)";[m
[32m+[m[32m            this.groupBox17.Controls.Add(this.TargetStepButton);[m[41m[m
[32m+[m[32m            this.groupBox17.Controls.Add(this.label66);[m[41m[m
[32m+[m[32m            this.groupBox17.Controls.Add(this.TargetNumStepsTextBox);[m[41m[m
[32m+[m[32m            this.groupBox17.Location = new System.Drawing.Point(13, 165);[m[41m[m
[32m+[m[32m            this.groupBox17.Name = "groupBox17";[m[41m[m
[32m+[m[32m            this.groupBox17.Size = new System.Drawing.Size(351, 64);[m[41m[m
[32m+[m[32m            this.groupBox17.TabIndex = 47;[m[41m[m
[32m+[m[32m            this.groupBox17.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox17.Text = "Target stepper";[m[41m[m
             // [m
[31m-            // MenloPZTStepTextBox[m
[32m+[m[32m            // TargetStepButton[m[41m[m
             // [m
[31m-            this.MenloPZTStepTextBox.BackColor = System.Drawing.Color.White;[m
[31m-            this.MenloPZTStepTextBox.Location = new System.Drawing.Point(138, 100);[m
[31m-            this.MenloPZTStepTextBox.Name = "MenloPZTStepTextBox";[m
[31m-            this.MenloPZTStepTextBox.Size = new System.Drawing.Size(64, 20);[m
[31m-            this.MenloPZTStepTextBox.TabIndex = 72;[m
[31m-            this.MenloPZTStepTextBox.Text = "0";[m
[32m+[m[32m            this.TargetStepButton.Location = new System.Drawing.Point(256, 20);[m[41m[m
[32m+[m[32m            this.TargetStepButton.Name = "TargetStepButton";[m[41m[m
[32m+[m[32m            this.TargetStepButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.TargetStepButton.TabIndex = 2;[m[41m[m
[32m+[m[32m            this.TargetStepButton.Text = "Step!";[m[41m[m
[32m+[m[32m            this.TargetStepButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.TargetStepButton.Click += new System.EventHandler(this.TargetStepButton_Click);[m[41m[m
             // [m
[31m-            // panel6[m
[32m+[m[32m            // label66[m[41m[m
             // [m
[31m-            this.panel6.Controls.Add(this.flPZT2StepZeroButton);[m
[31m-            this.panel6.Controls.Add(this.MenloPZTStepPlusButton);[m
[31m-            this.panel6.Controls.Add(this.MenloPZTStepMinusButton);[m
[31m-            this.panel6.Location = new System.Drawing.Point(194, 16);[m
[31m-            this.panel6.Name = "panel6";[m
[31m-            this.panel6.Size = new System.Drawing.Size(108, 29);[m
[31m-            this.panel6.TabIndex = 71;[m
[32m+[m[32m            this.label66.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label66.Location = new System.Drawing.Point(19, 25);[m[41m[m
[32m+[m[32m            this.label66.Name = "label66";[m[41m[m
[32m+[m[32m            this.label66.Size = new System.Drawing.Size(89, 13);[m[41m[m
[32m+[m[32m            this.label66.TabIndex = 1;[m[41m[m
[32m+[m[32m            this.label66.Text = "Number of pulses";[m[41m[m
             // [m
[31m-            // flPZT2StepZeroButton[m
[32m+[m[32m            // TargetNumStepsTextBox[m[41m[m
             // [m
[31m-            this.flPZT2StepZeroButton.AutoSize = true;[m
[31m-            this.flPZT2StepZeroButton.Checked = true;[m
[31m-            this.flPZT2StepZeroButton.Location = new System.Drawing.Point(74, 7);[m
[31m-            this.flPZT2StepZeroButton.Name = "flPZT2StepZeroButton";[m
[31m-            this.flPZT2StepZeroButton.Size = new System.Drawing.Size(31, 17);[m
[31m-            this.flPZT2StepZeroButton.TabIndex = 32;[m
[31m-            this.flPZT2StepZeroButton.TabStop = true;[m
[31m-            this.flPZT2StepZeroButton.Text = "0";[m
[31m-            this.flPZT2StepZeroButton.UseVisualStyleBackColor = true;[m
[32m+[m[32m            this.TargetNumStepsTextBox.Location = new System.Drawing.Point(158, 22);[m[41m[m
[32m+[m[32m            this.TargetNumStepsTextBox.Name = "TargetNumStepsTextBox";[m[41m[m
[32m+[m[32m            this.TargetNumStepsTextBox.Size = new System.Drawing.Size(66, 20);[m[41m[m
[32m+[m[32m            this.TargetNumStepsTextBox.TabIndex = 0;[m[41m[m
[32m+[m[32m            this.TargetNumStepsTextBox.Text = "10";[m[41m[m
             // [m
[31m-            // MenloPZTStepPlusButton[m
[32m+[m[32m            // groupBox15[m[41m[m
             // [m
[31m-            this.MenloPZTStepPlusButton.AutoSize = true;[m
[31m-            this.MenloPZTStepPlusButton.Location = new System.Drawing.Point(3, 6);[m
[31m-            this.MenloPZTStepPlusButton.Name = "MenloPZTStepPlusButton";[m
[31m-            this.MenloPZTStepPlusButton.Size = new System.Drawing.Size(31, 17);[m
[31m-            this.MenloPZTStepPlusButton.TabIndex = 32;[m
[31m-            this.MenloPZTStepPlusButton.Text = "+";[m
[31m-            this.MenloPZTStepPlusButton.UseVisualStyleBackColor = true;[m
[32m+[m[32m            this.groupBox15.Controls.Add(this.label33);[m[41m[m
[32m+[m[32m            this.groupBox15.Controls.Add(this.checkYagInterlockButton);[m[41m[m
[32m+[m[32m            this.groupBox15.Controls.Add(this.yagFlashlampVTextBox);[m[41m[m
[32m+[m[32m            this.groupBox15.Controls.Add(this.interlockStatusTextBox);[m[41m[m
[32m+[m[32m            this.groupBox15.Controls.Add(this.updateFlashlampVButton);[m[41m[m
[32m+[m[32m            this.groupBox15.Controls.Add(this.label34);[m[41m[m
[32m+[m[32m            this.groupBox15.Controls.Add(this.startYAGFlashlampsButton);[m[41m[m
[32m+[m[32m            this.groupBox15.Controls.Add(this.yagQDisableButton);[m[41m[m
[32m+[m[32m            this.groupBox15.Controls.Add(this.stopYagFlashlampsButton);[m[41m[m
[32m+[m[32m            this.groupBox15.Controls.Add(this.yagQEnableButton);[m[41m[m
[32m+[m[32m            this.groupBox15.Location = new System.Drawing.Point(13, 14);[m[41m[m
[32m+[m[32m            this.groupBox15.Name = "groupBox15";[m[41m[m
[32m+[m[32m            this.groupBox15.Size = new System.Drawing.Size(528, 145);[m[41m[m
[32m+[m[32m            this.groupBox15.TabIndex = 46;[m[41m[m
[32m+[m[32m            this.groupBox15.TabStop = false;[m[41m[m
[32m+[m[32m            this.groupBox15.Text = "YAG";[m[41m[m
             // [m
[31m-            // MenloPZTStepMinusButton[m
[32m+[m[32m            // label33[m[41m[m
             // [m
[31m-            this.MenloPZTStepMinusButton.AutoSize = true;[m
[31m-            this.MenloPZTStepMinusButton.Location = new System.Drawing.Point(40, 7);[m
[31m-            this.MenloPZTStepMinusButton.Name = "MenloPZTStepMinusButton";[m
[31m-            this.MenloPZTStepMinusButton.Size = new System.Drawing.Size(28, 17);[m
[31m-            this.MenloPZTStepMinusButton.TabIndex = 32;[m
[31m-            this.MenloPZTStepMinusButton.Text = "-";[m
[31m-            this.MenloPZTStepMinusButton.UseVisualStyleBackColor = true;[m
[32m+[m[32m            this.label33.Location = new System.Drawing.Point(16, 31);[m[41m[m
[32m+[m[32m            this.label33.Name = "label33";[m[41m[m
[32m+[m[32m            this.label33.Size = new System.Drawing.Size(144, 23);[m[41m[m
[32m+[m[32m            this.label33.TabIndex = 13;[m[41m[m
[32m+[m[32m            this.label33.Text = "Flashlamp voltage (V)";[m[41m[m
             // [m
[31m-            // updateflPZTButton[m
[32m+[m[32m            // checkYagInterlockButton[m[41m[m
             // [m
[31m-            this.updateflPZTButton.Location = new System.Drawing.Point(308, 19);[m
[31m-            this.updateflPZTButton.Name = "updateflPZTButton";[m
[31m-            this.updateflPZTButton.Size = new System.Drawing.Size(72, 23);[m
[31m-            this.updateflPZTButton.TabIndex = 64;[m
[31m-            this.updateflPZTButton.Text = "Update";[m
[31m-            this.updateflPZTButton.UseVisualStyleBackColor = true;[m
[32m+[m[32m            this.checkYagInterlockButton.Location = new System.Drawing.Point(256, 63);[m[41m[m
[32m+[m[32m            this.checkYagInterlockButton.Name = "checkYagInterlockButton";[m[41m[m
[32m+[m[32m            this.checkYagInterlockButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.checkYagInterlockButton.TabIndex = 45;[m[41m[m
[32m+[m[32m            this.checkYagInterlockButton.Text = "Check";[m[41m[m
[32m+[m[32m            this.checkYagInterlockButton.Click += new System.EventHandler(this.checkYagInterlockButton_Click);[m[41m[m
             // [m
[31m-            // MenloPZTTextBox[m
[32m+[m[32m            // yagFlashlampVTextBox[m[41m[m
             // [m
[31m-            this.MenloPZTTextBox.BackColor = System.Drawing.Color.LimeGreen;[m
[31m-            this.MenloPZTTextBox.Location = new System.Drawing.Point(127, 22);[m
[31m-            this.MenloPZTTextBox.Name = "MenloPZTTextBox";[m
[31m-            this.MenloPZTTextBox.Size = new System.Drawing.Size(64, 20);[m
[31m-            this.MenloPZTTextBox.TabIndex = 45;[m
[31m-            this.MenloPZTTextBox.Text = "0";[m
[32m+[m[32m            this.yagFlashlampVTextBox.Location = new System.Drawing.Point(160, 31);[m[41m[m
[32m+[m[32m            this.yagFlashlampVTextBox.Name = "yagFlashlampVTextBox";[m[41m[m
[32m+[m[32m            this.yagFlashlampVTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.yagFlashlampVTextBox.TabIndex = 12;[m[41m[m
[32m+[m[32m            this.yagFlashlampVTextBox.Text = "1220";[m[41m[m
             // [m
[31m-            // label87[m
[32m+[m[32m            // interlockStatusTextBox[m[41m[m
             // [m
[31m-            this.label87.Location = new System.Drawing.Point(6, 25);[m
[31m-            this.label87.Name = "label87";[m
[31m-            this.label87.Size = new System.Drawing.Size(93, 18);[m
[31m-            this.label87.TabIndex = 44;[m
[31m-            this.label87.Text = "Piezo Control (V)";[m
[32m+[m[32m            this.interlockStatusTextBox.BackColor = System.Drawing.Color.Black;[m[41m[m
[32m+[m[32m            this.interlockStatusTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m[41m[m
[32m+[m[32m            this.interlockStatusTextBox.Location = new System.Drawing.Point(160, 63);[m[41m[m
[32m+[m[32m            this.interlockStatusTextBox.Name = "interlockStatusTextBox";[m[41m[m
[32m+[m[32m            this.interlockStatusTextBox.ReadOnly = true;[m[41m[m
[32m+[m[32m            this.interlockStatusTextBox.Size = new System.Drawing.Size(64, 20);[m[41m[m
[32m+[m[32m            this.interlockStatusTextBox.TabIndex = 44;[m[41m[m
[32m+[m[32m            this.interlockStatusTextBox.Text = "0";[m[41m[m
             // [m
[31m-            // groupBox26[m
[32m+[m[32m            // updateFlashlampVButton[m[41m[m
             // [m
[31m-            this.groupBox26.Controls.Add(this.updateDiodeCurrentMonButton);[m
[31m-            this.groupBox26.Controls.Add(this.diodeCurrentTextBox);[m
[31m-            this.groupBox26.Controls.Add(this.stopDiodeCurrentPollButton);[m
[31m-            this.groupBox26.Controls.Add(this.startDiodeCurrentPollButton);[m
[31m-            this.groupBox26.Controls.Add(this.diodeCurrentPollTextBox);[m
[31m-            this.groupBox26.Controls.Add(this.label86);[m
[31m-            this.groupBox26.Controls.Add(this.diodeCurrentGraph);[m
[31m-            this.groupBox26.Location = new System.Drawing.Point(6, 6);[m
[31m-            this.groupBox26.Name = "groupBox26";[m
[31m-            this.groupBox26.Size = new System.Drawing.Size(685, 206);[m
[31m-            this.groupBox26.TabIndex = 0;[m
[31m-            this.groupBox26.TabStop = false;[m
[31m-            this.groupBox26.Text = "Current Supply";[m
[32m+[m[32m            this.updateFlashlampVButton.Location = new System.Drawing.Point(256, 31);[m[41m[m
[32m+[m[32m            this.updateFlashlampVButton.Name = "updateFlashlampVButton";[m[41m[m
[32m+[m[32m            this.updateFlashlampVButton.Size = new System.Drawing.Size(75, 23);[m[41m[m
[32m+[m[32m            this.updateFlashlampVButton.TabIndex = 14;[m[41m[m
[32m+[m[32m            this.updateFlashlampVButton.Text = "Update V";[m[41m[m
[32m+[m[32m            this.updateFlashlampVButton.Click += new System.EventHandler(this.updateFlashlampVButton_Click);[m[41m[m
             // [m
[31m-            // updateDiodeCurrentMonButton[m
[32m+[m[32m            // label34[m[41m[m
             // [m
[31m-            this.updateDiodeCurrentMonButton.Location = new System.Drawing.Point(178, 176);[m
[31m-            this.updateDiodeCurrentMonButton.Name = "updateDiodeCurrentMonButton";[m
[31m-            this.updateDiodeCurrentMonButton.Size = new System.Drawing.Size(72, 23);[m
[31m-            this.updateDiodeCurrentMonButton.TabIndex = 62;[m
[31m-            this.updateDiodeCurrentMonButton.Text = "Update";[m
[31m-            this.updateDiodeCurrentMonButton.UseVisualStyleBackColor = true;[m
[31m-            this.updateDiodeCurrentMonButton.Click += new System.EventHandler(this.updateDiodeCurrentMonButton_Click);[m
[32m+[m[32m            this.label34.Location = new System.Drawing.Point(16, 63);[m[41m[m
[32m+[m[32m            this.label34.Name = "label34";[m[41m[m
[32m+[m[32m            this.label34.Size = new System.Drawing.Size(104, 23);[m[41m[m
[32m+[m[32m            this.label34.TabIndex = 43;[m[41m[m
[32m+[m[32m            this.label34.Text = "Interlock failed";[m[41m[m
             // [m
[31m-            // diodeCurrentTextBox[m
[32m+[m[32m            // startYAGFlashlampsButton[m[41m[m
             // [m
[31m-            this.diodeCurrentTextBox.BackColor = System.Drawing.Color.Black;[m
[31m-            this.diodeCurrentTextBox.ForeColor = System.Drawing.Color.Chartreuse;[m
[31m-            this.diodeCurrentTextBox.Location = new System.Drawing.Point(35, 178);[m
[31m-            this.diodeCurrentTextBox.Name = "diodeCurrentTextBox";[m
[31m-            this.diodeCurrentTextBox.ReadOnly = true;[m
[31m-            this.diodeCurrentTextBox.Size = new System.Drawing.Size(137, 20);[m
[31m-            this.diodeCurrentTextBox.TabIndex = 61;[m
[31m-            this.diodeCurrentTextBox.Text = "0";[m
[32m+[m[32m            this.startYAGFlashlampsButton.Location = new System.Drawing.Point(16, 103);[m[41m[m
[32m+[m[32m            this.startYAGFlashlampsButton.Name = "startYAGFlashlampsButton";[m[41m[m
[32m+[m[32m            this.startYAGFlashlampsButton.Size = new System.Drawing.Size(112, 23);[m[41m[m
[32m+[m[32m            this.startYAGFlashlampsButton.TabIndex = 15;[m[41m[m
[32m+[m[32m            this.startYAGFlashlampsButton.Text = "Start Flashlamps";[m[41m[m
[32m+[m[32m            this.startYAGFlashlampsButton.Click += new System.EventHandler(this.startYAGFlashlampsButton_Click);[m[41m[m
             // [m
[31m-            // stopDiodeCurrentPollButton[m
[32m+[m[32m            // yagQDisableButton[m[41m[m
             // [m
[31m-            this.stopDiodeCurrentPollButton.Enabled = false;[m
[31m-            this.stopDiodeCurrentPollButton.Location = new System.Drawing.Point(604, 176);[m
[31m-            this.stopDiodeCurrentPollButton.Name = "stopDiodeCurrentPollButton";[m
[31m-            this.stopDiodeCurrentPollButton.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.stopDiodeCurrentPollButton.TabIndex = 60;[m
[31m-            this.stopDiodeCurrentPollButton.Text = "Stop poll";[m
[31m-            this.stopDiodeCurrentPollButton.UseVisualStyleBackColor = true;[m
[31m-            this.stopDiodeCurrentPollButton.Click += new System.EventHandler(this.stopDiodeCurrentPollButton_Click);[m
[32m+[m[32m            this.yagQDisableButton.Enabled = false;[m[41m[m
[32m+[m[32m            this.yagQDisableButton.Location = new System.Drawing.Point(400, 103);[m[41m[m
[32m+[m[32m            this.yagQDisableButton.Name = "yagQDisableButton";[m[41m[m
[32m+[m[32m            this.yagQDisableButton.Size = new System.Drawing.Size(112, 23);[m[41m[m
[32m+[m[32m            this.yagQDisableButton.TabIndex = 18;[m[41m[m
[32m+[m[32m            this.yagQDisableButton.Text = "Q-switch Disable";[m[41m[m
[32m+[m[32m            this.yagQDisableButton.Click += new System.EventHandler(this.yagQDisableButton_Click);[m[41m[m
             // [m
[31m-            // startDiodeCurrentPollButton[m
[32m+[m[32m            // stopYagFlashlampsButton[m[41m[m
             // [m
[31m-            this.startDiodeCurrentPollButton.Location = new System.Drawing.Point(523, 176);[m
[31m-            this.startDiodeCurrentPollButton.Name = "startDiodeCurrentPollButton";[m
[31m-            this.startDiodeCurrentPollButton.Size = new System.Drawing.Size(75, 23);[m
[31m-            this.startDiodeCurrentPollButton.TabIndex = 59;[m
[31m-            this.startDiodeCurrentPollButton.Text = "Start poll";[m
[31m-            this.startDiodeCurrentPollButton.UseVisualStyleBackColor = true;[m
[31m-            this.startDiodeCurrentPollButton.Click += new System.EventHandler(this.startDiodeCurrentPollButton_Click);[m
[32m+[m[32m            this.stopYagFlashlampsButton.Enabled = false;[m[41m[m
[32m+[m[32m            this.stopYagFlashlampsButton.Location = new System.Drawing.Point(144, 103);[m[41m[m
[32m+[m[32m            this.stopYagFlashlampsButton.Name = "stopYagFlashlampsButton";[m[41m[m
[32m+[m[32m            this.stopYagFlashlampsButton.Size = new System.Drawing.Size(112, 23);[m[41m[m
[32m+[m[32m            this.stopYagFlashlampsButton.TabIndex = 16;[m[41m[m
[32m+[m[32m            this.stopYagFlashlampsButton.Text = "Stop Flashlamps";[m[41m[m
[32m+[m[32m            this.stopYagFlashlampsButton.Click += new System.EventHandler(this.stopYagFlashlampsButton_Click);[m[41m[m
             // [m
[31m-            // diodeCurrentPollTextBox[m
[32m+[m[32m            // yagQEnableButton[m[41m[m
             // [m
[31m-            this.diodeCurrentPollTextBox.Location = new System.Drawing.Point(453, 178);[m
[31m-            this.diodeCurrentPollTextBox.Name = "diodeCurrentPollTextBox";[m
[31m-            this.diodeCurrentPollTextBox.Size = new System.Drawing.Size(64, 20);[m
[31m-            this.diodeCurrentPollTextBox.TabIndex = 58;[m
[31m-            this.diodeCurrentPollTextBox.Text = "100";[m
[32m+[m[32m            this.yagQEnableButton.Location = new System.Drawing.Point(272, 103);[m[41m[m
[32m+[m[32m            this.yagQEnableButton.Name = "yagQEnableButton";[m[41m[m
[32m+[m[32m            this.yagQEnableButton.Size = new System.Drawing.Size(112, 23);[m[41m[m
[32m+[m[32m            this.yagQEnableButton.TabIndex = 17;[m[41m[m
[32m+[m[32m            this.yagQEnableButton.Text = "Q-switch Enable";[m[41m[m
[32m+[m[32m            this.yagQEnableButton.Click += new System.EventHandler(this.yagQEnableButton_Click);[m[41m[m
             // [m
[31m-            // label86[m
[32m+[m[32m            // tabPage9[m[41m[m
             // [m
[31m-            this.label86.Location = new System.Drawing.Point(366, 181);[m
[31m-            this.label86.Name = "label86";[m
[31m-            this.label86.Size = new System.Drawing.Size(101, 23);[m
[31m-            this.label86.TabIndex = 57;[m
[31m-            this.label86.Text = "Poll period (ms)";[m
[32m+[m[32m            this.tabPage9.BackColor = System.Drawing.Color.Transparent;[m[41m[m
[32m+[m[32m            this.tabPage9.Controls.Add(this.switchScanTTLSwitch);[m[41m[m
[32m+[m[32m            this.tabPage9.Controls.Add(this.label97);[m[41m[m
[32m+[m[32m            this.tabPage9.Location = new System.Drawing.Point(4, 22);[m[41m[m
[32m+[m[32m            this.tabPage9.Name = "tabPage9";[m[41m[m
[32m+[m[32m            this.tabPage9.Padding = new System.Windows.Forms.Padding(3);[m[41m[m
[32m+[m[32m            this.tabPage9.Size = new System.Drawing.Size(697, 575);[m[41m[m
[32m+[m[32m            this.tabPage9.TabIndex = 8;[m[41m[m
[32m+[m[32m            this.tabPage9.Text = "Misc";[m[41m[m
             // [m
[31m-            // diodeCurrentGraph[m
[32m+[m[32m            // switchScanTTLSwitch[m[41m[m
             // [m
[31m-            this.diodeCurrentGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) [m
[31m-            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) [m
[31m-            | NationalInstruments.UI.GraphInteractionModes.PanX) [m
[31m-            | NationalInstruments.UI.GraphInteractionModes.PanY) [m
[31m-            | NationalInstruments.UI.GraphInteractionModes.DragCursor) [m
[31m-            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) [m
[31m-            | NationalInstruments.UI.GraphInteractionModes.EditRange)));[m
[31m-            this.diodeCurrentGraph.Location = new System.Drawing.Point(6, 19);[m
[31m-            this.diodeCurrentGraph.Name = "diodeCurrentGraph";[m
[31m-            this.diodeCurrentGraph.Plots.AddRange(new NationalInstruments.UI.WaveformPlot[] {[m
[31m-            this.diodeCurrentPlot});[m
[31m-            this.diodeCurrentGraph.Size = new System.Drawing.Size(673, 153);[m
[31m-            this.diodeCurrentGraph.TabIndex = 46;[m
[31m-            this.diodeCurrentGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {[m
[31m-            this.xAxis2});[m
[31m-            this.diodeCurrentGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {[m
[31m-            this.yAxis2});[m
[32m+[m[32m            this.switchScanTTLSwitch.Location = new System.Drawing.Point(6, 6);[m[41m[m
[32m+[m[32m            this.switchScanTTLSwitch.Name = "switchScanTTLSwitch";[m[41m[m
[32m+[m[32m            this.switchScanTTLSwitch.Size = new System.Drawing.Size(64, 96);[m[41m[m
[32m+[m[32m            this.switchScanTTLSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;[m[41m[m
[32m+[m[32m            this.switchScanTTLSwitch.TabIndex = 2;[m[41m[m
[32m+[m[32m            this.switchScanTTLSwitch.StateChanged += new NationalInstruments.UI.ActionEventHandler(this.switch1_StateChanged);[m[41m[m
             // [m
[31m-            // diodeCurrentPlot[m
[32m+[m[32m            // label97[m[41m[m
             // [m
[31m-            this.diodeCurrentPlot.AntiAliased = true;[m
[31m-            this.diodeCurrentPlot.HistoryCapacity = 10000;[m
[31m-            this.diodeCurrentPlot.LineWidth = 2F;[m
[31m-            this.diodeCurrentPlot.XAxis = this.xAxis2;[m
[31m-            this.diodeCurrentPlot.YAxis = this.yAxis2;[m
[32m+[m[32m            this.label97.AutoSize = true;[m[41m[m
[32m+[m[32m            this.label97.Location = new System.Drawing.Point(76, 53);[m[41m[m
[32m+[m[32m            this.label97.Name = "label97";[m[41m[m
[32m+[m[32m            this.label97.Size = new System.Drawing.Size(90, 13);[m[41m[m
[32m+[m[32m            this.label97.TabIndex = 1;[m[41m[m
[32m+[m[32m            this.label97.Text = "Switch Scan TTL";[m[41m[m
             // [m
[31m-            // xAxis2[m
[32m+[m[32m            // tabPage7[m[41m[m
             // [m
[31m-            this.xAxis2.Mode = NationalInstruments.UI.AxisMode.StripChart;[m
[31m-            this.xAxis2.Range = new NationalInstruments.UI.Range(0D, 500D);[m
[32m+[m[32m            this.tabPage7.BackColor = System.Drawing.Color.Transparent;[m[41m[m
[32m+[m[32m            this.tabPage7.Controls.Add(this.clearAlertButton);[m[41m[m
[32m+[m[32m            this.tabPage7.Controls.Add(this.alertTextBox);[m[41m[m
[32m+[m[32m            this.tabPage7.ImageKey = "(none)";[m[41m[m
[32m+[m[32m            this.tabPage7.Location = new System.Drawing.Point(4, 22);[m[41m[m
[32m+[m[32m            this.tabPage7.Name = "tabPage7";[m[41m[m
[32m+[m[32m            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);[m[41m[m
[32m+[m[32m            this.tabPage7.Size = new System.Drawing.Size(697, 575);[m[41m[m
[32m+[m[32m            this.tabPage7.TabIndex = 6;[m[41m[m
[32m+[m[32m            this.tabPage7.Text = "Alerts";[m[41m[m
             // [m
[31m-            // yAxis2[m
[32m+[m[32m            // clearAlertButton[m[41m[m
             // [m
[31m-            this.yAxis2.Mode = NationalInstruments.UI.AxisMode.Fixed;[m
[31m-            this.yAxis2.OriginLineVisible = true;[m
[32m+[m[32m            this.clearAlertButton.Location = new System.Drawing.Point(18, 540);[m[41m[m
[32m+[m[32m            this.clearAlertButton.Name = "clearAlertButton";[m[41m[m
[32m+[m[32m            this.clearAlertButton.Size = new System.Drawing.Size(140, 23);[m[41m[m
[32m+[m[32m            this.clearAlertButton.TabIndex = 1;[m[41m[m
[32m+[m[32m            this.clearAlertButton.Text = "Clear alert status";[m[41m[m
[32m+[m[32m            this.clearAlertButton.UseVisualStyleBackColor = true;[m[41m[m
[32m+[m[32m            this.clearAlertButton.Click += new System.EventHandler(this.clearAlertButton_Click);[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            // alertTextBox[m[41m[m
[32m+[m[32m            //[m[41m [m
[32m+[m[32m            this.alertTextBox.Location = new System.Drawing.Point(18, 22);[m[41m[m
[32m+[m[32m            this.alertTextBox.Multiline = true;[m[41m[m
[32m+[m[32m            this.alertTextBox.Name = "alertTextBox";[m[41m[m
[32m+[m[32m            this.alertTextBox.Size = new System.Drawing.Size(654, 512);[m[41m[m
[32m+[m[32m            this.alertTextBox.TabIndex = 0;[m[41m[m
             // [m
             // tabPage10[m
             // [m
[36m@@ -5726,53 +6599,6 @@[m [mnamespace EDMHardwareControl[m
             this.radioButton6.TabIndex = 32;[m
             this.radioButton6.Text = "-";[m
             this.radioButton6.UseVisualStyleBackColor = true;[m
[31m-            //[m
[31m-            // tabPage12[m
[31m-            // [m
[31m-          //  this.tabPage12.BackColor = System.Drawing.SystemColors.Control;[m
[31m-            //this.tabPage12.Controls.Add(this.groupBox121);[m
[31m-            //this.tabPage12.Controls.Add(this.groupBox11);[m
[31m-            //this.tabPage12.Controls.Add(this.groupBox10);[m
[31m-            //this.tabPage12.Controls.Add(this.groupBox18);[m
[31m-           // this.tabPage12.Location = new System.Drawing.Point(4, 22);[m
[31m-          //  this.tabPage12.Name = "tabPage12";[m
[31m-          //  this.tabPage12.Size = new System.Drawing.Size(697, 575);[m
[31m-            //this.tabPage12.TabIndex = 10;[m
[31m-          //  this.tabPage12.Text = "Microwaves";[m
[31m-            // [m
[31m-            // groupBox121[m
[31m-            // [m
[31m-          //  this.groupBox3.Controls.Add(this.label140);[m
[31m-           // this.groupBox121.Controls.Add(this.uWaveDCFMBox);[m
[31m-           // this.groupBox3.Controls.Add(this.greenOnCheck);[m
[31m-            //this.groupBox3.Controls.Add(this.label7);[m
[31m-            //this.groupBox3.Controls.Add(this.greenOnAmpBox);[m
[31m-           // this.groupBox3.Controls.Add(this.label8);[m
[31m-          //  this.groupBox3.Controls.Add(this.greenOnFreqBox);[m
[31m-           // this.groupBox121.Location = new System.Drawing.Point(8, 16);[m
[31m-          //  this.groupBox121.Name = "groupBox121";[m
[31m-          //  this.groupBox121.Size = new System.Drawing.Size(296, 160);[m
[31m-          //  this.groupBox3.TabIndex = 21;[m
[31m-           // this.groupBox3.TabStop = false;[m
[31m-          //  this.groupBox121.Text = "Direct synth control";[m
[31m-            // [m
[31m-            // label140[m
[31m-            // [m
[31m-         //   this.label140.Location = new System.Drawing.Point(6, 88);[m
[31m-          //  this.label140.Name = "label140";[m
[31m-           // this.label140.Size = new System.Drawing.Size(133, 23);[m
[31m-          //  this.label140.TabIndex = 23;[m
[31m-           // this.label140.Text = "Microwave synth DC FM (V)";[m
[31m-            // [m
[31m-            // uWaveDCFMBox[m
[31m-            // [m
[31m-           // this.uWaveDCFMBox.Location = new System.Drawing.Point(168, 85);[m
[31m-          //  this.uWaveDCFMBox.Name = "uWaveDCFMBox";[m
[31m-           // this.uWaveDCFMBox.Size = new System.Drawing.Size(64, 20);[m
[31m-          //  this.uWaveDCFMBox.TabIndex = 2;[m
[31m-          //  this.uWaveDCFMBox.Text = "0";[m
[31m-[m
[31m-          [m
             // [m
             // ControlWindow[m
             // [m
[36m@@ -5848,29 +6674,6 @@[m [mnamespace EDMHardwareControl[m
             ((System.ComponentModel.ISupportInitialize)(this.probeAOMtrackBar)).EndInit();[m
             this.panel5.ResumeLayout(false);[m
             this.panel5.PerformLayout();[m
[31m-            this.tabPage6.ResumeLayout(false);[m
[31m-            this.groupBox34.ResumeLayout(false);[m
[31m-            this.groupBox34.PerformLayout();[m
[31m-            this.groupBox35.ResumeLayout(false);[m
[31m-            this.groupBox35.PerformLayout();[m
[31m-            ((System.ComponentModel.ISupportInitialize)(this.pumpPolVoltTrackBar)).EndInit();[m
[31m-            ((System.ComponentModel.ISupportInitialize)(this.pumpPolModeSelectSwitch)).EndInit();[m
[31m-            this.groupBox32.ResumeLayout(false);[m
[31m-            this.groupBox32.PerformLayout();[m
[31m-            this.groupBox33.ResumeLayout(false);[m
[31m-            this.groupBox33.PerformLayout();[m
[31m-            ((System.ComponentModel.ISupportInitialize)(this.probePolVoltTrackBar)).EndInit();[m
[31m-            ((System.ComponentModel.ISupportInitialize)(this.probePolModeSelectSwitch)).EndInit();[m
[31m-            this.tabPage5.ResumeLayout(false);[m
[31m-            this.groupBox17.ResumeLayout(false);[m
[31m-            this.groupBox17.PerformLayout();[m
[31m-            this.groupBox15.ResumeLayout(false);[m
[31m-            this.groupBox15.PerformLayout();[m
[31m-            this.tabPage9.ResumeLayout(false);[m
[31m-            this.tabPage9.PerformLayout();[m
[31m-            ((System.ComponentModel.ISupportInitialize)(this.switchScanTTLSwitch)).EndInit();[m
[31m-            this.tabPage7.ResumeLayout(false);[m
[31m-            this.tabPage7.PerformLayout();[m
             this.tabPage8.ResumeLayout(false);[m
             this.groupBox36.ResumeLayout(false);[m
             this.groupBox36.PerformLayout();[m
[36m@@ -5896,6 +6699,47 @@[m [mnamespace EDMHardwareControl[m
             this.groupBox26.ResumeLayout(false);[m
             this.groupBox26.PerformLayout();[m
             ((System.ComponentModel.ISupportInitialize)(this.diodeCurrentGraph)).EndInit();[m
[32m+[m[32m            this.tabPage13.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox41.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox41.PerformLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.mixerVoltageTrackBar)).EndInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.uWaveDCFMTrackBar)).EndInit();[m[41m[m
[32m+[m[32m            this.tabPage12.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox40.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox40.PerformLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO155FreqTrackBar)).EndInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO155AmpTrackBar)).EndInit();[m[41m[m
[32m+[m[32m            this.groupBox39.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox39.PerformLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO30FreqTrackBar)).EndInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO30AmpTrackBar)).EndInit();[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox161MHzVCO.PerformLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO161FreqTrackBar)).EndInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.VCO161AmpTrackBar)).EndInit();[m[41m[m
[32m+[m[32m            this.tabPage6.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox34.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox34.PerformLayout();[m[41m[m
[32m+[m[32m            this.groupBox35.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox35.PerformLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.pumpPolVoltTrackBar)).EndInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.pumpPolModeSelectSwitch)).EndInit();[m[41m[m
[32m+[m[32m            this.groupBox32.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox32.PerformLayout();[m[41m[m
[32m+[m[32m            this.groupBox33.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox33.PerformLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.probePolVoltTrackBar)).EndInit();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.probePolModeSelectSwitch)).EndInit();[m[41m[m
[32m+[m[32m            this.tabPage5.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox17.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox17.PerformLayout();[m[41m[m
[32m+[m[32m            this.groupBox15.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.groupBox15.PerformLayout();[m[41m[m
[32m+[m[32m            this.tabPage9.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.tabPage9.PerformLayout();[m[41m[m
[32m+[m[32m            ((System.ComponentModel.ISupportInitialize)(this.switchScanTTLSwitch)).EndInit();[m[41m[m
[32m+[m[32m            this.tabPage7.ResumeLayout(false);[m[41m[m
[32m+[m[32m            this.tabPage7.PerformLayout();[m[41m[m
             this.tabPage10.ResumeLayout(false);[m
             this.groupBox19.ResumeLayout(false);[m
             this.groupBox19.PerformLayout();[m
[36m@@ -5907,15 +6751,9 @@[m [mnamespace EDMHardwareControl[m
             ((System.ComponentModel.ISupportInitialize)(this.I2ErrorSigGraph)).EndInit();[m
             this.menuStrip1.ResumeLayout(false);[m
             this.menuStrip1.PerformLayout();[m
[31m-           // this.tabPage12.ResumeLayout(false);[m
[31m-           // this.groupBox121.ResumeLayout(false);[m
[31m-          //  this.groupBox121.PerformLayout();[m
             this.ResumeLayout(false);[m
             this.PerformLayout();[m
 [m
[31m-[m
[31m-          [m
[31m-[m
 		}[m
 		#endregion[m
 [m
[36m@@ -6501,6 +7339,175 @@[m [mnamespace EDMHardwareControl[m
             controller.UpdatePumpAOMFreqMonitor();[m
         }[m
 [m
[32m+[m[32m        private void groupBox39_Enter(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO161AmpTrackBar_Scroll(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.UpdateVCO161AmpVoltage((Double)VCO161AmpTrackBar.Value / 100.0);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO161FreqTrackBar_Scroll(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.UpdateVCO161FreqVoltage((Double)VCO161FreqTrackBar.Value / 100.0);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO30AmpTrackBar_Scroll(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.UpdateVCO30AmpVoltage((Double)VCO30AmpTrackBar.Value / 100.0);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO30FreqTrackBar_Scroll(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.UpdateVCO30FreqVoltage((Double)VCO30FreqTrackBar.Value / 100.0);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO155AmpTrackBar_Scroll(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.UpdateVCO155AmpVoltage((Double)VCO155AmpTrackBar.Value / 100.0);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO155FreqTrackBar_Scroll(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.UpdateVCO155FreqVoltage((Double)VCO155FreqTrackBar.Value / 100.0);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO161UpdateButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.UpdateVCO161AmpV();[m[41m[m
[32m+[m[32m            controller.UpdateVCO161FreqV();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO30UpdateButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.UpdateVCO30AmpV();[m[41m[m
[32m+[m[32m            controller.UpdateVCO30FreqV();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO155UpdateButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.UpdateVCO155AmpV();[m[41m[m
[32m+[m[32m            controller.UpdateVCO155FreqV();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO161StepPlusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.IncreaseVCOVoltage();[m[41m[m
[32m+[m[32m            controller.TweakVCO161AmpV();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO161StepMinusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.DecreaseVCOVoltage();[m[41m[m
[32m+[m[32m            controller.TweakVCO161AmpV();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO161FreqStepPlusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.IncreaseVCOVoltage();[m[41m[m
[32m+[m[32m            controller.TweakVCO161FreqV();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private void VCO161FreqStepMinusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.DecreaseVCOVoltage();[m[41m[m
[32m+[m[32m            controller.TweakVCO161FreqV();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m       private void VCO30AmpStepPlusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.IncreaseVCOVoltage();[m[41m[m
[32m+[m[32m            controller.TweakVCO30AmpV();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m       private void VCO30AmpStepMinusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.DecreaseVCOVoltage();[m[41m[m
[32m+[m[32m           controller.TweakVCO30AmpV();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m       private void VCO30FreqStepPlusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.IncreaseVCOVoltage();[m[41m[m
[32m+[m[32m           controller.TweakVCO30FreqV();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m       private void VCO30FreqStepMinusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.DecreaseVCOVoltage();[m[41m[m
[32m+[m[32m           controller.TweakVCO30FreqV();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m       private void VCO155AmpStepPlusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.IncreaseVCOVoltage();[m[41m[m
[32m+[m[32m           controller.TweakVCO155AmpV();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m       private void VCO155AmpStepMinusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.DecreaseVCOVoltage();[m[41m[m
[32m+[m[32m           controller.TweakVCO155AmpV();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m       private void VCO155FreqStepPlusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.IncreaseVCOVoltage();[m[41m[m
[32m+[m[32m           controller.TweakVCO155FreqV();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m       private void VCO155FreqStepMinusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.DecreaseVCOVoltage();[m[41m[m
[32m+[m[32m           controller.TweakVCO155FreqV();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m         private void uWaveUpdateButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.UpdateuWaveDCFMV();[m[41m[m
[32m+[m[32m           controller.UpdateuWaveMixerV();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m         private void uWaveDCFMTrackBar_Scroll(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.UpdateuWaveDCFMVoltage((Double)uWaveDCFMTrackBar.Value / 100.0);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m         private void mixerVoltageTrackBar_Scroll(object sender, EventArgs e)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            controller.UpdateuWaveMixerVoltage((Double)mixerVoltageTrackBar.Value / 100.0);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m           private void uWaveDCFMPlusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.IncreaseuWaveVoltage();[m[41m[m
[32m+[m[32m           controller.TweakuWaveDCFMVoltage();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m       private void uWaveDCFMMinusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.DecreaseuWaveVoltage();[m[41m[m
[32m+[m[32m           controller.TweakuWaveDCFMVoltage();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m       private void mixerVoltagePlusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.IncreaseuWaveVoltage();[m[41m[m
[32m+[m[32m           controller.TweakMixerVoltage();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m       private void mixerVoltageMinusButton_Click(object sender, EventArgs e)[m[41m[m
[32m+[m[32m       {[m[41m[m
[32m+[m[32m           controller.DecreaseuWaveVoltage();[m[41m[m
[32m+[m[32m           controller.TweakMixerVoltage();[m[41m[m
[32m+[m[32m       }[m[41m[m
[32m+[m[41m[m
[32m+[m[41m[m
[32m+[m[41m        [m
[32m+[m[41m[m
 [m
     }[m
 }[m
[41m+       [m
\ No newline at end of file[m
[1mdiff --git a/EDMHardwareControl/Controller.cs b/EDMHardwareControl/Controller.cs[m
[1mindex 98037b2..ab9c870 100644[m
[1m--- a/EDMHardwareControl/Controller.cs[m
[1m+++ b/EDMHardwareControl/Controller.cs[m
[36m@@ -117,7 +117,14 @@[m [mnamespace EDMHardwareControl[m
         Task fibreAmpOutputTask;[m
         Task i2ErrorSignalInputTask;[m
         Task i2BiasOutputTask;[m
[31m-      //  Task uWaveDCFMOutputTask;[m
[32m+[m[32m        Task uWaveDCFMAnalogOutputTask;[m[41m[m
[32m+[m[32m        Task uWaveMixerAnalogOutputTask;[m[41m[m
[32m+[m[32m        Task VCO161AmpAnalogOutputTask;[m[41m[m
[32m+[m[32m        Task VCO161FreqAnalogOutputTask;[m[41m[m
[32m+[m[32m        Task VCO30AmpAnalogOutputTask;[m[41m[m
[32m+[m[32m        Task VCO30FreqAnalogOutputTask;[m[41m[m
[32m+[m[32m        Task VCO155AmpAnalogOutputTask;[m[41m[m
[32m+[m[32m        Task VCO155FreqAnalogOutputTask;[m[41m[m
 [m
         ControlWindow window;[m
 [m
[36m@@ -196,7 +203,14 @@[m [mnamespace EDMHardwareControl[m
             fibreAmpOutputTask = CreateAnalogOutputTask("fibreAmpPwr");[m
             //flAOMAnalogOutputTask = CreateAnalogOutputTask("fibreAOM");[m
             i2BiasOutputTask = CreateAnalogOutputTask("I2LockBias");[m
[31m-       //     uWaveDCFMOutputTask = CreateAnalogOutputTask("uWaveDCFM");[m
[32m+[m[32m            uWaveDCFMAnalogOutputTask = CreateAnalogOutputTask("uWaveDCFM");[m[41m[m
[32m+[m[32m            uWaveMixerAnalogOutputTask = CreateAnalogOutputTask("uWaveMixerV");[m[41m[m
[32m+[m[32m            VCO161AmpAnalogOutputTask = CreateAnalogOutputTask("VCO161Amp");[m[41m[m
[32m+[m[32m            VCO161FreqAnalogOutputTask = CreateAnalogOutputTask("VCO161Freq");[m[41m[m
[32m+[m[32m            VCO30AmpAnalogOutputTask = CreateAnalogOutputTask("VCO30Amp");[m[41m[m
[32m+[m[32m            VCO30FreqAnalogOutputTask = CreateAnalogOutputTask("VCO30Freq");[m[41m[m
[32m+[m[32m            VCO155AmpAnalogOutputTask = CreateAnalogOutputTask("VCO155Amp");[m[41m[m
[32m+[m[32m            VCO155FreqAnalogOutputTask = CreateAnalogOutputTask("VCO155Freq");[m[41m[m
             [m
 [m
             // analog inputs[m
[36m@@ -671,6 +685,11 @@[m [mnamespace EDMHardwareControl[m
             window.SetCheckBox(window.eBleedCheck, enabled);[m
         }[m
 [m
[32m+[m[32m        public void ChangePolarity(bool polarity)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            window.SetCheckBox(window.ePolarityCheck, polarity);[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
         public double CPlusVoltage[m
         {[m
             get[m
[36m@@ -1221,6 +1240,197 @@[m [mnamespace EDMHardwareControl[m
 [m
         }[m
 [m
[32m+[m[32m        public double VCO161AmpVoltage[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO161AmpVoltageTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO161AmpVoltageTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double VCO161FreqVoltage[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO161FreqTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO161FreqTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double VCO30AmpVoltage[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO30AmpVoltageTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO30AmpVoltageTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double VCO30FreqVoltage[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO30FreqVoltageTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO30FreqVoltageTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double VCO155AmpVoltage[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO155AmpVoltageTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO155AmpVoltageTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double VCO155FreqVoltage[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO155FreqVoltageTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO155FreqVoltageTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double VCO161AmpIncrement[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO161AmpStepTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO161AmpStepTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double VCO161FreqIncrement[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO161FreqStepTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO161FreqStepTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double VCO30AmpIncrement[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO30AmpStepTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO30AmpStepTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double VCO30FreqIncrement[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO30FreqStepTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO30FreqStepTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        public double VCO155AmpIncrement[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO155AmpStepTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO155AmpStepTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double VCO155FreqIncrement[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.VCO155FreqStepTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.VCO155FreqStepTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double uWaveDCFMVoltage[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.uWaveDCFMTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.uWaveDCFMTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double uWaveMixerVoltage[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.mixerVoltageTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.mixerVoltageTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double uWaveDCFMIncrement[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.uWaveDCFMStepTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.uWaveDCFMStepTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public double uWaveMixerIncrement[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            get[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                return Double.Parse(window.stepMixerVoltageTextBox.Text);[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m            set[m[41m[m
[32m+[m[32m            {[m[41m[m
[32m+[m[32m                window.SetTextBox(window.stepMixerVoltageTextBox, value.ToString());[m[41m[m
[32m+[m[32m            }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
 [m
         #endregion[m
 [m
[36m@@ -1331,6 +1541,16 @@[m [mnamespace EDMHardwareControl[m
         private double cPlusMonitorVoltage;[m
         private double cMinusMonitorVoltage;[m
 [m
[32m+[m[32m        private double hpVoltage;[m[41m[m
[32m+[m[32m        public double HPVoltage[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m         get[m[41m[m
[32m+[m[32m         {[m[41m[m
[32m+[m[32m             return hpVoltage;[m[41m[m
[32m+[m[32m         }[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[41m[m
         public double CPlusMonitorVoltage[m
         {[m
             get[m
[36m@@ -2010,6 +2230,12 @@[m [mnamespace EDMHardwareControl[m
             return rawReading;[m
         }[m
         */[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateBVoltage()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            hpVoltage=bCurrentMeter.ReadVoltage();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
         public void UpdateBCurrentMonitor()[m
         {[m
             // DB0 dB0[m
[36m@@ -3181,6 +3407,237 @@[m [mnamespace EDMHardwareControl[m
             return;[m
         }[m
 [m
[32m+[m[32m        public void UpdateVCO161AmpVoltage(double pztVoltage)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO161AmpAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO161AmpVoltageTextBox.Text = pztVoltage.ToString();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateVCO161FreqVoltage(double pztVoltage)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO161FreqAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO161FreqTextBox.Text = pztVoltage.ToString();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateVCO30AmpVoltage(double pztVoltage)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO30AmpAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO30AmpVoltageTextBox.Text = pztVoltage.ToString();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateVCO30FreqVoltage(double pztVoltage)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO30FreqAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO30FreqVoltageTextBox.Text = pztVoltage.ToString();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateVCO155AmpVoltage(double pztVoltage)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO155AmpAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO155AmpVoltageTextBox.Text = pztVoltage.ToString();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateVCO155FreqVoltage(double pztVoltage)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO155FreqAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO155FreqVoltageTextBox.Text = pztVoltage.ToString();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateVCO161AmpV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO161AmpVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO161AmpAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO161AmpTrackBar.Value = 100 * (int)pztVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateVCO161FreqV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO161FreqVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO161FreqAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO161FreqTrackBar.Value = 100 * (int)pztVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateVCO30AmpV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO30AmpVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO30AmpAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO30AmpTrackBar.Value = 100 * (int)pztVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateVCO30FreqV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO30FreqVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO30FreqAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO30FreqTrackBar.Value = 100 * (int)pztVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateVCO155AmpV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO155AmpVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO155AmpAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO155AmpTrackBar.Value = 100 * (int)pztVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateVCO155FreqV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO155FreqVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO155FreqAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.VCO155FreqTrackBar.Value = 100 * (int)pztVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        private int plusVoltage = 0;//For tweaking voltages in hardware controller[m[41m[m
[32m+[m[32m        private int minusVoltage = 0;[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void IncreaseVCOVoltage()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            plusVoltage++;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void DecreaseVCOVoltage()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            minusVoltage++;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void TweakVCO161AmpV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO161AmpVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            double newPZTVoltage = pztVoltage + VCO161AmpIncrement * (plusVoltage - minusVoltage);[m[41m[m
[32m+[m[32m            plusVoltage = 0;[m[41m[m
[32m+[m[32m            minusVoltage = 0;[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO161AmpAnalogOutputTask, newPZTVoltage);[m[41m[m
[32m+[m[32m            window.VCO161AmpVoltageTextBox.Text = newPZTVoltage.ToString();[m[41m[m
[32m+[m[32m            window.VCO161AmpTrackBar.Value = 100 * (int)newPZTVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void TweakVCO161FreqV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO161FreqVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            double newPZTVoltage = pztVoltage + VCO161FreqIncrement * (plusVoltage - minusVoltage);[m[41m[m
[32m+[m[32m            plusVoltage = 0;[m[41m[m
[32m+[m[32m            minusVoltage = 0;[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO161FreqAnalogOutputTask, newPZTVoltage);[m[41m[m
[32m+[m[32m            window.VCO161FreqTextBox.Text = newPZTVoltage.ToString();[m[41m[m
[32m+[m[32m            window.VCO161FreqTrackBar.Value = 100 * (int)newPZTVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void TweakVCO30AmpV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO30AmpVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            double newPZTVoltage = pztVoltage + VCO30AmpIncrement * (plusVoltage - minusVoltage);[m[41m[m
[32m+[m[32m            plusVoltage = 0;[m[41m[m
[32m+[m[32m            minusVoltage = 0;[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO30AmpAnalogOutputTask, newPZTVoltage);[m[41m[m
[32m+[m[32m            window.VCO30AmpVoltageTextBox.Text = newPZTVoltage.ToString();[m[41m[m
[32m+[m[32m            window.VCO30AmpTrackBar.Value = 100 * (int)newPZTVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void TweakVCO30FreqV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO30FreqVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            double newPZTVoltage = pztVoltage + VCO30FreqIncrement * (plusVoltage - minusVoltage);[m[41m[m
[32m+[m[32m            plusVoltage = 0;[m[41m[m
[32m+[m[32m            minusVoltage = 0;[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO30FreqAnalogOutputTask, newPZTVoltage);[m[41m[m
[32m+[m[32m            window.VCO30FreqVoltageTextBox.Text = newPZTVoltage.ToString();[m[41m[m
[32m+[m[32m            window.VCO30FreqTrackBar.Value = 100 * (int)newPZTVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[32m        public void TweakVCO155AmpV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO155AmpVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            double newPZTVoltage = pztVoltage + VCO155AmpIncrement * (plusVoltage - minusVoltage);[m[41m[m
[32m+[m[32m            plusVoltage = 0;[m[41m[m
[32m+[m[32m            minusVoltage = 0;[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO155AmpAnalogOutputTask, newPZTVoltage);[m[41m[m
[32m+[m[32m            window.VCO155AmpVoltageTextBox.Text = newPZTVoltage.ToString();[m[41m[m
[32m+[m[32m            window.VCO155AmpTrackBar.Value = 100 * (int)newPZTVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void TweakVCO155FreqV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = VCO155FreqVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            double newPZTVoltage = pztVoltage + VCO155FreqIncrement * (plusVoltage - minusVoltage);[m[41m[m
[32m+[m[32m            plusVoltage = 0;[m[41m[m
[32m+[m[32m            minusVoltage = 0;[m[41m[m
[32m+[m[32m            SetAnalogOutput(VCO155FreqAnalogOutputTask, newPZTVoltage);[m[41m[m
[32m+[m[32m            window.VCO155FreqVoltageTextBox.Text = newPZTVoltage.ToString();[m[41m[m
[32m+[m[32m            window.VCO155FreqTrackBar.Value = 100 * (int)newPZTVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateuWaveDCFMV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double voltage = uWaveDCFMVoltage;[m[41m[m
[32m+[m[32m            voltage = windowVoltage(voltage, -2.5, 2.5);[m[41m[m
[32m+[m[32m            SetAnalogOutput(uWaveDCFMAnalogOutputTask, voltage);[m[41m[m
[32m+[m[32m            window.uWaveDCFMTrackBar.Value = 100 * (int)voltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateuWaveMixerV()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double voltage = uWaveMixerVoltage;[m[41m[m
[32m+[m[32m            voltage = windowVoltage(voltage, 0, 10);[m[41m[m
[32m+[m[32m            SetAnalogOutput(uWaveMixerAnalogOutputTask, voltage);[m[41m[m
[32m+[m[32m            window.mixerVoltageTrackBar.Value = 100 * (int)voltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateuWaveDCFMVoltage(double pztVoltage)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            SetAnalogOutput(uWaveDCFMAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.uWaveDCFMTextBox.Text = pztVoltage.ToString();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void UpdateuWaveMixerVoltage(double pztVoltage)[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            SetAnalogOutput(uWaveMixerAnalogOutputTask, pztVoltage);[m[41m[m
[32m+[m[32m            window.mixerVoltageTextBox.Text = pztVoltage.ToString();[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void IncreaseuWaveVoltage()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            plusVoltage++;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void DecreaseuWaveVoltage()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            minusVoltage++;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void TweakuWaveDCFMVoltage()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = uWaveDCFMVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, -2.5, 2.5);[m[41m[m
[32m+[m[32m            double newPZTVoltage = pztVoltage + uWaveDCFMIncrement * (plusVoltage - minusVoltage);[m[41m[m
[32m+[m[32m            plusVoltage = 0;[m[41m[m
[32m+[m[32m            minusVoltage = 0;[m[41m[m
[32m+[m[32m            SetAnalogOutput(uWaveDCFMAnalogOutputTask, newPZTVoltage);[m[41m[m
[32m+[m[32m            window.uWaveDCFMTextBox.Text = newPZTVoltage.ToString();[m[41m[m
[32m+[m[32m            window.uWaveDCFMTrackBar.Value = 100 * (int)newPZTVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
[32m+[m[32m        public void TweakMixerVoltage()[m[41m[m
[32m+[m[32m        {[m[41m[m
[32m+[m[32m            double pztVoltage = uWaveMixerVoltage;[m[41m[m
[32m+[m[32m            pztVoltage = windowVoltage(pztVoltage, 0, 10);[m[41m[m
[32m+[m[32m            double newPZTVoltage = pztVoltage + uWaveMixerIncrement * (plusVoltage - minusVoltage);[m[41m[m
[32m+[m[32m            plusVoltage = 0;[m[41m[m
[32m+[m[32m            minusVoltage = 0;[m[41m[m
[32m+[m[32m            SetAnalogOutput(uWaveMixerAnalogOutputTask, newPZTVoltage);[m[41m[m
[32m+[m[32m            window.mixerVoltageTextBox.Text = newPZTVoltage.ToString();[m[41m[m
[32m+[m[32m            window.mixerVoltageTrackBar.Value = 100 * (int)newPZTVoltage;[m[41m[m
[32m+[m[32m        }[m[41m[m
[32m+[m[41m[m
         #endregion[m
 [m
     }[m
[1mdiff --git a/EDMHardwareControl/EDMHardwareControl.csproj b/EDMHardwareControl/EDMHardwareControl.csproj[m
[1mindex 2dae1f0..bdbce80 100644[m
[1m--- a/EDMHardwareControl/EDMHardwareControl.csproj[m
[1m+++ b/EDMHardwareControl/EDMHardwareControl.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">[m
[32m+[m[32m<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="12.0">[m[41m[m
   <PropertyGroup>[m
     <ProjectType>Local</ProjectType>[m
     <ProductVersion>9.0.21022</ProductVersion>[m
[36m@@ -28,7 +28,7 @@[m
     <SignManifests>false</SignManifests>[m
     <OldToolsVersion>3.5</OldToolsVersion>[m
     <ApplicationRevision>0</ApplicationRevision>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <TargetFrameworkProfile />[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Decelerator|AnyCPU' ">[m
[36m@@ -40,6 +40,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -50,9 +51,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -63,6 +66,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -73,6 +77,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -83,6 +88,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -93,22 +99,22 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
     <ProjectReference Include="..\SharedCode\SharedCode.csproj">[m
       <Project>{BA0A0540-3F1C-483B-A180-CB78DF424F15}</Project>[m
       <Name>SharedCode</Name>[m
     </ProjectReference>[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.DAQmx, Version=9.6.40.57, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
[31m-      <Private>False</Private>[m
[31m-    </Reference>[m
[31m-    <Reference Include="NationalInstruments.UI, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.UI.WindowsForms, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.VisaNS, Version=13.0.40.167, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.DAQmx, Version=15.5.45.109, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=x86" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI.WindowsForms, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.VisaNS, Version=13.0.45.167, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="System">[m
       <Name>System</Name>[m
     </Reference>[m
[1mdiff --git a/EDMHardwareControl/Properties/Resources.Designer.cs b/EDMHardwareControl/Properties/Resources.Designer.cs[m
[1mindex fea52cb..58c9491 100644[m
[1m--- a/EDMHardwareControl/Properties/Resources.Designer.cs[m
[1m+++ b/EDMHardwareControl/Properties/Resources.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.261[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/EDMHardwareControl/app.config b/EDMHardwareControl/app.config[m
[1mindex 045aad1..bc0f896 100644[m
[1m--- a/EDMHardwareControl/app.config[m
[1m+++ b/EDMHardwareControl/app.config[m
[36m@@ -13,4 +13,4 @@[m
 			</dependentAssembly>[m
 		</assemblyBinding>[m
 	</runtime>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup></configuration>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup></configuration>[m[41m[m
[1mdiff --git a/EDMPhaseLock/EDMPhaseLock.csproj b/EDMPhaseLock/EDMPhaseLock.csproj[m
[1mindex 0f11247..514275e 100644[m
[1m--- a/EDMPhaseLock/EDMPhaseLock.csproj[m
[1m+++ b/EDMPhaseLock/EDMPhaseLock.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">[m
[32m+[m[32m<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="12.0">[m[41m[m
   <PropertyGroup>[m
     <ProjectType>Local</ProjectType>[m
     <ProductVersion>9.0.21022</ProductVersion>[m
[36m@@ -26,7 +26,7 @@[m
     <UpgradeBackupLocation>[m
     </UpgradeBackupLocation>[m
     <OldToolsVersion>3.5</OldToolsVersion>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <TargetFrameworkProfile />[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Decelerator|AnyCPU' ">[m
[36m@@ -38,6 +38,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -48,9 +49,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -61,6 +64,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -71,6 +75,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -81,6 +86,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -91,18 +97,18 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.DAQmx, Version=9.6.40.57, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
[31m-      <Private>False</Private>[m
[31m-    </Reference>[m
[31m-    <Reference Include="NationalInstruments.UI, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.UI.WindowsForms, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.VisaNS, Version=13.0.40.167, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.DAQmx, Version=15.5.45.109, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=x86" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI.WindowsForms, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.VisaNS, Version=13.0.45.167, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="System">[m
       <Name>System</Name>[m
     </Reference>[m
[1mdiff --git a/EDMPhaseLock/app.config b/EDMPhaseLock/app.config[m
[1mindex 22d831c..833f5db 100644[m
[1m--- a/EDMPhaseLock/app.config[m
[1m+++ b/EDMPhaseLock/app.config[m
[36m@@ -13,4 +13,4 @@[m
 			</dependentAssembly>[m
 		</assemblyBinding>[m
 	</runtime>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup></configuration>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup></configuration>[m[41m[m
[1mdiff --git a/EDMSuite.sln b/EDMSuite.sln[m
[1mindex 33dc9e0..4507b46 100644[m
[1m--- a/EDMSuite.sln[m
[1m+++ b/EDMSuite.sln[m
[36m@@ -1,5 +1,7 @@[m
[31m-Microsoft Visual Studio Solution File, Format Version 11.00[m
[31m-# Visual Studio 2010[m
[32m+[m[32mMicrosoft Visual Studio Solution File, Format Version 12.00[m[41m[m
[32m+[m[32m# Visual Studio 2013[m[41m[m
[32m+[m[32mVisualStudioVersion = 12.0.21005.1[m[41m[m
[32m+[m[32mMinimumVisualStudioVersion = 10.0.40219.1[m[41m[m
 Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ScanMaster", "ScanMaster\ScanMaster.csproj", "{373F31F0-A7B3-4EDA-BFAC-8F9E948F6D40}"[m
 EndProject[m
 Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "DAQ", "DAQ\DAQ.csproj", "{BB737B99-2E9F-40C9-9809-895A7C51AD40}"[m
[36m@@ -106,8 +108,8 @@[m [mGlobal[m
 		{373F31F0-A7B3-4EDA-BFAC-8F9E948F6D40}.EDM|Any CPU.ActiveCfg = EDM|Any CPU[m
 		{373F31F0-A7B3-4EDA-BFAC-8F9E948F6D40}.EDM|Any CPU.Build.0 = EDM|Any CPU[m
 		{373F31F0-A7B3-4EDA-BFAC-8F9E948F6D40}.EDM|Mixed Platforms.ActiveCfg = EDM|Any CPU[m
[31m-		{373F31F0-A7B3-4EDA-BFAC-8F9E948F6D40}.EDM|Mixed Platforms.Build.0 = EDM|Any CPU[m
 		{373F31F0-A7B3-4EDA-BFAC-8F9E948F6D40}.EDM|x86.ActiveCfg = EDM|Any CPU[m
[32m+[m		[32m{373F31F0-A7B3-4EDA-BFAC-8F9E948F6D40}.EDM|x86.Build.0 = EDM|Any CPU[m[41m[m
 		{373F31F0-A7B3-4EDA-BFAC-8F9E948F6D40}.EDMAnalysis|Any CPU.ActiveCfg = EDMAnalysis|Any CPU[m
 		{373F31F0-A7B3-4EDA-BFAC-8F9E948F6D40}.EDMAnalysis|Mixed Platforms.ActiveCfg = EDMAnalysis|Any CPU[m
 		{373F31F0-A7B3-4EDA-BFAC-8F9E948F6D40}.EDMAnalysis|x86.ActiveCfg = EDMAnalysis|Any CPU[m
[36m@@ -160,7 +162,9 @@[m [mGlobal[m
 		{BB737B99-2E9F-40C9-9809-895A7C51AD40}.EDM|Any CPU.Build.0 = EDM|Any CPU[m
 		{BB737B99-2E9F-40C9-9809-895A7C51AD40}.EDM|Mixed Platforms.ActiveCfg = EDM|Any CPU[m
 		{BB737B99-2E9F-40C9-9809-895A7C51AD40}.EDM|Mixed Platforms.Build.0 = EDM|Any CPU[m
[32m+[m		[32m{BB737B99-2E9F-40C9-9809-895A7C51AD40}.EDM|Mixed Platforms.Deploy.0 = EDM|Any CPU[m[41m[m
 		{BB737B99-2E9F-40C9-9809-895A7C51AD40}.EDM|x86.ActiveCfg = EDM|Any CPU[m
[32m+[m		[32m{BB737B99-2E9F-40C9-9809-895A7C51AD40}.EDM|x86.Build.0 = EDM|Any CPU[m[41m[m
 		{BB737B99-2E9F-40C9-9809-895A7C51AD40}.EDMAnalysis|Any CPU.ActiveCfg = EDMAnalysis|Any CPU[m
 		{BB737B99-2E9F-40C9-9809-895A7C51AD40}.EDMAnalysis|Mixed Platforms.ActiveCfg = EDMAnalysis|Any CPU[m
 		{BB737B99-2E9F-40C9-9809-895A7C51AD40}.EDMAnalysis|x86.ActiveCfg = EDMAnalysis|Any CPU[m
[36m@@ -212,8 +216,8 @@[m [mGlobal[m
 		{BA0A0540-3F1C-483B-A180-CB78DF424F15}.EDM|Any CPU.ActiveCfg = EDM|Any CPU[m
 		{BA0A0540-3F1C-483B-A180-CB78DF424F15}.EDM|Any CPU.Build.0 = EDM|Any CPU[m
 		{BA0A0540-3F1C-483B-A180-CB78DF424F15}.EDM|Mixed Platforms.ActiveCfg = EDM|Any CPU[m
[31m-		{BA0A0540-3F1C-483B-A180-CB78DF424F15}.EDM|Mixed Platforms.Build.0 = EDM|Any CPU[m
 		{BA0A0540-3F1C-483B-A180-CB78DF424F15}.EDM|x86.ActiveCfg = EDM|Any CPU[m
[32m+[m		[32m{BA0A0540-3F1C-483B-A180-CB78DF424F15}.EDM|x86.Build.0 = EDM|Any CPU[m[41m[m
 		{BA0A0540-3F1C-483B-A180-CB78DF424F15}.EDMAnalysis|Any CPU.ActiveCfg = EDMAnalysis|Any CPU[m
 		{BA0A0540-3F1C-483B-A180-CB78DF424F15}.EDMAnalysis|Any CPU.Build.0 = EDMAnalysis|Any CPU[m
 		{BA0A0540-3F1C-483B-A180-CB78DF424F15}.EDMAnalysis|Mixed Platforms.ActiveCfg = EDMAnalysis|Any CPU[m
[36m@@ -253,12 +257,13 @@[m [mGlobal[m
 		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.Debug|x86.ActiveCfg = Decelerator|Any CPU[m
 		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.Decelerator|Any CPU.ActiveCfg = Decelerator|Any CPU[m
 		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.Decelerator|Mixed Platforms.ActiveCfg = Decelerator|Any CPU[m
[32m+[m		[32m{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.Decelerator|Mixed Platforms.Build.0 = Decelerator|Any CPU[m[41m[m
 		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.Decelerator|x86.ActiveCfg = Decelerator|Any CPU[m
 		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.EDM|Any CPU.ActiveCfg = EDM|Any CPU[m
 		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.EDM|Any CPU.Build.0 = EDM|Any CPU[m
 		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.EDM|Mixed Platforms.ActiveCfg = EDM|Any CPU[m
[31m-		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.EDM|Mixed Platforms.Build.0 = EDM|Any CPU[m
 		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.EDM|x86.ActiveCfg = EDM|Any CPU[m
[32m+[m		[32m{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.EDM|x86.Build.0 = EDM|Any CPU[m[41m[m
 		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.EDMAnalysis|Any CPU.ActiveCfg = EDMAnalysis|Any CPU[m
 		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.EDMAnalysis|Mixed Platforms.ActiveCfg = EDMAnalysis|Any CPU[m
 		{DFE81EE7-F9AF-4551-B3D7-336F14101AB6}.EDMAnalysis|x86.ActiveCfg = EDMAnalysis|Any CPU[m
[36m@@ -297,7 +302,6 @@[m [mGlobal[m
 		{5B795C2A-3D76-4F42-94DF-1E823ED7AB90}.Decelerator|Any CPU.ActiveCfg = Decelerator|Any CPU[m
 		{5B795C2A-3D76-4F42-94DF-1E823ED7AB90}.Decelerator|Any CPU.Build.0 = Decelerator|Any CPU[m
 		{5B795C2A-3D76-4F42-94DF-1E823ED7AB90}.Decelerator|Mixed Platforms.ActiveCfg = Decelerator|Any CPU[m
[31m-		{5B795C2A-3D76-4F42-94DF-1E823ED7AB90}.Decelerator|Mixed Platforms.Build.0 = Decelerator|Any CPU[m
 		{5B795C2A-3D76-4F42-94DF-1E823ED7AB90}.Decelerator|x86.ActiveCfg = Decelerator|Any CPU[m
 		{5B795C2A-3D76-4F42-94DF-1E823ED7AB90}.EDM|Any CPU.ActiveCfg = EDM|Any CPU[m
 		{5B795C2A-3D76-4F42-94DF-1E823ED7AB90}.EDM|Mixed Platforms.ActiveCfg = EDM|Any CPU[m
[36m@@ -379,12 +383,13 @@[m [mGlobal[m
 		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.Debug|x86.ActiveCfg = Decelerator|Any CPU[m
 		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.Decelerator|Any CPU.ActiveCfg = Decelerator|Any CPU[m
 		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.Decelerator|Mixed Platforms.ActiveCfg = Decelerator|Any CPU[m
[32m+[m		[32m{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.Decelerator|Mixed Platforms.Build.0 = Decelerator|Any CPU[m[41m[m
 		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.Decelerator|x86.ActiveCfg = Decelerator|Any CPU[m
 		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.EDM|Any CPU.ActiveCfg = EDM|Any CPU[m
 		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.EDM|Any CPU.Build.0 = EDM|Any CPU[m
 		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.EDM|Mixed Platforms.ActiveCfg = EDM|Any CPU[m
[31m-		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.EDM|Mixed Platforms.Build.0 = EDM|Any CPU[m
 		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.EDM|x86.ActiveCfg = EDM|Any CPU[m
[32m+[m		[32m{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.EDM|x86.Build.0 = EDM|Any CPU[m[41m[m
 		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.EDMAnalysis|Any CPU.ActiveCfg = EDMAnalysis|Any CPU[m
 		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.EDMAnalysis|Mixed Platforms.ActiveCfg = EDMAnalysis|Any CPU[m
 		{DE347012-B8E0-4A43-A37F-5F30D0F3CBE1}.EDMAnalysis|x86.ActiveCfg = EDMAnalysis|Any CPU[m
[36m@@ -422,12 +427,13 @@[m [mGlobal[m
 		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.Debug|x86.ActiveCfg = Decelerator|Any CPU[m
 		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.Decelerator|Any CPU.ActiveCfg = Decelerator|Any CPU[m
 		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.Decelerator|Mixed Platforms.ActiveCfg = Decelerator|Any CPU[m
[32m+[m		[32m{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.Decelerator|Mixed Platforms.Build.0 = Decelerator|Any CPU[m[41m[m
 		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.Decelerator|x86.ActiveCfg = Decelerator|Any CPU[m
 		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.EDM|Any CPU.ActiveCfg = EDM|Any CPU[m
 		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.EDM|Any CPU.Build.0 = EDM|Any CPU[m
 		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.EDM|Mixed Platforms.ActiveCfg = EDM|Any CPU[m
[31m-		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.EDM|Mixed Platforms.Build.0 = EDM|Any CPU[m
 		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.EDM|x86.ActiveCfg = EDM|Any CPU[m
[32m+[m		[32m{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.EDM|x86.Build.0 = EDM|Any CPU[m[41m[m
 		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.EDMAnalysis|Any CPU.ActiveCfg = EDMAnalysis|Any CPU[m
 		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.EDMAnalysis|Mixed Platforms.ActiveCfg = EDMAnalysis|Any CPU[m
 		{216E6D83-0304-4A0C-9BA7-F9DDF58F274A}.EDMAnalysis|x86.ActiveCfg = EDMAnalysis|Any CPU[m
[36m@@ -549,6 +555,7 @@[m [mGlobal[m
 		{F91026E7-ED05-403E-923B-686806A0DF4F}.EDM|Any CPU.ActiveCfg = EDM|Any CPU[m
 		{F91026E7-ED05-403E-923B-686806A0DF4F}.EDM|Mixed Platforms.ActiveCfg = EDM|Any CPU[m
 		{F91026E7-ED05-403E-923B-686806A0DF4F}.EDM|x86.ActiveCfg = EDM|Any CPU[m
[32m+[m		[32m{F91026E7-ED05-403E-923B-686806A0DF4F}.EDM|x86.Build.0 = EDM|Any CPU[m[41m[m
 		{F91026E7-ED05-403E-923B-686806A0DF4F}.EDMAnalysis|Any CPU.ActiveCfg = EDMAnalysis|Any CPU[m
 		{F91026E7-ED05-403E-923B-686806A0DF4F}.EDMAnalysis|Mixed Platforms.ActiveCfg = EDMAnalysis|Any CPU[m
 		{F91026E7-ED05-403E-923B-686806A0DF4F}.EDMAnalysis|x86.ActiveCfg = EDMAnalysis|Any CPU[m
[36m@@ -635,8 +642,8 @@[m [mGlobal[m
 		{F1BF43A3-5641-4ADF-81CB-35BC0D9AC423}.EDM|Any CPU.ActiveCfg = EDM|Any CPU[m
 		{F1BF43A3-5641-4ADF-81CB-35BC0D9AC423}.EDM|Any CPU.Build.0 = EDM|Any CPU[m
 		{F1BF43A3-5641-4ADF-81CB-35BC0D9AC423}.EDM|Mixed Platforms.ActiveCfg = EDM|Any CPU[m
[31m-		{F1BF43A3-5641-4ADF-81CB-35BC0D9AC423}.EDM|Mixed Platforms.Build.0 = EDM|Any CPU[m
 		{F1BF43A3-5641-4ADF-81CB-35BC0D9AC423}.EDM|x86.ActiveCfg = EDM|Any CPU[m
[32m+[m		[32m{F1BF43A3-5641-4ADF-81CB-35BC0D9AC423}.EDM|x86.Build.0 = EDM|Any CPU[m[41m[m
 		{F1BF43A3-5641-4ADF-81CB-35BC0D9AC423}.EDMAnalysis|Any CPU.ActiveCfg = EDMAnalysis|Any CPU[m
 		{F1BF43A3-5641-4ADF-81CB-35BC0D9AC423}.EDMAnalysis|Mixed Platforms.ActiveCfg = EDMAnalysis|Any CPU[m
 		{F1BF43A3-5641-4ADF-81CB-35BC0D9AC423}.EDMAnalysis|x86.ActiveCfg = EDMAnalysis|Any CPU[m
[1mdiff --git a/IMAQ/IMAQ.csproj b/IMAQ/IMAQ.csproj[m
[1mindex e5dfc56..dd9e631 100644[m
[1m--- a/IMAQ/IMAQ.csproj[m
[1m+++ b/IMAQ/IMAQ.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m
[32m+[m[32m<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m[41m[m
   <PropertyGroup>[m
     <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>[m
     <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>[m
[36m@@ -10,7 +10,7 @@[m
     <AppDesignerFolder>Properties</AppDesignerFolder>[m
     <RootNamespace>IMAQ</RootNamespace>[m
     <AssemblyName>IMAQ</AssemblyName>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <FileAlignment>512</FileAlignment>[m
     <FileUpgradeFlags>[m
     </FileUpgradeFlags>[m
[36m@@ -41,6 +41,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -49,9 +50,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -60,6 +63,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -68,6 +72,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -76,6 +81,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -84,13 +90,15 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
     <Reference Include="NationalInstruments.AxCWIMAQControlsLib.Interop, Version=9.0.0.0, Culture=neutral, PublicKeyToken=4544464cdeaab541" />[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="NationalInstruments.UI, Version=9.0.40.292, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL">[m
       <SpecificVersion>False</SpecificVersion>[m
       <HintPath>C:\Program Files (x86)\National Instruments\MeasurementStudioVS2010\DotNET\Assemblies\Current\NationalInstruments.UI.dll</HintPath>[m
[1mdiff --git a/MOTMaster/MOTMaster.csproj b/MOTMaster/MOTMaster.csproj[m
[1mindex 61a7f3d..d183279 100644[m
[1m--- a/MOTMaster/MOTMaster.csproj[m
[1m+++ b/MOTMaster/MOTMaster.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m
[32m+[m[32m<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m[41m[m
   <PropertyGroup>[m
     <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>[m
     <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>[m
[36m@@ -10,7 +10,7 @@[m
     <AppDesignerFolder>Properties</AppDesignerFolder>[m
     <RootNamespace>MOTMaster</RootNamespace>[m
     <AssemblyName>MOTMaster</AssemblyName>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <FileAlignment>512</FileAlignment>[m
     <FileUpgradeFlags>[m
     </FileUpgradeFlags>[m
[36m@@ -41,6 +41,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -49,9 +50,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -60,6 +63,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -68,6 +72,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -76,6 +81,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup>[m
     <StartupObject>MOTMaster.Runner</StartupObject>[m
[36m@@ -90,19 +96,19 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
     <Reference Include="ICSharpCode.SharpZipLib, Version=0.85.4.369, Culture=neutral, PublicKeyToken=1b03e6acf1164f73, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.DAQmx, Version=9.6.40.57, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
[31m-      <Private>False</Private>[m
[31m-    </Reference>[m
[31m-    <Reference Include="NationalInstruments.Net, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86" />[m
[31m-    <Reference Include="NationalInstruments.UI, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.UI.WindowsForms, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.DAQmx, Version=15.5.45.109, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=x86" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.Net, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=x86" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI.WindowsForms, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="PresentationCore">[m
       <RequiredTargetFramework>3.0</RequiredTargetFramework>[m
     </Reference>[m
[1mdiff --git a/MOTMaster/Properties/Resources.Designer.cs b/MOTMaster/Properties/Resources.Designer.cs[m
[1mindex f2f4e0b..227c5a0 100644[m
[1m--- a/MOTMaster/Properties/Resources.Designer.cs[m
[1m+++ b/MOTMaster/Properties/Resources.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.239[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/MOTMaster/Properties/Settings.Designer.cs b/MOTMaster/Properties/Settings.Designer.cs[m
[1mindex dbbd670..2f24928 100644[m
[1m--- a/MOTMaster/Properties/Settings.Designer.cs[m
[1m+++ b/MOTMaster/Properties/Settings.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.239[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[36m@@ -12,7 +12,7 @@[m [mnamespace MOTMaster.Properties {[m
     [m
     [m
     [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()][m
[31m-    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")][m
[32m+[m[32m    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")][m[41m[m
     internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {[m
         [m
         private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));[m
[1mdiff --git a/MOTMaster/app.config b/MOTMaster/app.config[m
[1mindex 297c7dd..6b710ed 100644[m
[1m--- a/MOTMaster/app.config[m
[1m+++ b/MOTMaster/app.config[m
[36m@@ -1,6 +1,6 @@[m
 <?xml version="1.0"?>[m
 <configuration>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup>	<runtime>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup>	<runtime>[m[41m[m
 		<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">[m
 			<dependentAssembly>[m
 				<assemblyIdentity name="NationalInstruments.Common.Native" publicKeyToken="18CBAE0F9955702A" culture="neutral"/>[m
[1mdiff --git a/ScanMaster/ScanMaster.csproj b/ScanMaster/ScanMaster.csproj[m
[1mindex 3d751db..781513c 100644[m
[1m--- a/ScanMaster/ScanMaster.csproj[m
[1m+++ b/ScanMaster/ScanMaster.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">[m
[32m+[m[32m<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="12.0">[m[41m[m
   <PropertyGroup>[m
     <ProjectType>Local</ProjectType>[m
     <ProductVersion>9.0.21022</ProductVersion>[m
[36m@@ -41,7 +41,7 @@[m
     <IsWebBootstrapper>false</IsWebBootstrapper>[m
     <UseApplicationTrust>false</UseApplicationTrust>[m
     <BootstrapperEnabled>true</BootstrapperEnabled>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <TargetFrameworkProfile />[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Decelerator|AnyCPU' ">[m
[36m@@ -53,6 +53,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -63,9 +64,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -76,6 +79,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -86,6 +90,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -96,6 +101,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -108,9 +114,11 @@[m
     <ErrorReport>prompt</ErrorReport>[m
     <CodeAnalysisIgnoreBuiltInRules>false</CodeAnalysisIgnoreBuiltInRules>[m
     <CodeAnalysisFailOnMissingRules>false</CodeAnalysisFailOnMissingRules>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
     <ProjectReference Include="..\TransferCavityLock2012\TransferCavityLock2012.csproj">[m
[36m@@ -120,7 +128,7 @@[m
     <Reference Include="alglibnet2">[m
       <HintPath>.\alglibnet2.dll</HintPath>[m
     </Reference>[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="NationalInstruments.DAQmx, Version=9.2.40.82, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
       <SpecificVersion>False</SpecificVersion>[m
       <HintPath>..\..\..\..\..\..\..\..\Program Files (x86)\National Instruments\MeasurementStudioVS2010\DotNET\Assemblies\Current\NationalInstruments.DAQmx.dll</HintPath>[m
[1mdiff --git a/ScanMaster/StandardViewerWindow.cs b/ScanMaster/StandardViewerWindow.cs[m
[1mindex 7388798..9dbe1ef 100644[m
[1m--- a/ScanMaster/StandardViewerWindow.cs[m
[1m+++ b/ScanMaster/StandardViewerWindow.cs[m
[36m@@ -509,7 +509,7 @@[m [mnamespace ScanMaster.GUI[m
             this.tofFitResultsLabel.ForeColor = System.Drawing.Color.Blue;[m
             this.tofFitResultsLabel.Location = new System.Drawing.Point(260, 602);[m
             this.tofFitResultsLabel.Name = "tofFitResultsLabel";[m
[31m-            this.tofFitResultsLabel.Size = new System.Drawing.Size(100, 24);[m
[32m+[m[32m            this.tofFitResultsLabel.Size = new System.Drawing.Size(103, 49);[m[41m[m
             this.tofFitResultsLabel.TabIndex = 23;[m
             this.tofFitResultsLabel.Text = "...";[m
             this.tofFitResultsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;[m
[36m@@ -558,7 +558,7 @@[m [mnamespace ScanMaster.GUI[m
             // splitContainer1[m
             // [m
             this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Bottom;[m
[31m-            this.splitContainer1.Location = new System.Drawing.Point(0, 636);[m
[32m+[m[32m            this.splitContainer1.Location = new System.Drawing.Point(0, 669);[m[41m[m
             this.splitContainer1.Name = "splitContainer1";[m
             // [m
             // splitContainer1.Panel1[m
[36m@@ -568,7 +568,7 @@[m [mnamespace ScanMaster.GUI[m
             // splitContainer1.Panel2[m
             // [m
             this.splitContainer1.Panel2.Controls.Add(this.statusBar1);[m
[31m-            this.splitContainer1.Size = new System.Drawing.Size(970, 23);[m
[32m+[m[32m            this.splitContainer1.Size = new System.Drawing.Size(971, 23);[m[41m[m
             this.splitContainer1.SplitterDistance = 371;[m
             this.splitContainer1.TabIndex = 30;[m
             // [m
[36m@@ -612,7 +612,7 @@[m [mnamespace ScanMaster.GUI[m
             // [m
             this.statusBar1.Location = new System.Drawing.Point(0, 0);[m
             this.statusBar1.Name = "statusBar1";[m
[31m-            this.statusBar1.Size = new System.Drawing.Size(595, 23);[m
[32m+[m[32m            this.statusBar1.Size = new System.Drawing.Size(596, 23);[m[41m[m
             this.statusBar1.SizingGrip = false;[m
             this.statusBar1.TabIndex = 14;[m
             this.statusBar1.Text = "Ready";[m
[36m@@ -649,7 +649,7 @@[m [mnamespace ScanMaster.GUI[m
             // StandardViewerWindow[m
             // [m
             this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);[m
[31m-            this.ClientSize = new System.Drawing.Size(970, 659);[m
[32m+[m[32m            this.ClientSize = new System.Drawing.Size(971, 692);[m[41m[m
             this.Controls.Add(this.noiseResultsLabel);[m
             this.Controls.Add(this.updateNoiseResultsbutton);[m
             this.Controls.Add(this.label3);[m
[1mdiff --git a/ScanMaster/app.config b/ScanMaster/app.config[m
[1mindex 969b367..c12dd33 100644[m
[1m--- a/ScanMaster/app.config[m
[1m+++ b/ScanMaster/app.config[m
[36m@@ -17,4 +17,4 @@[m
 			</dependentAssembly>[m
 		</assemblyBinding>[m
 	</runtime>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup></configuration>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup></configuration>[m[41m[m
[1mdiff --git a/SharedCode/SharedCode.csproj b/SharedCode/SharedCode.csproj[m
[1mindex 582e43e..85d9ec8 100644[m
[1m--- a/SharedCode/SharedCode.csproj[m
[1m+++ b/SharedCode/SharedCode.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">[m
[32m+[m[32m<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="12.0">[m[41m[m
   <PropertyGroup>[m
     <ProjectType>Local</ProjectType>[m
     <ProductVersion>9.0.21022</ProductVersion>[m
[36m@@ -28,7 +28,7 @@[m
     <UpgradeBackupLocation>[m
     </UpgradeBackupLocation>[m
     <OldToolsVersion>3.5</OldToolsVersion>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <TargetFrameworkProfile />[m
     <PublishUrl>publish\</PublishUrl>[m
     <Install>true</Install>[m
[36m@@ -55,6 +55,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>AnyCPU</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -65,12 +66,14 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>AnyCPU</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
     <DefineConstants>DEBUG</DefineConstants>[m
     <DebugType>full</DebugType>[m
     <DebugSymbols>true</DebugSymbols>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -81,6 +84,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>AnyCPU</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -91,6 +95,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>AnyCPU</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -101,6 +106,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>AnyCPU</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -111,9 +117,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>AnyCPU</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
     <Reference Include="ICSharpCode.SharpZipLib">[m
[1mdiff --git a/SirCachealot/Properties/Resources.Designer.cs b/SirCachealot/Properties/Resources.Designer.cs[m
[1mindex 6fde3f2..7546a4d 100644[m
[1m--- a/SirCachealot/Properties/Resources.Designer.cs[m
[1m+++ b/SirCachealot/Properties/Resources.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.235[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/SirCachealot/Properties/Settings.Designer.cs b/SirCachealot/Properties/Settings.Designer.cs[m
[1mindex 775fe9e..2569bf6 100644[m
[1m--- a/SirCachealot/Properties/Settings.Designer.cs[m
[1m+++ b/SirCachealot/Properties/Settings.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.235[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[36m@@ -12,7 +12,7 @@[m [mnamespace SirCachealot.Properties {[m
     [m
     [m
     [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()][m
[31m-    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")][m
[32m+[m[32m    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")][m[41m[m
     internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {[m
         [m
         private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));[m
[1mdiff --git a/SirCachealot/SirCachealot.csproj b/SirCachealot/SirCachealot.csproj[m
[1mindex 06136e0..59cffc0 100644[m
[1m--- a/SirCachealot/SirCachealot.csproj[m
[1m+++ b/SirCachealot/SirCachealot.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">[m
[32m+[m[32m<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="12.0">[m[41m[m
   <PropertyGroup>[m
     <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>[m
     <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>[m
[36m@@ -17,7 +17,7 @@[m
     <UpgradeBackupLocation>[m
     </UpgradeBackupLocation>[m
     <ApplicationIcon>App.ico</ApplicationIcon>[m
[31m-    <TargetFrameworkVersion>v3.5</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <TargetFrameworkProfile />[m
     <PublishUrl>publish\</PublishUrl>[m
     <Install>true</Install>[m
[36m@@ -42,6 +42,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -50,12 +51,14 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
     <DefineConstants>DEBUG</DefineConstants>[m
     <DebugType>full</DebugType>[m
     <DebugSymbols>true</DebugSymbols>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -64,6 +67,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -72,6 +76,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -80,6 +85,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -88,9 +94,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
     <Reference Include="MongoDB.Bson, Version=1.0.0.4098, Culture=neutral, PublicKeyToken=f686731cfb9cc103, processorArchitecture=MSIL">[m
[1mdiff --git a/SirCachealot/app.config b/SirCachealot/app.config[m
[1mindex cb2586b..b7a7ef1 100644[m
[1m--- a/SirCachealot/app.config[m
[1m+++ b/SirCachealot/app.config[m
[36m@@ -1,3 +1,3 @@[m
 <?xml version="1.0"?>[m
 <configuration>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup></configuration>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup></configuration>[m[41m[m
[1mdiff --git a/SonOfSirCachealot/BlockDatabase.designer.cs b/SonOfSirCachealot/BlockDatabase.designer.cs[m
[1mindex 5b038fb..b1f0e06 100644[m
[1m--- a/SonOfSirCachealot/BlockDatabase.designer.cs[m
[1m+++ b/SonOfSirCachealot/BlockDatabase.designer.cs[m
[36m@@ -2,7 +2,7 @@[m
 //------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.235[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/SonOfSirCachealot/Properties/Resources.Designer.cs b/SonOfSirCachealot/Properties/Resources.Designer.cs[m
[1mindex dc000e8..616fb13 100644[m
[1m--- a/SonOfSirCachealot/Properties/Resources.Designer.cs[m
[1m+++ b/SonOfSirCachealot/Properties/Resources.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.225[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/SonOfSirCachealot/Properties/Settings.Designer.cs b/SonOfSirCachealot/Properties/Settings.Designer.cs[m
[1mindex bd466a1..e064614 100644[m
[1m--- a/SonOfSirCachealot/Properties/Settings.Designer.cs[m
[1m+++ b/SonOfSirCachealot/Properties/Settings.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.225[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[36m@@ -12,7 +12,7 @@[m [mnamespace SonOfSirCachealot.Properties {[m
     [m
     [m
     [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()][m
[31m-    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")][m
[32m+[m[32m    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")][m[41m[m
     internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {[m
         [m
         private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));[m
[1mdiff --git a/SonOfSirCachealot/SonOfSirCachealot.csproj b/SonOfSirCachealot/SonOfSirCachealot.csproj[m
[1mindex f712a07..8ffd5c8 100644[m
[1m--- a/SonOfSirCachealot/SonOfSirCachealot.csproj[m
[1m+++ b/SonOfSirCachealot/SonOfSirCachealot.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">[m
[32m+[m[32m<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="12.0">[m[41m[m
   <PropertyGroup>[m
     <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>[m
     <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>[m
[36m@@ -18,7 +18,7 @@[m
     </UpgradeBackupLocation>[m
     <ApplicationIcon>[m
     </ApplicationIcon>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <TargetFrameworkProfile />[m
     <PublishUrl>publish\</PublishUrl>[m
     <Install>true</Install>[m
[36m@@ -43,6 +43,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -51,12 +52,14 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
     <DefineConstants>DEBUG</DefineConstants>[m
     <DebugType>full</DebugType>[m
     <DebugSymbols>true</DebugSymbols>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -65,6 +68,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -73,6 +77,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -81,6 +86,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -89,9 +95,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
     <Reference Include="MongoDB.Bson, Version=1.0.0.4098, Culture=neutral, PublicKeyToken=f686731cfb9cc103, processorArchitecture=MSIL">[m
[36m@@ -130,6 +138,7 @@[m
     <Compile Include="Properties\AssemblyInfo.cs" />[m
     <Compile Include="Runner.cs" />[m
     <Compile Include="ThreadMonitor.cs" />[m
[32m+[m[32m    <None Include="app.config" />[m[41m[m
     <None Include="BlockDatabase.dbml.layout">[m
       <DependentUpon>BlockDatabase.dbml</DependentUpon>[m
     </None>[m
[1mdiff --git a/SonOfSirCachealot/app.config b/SonOfSirCachealot/app.config[m
[1mindex abe130c..7209cb3 100644[m
[1m--- a/SonOfSirCachealot/app.config[m
[1m+++ b/SonOfSirCachealot/app.config[m
[36m@@ -3,8 +3,6 @@[m
 <configSections>[m
 </configSections>[m
 <connectionStrings>[m
[31m-    <add name="SirCachealot.Properties.Settings.EDMDatabaseConnectionString"[m
[31m-        connectionString="Data Source=.\sqlexpress;Initial Catalog=EDMDatabase;Integrated Security=True"[m
[31m-        providerName="System.Data.SqlClient" />[m
[32m+[m[32m    <add name="SirCachealot.Properties.Settings.EDMDatabaseConnectionString" connectionString="Data Source=.\sqlexpress;Initial Catalog=EDMDatabase;Integrated Security=True" providerName="System.Data.SqlClient"/>[m[41m[m
 </connectionStrings>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup></configuration>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup></configuration>[m[41m[m
[1mdiff --git a/SympatheticHardwareControl/Properties/Resources.Designer.cs b/SympatheticHardwareControl/Properties/Resources.Designer.cs[m
[1mindex 33f9644..a526e13 100644[m
[1m--- a/SympatheticHardwareControl/Properties/Resources.Designer.cs[m
[1m+++ b/SympatheticHardwareControl/Properties/Resources.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.225[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/SympatheticHardwareControl/Properties/Settings.Designer.cs b/SympatheticHardwareControl/Properties/Settings.Designer.cs[m
[1mindex b0ba935..c10a201 100644[m
[1m--- a/SympatheticHardwareControl/Properties/Settings.Designer.cs[m
[1m+++ b/SympatheticHardwareControl/Properties/Settings.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.225[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[36m@@ -12,7 +12,7 @@[m [mnamespace SympatheticHardwareControl.Properties {[m
     [m
     [m
     [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()][m
[31m-    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")][m
[32m+[m[32m    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")][m[41m[m
     internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {[m
         [m
         private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));[m
[1mdiff --git a/SympatheticHardwareControl/SympatheticHardwareControl.csproj b/SympatheticHardwareControl/SympatheticHardwareControl.csproj[m
[1mindex 9c51377..ec40ede 100644[m
[1m--- a/SympatheticHardwareControl/SympatheticHardwareControl.csproj[m
[1m+++ b/SympatheticHardwareControl/SympatheticHardwareControl.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="4.0">[m
[32m+[m[32m<Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003" ToolsVersion="12.0">[m[41m[m
   <PropertyGroup>[m
     <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>[m
     <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>[m
[36m@@ -15,7 +15,7 @@[m
     <OldToolsVersion>3.5</OldToolsVersion>[m
     <UpgradeBackupLocation>[m
     </UpgradeBackupLocation>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <TargetFrameworkProfile />[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Decelerator|AnyCPU' ">[m
[36m@@ -25,6 +25,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -33,9 +34,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -44,6 +47,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -52,6 +56,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -60,6 +65,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup>[m
     <ApplicationIcon>SHC.ico</ApplicationIcon>[m
[36m@@ -71,19 +77,17 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
     <Reference Include="Microsoft.VisualBasic" />[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.DAQmx, Version=9.6.40.57, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
[31m-      <Private>False</Private>[m
[31m-    </Reference>[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="NationalInstruments.UI, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
     <Reference Include="NationalInstruments.UI.WindowsForms, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.VisaNS, Version=13.0.40.167, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
     <Reference Include="System" />[m
     <Reference Include="System.Core">[m
       <RequiredTargetFramework>3.5</RequiredTargetFramework>[m
[1mdiff --git a/SympatheticHardwareControl/app.config b/SympatheticHardwareControl/app.config[m
[1mindex 8ef6a97..a7acf4d 100644[m
[1m--- a/SympatheticHardwareControl/app.config[m
[1m+++ b/SympatheticHardwareControl/app.config[m
[36m@@ -1,6 +1,6 @@[m
 <?xml version="1.0"?>[m
 <configuration>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup>	<runtime>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup>	<runtime>[m[41m[m
 		<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">[m
 			<dependentAssembly>[m
 				<assemblyIdentity name="NationalInstruments.Vision.Common" publicKeyToken="18CBAE0F9955702A" culture="neutral"/>[m
[1mdiff --git a/SympatheticMOTMasterScripts/SympatheticMOTMasterScripts.csproj b/SympatheticMOTMasterScripts/SympatheticMOTMasterScripts.csproj[m
[1mindex fcb1896..7b80e0a 100644[m
[1m--- a/SympatheticMOTMasterScripts/SympatheticMOTMasterScripts.csproj[m
[1m+++ b/SympatheticMOTMasterScripts/SympatheticMOTMasterScripts.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m
[32m+[m[32m<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m[41m[m
   <PropertyGroup>[m
     <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>[m
     <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>[m
[36m@@ -10,7 +10,7 @@[m
     <AppDesignerFolder>Properties</AppDesignerFolder>[m
     <RootNamespace>SympatheticMOTMasterScripts</RootNamespace>[m
     <AssemblyName>SympatheticMOTMasterScripts</AssemblyName>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <FileAlignment>512</FileAlignment>[m
     <FileUpgradeFlags>[m
     </FileUpgradeFlags>[m
[36m@@ -41,6 +41,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -49,9 +50,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -60,6 +63,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -68,6 +72,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -76,6 +81,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup>[m
     <StartupObject />[m
[36m@@ -87,9 +93,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />[m
   <!-- To modify your build process, add your task inside one of the targets below and uncomment it. [m
[1mdiff --git a/SympatheticMOTMasterScripts/app.config b/SympatheticMOTMasterScripts/app.config[m
[1mindex cb2586b..b7a7ef1 100644[m
[1m--- a/SympatheticMOTMasterScripts/app.config[m
[1m+++ b/SympatheticMOTMasterScripts/app.config[m
[36m@@ -1,3 +1,3 @@[m
 <?xml version="1.0"?>[m
 <configuration>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup></configuration>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup></configuration>[m[41m[m
[1mdiff --git a/TransferCavityLock/Properties/Resources.Designer.cs b/TransferCavityLock/Properties/Resources.Designer.cs[m
[1mindex e1f2d78..2f65160 100644[m
[1m--- a/TransferCavityLock/Properties/Resources.Designer.cs[m
[1m+++ b/TransferCavityLock/Properties/Resources.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.225[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/TransferCavityLock/Properties/Settings.Designer.cs b/TransferCavityLock/Properties/Settings.Designer.cs[m
[1mindex 70bf1f5..006b7c4 100644[m
[1m--- a/TransferCavityLock/Properties/Settings.Designer.cs[m
[1m+++ b/TransferCavityLock/Properties/Settings.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.225[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[36m@@ -12,7 +12,7 @@[m [mnamespace TransferCavityLock.Properties {[m
     [m
     [m
     [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()][m
[31m-    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")][m
[32m+[m[32m    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")][m[41m[m
     internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {[m
         [m
         private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));[m
[1mdiff --git a/TransferCavityLock/TransferCavityLock.csproj b/TransferCavityLock/TransferCavityLock.csproj[m
[1mindex 4622040..06ecd6b 100644[m
[1m--- a/TransferCavityLock/TransferCavityLock.csproj[m
[1m+++ b/TransferCavityLock/TransferCavityLock.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m
[32m+[m[32m<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m[41m[m
   <PropertyGroup>[m
     <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>[m
     <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>[m
[36m@@ -10,7 +10,7 @@[m
     <AppDesignerFolder>Properties</AppDesignerFolder>[m
     <RootNamespace>TransferCavityLock</RootNamespace>[m
     <AssemblyName>TransferCavityLock</AssemblyName>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <FileAlignment>512</FileAlignment>[m
     <FileUpgradeFlags>[m
     </FileUpgradeFlags>[m
[36m@@ -25,6 +25,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -33,9 +34,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -44,6 +47,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -52,6 +56,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -62,6 +67,7 @@[m
     <DefineConstants>DEBUG;TRACE</DefineConstants>[m
     <ErrorReport>prompt</ErrorReport>[m
     <WarningLevel>4</WarningLevel>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -70,18 +76,20 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="NationalInstruments.DAQmx, Version=9.2.40.82, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
       <SpecificVersion>False</SpecificVersion>[m
       <HintPath>..\..\..\..\..\..\..\..\Program Files (x86)\National Instruments\MeasurementStudioVS2010\DotNET\Assemblies\Current\NationalInstruments.DAQmx.dll</HintPath>[m
     </Reference>[m
[31m-    <Reference Include="NationalInstruments.UI, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.UI.WindowsForms, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI.WindowsForms, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="System" />[m
     <Reference Include="System.Runtime.Remoting" />[m
     <Reference Include="System.Data" />[m
[1mdiff --git a/TransferCavityLock/app.config b/TransferCavityLock/app.config[m
[1mindex 8ef6a97..a7acf4d 100644[m
[1m--- a/TransferCavityLock/app.config[m
[1m+++ b/TransferCavityLock/app.config[m
[36m@@ -1,6 +1,6 @@[m
 <?xml version="1.0"?>[m
 <configuration>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup>	<runtime>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup>	<runtime>[m[41m[m
 		<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">[m
 			<dependentAssembly>[m
 				<assemblyIdentity name="NationalInstruments.Vision.Common" publicKeyToken="18CBAE0F9955702A" culture="neutral"/>[m
[1mdiff --git a/TransferCavityLock2012/Controller.cs b/TransferCavityLock2012/Controller.cs[m
[1mindex 66f2e06..ad47858 100644[m
[1m--- a/TransferCavityLock2012/Controller.cs[m
[1m+++ b/TransferCavityLock2012/Controller.cs[m
[36m@@ -1,6 +1,7 @@[m
 ﻿using System;[m
 using System.Threading;[m
 using NationalInstruments.DAQmx;[m
[32m+[m[32m//using NationalInstruments.Common;[m[41m[m
 using DAQ.TransferCavityLock2012;[m
 using DAQ.Environment;[m
 using DAQ.HAL;[m
[1mdiff --git a/TransferCavityLock2012/LockControlPanel.cs b/TransferCavityLock2012/LockControlPanel.cs[m
[1mindex d5a3e8d..e2e2a85 100644[m
[1m--- a/TransferCavityLock2012/LockControlPanel.cs[m
[1m+++ b/TransferCavityLock2012/LockControlPanel.cs[m
[36m@@ -233,14 +233,17 @@[m [mnamespace TransferCavityLock2012[m
 [m
          public void AppendToErrorGraph(double[] x, double[] y)[m
         {[m
[32m+[m[41m[m
             double cf = Double.Parse(fsrTextBox.Text);[m
[31m-            double[] ylist=y;[m
[31m-            foreach (int i in y) [m
[32m+[m[32m            double[] ylist = y;[m[41m[m
[32m+[m[32m            int length = ylist.Length;[m[41m[m
[32m+[m[32m            for (int i = 0; i < length; i++)[m[41m[m
             {[m
[31m-                ylist[i] = 1500* y[i]/cf ;[m
[32m+[m[32m                ylist[i] = 1500 * y[i] / cf;[m[41m[m
             };[m
             PlotXYAppend(ErrorScatterGraph, ErrorPlot, x, ylist);[m
[31m-        }[m
[32m+[m[41m[m
[32m+[m[32m         }[m[41m[m
 [m
          public void ClearErrorGraph()[m
          {[m
[1mdiff --git a/TransferCavityLock2012/MainForm.cs b/TransferCavityLock2012/MainForm.cs[m
[1mindex 62a09b9..061ae22 100644[m
[1m--- a/TransferCavityLock2012/MainForm.cs[m
[1m+++ b/TransferCavityLock2012/MainForm.cs[m
[36m@@ -354,11 +354,11 @@[m [mnamespace TransferCavityLock2012[m
 [m
         }[m
 [m
[31m-[m
[32m+[m[41m[m
         private void voltageRampControl_Enter(object sender, EventArgs e)[m
         {[m
 [m
[31m-        } [m
[32m+[m[32m        }[m[41m [m
        [m
     }[m
 }[m
[1mdiff --git a/TransferCavityLock2012/Properties/Resources.Designer.cs b/TransferCavityLock2012/Properties/Resources.Designer.cs[m
[1mindex 710a8e2..2f65160 100644[m
[1m--- a/TransferCavityLock2012/Properties/Resources.Designer.cs[m
[1m+++ b/TransferCavityLock2012/Properties/Resources.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.18444[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[1mdiff --git a/TransferCavityLock2012/Properties/Settings.Designer.cs b/TransferCavityLock2012/Properties/Settings.Designer.cs[m
[1mindex fd5fb38..006b7c4 100644[m
[1m--- a/TransferCavityLock2012/Properties/Settings.Designer.cs[m
[1m+++ b/TransferCavityLock2012/Properties/Settings.Designer.cs[m
[36m@@ -1,7 +1,7 @@[m
 ﻿//------------------------------------------------------------------------------[m
 // <auto-generated>[m
 //     This code was generated by a tool.[m
[31m-//     Runtime Version:4.0.30319.18444[m
[32m+[m[32m//     Runtime Version:4.0.30319.42000[m[41m[m
 //[m
 //     Changes to this file may cause incorrect behavior and will be lost if[m
 //     the code is regenerated.[m
[36m@@ -12,7 +12,7 @@[m [mnamespace TransferCavityLock.Properties {[m
     [m
     [m
     [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()][m
[31m-    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")][m
[32m+[m[32m    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")][m[41m[m
     internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {[m
         [m
         private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));[m
[1mdiff --git a/TransferCavityLock2012/SlaveLaser.cs b/TransferCavityLock2012/SlaveLaser.cs[m
[1mindex 55afedf..835534e 100644[m
[1m--- a/TransferCavityLock2012/SlaveLaser.cs[m
[1m+++ b/TransferCavityLock2012/SlaveLaser.cs[m
[36m@@ -5,7 +5,6 @@[m [musing System.Text;[m
 using DAQ.Environment;[m
 using DAQ.TransferCavityLock2012;[m
 using NationalInstruments.DAQmx;[m
[31m-using DAQ.Environment;[m
 using DAQ.HAL;[m
 [m
 namespace TransferCavityLock2012[m
[1mdiff --git a/TransferCavityLock2012/TransferCavityLock2012.csproj b/TransferCavityLock2012/TransferCavityLock2012.csproj[m
[1mindex 3db028f..a14a248 100644[m
[1m--- a/TransferCavityLock2012/TransferCavityLock2012.csproj[m
[1m+++ b/TransferCavityLock2012/TransferCavityLock2012.csproj[m
[36m@@ -1,5 +1,5 @@[m
 ﻿<?xml version="1.0" encoding="utf-8"?>[m
[31m-<Project ToolsVersion="4.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m
[32m+[m[32m<Project ToolsVersion="12.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">[m[41m[m
   <PropertyGroup>[m
     <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>[m
     <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>[m
[36m@@ -10,7 +10,7 @@[m
     <AppDesignerFolder>Properties</AppDesignerFolder>[m
     <RootNamespace>TransferCavityLock</RootNamespace>[m
     <AssemblyName>TransferCavityLock</AssemblyName>[m
[31m-    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>[m
[32m+[m[32m    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>[m[41m[m
     <FileAlignment>512</FileAlignment>[m
     <FileUpgradeFlags>[m
     </FileUpgradeFlags>[m
[36m@@ -25,6 +25,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDM|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -33,9 +34,11 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'EDMAnalysis|AnyCPU' ">[m
     <OutputPath>bin\EDMAnalysis\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Buffer|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -44,6 +47,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Sympathetic|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -52,6 +56,7 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'SrF|AnyCPU' ">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -62,6 +67,7 @@[m
     <DefineConstants>DEBUG;TRACE</DefineConstants>[m
     <ErrorReport>prompt</ErrorReport>[m
     <WarningLevel>4</WarningLevel>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'BufferClassic|AnyCPU'">[m
     <DebugSymbols>true</DebugSymbols>[m
[36m@@ -70,18 +76,23 @@[m
     <DebugType>full</DebugType>[m
     <PlatformTarget>x86</PlatformTarget>[m
     <ErrorReport>prompt</ErrorReport>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <PropertyGroup Condition="'$(Configuration)|$(Platform)' == 'CaF|AnyCPU'">[m
     <OutputPath>bin\CaF\</OutputPath>[m
[32m+[m[32m    <Prefer32Bit>false</Prefer32Bit>[m[41m[m
   </PropertyGroup>[m
   <ItemGroup>[m
[31m-    <Reference Include="NationalInstruments.Common, Version=13.0.40.190, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.DAQmx, Version=9.2.40.82, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=x86">[m
[32m+[m[32m    <Reference Include="NationalInstruments.Common, Version=15.1.40.49152, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL">[m[41m[m
       <SpecificVersion>False</SpecificVersion>[m
[31m-      <HintPath>..\..\..\..\..\..\..\..\Program Files (x86)\National Instruments\MeasurementStudioVS2010\DotNET\Assemblies\Current\NationalInstruments.DAQmx.dll</HintPath>[m
[32m+[m[32m      <HintPath>C:\Program Files (x86)\National Instruments\MeasurementStudioVS2012\DotNET\Assemblies\Current\NationalInstruments.Common.dll</HintPath>[m[41m[m
     </Reference>[m
[31m-    <Reference Include="NationalInstruments.UI, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[31m-    <Reference Include="NationalInstruments.UI.WindowsForms, Version=9.1.40.204, Culture=neutral, PublicKeyToken=dc6ad606294fc298, processorArchitecture=MSIL" />[m
[32m+[m[32m    <Reference Include="NationalInstruments.DAQmx, Version=15.5.45.109, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=x86">[m[41m[m
[32m+[m[32m      <SpecificVersion>False</SpecificVersion>[m[41m[m
[32m+[m[32m      <HintPath>C:\Program Files (x86)\National Instruments\MeasurementStudioVS2012\DotNET\Assemblies\Current\NationalInstruments.DAQmx.dll</HintPath>[m[41m[m
[32m+[m[32m    </Reference>[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
[32m+[m[32m    <Reference Include="NationalInstruments.UI.WindowsForms, Version=15.0.45.49153, Culture=neutral, PublicKeyToken=4febd62461bf11a4, processorArchitecture=MSIL" />[m[41m[m
     <Reference Include="System" />[m
     <Reference Include="System.Runtime.Remoting" />[m
     <Reference Include="System.Data" />[m
[1mdiff --git a/TransferCavityLock2012/app.config b/TransferCavityLock2012/app.config[m
[1mindex 64f6d18..7c5d6ee 100644[m
[1m--- a/TransferCavityLock2012/app.config[m
[1m+++ b/TransferCavityLock2012/app.config[m
[36m@@ -1,6 +1,6 @@[m
 <?xml version="1.0"?>[m
 <configuration>[m
[31m-<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup>	<runtime>[m
[32m+[m[32m<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5"/></startup>	<runtime>[m[41m[m
 		<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">[m
 			<dependentAssembly>[m
 				<assemblyIdentity name="NationalInstruments.Common.Native" publicKeyToken="DC6AD606294FC298" culture="neutral"/>[m
