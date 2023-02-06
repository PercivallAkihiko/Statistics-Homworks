using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prob_Distribution
{
    public partial class Form1 : Form
    {
        public Chart AbsoluteChart { get; set; }
        public Chart InterarrivalChart { get; set; }
        public ProbDistribution _ProbDistribution { get; set; }
        public Form1()
        {
            InitializeComponent();
            AbsoluteChart = new Chart(pictureBox1.Width, pictureBox1.Height, 0, 0, pictureBox1);
            InterarrivalChart = new Chart(pictureBox2.Width, pictureBox2.Height, 0, 0, pictureBox2);

            textBox1.Text = Global.TOTAL.ToString();
            textBox2.Text = Global.LAMBDA.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Global.TOTAL = Convert.ToInt32(textBox1.Text);
            Global.LAMBDA = Convert.ToInt32(textBox2.Text);

            AbsoluteChart = new Chart(pictureBox1.Width, pictureBox1.Height, Global.TOTAL, Global.TOTAL, pictureBox1);
            InterarrivalChart = new Chart(pictureBox2.Width, pictureBox2.Height, Global.TOTAL, Global.TOTAL / 2, pictureBox2);
            _ProbDistribution = new ProbDistribution(Global.LAMBDA, Global.TOTAL);

            AbsoluteChart.InsertCollection(_ProbDistribution.AbsoluteFrequency, Color.Black);
            InterarrivalChart.InsertCandles(_ProbDistribution.InterarrivalTimes, Color.Black);
            
        }
    }

    public static class Global
    {
        public static int TOTAL = 100;
        public static int LAMBDA = 20;
        public static int PRECISION = 10000;

        public static Random rng = new Random();
    }

    public static class RandomSelection
    {
        public static bool BoolProbability(double p)
        {
            double f = NextDouble(1, 100);
            return f < p;
        }

        public static double NextDouble(double minimum, double maximum)
        {
            return Global.rng.NextDouble() * (maximum - minimum) + minimum;
        }

        public static List<T> SelectionSampling<T>(List<T> l, int n)
        {
            List<T> result = new List<T>();
            for (int i = 0; i < l.Count; i++)
            {
                double p = ((double)(n - result.Count) / (double)(l.Count - i)) * 100;
                if (BoolProbability(p)) { result.Add(l[i]); }
            }
            return result;
        }
    }

    public class ProbDistribution
    {
        public List<(int, int)> AbsoluteFrequency { get; set; } 
        public List<(int, int)> InterarrivalTimes { get; set; }
        public ProbDistribution(int lambda, int total)
        {
            AbsoluteFrequency = new List<(int, int)>();
            InterarrivalTimes = new List<(int, int)>();

            int passed = 0;
            int interarrivalCount = 0;
            int interrarrivalX = 1;

            for (int i=0; i<total; i++)
            {
                int generated = Global.rng.Next(1, total);
                if(generated < lambda) { 
                    passed += 1;
                    if(interarrivalCount > 0)
                    {
                        InterarrivalTimes.Add((interrarrivalX, interarrivalCount));
                        interarrivalCount = 0;
                        interrarrivalX += 1;
                    }
                }
                else { interarrivalCount += 1; }
                AbsoluteFrequency.Add((i, passed));                
            }
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

        public Chart(int width, int height, int maxX, int maxY, PictureBox pictureBox)
        {
            _Bitmap = new Bitmap(width, height);
            _Graphics = Graphics.FromImage(_Bitmap);
            _PictureBox = pictureBox;

            Width = width;
            Height = height;

            _Graphics.DrawRectangle(new Pen(Color.Black, 1), 0, 0, Width - 1, Height - 1);

            MaxX = maxX;
            MaxY = maxY;
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

        public void InsertCollection(List<(int, int)> collection, Color color)
        {
            (int, int) buffer = collection[0];
            Pen pen = new Pen(color, 1);

            foreach ((int, int) item in collection.Skip(1))
            {
                DrawLine(ConvertCoordinates(buffer), ConvertCoordinates(item), pen);
                buffer = item;
            }
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
