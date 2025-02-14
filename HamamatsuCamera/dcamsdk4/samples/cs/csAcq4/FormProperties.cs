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
    public partial class FormProperties : Form
    {
        // ----------------

        internal class dataprop
        {
            MyDcamProp _dcamprop;
            string _propname;
            double _propvalue;
            Dictionary<string,double> _dicttextvalue;
            Dictionary<double,string> _dictvaluetext;

            public string Name {
                get {
                    return _propname;
                }
            }
            public string Value {
                get {
                    if( _dictvaluetext == null )
                        return string.Format("{0}",_propvalue);

                    if( ! _dictvaluetext.ContainsKey(_propvalue))
                    {
                        // this should not happen
                        return string.Format("{0}",_propvalue);
                    }

                    return _dictvaluetext[_propvalue];
                }
                set {
                    if( _dicttextvalue == null || ! _dicttextvalue.ContainsKey(value) )
                    {
                        _propvalue = double.Parse(value);
                    }
                    else
                    {
                        _propvalue = _dicttextvalue[value];
                    }
                    _dcamprop.setgetvalue(ref _propvalue);
                }
            }

            public dataprop(MyDcamProp myprop)
            {
                _dcamprop = myprop;
                _propname = _dcamprop.getname();
                _dcamprop.getvalue(ref _propvalue);
            }

            public bool is_readonly() 
            {
                return _dcamprop.is_attr_readonly();
            }

            public DataGridViewCell get_cell()
            {
                DataGridViewCell cell;

                _dcamprop.update_attr();

                if( _dcamprop.is_attrtype_mode() )
                {
                    _dicttextvalue = new Dictionary<string, double>();
                    _dictvaluetext = new Dictionary<double, string>();

                    DataGridViewComboBoxCell cbcell = new DataGridViewComboBoxCell();
                    double value;
                    value = _dcamprop.m_attr.valuemin;

                    while( value <= _dcamprop.m_attr.valuemax )
                    {
                        string text;
                        text = _dcamprop.getvaluetext(value);
                        _dicttextvalue.Add(text, value);
                        _dictvaluetext.Add(value, text);
                        cbcell.Items.Add(text);
                        if( ! _dcamprop.queryvalue_next(ref value) )
                            break;
                    }

                    cbcell.Value = _dictvaluetext[_propvalue];

                    cell = cbcell;
                }
                else
                {
                    _dicttextvalue = null;
                    _dictvaluetext = null;

                    DataGridViewTextBoxCell tbcell = new DataGridViewTextBoxCell();
                    cell = tbcell;
                }

                return cell;
            }
        }
        
        // ----------------

        public FormProperties()
        {
            InitializeComponent();
        }

        public void set_mydcam(ref MyDcam mydcam)
        {
            // update DataSource of DataGridView
            List<dataprop> listprop = new List<dataprop>();

            MyDcamProp myprop = new MyDcamProp(mydcam, DCAMIDPROP.ZERO);
            while( myprop.nextid() )
            {
                listprop.Add(new dataprop(myprop.Clone()));
            }
            DataGridViewProp.DataSource = listprop;
        }

        public void update_properties()
        {
            // update cells in DataGridView
            DataGridViewProp.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            foreach( DataGridViewRow row in DataGridViewProp.Rows )
            {
                int i;
                i = row.Index;

                dataprop aPropdata = row.DataBoundItem as dataprop;
                if( aPropdata != null )
                {
                    DataGridViewCell cell = aPropdata.get_cell();
                    DataGridViewProp[1, i] = cell;
                    if( aPropdata.is_readonly() )
                        cell.ReadOnly = true;        // Readonly can change only after attached DataGridView
                }
            }
        }

        private void FormProperties_Load(object sender, EventArgs e)
        {
            // set DataGridViewProp style
            DataGridViewProp.AllowUserToAddRows = false;
            DataGridViewProp.AllowUserToDeleteRows = false;
            DataGridViewProp.RowHeadersVisible = false;
        }
    }
}
