namespace random_timer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (progressBar1.Value < 100)
                progressBar1.Increment(1);
            else
                progressBar1.Value = 0;
        }

        private void random_btn_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            int rInt = r.Next(0, 100);
            progressBar1.Value = rInt;
        }

        private void start_btn_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void stop_btn_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }
    }
}