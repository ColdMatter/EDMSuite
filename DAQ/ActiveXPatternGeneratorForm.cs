//using System;
//using System.Drawing;
//using System.Collections;
//using System.ComponentModel;
//using System.Windows.Forms;
//
//using DAQ.Environment;
//
//namespace DAQ.HAL
//{
//	/// <summary>
//	/// I think this is the most horrible class of all. It's a wrapper for the NI VB ActiveX
//	/// Digital output controls. Pattern output is done this way because the NI-DAQ routines
//	/// have some (undocumented) memory weirdness that leads to computer crashes if you simply
//	/// call into the NI-DAQ dll. This class has a (hidden) windows form that acts as the ActiveX
//	/// host for the CWDO control (I think that's right). All of the methods are wrapped to be
//	/// thread-safe, as all calls to the ActiveX control must be on the GUI thread (yuck).
//	/// This class conforms to the PatternGenerator interface.
//	/// There are also some methods kludged on to support simple writes to digital ports (in
//	/// support of the OldSkoolEDM program). These methods currently don't have thread-safe
//	/// wrappers.
//	/// Note about the ActiveX CWDO write method - the 'pattern' argument needs to be an array of bytes.
//	/// The 1st byte is pattern 1 for group A. The 2nd byte is pattern 1 for group B. The 3rd byte is pattern 1
//	/// for group C. The 4th byte is pattern 1 for group D. The 5th byte is pattern 2 for group A...and so on.
//	/// This appears not to be documented anywhere.
//	///
//	/// </summary>
//	public class ActiveXPatternGeneratorForm : System.Windows.Forms.Form, PatternGenerator
//	{
//		private System.ComponentModel.Container components = null;
//
//		private int DEVICE;
//	
//		public event DisruptivePatternChangeStartingEventHandler DisruptivePatternChangeStarting;
//		public event DisruptivePatternChangeEndedEventHandler DisruptivePatternChangeEnded;
//
//		private bool outputActive = false;
//		private bool configChanged = false;
//		private AxCWDAQControlsLib.AxCWDO pgController;
//		private AxCWDAQControlsLib.AxCWDIO dioControl;
//		private int nPorts = 0;
//
//		public ActiveXPatternGeneratorForm(int device)
//		{
//			DEVICE = device;
//			InitializeComponent();
//		}
//
//		/// <summary>
//		/// Clean up any resources being used.
//		/// </summary>
//		protected override void Dispose( bool disposing )
//		{
//			if( disposing )
//			{
//				if(components != null)
//				{
//					components.Dispose();
//				}
//			}
//			base.Dispose( disposing );
//		}
//
//		#region Windows Form Designer generated code
//		/// <summary>
//		/// Required method for Designer support - do not modify
//		/// the contents of this method with the code editor.
//		/// </summary>
//		private void InitializeComponent()
//		{
//			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ActiveXPatternGeneratorForm));
//			this.pgController = new AxCWDAQControlsLib.AxCWDO();
//			this.dioControl = new AxCWDAQControlsLib.AxCWDIO();
//			((System.ComponentModel.ISupportInitialize)(this.pgController)).BeginInit();
//			((System.ComponentModel.ISupportInitialize)(this.dioControl)).BeginInit();
//			this.SuspendLayout();
//			// 
//			// pgController
//			// 
//			this.pgController.Enabled = true;
//			this.pgController.Location = new System.Drawing.Point(8, 8);
//			this.pgController.Name = "pgController";
//			this.pgController.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("pgController.OcxState")));
//			this.pgController.Size = new System.Drawing.Size(32, 32);
//			this.pgController.TabIndex = 0;
//			// 
//			// dioControl
//			// 
//			this.dioControl.Enabled = true;
//			this.dioControl.Location = new System.Drawing.Point(72, 8);
//			this.dioControl.Name = "dioControl";
//			this.dioControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("dioControl.OcxState")));
//			this.dioControl.Size = new System.Drawing.Size(32, 32);
//			this.dioControl.TabIndex = 1;
//			// 
//			// ActiveXPatternGeneratorForm
//			// 
//			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
//			this.ClientSize = new System.Drawing.Size(115, 53);
//			this.Controls.Add(this.dioControl);
//			this.Controls.Add(this.pgController);
//			this.Name = "ActiveXPatternGeneratorForm";
//			this.Text = "ActiveXPatternGenerator";
//			((System.ComponentModel.ISupportInitialize)(this.pgController)).EndInit();
//			((System.ComponentModel.ISupportInitialize)(this.dioControl)).EndInit();
//			this.ResumeLayout(false);
//
//		}
//		#endregion
//
//		public void Configure( double clockFrequency, bool loop, bool fullWidth, bool lowGroup )
//		{
//			this.Invoke(new ConfigureDelegate(ConfigureInternal),new object[]
//								{clockFrequency, loop, fullWidth, lowGroup} );
//		}
//		private delegate void ConfigureDelegate( double clockFrequency, bool loop, bool fullWidth, bool lowGroup );
//		private void ConfigureInternal( double clockFrequency, bool loop, bool fullWidth, bool lowGroup )
//		{
//			if (loop)
//			{
//				pgController.AllowRegeneration = true;
//				pgController.Continuous = true;
//			}
//			if (fullWidth) 
//			{
//				pgController.ChannelString = "0:3";
//				nPorts = 4;
//			} 
//			else
//			{
//				if (lowGroup) pgController.ChannelString = "0,1";
//				else pgController.ChannelString = "2,3";
//				nPorts = 2;
//			}
//			pgController.Device = (short)DEVICE;
//			if (clockFrequency == -1) 
//			{
//				pgController.UpdateClock.ClockSourceType = CWDAQControlsLib.CWDIOClockSources.cwdioCSExternalClock;
//			} 
//			else
//			{
//				pgController.UpdateClock.ClockSourceType = CWDAQControlsLib.CWDIOClockSources.cwdioCSInternalClock;
//				pgController.UpdateClock.Frequency = (float)clockFrequency;
//			}
//			configChanged = true;
//		}
//
//		// Note about blocking. This function blocks until the pattern has been written to the board.
//		// This is necessary to ensure that the tweak, and particular PG parameter scans work correctly.
//		public void OutputPattern(byte[] pattern)
//		{
//			this.Invoke(new OutputPatternDelegate(this.OutputPatternInternal), new object[]
//							{pattern} );
//		}
//		private delegate void OutputPatternDelegate(byte[] pattern);
//		// this call will not return until the write has finished.
//		private void OutputPatternInternal(byte[] pattern)
//		{
//			// is pattern output in progress ?
//			if (outputActive)
//			{
//				if (configChanged | (pattern.Length / nPorts != pgController.NPatterns))
//				{
//					// no choice but to stop pattern gen, reconfigure and restart
//					// send an event about the impending change to anybody who cares (like a fussy YAG)
//					OnDisruptivePatternChangeStarting();
//					pgController.NPatterns = pattern.Length / nPorts;
//					CallConfigure();
//					if (!Environs.Debug)
//					{
//						pgController.Write(pattern, -1);
//						pgController.Start();
//					}
//					outputActive = true;
//					OnDisruptivePatternChangeEnded();
//				}
//				else
//				{
//					// we should just be able to queue the new pattern for output
//					if (!Environs.Debug) pgController.Write(pattern, -1);
//				}
//			}
//			else
//			{
//				// if not go ahead and start a new output
//				pgController.NPatterns = pattern.Length / nPorts;
//				CallConfigure();
//				if (!Environs.Debug) 
//				{
//					pgController.Write(pattern, -1);
//					pgController.Start();
//				}
//				outputActive = true;
//			}
//
//		}
//
//		public void StopPattern()
//		{
//			this.Invoke(new StopPatternDelegate(StopPatternInternal),null);
//		}
//		private delegate void StopPatternDelegate();
//		private void StopPatternInternal()
//		{
//			pgController.Reset();
//			outputActive = false;
//		}
//
//		private void CallConfigure()
//		{
//			if (!Environs.Debug) pgController.Configure();
//			configChanged = false;
//		}
//
//		private void OnDisruptivePatternChangeStarting()
//		{
//			if (DisruptivePatternChangeStarting != null)
//				DisruptivePatternChangeStarting(this, new EventArgs());
//		}
//
//		private void OnDisruptivePatternChangeEnded()
//		{
//			if (DisruptivePatternChangeEnded != null)
//				DisruptivePatternChangeEnded(this, new EventArgs());
//		}
//
//		public void ConfigureDIOPorts(int port, bool output)
//		{
//			dioControl.Device = (short)DEVICE;
//			if (output)
//				if (!Environs.Debug) dioControl.Ports.Item(port).Assignment = CWDAQControlsLib.CWDIOAssignments.cwdioOutput;
//			else
//				if (!Environs.Debug) dioControl.Ports.Item(port).Assignment = CWDAQControlsLib.CWDIOAssignments.cwdioInput;
//		}
//
//		public void WriteDIOPort(int port, Int16 newValue)
//		{
//			if (!Environs.Debug) dioControl.Ports.Item(port).SingleWrite(newValue);
//		}
//
//		public Int16 ReadDIOPort(int port)
//		{
//			object resObj = new Int16();
//			if (!Environs.Debug) dioControl.Ports.Item(port).SingleRead(ref resObj);
//			return (Int16)resObj;
//		}
//	}
//}
