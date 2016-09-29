using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Pattern;
using DAQ.Analog;
using DAQ;

namespace MOTMaster.SnippetLibrary
{
    class PIDController
    {

        //The idea here is to make the MOTMaster script more readable by having the PIDController sequence built by a separate class.
        //Then we can set the state of the controller object in the MOTMaster Script using the enumerable controllerState.
        //There will be three digital lines controlling the PID controller. 

        public enum controllerState {AWAIT_RECAPTURE,IDLE };

        private PatternBuilder32 pattern;

        private Dictionary<string, bool> idleStateSettings;

        private Dictionary<string, bool> awaitRecaptureStateSettings;

        private Dictionary<controllerState, Dictionary<string, bool>> stateSettings;

        private void addEdges(controllerState state, int time)
        {
            foreach (string channel in stateSettings[state].Keys)
            {
                pattern.AddEdge(channel, time, stateSettings[state][channel]);
            }
        }

        public void invertErrorSignal(int time)
        {
            pattern.AddEdge("", time,true);
        }

        public PIDController(PatternBuilder32 p)
        {
            pattern = p;
            initialiseDictionaries();
        }

        private void initialiseDictionaries()
        {
            stateSettings = new Dictionary<controllerState, Dictionary<string, bool>>();

            #region idle state
            idleStateSettings = new Dictionary<string, bool>();
            idleStateSettings.Add("",true);
            #endregion

            #region
            awaitRecaptureStateSettings = new Dictionary<string,bool>();
            awaitRecaptureStateSettings.Add("",true);
            #endregion

            stateSettings[controllerState.IDLE] = idleStateSettings;
            stateSettings[controllerState.AWAIT_RECAPTURE] = awaitRecaptureStateSettings;
        }
       
    }
}
