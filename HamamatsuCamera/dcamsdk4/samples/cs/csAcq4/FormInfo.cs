using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Hamamatsu.DCAM4;

namespace csAcq4
{
    public partial class FormInfo : Form
    {
        // ----------------

        internal class datainfo
        {
            string _infokind;
            string _infovalue;

            public string Kind
            {
                get
                {
                    return _infokind;
                }
            }
            public string Value
            {
                get
                {
                    return _infovalue;
                }
            }

            public datainfo(string kind, string value)
            {
                _infokind = kind;
                _infovalue = value;
            }
        }

        // ----------------

        public FormInfo()
        {
            InitializeComponent();
        }

        void add_info(ref List<datainfo> listinfo, ref MyDcam mydcam, DCAMIDSTR idstr, string strkind)
        {
            string strvalue = mydcam.dev_getstring(idstr);
            if (strvalue.Length > 0)
            {
                listinfo.Add(new datainfo(strkind, strvalue));
            }
        }

        public void set_mydcam(ref MyDcam mydcam)
        {
            // update DataSource of DataGridView
            List<datainfo> listinfo = new List<datainfo>();

            if (mydcam != null)
            {
                add_info(ref listinfo, ref mydcam, DCAMIDSTR.BUS, "BUS");
                add_info(ref listinfo, ref mydcam, DCAMIDSTR.CAMERAID, "CAMERAID");
                add_info(ref listinfo, ref mydcam, DCAMIDSTR.VENDOR, "VENDOR");
                add_info(ref listinfo, ref mydcam, DCAMIDSTR.MODEL, "MODEL");
                add_info(ref listinfo, ref mydcam, DCAMIDSTR.CAMERAVERSION, "CAMERA VERSION");
                add_info(ref listinfo, ref mydcam, DCAMIDSTR.DRIVERVERSION, "DRIVER VERSION");
                add_info(ref listinfo, ref mydcam, DCAMIDSTR.MODULEVERSION, "MODULE VERSION");
                add_info(ref listinfo, ref mydcam, DCAMIDSTR.DCAMAPIVERSION, "DCAM-API VERSION");
                add_info(ref listinfo, ref mydcam, DCAMIDSTR.CAMERA_SERIESNAME, "SERIES NAME");
            }
            DataGridViewInfo.DataSource = listinfo;
        }
    }
}
