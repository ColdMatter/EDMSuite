using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using DAQ.Environment;

using ScanMaster.GUI;

namespace ScanMaster
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class ParameterHelper
	{
		private ParameterHelperWindow window;
		private String paramFilePath;
		private DataTable parameterTable;

		public void Initialise()
		{
			String settingsPath = (string)Environs.FileSystem.Paths["settingsPath"];
			paramFilePath = settingsPath + "\\ScanMaster\\parameters.bin";

			window = new ParameterHelperWindow(this);

			DeserializeSettings();
			BuildGUI();
			bool b = HasParameter("jony");
			double d = GetParameter("jony");
		}

		public void Exit()
		{
			SerializeSettings();
		}

		public bool HasParameter(String key)
		{
			DataRow row = parameterTable.Rows.Find(key);
			return (row != null);
		}

		public double GetParameter(String key)
		{
			DataRow row = parameterTable.Rows.Find(key);
			if (row !=null) return (double)row["value"];
			else return 0;
		}

		private void DeserializeSettings()
		{
			if (File.Exists(paramFilePath))
			{
				BinaryFormatter s = new BinaryFormatter();
                FileStream fs = new FileStream(paramFilePath, FileMode.Open);
				parameterTable = (DataTable)s.Deserialize(fs);
                fs.Close();
			}
			else
			{
				parameterTable = new DataTable("Parameters");
			
				DataColumn paramColumn = new DataColumn();
				paramColumn.DataType = System.Type.GetType("System.String");
				paramColumn.ColumnName = "parameter";
				paramColumn.ReadOnly = false;
				parameterTable.Columns.Add(paramColumn);

				parameterTable.PrimaryKey = new DataColumn[] {paramColumn};
 
				DataColumn valueColumn = new DataColumn();
				valueColumn.DataType = System.Type.GetType("System.Double");
				valueColumn.ColumnName = "value";
				valueColumn.ReadOnly = false;
				parameterTable.Columns.Add(valueColumn);
 
				DataRow testRow = parameterTable.NewRow();
				testRow["parameter"] = "test";
				testRow["value"] = 999.5;
				parameterTable.Rows.Add(testRow);
			}
		}

		private void BuildGUI()
		{
			System.Windows.Forms.DataGridTableStyle style = new System.Windows.Forms.DataGridTableStyle();
			style.MappingName = "Parameters";
			style.AlternatingBackColor = System.Drawing.Color.LightGray;
			style.BackColor = System.Drawing.Color.LightSteelBlue;
			style.PreferredColumnWidth = 160;
			window.dataGrid1.TableStyles.Clear();
			window.dataGrid1.TableStyles.Add(style);

			window.dataGrid1.DataSource = parameterTable;

			window.Show();
		}

		private void SerializeSettings()
		{
			BinaryFormatter s = new BinaryFormatter();
			s.Serialize(new FileStream(paramFilePath, FileMode.Create), parameterTable);
		}
	}
}
