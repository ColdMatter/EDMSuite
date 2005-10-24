using System;

using ScanMaster.Acquire;

namespace ScanMaster.GUI
{
	/// <summary>
	/// 
	/// </summary>
	public interface Viewer
	{
		String Name
		{
			get;
		}

		void Show();
		void Hide();
		void ToggleVisible();
		void AcquireStart();
		void AcquireStop();
		void HandleDataPoint(DataEventArgs e);
		void ScanFinished();

		// Called when a scan is loaded. The Viewer should attempt to display something
		// useful about the scan.
		void NewScanLoaded();

	}
}
