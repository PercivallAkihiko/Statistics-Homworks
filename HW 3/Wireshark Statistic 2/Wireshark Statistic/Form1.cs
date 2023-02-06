using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.VisualBasic.FileIO;


namespace Wireshark_Statistic
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Hello world");
        }

        private void computeChart(Chart chart, Dictionary<string, int> dict)
        {
            foreach (KeyValuePair<string, int> entry in dict)
            {
                chart.Series["Series1"].Points.AddXY(entry.Key, entry.Value);
            }

            foreach (DataPoint p in chart.Series["Series1"].Points)
            {                
                Debug.WriteLine(p.AxisLabel);
                p.Label = $"#PERCENT-{p.AxisLabel}";
            }
        }

        public void updateValue(int rowIndex, int columnIndex, int newValue)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                object obj = row.Cells[columnIndex].Value;
                if (obj == null)
                    continue;

                string value = obj.ToString();
                if (data.ContainsKey(value))
                    data[value] += 1;
                else
                    data.Add(value, 1);
            }
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Dictionary<string, int> protocolDict = new Dictionary<string, int>();
            Dictionary<string, Address> sourceDict = new Dictionary<string, Address>();
            Dictionary<string, Address> destinationDict = new Dictionary<string, Address>();

            using (TextFieldParser parser = new TextFieldParser(files[0]))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                string[] fields2 = parser.ReadFields();
                dataGridView1.Columns.Add("", "");
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    string protocol = fields[4];                    
                    string source = fields[2];
                    string destination = fields[3];
                    //dataGridView1.Rows.Add(fields);

                    if (!protocolDict.ContainsKey(protocol))
                        protocolDict.Add(protocol, 0);
                    protocolDict[protocol] += 1;

                    if (!sourceDict.ContainsKey(source)) sourceDict.Add(source, new Address(source, dataGridView1.RowCount - 1));         
                    if (!destinationDict.ContainsKey(destination)) destinationDict.Add(destination, new Address(destination, dataGridView1.ColumnCount - 1));
                    Address sourceObj= sourceDict[source];
                    Address destinationObj = destinationDict[destination];

                    if(!sourceObj.Check(destination)) sourceObj.AddNew(destination, destinationObj.Index);
                    if (!destinationObj.Check(source)) sourceObj.AddNew(source, sourceObj.Index);

                    sourceObj.Increase(destination);
                    destinationObj.Increase(source);

                    dataGridView1.Rows.Insert(dataGridView1.RowCount, 1);
                    dataGridView1.Columns.Add(destination, destination);
                }
            }

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            computeChart(chart1, protocolDict);
        }

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
    }
}
