using System;
using System.Collections;
using System.Collections.Generic;

using System.Xml.Serialization;

using DAQ.Environment;
using DAQ.Pattern;

using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;


namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin for generating the pattern via MOTMaster
    /// 
    /// </summary>
    [Serializable]
    public class MMPatternPlugin : ScanMaster.Acquire.Plugin.PatternPlugin
    {
        [NonSerialized]
        MOTMaster.Controller mmc;

        protected override void InitialiseSettings()
        {
            
        }

        public override void ReloadPattern()
        {
            
        }

        public override void AcquisitionStarting()
        {
            mmc = (MOTMaster.Controller)Activator.GetObject(typeof(MOTMaster.Controller),
               "tcp://localhost:1187/controller.rem");
            mmc.SetRunUntilStopped(true);
            Dictionary<String, Object> dict = new Dictionary<string,object>();
            dict.Add("PMTTrigger", (int)config.shotGathererPlugin.Settings["gateStartTime"]);
            
            mmc.Run(dict);
        }
        public override void ScanStarting()
        {

        }
        public override void ScanFinished()
        {

        }
        public override void AcquisitionFinished()
        {
            mmc.Stop();
        }
    }
}

