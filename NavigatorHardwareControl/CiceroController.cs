using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using DataStructures;
using DAQ.Environment;

namespace NavigatorHardwareControl
{
    /// <summary>
    /// This class acts as an iterface to control Cicero from the Navigator hardware controller
    /// </summary>
    public class CiceroController
    {
        public SequenceData sequenceData;
        public SettingsData settingsData;
        public Controller controller;
        public bool ciceroConnected;
        public List<Variable> ciceroVariables;
        public List<int> listNums;
        public CiceroController()
        {
            listNums = new List<int>();
            for (int i=1;i<11;i++)
            {
                listNums.Add(i);
            }
        }

        public void ConnectToCicero()
        {
            sequenceData = (SequenceData)Activator.GetObject(typeof(SequenceData), "tcp://localhost:1337/sequence.rem");
            settingsData = (SettingsData)Activator.GetObject(typeof(SettingsData), "tcp://localhost:1337/settings.rem");
            settingsData.SavePath = (string)Environs.FileSystem.Paths["DataPath"];
            ciceroConnected = true;
            //if (RemotingServices.IsTransparentProxy(sequenceData))
            //    ciceroConnected = false;
            List<string> names = controller.hardwareState.analogs.Keys.ToList();
            try
            {
                ciceroVariables = new List<Variable>(sequenceData.Variables.Where(x => names.Contains(x.VariableName)));
             
            }
            catch (RemotingException e)
            {
                ciceroConnected = false;
                Console.WriteLine("Couldn't connect to cicero: " + e.Message);
            }
        }

        public void SendVariablesToCicero()
        {
            if (ciceroConnected)
            {
                foreach (Variable variable in ciceroVariables)
                {
                    variable.VariableValue = controller.hardwareState.analogs[variable.VariableName];
                }
            }
            else
            {
                Console.WriteLine("Cicero not connected. Couldn't remotely update.");
            }
        }

        public List<Variable> GetVariables()
        {
            return sequenceData.Variables.Where(x => x.IsSpecialVariable == false).ToList();
        }

        public void ClearRunningList()
        {
           sequenceData.Lists.ListLocked = false;
           foreach (Variable v in sequenceData.Variables)
           {
               if (v.ListDriven)
               {
                   int n = v.ListNumber;
                   sequenceData.Lists.Lists[n] = null;
                   v.ListDriven = false; 
               }
           }
        }

        public void FillList(int listNo, double start,double end, double step)
        {
            int nStep = (int)((end-start)/step);
            List<double> listItems = new List<double>();
            for (int i = 0; i< nStep; i++)
            {
                start += i * step;
                listItems.Add(start);
            }
            sequenceData.Lists.Lists[listNo] = listItems;

        }

    }
}
