using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeightDistribution
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
    }

    public static class Global
    {
        public static int TOTAL = 10000;
        public static int SAMPLE_NUMBER = 5;
        public static int SAMPLE_CARDINALITY = 100;
        public static int PRECISION = 10000;
        public static int MAX_WEIGHT = 180;
        public static int MIN_WEIGHT = 5;
        public static Random rng = new Random();
    }
}
