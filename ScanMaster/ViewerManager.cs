using System;
using System.Collections;

using ScanMaster.Acquire;

namespace ScanMaster.GUI
{
	/// <summary>
	/// 
	/// </summary>
	public class ViewerManager
	{
		private Hashtable viewers = new Hashtable();
		public Hashtable Viewers
		{
			get { return viewers; }
		}

		
		public ViewerManager()
		{
			viewers.Add("StandardViewer", new StandardViewer());
            viewers.Add("TweakViewer", new TweakViewer());
            //viewers.Add("StatisticsViewer", new StatisticsViewer());
            //viewers.Add("RollViewer", new RollViewer());
			foreach (DictionaryEntry de in viewers) ((Viewer)(de.Value)).Show();
			foreach (DictionaryEntry de in viewers) ((Viewer)(de.Value)).Hide();
			((Viewer)viewers["StandardViewer"]).Show();
		}

		public void AcquireStart()
		{
			foreach (DictionaryEntry de in viewers) ((Viewer)(de.Value)).AcquireStart();
		}
	
		public void AcquireStop()
		{
			foreach (DictionaryEntry de in viewers) ((Viewer)(de.Value)).AcquireStop();
		}

		public void HandleDataPoint(DataEventArgs e)
		{
			foreach (DictionaryEntry de in viewers) ((Viewer)(de.Value)).HandleDataPoint(e);
		}

		public void ScanFinished()
		{
			foreach (DictionaryEntry de in viewers) ((Viewer)(de.Value)).ScanFinished();
		}

		public void NewScanLoaded()
		{
			foreach (DictionaryEntry de in viewers) ((Viewer)(de.Value)).NewScanLoaded();
		}

		public Viewer GetViewer(String name)
		{
			if (viewers.Contains(name)) return (Viewer)viewers[name];
			else return null;
		}

	}
}
