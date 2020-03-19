using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAQ.HAL;
using CommandInterfaceXPS;

namespace MoleculeMOTHardwareControl.Controls
{
    public partial class TrackView : MoleculeMOTHardwareControl.Controls.GenericView
    {
        protected TrackController castController;
            
        public TrackView(TrackController controllerInstance)
            : base(controllerInstance)
        {
            InitializeComponent();
            castController = (TrackController)controller; // saves casting in every method
            //log
            strAssemblyPath = System.IO.Directory.GetParent(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)).FullName;
            logINIPath = "\\data\\";
            LogINIFullPath = strAssemblyPath + logINIPath; 

            // Initialization
            label_MessageCommunication.ForeColor = Color.Red;
            label_MessageCommunication.Text = string.Format("Disconnected from XPS");
            m_IsPositioner = false;
            m_CommunicationOK = false;
            m_pollingFlag = false;
            m_PollingInterval = POLLING_INTERVALLE_MS; // milliseconds
            m_CurrentGroupStatus = 0;
            for (int i = 0; i < NB_POSITIONERS; i++)
                m_TargetPosition[i] = 0;

            // Events
            if (this != null)
            {
                this.PositionChanged += new ChangedCurrentPositionHandler(CurrentPositionHandlerChanged);
                this.VelocityChanged += new ChangedCurrentVelocityHandler(CurrentVelocityHandlerChanged);
                this.AccelerationChanged += new ChangedCurrentAccelerationHandler(CurrentAccelerationHandlerChanged);
 
                this.GroupStatusChanged += new ChangedCurrentGroupStateHandler(CurrentGroupStateHandlerChanged);
                this.ErrorMessageChanged += new ChangedLabelErrorMessageHandler(ErrorMessageHandlerChanged);           
            }

           
        }

        const int DEFAULT_TIMEOUT = 10000;
        const int POLLING_INTERVALLE_MS = 100; // Milliseconds
        const int NB_POSITIONERS = 1;

        string strAssemblyPath = string.Empty;
        string logINIPath = string.Empty;
        string LogINIFullPath = string.Empty;

        List<string> log = new List<string>();
        bool logFlag = true;

        CommandInterfaceXPS.XPS m_xpsInterface = null;           // Socket #1 (order)
        CommandInterfaceXPS.XPS m_xpsInterfaceForPolling = null; // Socket #2 (polling)

        string m_IPAddress;
        int      m_IPPort;
        bool     m_CommunicationOK;
        string   m_GroupName;
        string   m_PositionerName;
        bool     m_IsPositioner;
        double[] m_TargetPosition = new double[NB_POSITIONERS];
        double[] m_CurrentPosition = new double[NB_POSITIONERS];
        double[] m_CurrentVelocity = new double[NB_POSITIONERS];
        double[] m_CurrentAcceleration = new double[NB_POSITIONERS];
        int m_CurrentGroupStatus;
        int sweep, sweepCount;
        string   m_CurrentGroupStatusDescription;
        string   m_XPSControllerVersion;
        string   m_errorDescription;        

