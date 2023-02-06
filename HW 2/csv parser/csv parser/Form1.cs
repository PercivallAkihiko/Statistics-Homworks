using Microsoft.VisualBasic.FileIO;
using System.DirectoryServices;
using System.Text;
using System.Windows.Markup;

namespace csv_parser
{
    public partial class Form1 : Form
    {

        public int[][] values;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog choofdlog = new OpenFileDialog();
            choofdlog.Filter = "All Files (*.*)|*.*";
            choofdlog.FilterIndex = 1;
            choofdlog.Multiselect = true;

            if (choofdlog.ShowDialog() == DialogResult.OK)
            {
                string sFileName = choofdlog.FileName;
                using (TextFieldParser parser = new TextFieldParser(sFileName))
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
                        dataGridView1.Rows.Add(fields);
                    }
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int columnIndex = e.ColumnIndex;
            Dictionary<string, int> data = new Dictionary<string, int>();

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

            richTextBox1.Clear();
            foreach (var item in data)
                richTextBox1.AppendText($"{item.Key} - {item.Value} \n");

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}