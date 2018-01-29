using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PaddlePolStabiliser
{
    /// <summary>
    /// This is a program that uses an actuator to feedback to a voltage value is was written
    /// to stabilise the polarisation in an optical fibre using a paddle fibre polarisation
    /// controller against a photodiode
    /// </summary>
    public class Controller
    {
        #region Declarations
        
        private MainForm gui;

        public Dictionary<string, string> progSettings = new Dictionary<string, string>();
        public History<double> StoredData = new History<double>(100);
        public double LastData;
        public double LockLevel = 0;
        public double Gain = 0;

        public enum ControllerState
        {
            UNLOCKED, LOCKING, LOCKED
        };
        public ControllerState LockState = ControllerState.UNLOCKED;

        public enum ProgramState
        {
            STOPPED, RUNNING
        };
        public ProgramState ProgState = ProgramState.STOPPED;

        private static Controller controllerInstance;
        private static Acquiring acquiring;

        private SerialControl serialController;

        /// This is to update the GUI using the main form thread rather than controller
        /// and hopefully speed up the lock loop
        public delegate void GUIUpdateHandler(Controller controllerInstance, EventArgs e);
        public event GUIUpdateHandler GUIUpdate;
        public EventArgs e = null;
        #endregion

        #region Initialisation

        // This is the right way to get a reference to the controller. You shouldn't create a
        // controller yourself.
        public static Controller GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new Controller();
            }
            return controllerInstance;
        }

        #endregion

        public void Start()
        {
            gui = new MainForm(this);
            gui.controller = this;
            Application.Run(gui);
            //gui.ChangeLabel();
        }

        public void StartLoop()
        {
            ProgState = ProgramState.RUNNING;
            gui.DisAbleRun();
            Thread LoopThread = new Thread(new ThreadStart(controllerInstance.RunLoop));
            LoopThread.Name = "Loop Thread";
            LoopThread.Start();
        }

        public void RunLoop()
        {
            acquiring = Acquiring.GetAcquiring();

            int number = 0;

            gui.ClearEndOfTravel();
            acquiring.SetupTask();
            ConnectToSerial();

            while(Convert.ToBoolean(ProgState == ProgramState.RUNNING))
                {
                    double newpoint = acquiring.ReadPoint();
                    StoredData.Enqueue(newpoint);
                    LastData = newpoint;
                    UpdateTheFront(e);
                    number = number + 1;
                    if (gui.LockEngaged == true)
                    {
                        string currentstatus = GetSerialStatus();
                        int length = currentstatus.Length;
                        string x = currentstatus.Substring(28, 1);
                        string y = currentstatus.Substring(19, 1);
                        if (currentstatus.Substring(28, 1) == "0" && 
                            currentstatus.Substring(20, 1) == "0")
                        {
                            double feedback = SetFeedback();
                            MoveSerialStage(feedback);
                        }
                        else
                        {
                            ProgState = ProgramState.STOPPED;
                            gui.EndOfTravel();
                        }
                        
                    }
                    Thread.Sleep(1);
                }

            //stopping the loop 
            DisconnectFromSerial();
            acquiring.EndTask();
            gui.DisAbleRun();
        }

        private void ConnectToSerial()
        {
            serialController = new SerialControl(progSettings["ControllerChannel"]);
            serialController.Connect();
        }

        private void DisconnectFromSerial()
        {
            serialController.Disconnect();
        }

        private string GetSerialStatus()
        {
            serialController.Write("PH\x0D");
            Thread.Sleep(50);
            string status = serialController.Read();
            string statusbits = Convert.ToString(Convert.ToInt32(status.Substring(0, 8), 16), 2);
            return statusbits;
        }

        private void MoveSerialStage(double feedback)
        {
            serialController.Write("1PR" + String.Format("{0:0.0000}", feedback) + "\x0D");
        }

        private double SetFeedback()
        {
            double error;
            error = LastData - LockLevel;

            double feedback = Gain * error;
            return feedback;
        }

        protected virtual void UpdateTheFront(EventArgs e)
        {
            if (GUIUpdate != null)
            {
                GUIUpdate(this, e);
            }
        }
    }
}
