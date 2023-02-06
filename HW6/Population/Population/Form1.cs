using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Population
{
    public partial class Form1 : Form
    {
        public Chart _ChartMean;
        public Chart _ChartVariance;
        public List<WeightDistribution> WeightDistributions;
        public WeightDistribution _WeightDistribution;
        public Form1()
        {
            InitializeComponent();
            _ChartMean = new Chart(pictureBox1.Width, pictureBox1.Height, 0, pictureBox1);
            _ChartVariance = new Chart(pictureBox1.Width, pictureBox1.Height, 0, pictureBox2);

            textBox5.Text = Global.TOTAL.ToString();
            textBox1.Text = Global.SAMPLE_NUMBER.ToString();
            textBox2.Text = Global.SAMPLE_CARDINALITY.ToString();
            textBox3.Text = Global.MAX_WEIGHT.ToString();
            textBox4.Text = Global.MIN_WEIGHT.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Global.TOTAL = Int32.Parse(textBox5.Text);
            Global.SAMPLE_NUMBER = Int32.Parse(textBox1.Text);
            Global.SAMPLE_CARDINALITY = Int32.Parse(textBox2.Text);
            Global.MAX_WEIGHT = Int32.Parse(textBox3.Text);
            Global.MIN_WEIGHT = Int32.Parse(textBox4.Text);

            _WeightDistribution = new WeightDistribution(Global.TOTAL);
            WeightDistributions = new List<WeightDistribution>();
            float maxVar = 0;

            List<(int, int)> candlesMean = new List<(int, int)>();
            List<(int, int)> candlesVariance = new List<(int, int)>();

            for (int i = 0; i < Global.SAMPLE_NUMBER; i++)
            {
                List<int> sample = RandomSelection.SelectionSampling(_WeightDistribution.WeightList, Global.SAMPLE_CARDINALITY);
                WeightDistribution sampleDistribution = new WeightDistribution(sample);
                WeightDistributions.Add(sampleDistribution);
                if (maxVar < sampleDistribution.Variance) { maxVar = sampleDistribution.Variance; }
            }

            _ChartMean = new Chart(pictureBox1.Width, pictureBox1.Height, Global.MAX_WEIGHT * Global.PRECISION, pictureBox1);
            _ChartVariance = new Chart(pictureBox1.Width, pictureBox1.Height, (int)(maxVar * 2 * Global.PRECISION), pictureBox2);

            for (int i = 0; i < WeightDistributions.Count; i++)
            {
                candlesMean.Add((i + 1, (int)(WeightDistributions[i].Mean * Global.PRECISION)));
                candlesVariance.Add((i + 1, (int)(WeightDistributions[i].Variance * Global.PRECISION)));
                Debug.WriteLine($"Mean: {WeightDistributions[i].Mean}, Variance: {WeightDistributions[i].Variance}");
            }
            _ChartMean.InsertCandles(candlesMean, Color.Black);
            _ChartVariance.InsertCandles(candlesVariance, Color.Black);
            _ChartMean.InsertYLine((int)(_WeightDistribution.Mean * Global.PRECISION));
            _ChartVariance.InsertYLine((int)(_WeightDistribution.Variance * Global.PRECISION));

            _ChartMean.InsertText($"Mean {_WeightDistribution.Mean}", (0, (int)(_WeightDistribution.Mean * Global.PRECISION) + (10 * Global.PRECISION)));
            _ChartVariance.InsertText($"Variance {_WeightDistribution.Variance}", (0, (int)(_WeightDistribution.Variance * Global.PRECISION) + (500 * Global.PRECISION)));

            Debug.WriteLine("--- FINAL ---");
            Debug.WriteLine($"Mean: {_WeightDistribution.Mean}, Variance: {_WeightDistribution.Variance}");

            label8.Text = $"{_WeightDistribution.Mean}";
            label9.Text = $"{_WeightDistribution.Variance}";
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            _ChartMean.ResetDraw();
            _ChartVariance.ResetDraw();
        }

        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            _ChartMean.ResetDraw();
            _ChartVariance.ResetDraw();
        }
    }

    public static class Global
    {
        public static int TOTAL = 100000;
        public static int SAMPLE_NUMBER = 20;
        public static int SAMPLE_CARDINALITY = 2500;
        public static int PRECISION = 10000;
        public static int MAX_WEIGHT = 180;
        public static int MIN_WEIGHT = 5;

        public static Random rng = new Random();
    }

    public static class RandomSelection
    {        
        public static bool BoolProbability(double p)
        {
            double f = NextDouble(1, 100);
            return f < p;
        }

        public static double NextDouble(double minimum, double maximum  )
        {
            return Global.rng.NextDouble() * (maximum - minimum) + minimum;            
        }

        public static List<T> SelectionSampling<T>(List<T> l, int n) 
        {
            List<T> result = new List<T> ();
            for(int i=0; i<l.Count; i++){
                double p = ((double)(n - result.Count) / (double)(l.Count - i)) * 100;                
                if(BoolProbability(p)) { result.Add(l[i]); }
            }
            return result;
        }
    }

    public class WeightDistribution
    {
        public List<int> WeightList { get; set; }
        public float Mean { get; set; }
        public float Variance { get; set; }
        public WeightDistribution(int n)
        {
            int sum = 0;
            float varianceSum = 0;
            WeightList = new List<int> ();
            Random rnd = new Random();

            for (int i=0; i<n; i += 1)
            {
                int weight = Global.rng.Next(Global.MIN_WEIGHT, Global.MAX_WEIGHT);
                sum += weight;
                WeightList.Add(weight);
            }

            Mean = (float)sum / (float)WeightList.Count;
            foreach (int weight in WeightList){ varianceSum += ((weight - Mean) * (weight - Mean)); }            
            Variance = varianceSum / (float)WeightList.Count;
        }
        public WeightDistribution(List<int> weightList)
        {
            WeightList = weightList;
            int sum = 0;
            float varianceSum = 0;

            foreach (int weight in WeightList) { sum += weight; }
            Mean = (float)sum / (float)WeightList.Count;
            foreach (int weight in WeightList) { varianceSum += ((weight - Mean) * (weight - Mean)); }
            Variance = varianceSum / (WeightList.Count - 1);
        }
    }

    public class Chart
    {
        public Bitmap _Bitmap { get; private set; }
        public Graphics _Graphics { get; private set; }   
        public PictureBox _PictureBox { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MinX { get; set; }
        public int MinY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public (List<(int, int)>, Color) CandlesMemory;
        public int YMemory;
        public (string, (int, int)) TextMemory;

        public Chart(int width, int height, int precision, PictureBox pictureBox)
        {
            _Bitmap = new Bitmap(width, height);
            _Graphics = Graphics.FromImage(_Bitmap);
            _PictureBox = pictureBox;

            Width = width;
            Height = height;

            _Graphics.DrawRectangle(new Pen(Color.Black, 1), 0, 0, Width - 1, Height - 1);

            MaxY = precision;
            MinX = 0;
            MinY = 0;

            _PictureBox.Image = _Bitmap;
        }        

        public void ResetDraw()
        {    
            _Graphics.Clear(Color.White);
            Width = _PictureBox.Width;
            Height = _PictureBox.Height;
            _Graphics.DrawRectangle(new Pen(Color.Black, 1), 0, 0, Width - 1, Height - 1);
            if (MaxY != 0)
            {
                InsertCandles(CandlesMemory.Item1, CandlesMemory.Item2);
                InsertYLine(YMemory);
                InsertText(TextMemory.Item1, TextMemory.Item2);
            }
            _PictureBox.Image = _Bitmap;
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

            return ((int)x, (int)y);
        }
        public void InsertCandles(List<(int, int)> collection, Color color)
        {
            CandlesMemory = (collection, color);
            int thickness = Width / (collection.Count + 2);
            MaxX = collection.Count + 1;

            foreach ((int, int) item in collection) { DrawCandle(item.Item1, item.Item2, thickness, color); }
            _PictureBox.Image = _Bitmap;
        }        
        public void InsertYLine(int y)
        {
            YMemory = y;
            DrawLine(ConvertCoordinates((0, y)), ConvertCoordinates((MaxX, y)), new Pen(Color.Red, 2));
        }
        public void InsertText(string text, (int, int) coordinates)
        {
            TextMemory = (text, coordinates);
            using (Font myFont = new Font("Calibri", 12))
            {
                (int, int) point = ConvertCoordinates(coordinates);
                _Graphics.DrawString(text, myFont, Brushes.Red, new Point(point.Item1, point.Item2));
            }
            _PictureBox.Image = _Bitmap;
        }
    }
}
