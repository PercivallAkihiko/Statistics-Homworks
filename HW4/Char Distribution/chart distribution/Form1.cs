using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace chart_distribution
{
    public class Global
    {
        public static int PERCENTAGE_ACCURACY = 10000;
        public static int TRIAL_NUMBER = 100;
        public static int COLLECTION_NUMBER = 10;
        public static List<DiceDistribution> DiceCollection = new List<DiceDistribution>();

        public static bool IsRelative = true;
        public static bool IsAbsolute = false;
        public static bool IsNormalized = false;
    }
    public partial class Form1 : Form
    {
        public Chart chart;
        public Chart relativeChart;
        public Chart absoluteChart;
        public Chart normalizedChart;
        public Form1()
        {
            InitializeComponent();
            chart = new Chart(pictureBox1.Width, pictureBox1.Height, 100, 100);
            relativeChart = new Chart(RelativeFrequency_Box.Width, RelativeFrequency_Box.Height, 100, 100);
            absoluteChart = new Chart(AbsoluteFrequency_Box.Width, AbsoluteFrequency_Box.Height, 100, 100);
            normalizedChart = new Chart(NormalizedFrequency_Box.Width, NormalizedFrequency_Box.Height, 100, 100);

            pictureBox1.Image = chart._Bitmap;            
            RelativeFrequency_Box.Image = relativeChart._Bitmap;            
            AbsoluteFrequency_Box.Image = absoluteChart._Bitmap;            
            NormalizedFrequency_Box.Image = normalizedChart._Bitmap;
        }

        private void RelativeFrequency_Click(object sender, EventArgs e)
        {
            Global.IsRelative = true;
            Global.IsAbsolute = false;
            Global.IsNormalized = false;

            chart = new Chart(pictureBox1.Width, pictureBox1.Height, Global.TRIAL_NUMBER, Global.PERCENTAGE_ACCURACY);
            pictureBox1.Image = chart._Bitmap;
            foreach(DiceDistribution dice in Global.DiceCollection)
            {
                chart.InsertCollection(dice.RelativeFrequency, dice._Color);
            }
        }

        private void AbsoluteFrequency_Click(object sender, EventArgs e)
        {
            Global.IsRelative = false;
            Global.IsAbsolute = true;
            Global.IsNormalized = false;

            chart = new Chart(pictureBox1.Width, pictureBox1.Height, Global.TRIAL_NUMBER, Global.TRIAL_NUMBER);
            pictureBox1.Image = chart._Bitmap;
            foreach (DiceDistribution dice in Global.DiceCollection)
            {
                chart.InsertCollection(dice.AbsoluteFrequency, dice._Color);
            }
        }

        private void NormalizedFrequency_Click(object sender, EventArgs e)
        {
            Global.IsRelative = false;
            Global.IsAbsolute = false;
            Global.IsNormalized = true;

            chart = new Chart(pictureBox1.Width, pictureBox1.Height, Global.TRIAL_NUMBER, Global.PERCENTAGE_ACCURACY);
            pictureBox1.Image = chart._Bitmap;
            foreach (DiceDistribution dice in Global.DiceCollection)
            {
                chart.InsertCollection(dice.NormalizedFrequency, dice._Color);
            }
        }

        private void Generate_Click(object sender, EventArgs e)
        {
            Global.DiceCollection = new List<DiceDistribution>();
            relativeChart = new Chart(RelativeFrequency_Box.Width, RelativeFrequency_Box.Height, Global.COLLECTION_NUMBER + 1, Global.PERCENTAGE_ACCURACY);
            absoluteChart = new Chart(AbsoluteFrequency_Box.Width, AbsoluteFrequency_Box.Height, Global.COLLECTION_NUMBER + 1, Global.TRIAL_NUMBER);
            normalizedChart = new Chart(NormalizedFrequency_Box.Width, NormalizedFrequency_Box.Height, Global.COLLECTION_NUMBER + 1, Global.PERCENTAGE_ACCURACY);

            pictureBox1.Image = chart._Bitmap;
            RelativeFrequency_Box.Image = relativeChart._Bitmap;
            AbsoluteFrequency_Box.Image = absoluteChart._Bitmap;
            NormalizedFrequency_Box.Image = normalizedChart._Bitmap;

            for (int i = 0; i < Global.COLLECTION_NUMBER; i++)
            {
                DiceDistribution dice = new DiceDistribution(Global.TRIAL_NUMBER, Global.PERCENTAGE_ACCURACY);
                Global.DiceCollection.Add(dice);
                System.Threading.Thread.Sleep(100);

                relativeChart.DrawCandle(i + 1, dice.LastRelative, 25, dice._Color);
                absoluteChart.DrawCandle(i + 1, dice.LastAbsolute, 25, dice._Color);
                normalizedChart.DrawCandle(i + 1, dice.LastNormalized, 25, dice._Color);
            }

            if (Global.IsRelative) RelativeFrequency_Click(sender, e);
            else if (Global.IsAbsolute) AbsoluteFrequency_Click(sender, e);
            else NormalizedFrequency_Click(sender, e);
        }
    }

    public class DiceDistribution
    {
        public Color _Color { get; set; }             
        public int TrialNumber { get; set; }
        public int PercentageAccuracy { get; set; } 
        public List<(int, int)> RelativeFrequency { get; set; }
        public List<(int, int)> AbsoluteFrequency { get; set; }
        public List<(int, int)> NormalizedFrequency { get; set; }

        public int LastRelative { get; set; }
        public int LastAbsolute { get; set; }
        public int LastNormalized { get; set; }
        public DiceDistribution(int trialNumber, int percentageAccuracy)
        {
            Random rand = new Random();

            _Color = Color.FromArgb(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256));            
            TrialNumber = trialNumber;
            PercentageAccuracy = percentageAccuracy;
            RelativeFrequency = new List<(int, int)>();
            AbsoluteFrequency = new List<(int, int)>();
            NormalizedFrequency = new List<(int, int)>();

            int head = 0;
            for (int i = 0; i < TrialNumber; i++)
            {
                if (rand.Next(0, 2) == 1)
                    head += 1;
                int percent = (int)((float)head / ((float)i + 1) * PercentageAccuracy);
                int normalized = (int)((float)head / ((float)Math.Sqrt(i + 1)) * (PercentageAccuracy / 100));                
                
                RelativeFrequency.Add((i + 1, percent));
                AbsoluteFrequency.Add((i + 1, head));
                NormalizedFrequency.Add((i + 1, normalized));
            }

            LastRelative = RelativeFrequency[RelativeFrequency.Count - 1].Item2;
            LastAbsolute = AbsoluteFrequency[AbsoluteFrequency.Count - 1].Item2;
            LastNormalized = NormalizedFrequency[NormalizedFrequency.Count - 1].Item2;
        }
    }
    public class Chart
    {
        public Bitmap _Bitmap { get; private set; }
        public Graphics _Graphics { get; private set; }
        private List<List<(int, int)>> Series { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MinX { get; set; }
        public int MinY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Chart(int width, int height, int x, int y)
        {
            _Bitmap = new Bitmap(width, height);
            _Graphics = Graphics.FromImage(_Bitmap);
            Series = new List<List<(int, int)>>();

            Width = width;
            Height = height;

            _Graphics.DrawRectangle(new Pen(Color.Black, 1), 0, 0, Width-1, Height-1);

            MaxX = x;
            MaxY = y;
            MinX = 0;
            MinY = 0;             
        }

        public void DrawLine(int x1, int y1, int x2, int y2, Pen pen)
        {            
            _Graphics.DrawLine(pen, x1, y1, x2, y2);
        }
        public void DrawLine((int, int) p1, (int, int) p2, Pen pen)
        {            
            _Graphics.DrawLine(pen, p1.Item1, p1.Item2, p2.Item1, p2.Item2);
        }
        public void DrawCandle(int x, int y, int thickness, Color color)
        {
            Pen pen = new Pen(color, thickness);
            DrawLine(ConvertCoordinates((x, 0)), ConvertCoordinates((x, y)), pen);
        }

        public (int, int) ConvertCoordinates((int, int) point)
        {
            float x = ((float)(point.Item1 - MinX) / (float)(MaxX - MinX)) * Width;            
            float y = Height - (((float)(point.Item2 - MinY) / (float)(MaxY - MinY)) * Height);

            float new_x = ((point.Item1 - MinX) / (MaxX - MinX)) * Width;
            float new_yy = Height - (((point.Item2 - MinY) / (MaxY - MinY)) * Height);

            return ((int) x, (int) y);
        }
        public void InsertCollection(List<(int, int)> collection, Color color)
        {
            Series.Add(collection);
            (int, int) buffer = collection[0];
            Pen pen = new Pen(color, 2);

            foreach((int, int) item in collection.Skip(1))
            {                               
                DrawLine(ConvertCoordinates(buffer), ConvertCoordinates(item), pen);
                buffer = item;
            }
        }
    }
}
