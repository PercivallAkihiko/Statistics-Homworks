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
        
        private void pictureBox1_Click(object sender, EventArgs e)
        {            
            //_MovableRectangle._Rectangle = new Rectangle(100, 100, 150, 150);
            //_MovableRectangle.Draw();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) =>_MovableRectangle.MouseDown(e);        

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _MovableRectangle.isMoving = false;
            _MovableRectangle.isResizing = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) => _MovableRectangle.Moving(e.X, e.Y);

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e) => _MovableRectangle.MouseWheel(e);
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

        public bool isMoving { get; set; }
        public bool isResizing { get; set; }

        public MovableRectangle(int width, int heigth, Action updatePictureBox)
        {
            _BitMap = new Bitmap(width, heigth);
            _Graphics = Graphics.FromImage(_BitMap);
            _Rectangle = new Rectangle(10, 10, 50, 50);
            _UpdatePictureBox = updatePictureBox;

            isMoving = false;
        }

        public void Draw()
        {
            _Graphics.Clear(Color.White);
            _Graphics.DrawRectangle(new Pen(Color.Black, 1), _Rectangle);
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

        public void MouseWheel(MouseEventArgs e)
        {
            if(e.Delta > 0 & _Rectangle.Contains(e.X, e.Y))
            {
                _Rectangle = new Rectangle(_Rectangle.X - 5, _Rectangle.Y - 5, _Rectangle.Width + 10, _Rectangle.Height + 10);    
                Draw();
            }
            else if(e.Delta < 0 & _Rectangle.Contains(e.X, e.Y)){
                _Rectangle = new Rectangle(_Rectangle.X + 5, _Rectangle.Y + 5, _Rectangle.Width - 10, _Rectangle.Height - 10);
                Draw();
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
                Debug.WriteLine($"{newHeight} - {newWidth}");
                Draw();
            }
        }

    }
}