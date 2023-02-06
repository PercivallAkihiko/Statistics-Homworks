using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Dictionary<string, int> protocolDict = new Dictionary<string, int>();
            Dictionary<string, int> ipDict = new Dictionary<string, int>();

            using (TextFieldParser parser = new TextFieldParser(files[0]))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                string[] fields2 = parser.ReadFields();
                foreach (string field in fields2)
                {
                    dataGridView1.Columns.Add(field, field);
                }
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    string protocol = fields[4];                    
                    string source = fields[2];
                    string destination = fields[3];
                    dataGridView1.Rows.Add(fields);

                    if (!protocolDict.ContainsKey(protocol))
                        protocolDict.Add(protocol, 0);
                    protocolDict[protocol] += 1;

                    if (!ipDict.ContainsKey(source)) ipDict.Add(source, 0);
                    if (!ipDict.ContainsKey(destination)) ipDict.Add(destination, 0);
                    ipDict[source] += 1;
                    ipDict[destination] += 1;

                    
                }
            }

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            computeChart(chart1, protocolDict);
            computeChart(chart2, ipDict);
        }

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
    }
}
