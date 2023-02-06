using System.Diagnostics;
using System.DirectoryServices;
using System.Windows.Forms;

namespace Drag_Rectangle
{    
    public partial class Form1 : Form
    {
        public MovableRectangle _MovableRectangle;
        public Form1()
        {
            InitializeComponent();
            _MovableRectangle = new MovableRectangle(pictureBox1.Width, pictureBox1.Height, UpdatePictureBox);
            
            pictureBox1.Image = _MovableRectangle._BitMap;
            _MovableRectangle.Draw();
        }

        public void UpdatePictureBox() => pictureBox1.Image = _MovableRectangle._BitMap;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) =>_MovableRectangle.MouseDown(e);        

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _MovableRectangle.isMoving = false;
            _MovableRectangle.isResizing = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) => _MovableRectangle.Moving(e.X, e.Y);

        private void button1_Click(object sender, EventArgs e) => _MovableRectangle.ToggleHorizontal();        
    }

    public class MovableRectangle
    {
        public Bitmap _BitMap { get; set; }
        public Graphics _Graphics { get; set; }
        public Rectangle _Rectangle { get; set; }
        public int LastRectangleX { get; set; }
        public int LastRectangleY { get; set; }
        public int LastMouseX { get; set; }
        public int LastMouseY { get; set; }
        public int LastWidth{ get; set; }
        public int LastHeight { get; set; }
        public event Action _UpdatePictureBox;

        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MinX { get; set; }
        public int MinY { get; set; }

        public bool isMoving { get; set; }
        public bool isResizing { get; set; }

        public bool isHorizontal = false;

        private List<(int, int)> Series = new List<(int, int)>()
        {
            (1, 37),
            (2, 95),
            (3, 72),
            (4, 89),
            (5, 69),
            (6, 59),
            (7, 96),
            (8, 69),
            (9, 72),
            (10, 4),
        };

        public MovableRectangle(int width, int heigth, Action updatePictureBox)
        {
            _BitMap = new Bitmap(width, heigth);
            _Graphics = Graphics.FromImage(_BitMap);
            _Rectangle = new Rectangle(10, 10, 300, 300);
            _UpdatePictureBox = updatePictureBox;

            isMoving = false;

            MaxX = Series.Count + 1;
            MaxY = 100;
            MinX = 0;
            MinY = 0;
        }

        public void Draw()
        {
            _Graphics.Clear(Color.White);
            _Graphics.DrawRectangle(new Pen(Color.Black, 1), _Rectangle);

            foreach((int, int) pos in Series)
            {
                DrawCandle(pos.Item1, pos.Item2, Color.Black);
            }

            _UpdatePictureBox?.Invoke();
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (_Rectangle.Contains(e.X, e.Y))
            {
                LastMouseX = e.X;
                LastMouseY = e.Y;
                LastRectangleX = _Rectangle.X;
                LastRectangleY = _Rectangle.Y;
                LastWidth = _Rectangle.Width;
                LastHeight = _Rectangle.Height;

                if(e.Button == MouseButtons.Left) { isMoving = true; }
                else { isResizing = true; }
            }
        }

        public void Moving(int actualX, int actualY)
        {
            if (isMoving)
            {
                int newX = LastRectangleX + actualX - LastMouseX;
                int newY = LastRectangleY + actualY - LastMouseY;
                Rectangle oldRectangle = _Rectangle;

                _Rectangle = new Rectangle(newX, newY, oldRectangle.Width, oldRectangle.Height);
                Draw();
            }
            else if(isResizing)
            {
                int newWidth = LastWidth + actualX - LastMouseX;
                int newHeight = LastHeight + actualY - LastMouseY;

                _Rectangle = new Rectangle(LastRectangleX, LastRectangleY, newWidth, newHeight);
                Draw();
            }
        }

        public void DrawLine((int, int) p1, (int, int) p2, Pen pen)
        {
            _Graphics.DrawLine(pen, p1.Item1, p1.Item2, p2.Item1, p2.Item2);
        }

        public void DrawCandle(int x, int y, Color color)
        {            
            if (isHorizontal) { DrawLine(ConvertCoordinates((0, x)), ConvertCoordinates((y, x)), new Pen(color, _Rectangle.Height / (Series.Count + 2))); }
            else { DrawLine(ConvertCoordinates((x, 0)), ConvertCoordinates((x, y)), new Pen(color, _Rectangle.Width / (Series.Count + 2))); }
        }

        public (int, int) ConvertCoordinates((int, int) point)
        {
            float x = ((float)(point.Item1 - MinX) / (float)(MaxX - MinX)) * _Rectangle.Width;
            float y = _Rectangle.Height - (((float)(point.Item2 - MinY) / (float)(MaxY - MinY)) * _Rectangle.Height);

            return (_Rectangle.X + (int)x, _Rectangle.Y + (int)y);
        }

        public void ToggleHorizontal()
        {
            int buffer = MaxX;
            MaxX = MaxY;
            MaxY = buffer;
            if(isHorizontal) { isHorizontal = false; }
            else { isHorizontal = true; }

            Draw();
        }

    }
}