        int            m_PollingInterval;
        bool           m_pollingFlag;
        private Thread m_PollingThread;
        // status
        public delegate void ChangedCurrentGroupStateHandler(int currentGroupStatus, string description);
        private event ChangedCurrentGroupStateHandler m_CurrentGroupStateChanged;
        public event ChangedCurrentGroupStateHandler GroupStatusChanged
        {
            add { m_CurrentGroupStateChanged += value; }
            remove { m_CurrentGroupStateChanged -= value; }
        }
        // position
        public delegate void ChangedCurrentPositionHandler(double[] currentPositions);
        private event ChangedCurrentPositionHandler m_CurrentPositionChanged;
        public event ChangedCurrentPositionHandler PositionChanged
        {
            add { m_CurrentPositionChanged += value; }
            remove { m_CurrentPositionChanged -= value; }
        }
        // velocity
        public delegate void ChangedCurrentVelocityHandler(double[] currentVelocities);
        private event ChangedCurrentVelocityHandler m_CurrentVelocityChanged;
        public event ChangedCurrentVelocityHandler VelocityChanged
        {
            add { m_CurrentVelocityChanged += value; }
            remove { m_CurrentVelocityChanged -= value; }
        }
        // Acceleration
        public delegate void ChangedCurrentAccelerationHandler(double[] currentAccelerations);
        private event ChangedCurrentAccelerationHandler m_CurrentAccelerationChanged;
        public event ChangedCurrentAccelerationHandler AccelerationChanged
        {
            add { m_CurrentAccelerationChanged += value; }
            remove { m_CurrentAccelerationChanged -= value; }
        }
        // errorMessage
        public delegate void ChangedLabelErrorMessageHandler(string currentErrorMessage);
        private event ChangedLabelErrorMessageHandler m_ErrorMessageChanged;
        public event ChangedLabelErrorMessageHandler ErrorMessageChanged
        {
            add { m_ErrorMessageChanged += value; }
            remove { m_ErrorMessageChanged -= value; }
        }
        // ProgressBar
        public delegate void ChangedProgressBarHandler(int currentProgressBar);
        private event ChangedProgressBarHandler m_ProgressBarChanged;
        public event ChangedProgressBarHandler ProgressBarChanged
        {
            add { m_ProgressBarChanged += value; }
            remove { m_ProgressBarChanged -= value; }
        }
        // ProgressText
        public delegate void ChangedProgressTextHandler(int currentProgressText);
        private event ChangedProgressTextHandler m_ProgressTextChanged;
        public event ChangedProgressTextHandler ProgressTextChanged
        {
            add { m_ProgressTextChanged += value; }
            remove { m_ProgressTextChanged -= value; }
        }
        // UpdateButtons
        public delegate void ChangedUpdateButtonsHandler(bool currentUpdateButtons);
        private event ChangedUpdateButtonsHandler m_UpdateButtonsChanged;
        public event ChangedUpdateButtonsHandler UpdateButtonsChanged
        {
            add { m_UpdateButtonsChanged += value; }
            remove { m_UpdateButtonsChanged -= value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
           
        private void CurrentPositionHandlerChanged(double[] currentValues)
        {
            string strPosition = currentValues[0].ToString("F2", CultureInfo.CurrentCulture.NumberFormat);
            
            textBoxPosition.BeginInvoke(
                   new Action(() =>
                   {
                       textBoxPosition.Text = strPosition;
                   }
                ));
        }
        private void CurrentVelocityHandlerChanged(double[] currentValues)
        {
            string strVelocity = currentValues[0].ToString("F2", CultureInfo.CurrentCulture.NumberFormat);
            textBoxVelocity.BeginInvoke(
                   new Action(() =>
                   {
                       textBoxVelocity.Text = strVelocity;
                   }
                ));
        }
        private void CurrentAccelerationHandlerChanged(double[] currentValues)
        {
            string strAcceleration = currentValues[0].ToString("F2", CultureInfo.CurrentCulture.NumberFormat);
            textBoxAcceleration.BeginInvoke(
                   new Action(() =>
                   {
                       textBoxAcceleration.Text = strAcceleration;
                   }
                ));
        }
        private void CurrentGroupStateHandlerChanged(int currentGroupStatus, string strGroupStatusDescription)
        {
            try
            {
                string strStatus = currentGroupStatus.ToString("F0", CultureInfo.CurrentCulture.NumberFormat);                               

                label_GroupStatusDescription.BeginInvoke(
                   new Action(() =>
                   {
                       label_GroupStatusDescription.Text = strGroupStatusDescription+" ("+ strStatus + ")";
                   }
                ));           
            }
            catch (Exception ex)
            {
                label_GroupStatusDescription.Text = "Exception in CurrentGroupStateHandlerChanged: " + ex.Message; // DEBUG
            }
        }
        private void ErrorMessageHandlerChanged(string Message)
        {            
            label_ErrorMessage.BeginInvoke(
               new Action(() =>
               {
                   label_ErrorMessage.Text = Message;
               }
            ));
        }
        // end of events handlers

        public void UpdateGroupStatus()
        {
            try
            {
                int lastGroupState = m_CurrentGroupStatus;
                if (m_xpsInterfaceForPolling != null)
                {
                    string errorString = string.Empty;
                    int result = m_xpsInterfaceForPolling.GroupStatusGet(m_GroupName, out m_CurrentGroupStatus, out errorString);
                    if (result == CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XPS 
                    {
                        m_CurrentGroupStatus = 0;
                        if (errorString.Length > 0)
                        {
                            int errorCode = 0;
                            int.TryParse(errorString, out errorCode);
                            m_xpsInterface.ErrorStringGet(errorCode, out m_errorDescription, out errorString);
                            m_ErrorMessageChanged(string.Format("GroupStatusGet ERROR {0}: {1}", result, m_errorDescription));
                        }
                        else
                            m_ErrorMessageChanged(string.Format("Communication failure with XPS after GroupStatusGet "));
                    }
                    else
                        result = m_xpsInterfaceForPolling.GroupStatusStringGet(m_CurrentGroupStatus, out m_CurrentGroupStatusDescription, out errorString);

                    if ((m_CurrentGroupStatus != lastGroupState) && m_CurrentGroupStateChanged != null)
                        m_CurrentGroupStateChanged(m_CurrentGroupStatus, m_CurrentGroupStatusDescription);
                }
            }
            catch (Exception ex)
            {
                m_ErrorMessageChanged("Exception in UpdateGroupStatus: " + ex.Message);
            }
        }

        public void UpdateCurrentPosition()
        {
            try
            {
                double lastCurrentPosition = m_CurrentPosition[0];
                if (m_xpsInterfaceForPolling != null)
                {
                    if (m_IsPositioner == true)
                    {
                        string errorString = string.Empty;
                        int result = m_xpsInterfaceForPolling.GroupPositionCurrentGet(m_PositionerName, out m_CurrentPosition, NB_POSITIONERS, out errorString);
                        if (result == CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XPS 
                        {
                            m_CurrentPosition[0] = 0;
                            if (errorString.Length > 0)
                            {
                                int errorCode = 0;
                                int.TryParse(errorString, out errorCode);
                                m_xpsInterface.ErrorStringGet(errorCode, out m_errorDescription, out errorString);
                                m_ErrorMessageChanged(string.Format("GroupPositionCurrentGet ERROR {0}: {1}", result, m_errorDescription));
                            }
                            else
                                m_ErrorMessageChanged(string.Format("Communication failure with XPS after GroupPositionCurrentGet "));
                        }
                    }
                    if ((m_CurrentPosition[0] != lastCurrentPosition) && m_CurrentPositionChanged != null)
                        m_CurrentPositionChanged(m_CurrentPosition);
                }
            }
            catch (Exception ex)
            {
                m_ErrorMessageChanged("Exception in UpdateCurrentPosition: " + ex.Message);
            }
        }
        private double LastVelocity, EstimAcceleration, tm;
        public void UpdateCurrentVelocity()
        {
            try
            {
                double lastCurrentVelocity = m_CurrentVelocity[0];
                if (m_xpsInterfaceForPolling != null)
                {
                    if (m_IsPositioner == true)
                    {
                        string errorString = string.Empty;
                        int result = m_xpsInterfaceForPolling.GroupVelocityCurrentGet(m_PositionerName, out m_CurrentVelocity, NB_POSITIONERS, out errorString);
                        if (result == CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XPS 
                        {
                            m_CurrentVelocity[0] = 0;
                            if (errorString.Length > 0)
                            {
                                int errorCode = 0;
                                int.TryParse(errorString, out errorCode);
                                m_xpsInterface.ErrorStringGet(errorCode, out m_errorDescription, out errorString);
                                m_ErrorMessageChanged(string.Format("GroupVelocityCurrentGet ERROR {0}: {1}", result, m_errorDescription));
                            }
                            else
                                m_ErrorMessageChanged(string.Format("Communication failure with XPS after GroupVelocityCurrentGet "));
                        }
                    }

                    if ((m_CurrentVelocity[0] != lastCurrentVelocity) && m_CurrentVelocityChanged != null)
                    {
                        m_CurrentVelocityChanged(m_CurrentVelocity);
                        if (DateTime.Now.TimeOfDay.TotalMilliseconds != tm)
                        EstimAcceleration = 1000 * (m_CurrentVelocity[0] - LastVelocity)/(DateTime.Now.TimeOfDay.TotalMilliseconds - tm);
                        LastVelocity = m_CurrentVelocity[0];
                        tm = DateTime.Now.TimeOfDay.TotalMilliseconds;   
                    }                       
                }
            }
            catch (Exception ex)
            {
                m_ErrorMessageChanged("Exception in UpdateCurrentVelocity: " + ex.Message);
            }
        }

        public void UpdateCurrentAcceleration()
        {
            try
            {                
                if (m_xpsInterfaceForPolling != null)
                {
                    if (m_IsPositioner == true)
                    {
                        m_CurrentAcceleration[0] = EstimAcceleration;
                    }

                    if (m_CurrentAccelerationChanged != null)
                        m_CurrentAccelerationChanged(m_CurrentAcceleration);
                }
            }
            catch (Exception ex)
            {
                m_ErrorMessageChanged("Exception in UpdateCurrentAcceleration: " + ex.Message);
            }
        }

        public void UpdateCurrentAll()
        {
            UpdateGroupStatus();
            UpdateCurrentPosition();
            UpdateCurrentVelocity();
            UpdateCurrentAcceleration();
        }

        #region POLLING 
        public void StartPolling()
        {
            try
            {
                if (m_pollingFlag == false)
                {
                    m_pollingFlag = true; // Start polling

                    // Create thread and start it
                    m_PollingThread = new Thread(new ParameterizedThreadStart(poll));
                    m_PollingThread.IsBackground = true;
                    m_PollingThread.Start();
                }
            }
            catch (Exception ex)
            {
                m_ErrorMessageChanged("Exception in StartPolling: " + ex.Message);
            }
        }

        public void StopPolling()
        {
            try
            {
                m_pollingFlag = false; // Stop the polling
                if (m_PollingThread != null)
                    m_PollingThread.Abort();
            }
            catch (Exception ex)
            {
                m_ErrorMessageChanged("Exception in StopPolling: " + ex.Message);
            }
        }
       
        private bool AdjustShuttleFlag = false;
        private const double maxSpeed = 700;
        private const double tol = 5;
 
        public void poll(object obj)
        {
            try
            {
                while ((m_pollingFlag == true) && (m_CommunicationOK == true))
                {                                        
                     UpdateCurrentAll();

                    // Tempo in relation to the polling frequency
                    Thread.Sleep(m_PollingInterval);
                }
            }
            catch (Exception ex)
            {
                m_ErrorMessageChanged("Exception in poll: " + ex.Message);
            }
        }
        #endregion

        #region Communication/Initiation
        /// <summary>
        /// Socket opening and start polling
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton(object sender, EventArgs e)
        {
            // Get IP address and Ip port from form front panel
            m_IPAddress = textBox_IPAddress.Text;
            int.TryParse(textBox_IPPort.Text, out m_IPPort);

            m_PositionerName = TextBox_Group.Text;
            int index = m_PositionerName.LastIndexOf('.');
            if (index != -1)
            {
                m_IsPositioner = true;
                m_GroupName = m_PositionerName.Substring(0, index);
                label_ErrorMessage.Text = string.Empty;
            }
            else
            {
                m_IsPositioner = false;
                m_GroupName = m_PositionerName;
                ErrorMessageHandlerChanged("Must be a positioner name not a group name");
            }

            label_GroupStatusDescription.Text = string.Empty;
            m_XPSControllerVersion = string.Empty;
            m_errorDescription = string.Empty;

            try
            {
                // Open socket #1 to order
                if (m_xpsInterface == null)
                    m_xpsInterface = new CommandInterfaceXPS.XPS();
                if (m_xpsInterface != null)
                {
                    // Open socket
                    int returnValue = m_xpsInterface.OpenInstrument(m_IPAddress, m_IPPort, DEFAULT_TIMEOUT);
                    if (returnValue == 0)
                    {
                        string errorString = string.Empty;
                        int result = m_xpsInterface.FirmwareVersionGet(out m_XPSControllerVersion, out errorString);
                        if (result == CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XPS 
                        {
                            if (errorString.Length > 0)
                            {
                                int errorCode = 0;
                                int.TryParse(errorString, out errorCode);
                                m_xpsInterface.ErrorStringGet(errorCode, out m_errorDescription, out errorString);
                                m_XPSControllerVersion = string.Format("FirmwareVersionGet ERROR {0}: {1}", result, m_errorDescription);
                            }
                            else
                                m_XPSControllerVersion = string.Format("Communication failure with XPS after FirmwareVersionGet ");
                        }
                        else
                        {
                            label_MessageCommunication.ForeColor = Color.Green;
                            label_MessageCommunication.Text = string.Format("Connected to XPS");
                            m_CommunicationOK = true;
                        }
                    }
                }
                else
                    m_XPSControllerVersion = "XPS instance is NULL";

                // Open socket #2 for polling
                if (m_xpsInterfaceForPolling == null)
                    m_xpsInterfaceForPolling = new CommandInterfaceXPS.XPS();
                if (m_xpsInterfaceForPolling != null)
                {
                    // Open socket
                    int returnValue = m_xpsInterfaceForPolling.OpenInstrument(m_IPAddress, m_IPPort, DEFAULT_TIMEOUT);
                    if (returnValue == 0)
                    {
                        string errorString = string.Empty;
                        int result = m_xpsInterfaceForPolling.FirmwareVersionGet(out m_XPSControllerVersion, out errorString);
                        if (result != CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XPS 
                            StartPolling();
                    }
                }

                if (m_XPSControllerVersion.Length <= 0)
                    m_XPSControllerVersion = "No detected XPS";

                this.Text = string.Format("Axel Track v1.3 - {0}", m_XPSControllerVersion);
            }
            catch (Exception ex)
            {
                ErrorMessageHandlerChanged("Exception in ConnectButton: " + ex.Message);
            }
        }

        /// <summary>
        /// Stop polling and Close socket
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                m_CommunicationOK = false;
                m_pollingFlag = false;

                if (m_xpsInterfaceForPolling != null)
                    m_xpsInterfaceForPolling.CloseInstrument();

                if (m_xpsInterface != null)
                    m_xpsInterface.CloseInstrument();
                
                label_MessageCommunication.ForeColor = Color.Red;
                label_MessageCommunication.Text = string.Format("Disconnected from XPS");
                label_ErrorMessage.Text = string.Empty;
                label_GroupStatusDescription.Text = string.Empty;
                m_XPSControllerVersion = string.Empty;
                m_errorDescription = string.Empty;
                this.Text = "XPS Application";
            }
            catch (Exception ex)
            {
                ErrorMessageHandlerChanged("Exception in buttonDisconnect_Click: " + ex.Message);
            }
        }

        public Dictionary<string, string> readINI(string filename)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (!File.Exists(filename)) throw new Exception("File not found: " + filename);
            List<string> ls = new List<string>();
            string line;
            foreach (string wline in File.ReadLines(filename))
            {
                char ch = wline[0];
                if (ch.Equals('[')) continue;
                int sc = wline.IndexOf(';');
                if (sc > -1) line = wline.Remove(sc);
                else line = wline;
                if (line.Equals("")) continue;

                string[] sb = line.Split('=');
                if (sb.Length != 2) break;
                dict[sb[0]] = sb[1];
            }
            return dict;
        }
        /// <summary>
        /// Button to perform a GroupInitialize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonInitialize_Click(object sender, EventArgs e)
        {
            try
            {
                ftp ftpClient = new ftp(@"ftp://192.168.0.254", "Administrator", "Administrator");
                LogINIFullPath = strAssemblyPath + logINIPath + "stages.ini";
                LogINIFullPath += ".log";
                ftpClient.download("Config/stages.ini", LogINIFullPath);
                Thread.Sleep(500);               

                ftpClient = null;
                Dictionary<string, string> ini = readINI(LogINIFullPath);
                TxMaxAccel.Text = ini["MaximumAcceleration"]; textBoxAccelerationInput.Text = TxMaxAccel.Text;
                TxMaxVel.Text = ini["MaximumVelocity"]; textBoxVelocityInput.Text = TxMaxVel.Text;
                TxBoundSource.Text = ini["MinimumTargetPosition"]; textBoxSource.Text = TxBoundSource.Text;
                TxBoundTweezer.Text = ini["MaximumTargetPosition"]; textBoxTweezer.Text = TxBoundTweezer.Text;

                label_ErrorMessage.Text = string.Empty;
                if (m_CommunicationOK == false)
                    label_ErrorMessage.Text = string.Format("Not connected to XPS");

                if (m_xpsInterface != null)
                {
                    string errorString = string.Empty;
                    int result = m_xpsInterface.GroupInitialize(m_GroupName, out errorString);
                    if (result == CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XPS 
                    {
                        if (errorString.Length > 0)
                        {
                            int errorCode = 0;
                            int.TryParse(errorString, out errorCode);
                            m_xpsInterface.ErrorStringGet(errorCode, out m_errorDescription, out errorString);
                            label_ErrorMessage.Text = string.Format("GroupInitialize ERROR {0}: {1}", result, m_errorDescription);
                        }
                        else
                            label_ErrorMessage.Text = string.Format("Communication failure with XPS after GroupInitialize ");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessageHandlerChanged("Exception in buttonInitialize_Click: " + ex.Message);
            }
        }

        /// <summary>
        /// Button to perform a group home search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonHome_Click(object sender, EventArgs e)
        {
            try
            {
                label_ErrorMessage.Text = string.Empty;
                if (m_CommunicationOK == false)
                    label_ErrorMessage.Text = string.Format("Not connected to XPS");

                if (m_xpsInterface != null)
                {
                    string errorString = string.Empty;
                    int result = m_xpsInterface.GroupHomeSearch(m_GroupName, out errorString);

                    //int result = m_xpsInterface.GroupReferencingStart(m_GroupName, out errorString); // arbitrary home (manual position)
                    //result = m_xpsInterface.GroupReferencingStop(m_GroupName, out errorString);

                    if (result == CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XPS 
                    {
                        if (errorString.Length > 0)
                        {
                            int errorCode = 0;
                            int.TryParse(errorString, out errorCode);
                            m_xpsInterface.ErrorStringGet(errorCode, out m_errorDescription, out errorString);
                            label_ErrorMessage.Text = string.Format("GroupHomeSearch ERROR {0}: {1}", result, m_errorDescription);
                        }
                        else
                            label_ErrorMessage.Text = string.Format("Communication failure with XPS after GroupHomeSearch ");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessageHandlerChanged("Exception in buttonHome_Click: " + ex.Message);
            }
        }

        /// <summary>
        /// Button to perform a group kill
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonKill_Click(object sender, EventArgs e)
        {
            try
            {
                label_ErrorMessage.Text = string.Empty;
                if (m_CommunicationOK == false)
                    label_ErrorMessage.Text = string.Format("Not connected to XPS");

                if (m_xpsInterface != null)
                {
                    string errorString = string.Empty;
                    int result = m_xpsInterface.GroupKill(m_GroupName, out errorString);
                    if (result == CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XPS 
                    {
                        if (errorString.Length > 0)
                        {
                            int errorCode = 0;
                            int.TryParse(errorString, out errorCode);
                            m_xpsInterface.ErrorStringGet(errorCode, out m_errorDescription, out errorString);
                            label_ErrorMessage.Text = string.Format("GroupKill ERROR {0}: {1}", result, m_errorDescription);
                        }
                        else
                            label_ErrorMessage.Text = string.Format("Communication failure with XPS after GroupKill ");
                    }
                }
            }
            catch (Exception ex)
            {
                label_ErrorMessage.Text = "Exception in buttonKill_Click: " + ex.Message;
            }
        }

        /*    private void buttonInitiate_Click(object sender, EventArgs e)
            {
                buttonInitiate.Text = "Doing it...";
                buttonInitiate.ForeColor = System.Drawing.Color.DarkGray;
                Application.DoEvents();
                if (m_CommunicationOK) buttonDisconnect_Click(null, null);
                ConnectButton(null, null);
                buttonKill_Click(null, null);
                buttonInitialize_Click(null, null);
                buttonHome_Click(null, null);

                buttonInitiate.Text = "Reset";
                buttonInitiate.ForeColor = System.Drawing.Color.DarkOrange;

                buttonAbort.ForeColor = System.Drawing.Color.Red;
                buttonAbort.Enabled = true;
            } 

            private void buttonAbort_Click(object sender, EventArgs e)
            {
                string errorString = string.Empty;
                int result = m_xpsInterface.GroupMoveAbort(m_GroupName, out errorString);
                if (result == CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XPS 
                {
                    if (errorString.Length > 0)
                    {
                        int errorCode = 0;
                        int.TryParse(errorString, out errorCode);
                        m_xpsInterface.ErrorStringGet(errorCode, out m_errorDescription, out errorString);
                        label_ErrorMessage.Text = string.Format("GroupMoveAbortFast ERROR {0}: {1}", result, m_errorDescription);
                    }
                    else
                        label_ErrorMessage.Text = string.Format("Communication failure with XPS after GroupMoveAbortFaste ");
                }
            }*/
        #endregion

        /// <summary>
        /// Button to perform an absolute motion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonMoveTo_Click(object sender, EventArgs e)
        {
            ErrorMessageHandlerChanged("");
            if (!RBManual.Checked)
            {
                ErrorMessageHandlerChanged("Wrong mode - switch to Manual mode");
                return;
            }

            double tp, minP, maxP;
            double.TryParse(textBoxTarget.Text, out tp); 
            double.TryParse(TxBoundTweezer.Text, out maxP); double.TryParse(TxBoundSource.Text, out minP);
            if (tp < minP) textBoxTarget.Text = TxBoundSource.Text;
            if (tp > maxP) textBoxTarget.Text = TxBoundTweezer.Text;
            try
            {
                label_ErrorMessage.Text = string.Empty;
                if (m_CommunicationOK == false)
                    label_ErrorMessage.Text = string.Format("Not connected to XPS");

                if (m_IsPositioner == true)
                {
                    double.TryParse(textBoxTarget.Text, out m_TargetPosition[0]);
                    if ((m_xpsInterface != null) && (m_CommunicationOK == true))
                    {
                        string errorString = string.Empty; int result = 0;
                        result += m_xpsInterface.GroupMoveAbsolute(m_PositionerName, m_TargetPosition, 1, out errorString);
                        if (result == CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XPS 
                        {
                            if (errorString.Length > 0)
                            {
                                int errorCode = 0;
                                int.TryParse(errorString, out errorCode);
                                m_xpsInterface.ErrorStringGet(errorCode, out m_errorDescription, out errorString);
                                m_ErrorMessageChanged(string.Format("GroupMoveAbsolute ERROR {0}: {1}", result, m_errorDescription));
                            }
                            else
                                m_ErrorMessageChanged(string.Format("Communication failure with XPS after GroupMoveAbsolute "));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_ErrorMessageChanged("Exception in buttonMoveTo_Click: " + ex.Message);
            }
        }

        private bool fShuttleFlag = false;
        private double initPos = 0;
        public bool ShuttleFlag
        {
            get
            {
                return fShuttleFlag;
            }
            set
            {
                fShuttleFlag = value;

                string errorString = string.Empty;
                int result;
                if (value) // switching ON
                {
                    UpdateCurrentAll();
                    initPos = m_CurrentPosition[0];

                    result = m_xpsInterface.GroupJogModeEnable(m_GroupName, out errorString);
                }
                else // going OFF
                {
                    if (!emergency)
                    {
                    /*    double[] velos = new double[NB_POSITIONERS];
                        double[] accels = new double[NB_POSITIONERS];
                        velos[0] = 0;
                        accels[0] = 100;*/
                        Thread.Sleep(1000);
                        result = m_xpsInterface.GroupJogModeDisable(m_GroupName, out errorString);
                        if (result == CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XP
                        {
                            if (errorString.Length > 0)
                            {
                                int errorCode = 0;
                                int.TryParse(errorString, out errorCode);
                                m_xpsInterface.ErrorStringGet(errorCode, out m_errorDescription, out errorString);
                                m_ErrorMessageChanged(string.Format("GroupJogMode ERROR {0}: {1}", result, m_errorDescription));
                            }
                            else
                                m_ErrorMessageChanged(string.Format("Communication failure with XPS after GroupJogMode "));
                        }
                    }
                }
                m_UpdateButtonsChanged(value);
            }
        } 
   
        bool emergency = false;

        private void RBManual_MouseDown(object sender, MouseEventArgs e)
        {
            RBManual.Checked = true;
            RBTrigger.Checked = false;
        }

        private void RBTrigger_MouseDown(object sender, MouseEventArgs e)
        {
            RBManual.Checked = false;
            RBTrigger.Checked = true;
        }

        private void BTN_update_Click(object sender, EventArgs e)
        {
            string errorString = string.Empty; int result = 0;
            double vel, maxVel, maxAccel, accel, minJerk, maxJerk;
            result = m_xpsInterface.PositionerSGammaParametersGet(m_PositionerName, out vel, out accel, out minJerk, out maxJerk, out errorString);
            double.TryParse(textBoxVelocityInput.Text, out vel); double.TryParse(textBoxAccelerationInput.Text, out accel);
            double.TryParse(TxMaxVel.Text, out maxVel); double.TryParse(TxMaxAccel.Text, out maxAccel);
            if (vel > maxVel) textBoxVelocityInput.Text = TxMaxVel.Text;
            if (accel > maxAccel) textBoxAccelerationInput.Text = TxMaxAccel.Text;
            double.TryParse(textBoxVelocityInput.Text, out vel); double.TryParse(textBoxAccelerationInput.Text, out accel);
            result += m_xpsInterface.PositionerSGammaParametersSet(m_PositionerName, vel, accel, minJerk, maxJerk, out errorString);
        }

        private void StopShuttle()
        {
            int result;
            string errorString = string.Empty;
            if (logFlag)
            {                
                result = m_xpsInterface.GatheringStop(out errorString);
                result = m_xpsInterface.GatheringStopAndSave(out errorString);

                ftp ftpClient = new ftp(@"ftp://192.168.0.254", "Administrator", "Administrator");
                LogINIFullPath = strAssemblyPath + logINIPath + DateTime.Now.ToString("yy-MM-dd_H-mm-ss");
                LogINIFullPath += ".log";
                ftpClient.download("Public/Gathering.dat", LogINIFullPath);
                Thread.Sleep(500);

                ftpClient = null;
            }
            if (emergency)
            {
                result = m_xpsInterface.GroupKill(m_GroupName, out errorString); // GroupMoveAbortFast, GroupMoveAbort
                if (result == CommandInterfaceXPS.XPS.FAILURE) // Communication failure with XP
                {
                    if (errorString.Length > 0)
                    {
                        int errorCode = 0;
                        int.TryParse(errorString, out errorCode);
                        m_xpsInterface.ErrorStringGet(errorCode, out m_errorDescription, out errorString);
                        m_ErrorMessageChanged(string.Format("GroupKill ERROR {0}: {1}", result, m_errorDescription));
                    }
                    else
                        m_ErrorMessageChanged(string.Format("Communication failure with XPS after GroupKill "));
                }
                AdjustShuttleFlag = false;
            }
            do
            {
               Application.DoEvents(); // wait for current cycle to end if in the middle of it
            } while (AdjustShuttleFlag);

            ShuttleFlag = false; // go off jog
            emergency = false;
        }

        private void buttonStopShuttle_Click(object sender, EventArgs e)
        {
            sweep = sweepCount;
        }

        private void picTrain_Click(object sender, EventArgs e)
        {
            MessageBox.Show("     Axel Track v1.2 \n   by Teodor Krastev \nfor Imperial College, London", "About");
        }
        private bool running = false;

        public void TCLscript(string arg1, string arg2, string arg3)
        {
            if (!arg1.Equals("")) textBoxSource.Text = arg1;
            if (!arg2.Equals("")) textBoxTweezer.Text = arg2;
            if (!arg3.Equals("")) textBoxIterations.Text = arg3;
            string rslt, err = "";        
            ErrorMessageHandlerChanged("");
            if (!RBTrigger.Checked)
            {
                ErrorMessageHandlerChanged("Wrong mode - switch to Trigger mode");
                return;
            }
            if (running)
            {
                ErrorMessageHandlerChanged("The TCL script is already running");
                return;
            }
            running = true;
            double src, twr, srcP, twrP; double.TryParse(TxBoundSource.Text, out srcP); double.TryParse(TxBoundTweezer.Text, out twrP); 
            double.TryParse(textBoxSource.Text, out src); if (src < srcP) textBoxSource.Text = TxBoundSource.Text;
            double.TryParse(textBoxSource.Text, out twr); if (twr > twrP) textBoxTweezer.Text = TxBoundTweezer.Text;
            if(src > twr)
            {
                ErrorMessageHandlerChanged("Source posistion is bigger than tweezer position!");
                return;
            }
            string args = textBoxSource.Text + "," + textBoxTweezer.Text + "," + textBoxIterations.Text;
            m_xpsInterface.TCLScriptExecuteAndWait("TransportTrapProgramm1.tcl", "script1", args, out rslt, out err);
            ErrorMessageHandlerChanged(err);
            running = false;
        }

        private void buttonRunTCL_Click(object sender, EventArgs e)
        {
            TCLscript("", "", "");
        }

        private void RBTrigger_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        
    }
}
