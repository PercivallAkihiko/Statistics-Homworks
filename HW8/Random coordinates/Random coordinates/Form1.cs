using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Random_coordinates
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Global.TOTAL = Int32.Parse(textBox1.Text);

            Chart chart = new Chart(pictureBox1.Width, pictureBox1.Height, 1000, 1000, pictureBox1);
            Chart chartXDistribution = new Chart(pictureBox2.Width, pictureBox2.Height, 0, 0, pictureBox2);
            Chart chartYDistribution = new Chart(pictureBox3.Width, pictureBox3.Height, 0, 0, pictureBox3);

            PointDistribution pointDistribution = new PointDistribution(Global.TOTAL, 500);
            chart.InsertPoints(pointDistribution.PointCollection, 2, Color.Black);

            chartXDistribution.InsertCandles(pointDistribution.EmpiricalDistributionX, pointDistribution.EmpiricalDistributionX.Count * 2, pointDistribution.MaxX, Color.Green);
            chartYDistribution.InsertCandles(pointDistribution.EmpiricalDistributionY, pointDistribution.MaxY, pointDistribution.EmpiricalDistributionY.Count * 2, Color.Green, false);
        }
    }

    public static class Global
    {
        public static int TOTAL = 10000;
        public static int MIN_RADIANT = 1; 
        public static int MAX_RADIANT = 628319;
        public static int PRECISION_RAD = 100000;
        public static int MAX_RADIUS = 500;

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

    public class PointDistribution
    {
        public List<(double, double)> PointCollection { get; set; }
        public List<(int, int)> EmpiricalDistributionX { get; set; }
        public List<(int, int)> EmpiricalDistributionY { get; set; }
        public int MaxX = 0;
        public int MaxY = 0;

        public PointDistribution(int n, int offset)
        {
            PointCollection = new List<(double, double)>();
            EmpiricalDistributionX = new List<(int, int)>();
            EmpiricalDistributionY = new List<(int, int)>();            

            List<int> empiricalDistributionXValue = new List<int>();
            List<int> empiricalDistributionYValue = new List<int>();

            for(int i=0; i<100; i++)
            {
                empiricalDistributionXValue.Add(0);
                empiricalDistributionYValue.Add(0);
            }

            for (int i = 0; i < n; i++)
            {                
                double angle = (double)(Global.rng.Next(Global.MIN_RADIANT, Global.MAX_RADIANT)) / (double)Global.PRECISION_RAD;
                int radius = Global.rng.Next(1, Global.MAX_RADIUS);

                double x = ((double)radius * Math.Cos(angle)) + (double)offset;
                double y = ((double)radius * Math.Sin(angle)) + (double)offset;

                empiricalDistributionXValue[(int)x/(int)10] += 1;
                empiricalDistributionYValue[(int)y/(int)10] += 1;
                
                PointCollection.Add((x, y));
            }

            for (int i=0; i<100; i++)
            {
                EmpiricalDistributionX.Add((i+1, empiricalDistributionXValue[i]));
                EmpiricalDistributionY.Add((i+1, empiricalDistributionYValue[i]));

                if(MaxX < empiricalDistributionXValue[i]) { MaxX = empiricalDistributionXValue[i]; }
                if(MaxY < empiricalDistributionYValue[i]) { MaxY = empiricalDistributionYValue[i]; }

                Debug.WriteLine(EmpiricalDistributionX[i]);
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

            //_Graphics.DrawRectangle(new Pen(Color.Black, 1), 0, 0, Width - 1, Height - 1);

            MaxX = maxX;
            MaxY = maxY;
            MinX = 0;
            MinY = 0;

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
        public void DrawCandle(int x, int y, int thickness, Color color, bool vertical=true)
        {
            Pen pen = new Pen(color, thickness);
            if (vertical) { DrawLine(ConvertCoordinates((x, 0)), ConvertCoordinates((x, y)), pen); }
            else { DrawLine(ConvertCoordinates((0, x)), ConvertCoordinates((y, x)), pen); }
        }
        public void DrawFilledCircle((int, int) point, int radius, SolidBrush brush)
        {
            _Graphics.FillEllipse(brush, point.Item1, point.Item2, radius, radius);
        }

        public (int, int) ConvertCoordinates((int, int) point)
        {
            float x = ((float)(point.Item1 - MinX) / (float)(MaxX - MinX)) * Width;
            float y = Height - (((float)(point.Item2 - MinY) / (float)(MaxY - MinY)) * Height);

            return ((int)x, (int)y);
        }
        public (int, int) ConvertCoordinates((double, double) point)
        {
            float x = ((float)(point.Item1 - MinX) / (float)(MaxX - MinX)) * Width;
            float y = Height - (((float)(point.Item2 - MinY) / (float)(MaxY - MinY)) * Height);

            return ((int)x, (int)y);
        }
        public void InsertCandles(List<(int, int)> collection, int maxX, int maxY, Color color, bool vertical = true)
        {
            CandlesMemory = (collection, color);
            int thickness;
            if (vertical) { thickness = (Width / collection.Count) + 1; }
            else { thickness = (Height / collection.Count) + 1; }
            MaxX = maxX;
            MaxY = maxY;

            Debug.WriteLine(MaxX);
            Debug.WriteLine(MaxY);

            foreach ((int, int) item in collection) {
                //Debug.WriteLine($"{(item.Item1 * 2)-1} {item.Item2}");
                DrawCandle((item.Item1 * 2) - 1, item.Item2, thickness, color, vertical); 
            }
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
        public void InsertPoints(List<(double, double)> collection , int radius, Color color)
        {
            SolidBrush brush = new SolidBrush(color);
            foreach ((double, double) point in collection) { DrawFilledCircle(ConvertCoordinates(point), radius, brush); }
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
