using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;
using NationalInstruments;

namespace ConfocalMicroscopeControl
{
    class Controller : MarshalByRefObject
    {

        MainWindow window;

        #region Class members

        public enum AppState { stopped, running };

        private Acquisitor acquisitor;
        public Acquisitor Acquisitor
        {
            get { return acquisitor; }
        }

        private DataStore dataStore = new DataStore();
        public DataStore DataStore
        {
            get { return dataStore; }
        }

        private static Controller controllerInstance;
        public AppState appState = AppState.stopped;

        #endregion

        #region Initialisation

        public static Controller GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new Controller();
            }
            return controllerInstance;
        }

        #endregion

        #region Tasks

        private AnalogInputChannel GalvoXInput = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["GalvoX"];
        private AnalogInputChannel GalvoYInput = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["GalvoY"];
        private AnalogOutputChannel GalvoXOutput = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["GalvoXControl"];
        private AnalogOutputChannel GalvoXOutput = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["GalvoXControl"];
        private Task ReflectionChannelTask = new Task();

        #endregion
    }
}
