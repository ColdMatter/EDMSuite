using System;
using System.Drawing;
using System.Windows.Forms;

using DAQ.Mathematica;

using Wolfram.NETLink;
using Wolfram.NETLink.UI;


namespace DAQ.Analyze
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class Analysis
	{

		private Form form = null;

		protected String TitleText = "Generic Analysis";
		protected Size FormSize = new Size(480,410);
		protected int TextAreaHeight = 90;

		protected IKernelLink kernel;

		private TextBox resultsBox;


		abstract public void DoAnalysis( AnalysisData data );
		abstract public AnalysisResult GetResult();
		abstract public String GetText();
	
		public void ShowWindow()
		{
			if (form == null) CreateWindow();
			resultsBox.Text = GetText();
			form.Show();
		}
		
		public void CloseWindow()
		{
			form.Dispose();
			form = null;
		}

		private void CreateWindow()
		{
			form = new Form();
			form.Text = TitleText;
			form.Size = FormSize;
			form.Closed +=new EventHandler(formClosed);
					
			resultsBox = new TextBox();
			resultsBox.Size = new System.Drawing.Size(FormSize.Width, TextAreaHeight);
			resultsBox.Location = new System.Drawing.Point(0, FormSize.Height - TextAreaHeight);
			resultsBox.BackColor = System.Drawing.Color.White;
			resultsBox.ForeColor = System.Drawing.Color.Tomato;
			resultsBox.Multiline = true;
			resultsBox.ReadOnly = true;
					
			MathPictureBox fitGraph = new MathPictureBox(kernel);
			fitGraph.UseFrontEnd = false;
			fitGraph.Size = new System.Drawing.Size(FormSize.Width, FormSize.Height - TextAreaHeight);
			fitGraph.Location = new System.Drawing.Point(0,0);
			fitGraph.PictureType = "Automatic";
			fitGraph.MathCommand = "Show[getFitGraph[]]";
					
			form.Controls.Add(fitGraph);
			form.Controls.Add(resultsBox);
		}

		private void formClosed(object sender, EventArgs e)
		{
			form = null;
		}

		protected void initialiseKernel()
		{
			if (kernel == null) kernel = MathematicaService.GetKernel();
		}
	}
}
