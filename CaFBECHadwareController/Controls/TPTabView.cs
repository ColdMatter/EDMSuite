using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Data;

namespace CaFBECHadwareController.Controls
{
    public partial class TPTabView : CaFBECHadwareController.Controls.GenericView
    {
        protected TPTabController castController;
  
        public TPTabView(TPTabController controllerInstance) : base(controllerInstance)
        {
            InitializeComponent();
            castController = (TPTabController)controller;
        }


        public void SetTextField(Control box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }
        private delegate void SetTextDelegate(Control box, string text);
        private void SetTextHelper(Control box, string text)
        {
            box.Text = text;
        }

        private void TPTabView_Load(object sender, EventArgs e)
        {
            //castController.WindowLoaded();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void CryoON_Click(object sender, EventArgs e)
        {
            castController.ToggleCryo();
        }

        private void CycleButton_Click(object sender, EventArgs e)
        {
            castController.ToggleCycleSource();

        }

        private void HeaterON_Click(object sender, EventArgs e)
        {
            castController.ToggleHeater();
        }

        public void UpdateRenderedObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            obj.Invoke(new UpdateObjectDelegate<T>(UpdateObject), new object[] { obj, updateFunc });
        }

        private delegate void UpdateObjectDelegate<T>(T obj, Action<T> updateFunc) where T : Control;

        private void UpdateObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            updateFunc(obj);
        }

        private void StartReading_Click(object sender, EventArgs e)
        {
            castController.ToggleReading();
        }

        
    }
}
