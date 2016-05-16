using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LifeTile
{
    public partial class Form1 : Form, IMessageFilter
    {
        int xS = -1, yS = -1;
        int targetX = -1, targetY = -1;
        int kx = 0, ky = 0; 

        Bitmap image2;
        Bitmap imageCroped;
        float scaleDown = 0.7f;
        int curMode = 0;
        int latestIdx = 0;
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;
        Keys lastKeyPressed = Keys.None;

        public Form1()
        {
            InitializeComponent();
            Application.AddMessageFilter(this);
            this.FormClosed += (s, e) => Application.RemoveMessageFilter(this);
        }

        void DrawGrid()
        {
             Bitmap default_image = new Bitmap(pictureBox1.Image);
             default_image.MakeTransparent(Color.White);

            pictureBox1.Image = default_image;

            using (Graphics gr = Graphics.FromImage(pictureBox1.Image))
            {

                int dx = pictureBox1.Image.Width / 2;
                int dy = pictureBox1.Image.Height / 8;

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        gr.DrawRectangle(Pens.Black, dx * j, dy * i, dx - 2, dy - 2);

                        if(i == xS && j == yS)
                            gr.DrawRectangle(Pens.Red, dx * j, dy * i, dx - 2, dy - 2);

                    }
                }


                
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            image2 = new Bitmap(pictureBox2.Width, pictureBox2.Height);

            int dx = pictureBox1.Image.Width / 2;
            int dy = pictureBox1.Image.Height / 8;
            imageCroped = new Bitmap((int)(10 + dx * scaleDown), (int)(10 + dy * scaleDown));

            using (Graphics g = Graphics.FromImage(image2))
            {
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, pictureBox2.Width, pictureBox2.Height);
               
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
            //bmp.MakeTransparent(Color.Black);
            //bmp.Save("test.bmp");
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
              int dx = pictureBox1.Width / 2;
              int dy = pictureBox1.Height / 8;
             
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 2; j++)
                        if (e.X > j * dx && e.X - dx < j * dx && e.Y > i * dy && e.Y - dy < i * dy)
                        {
                            xS = i;
                            yS = j;
                        }

                this.Refresh();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            DrawGrid();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            int dx = pictureBox1.Image.Width / 2;
            int dy = pictureBox1.Image.Height / 8;
            
            e.Graphics.DrawImage(image2, 0, 0);

            
            int width = dx;
            int height = dy;
           RectangleF destinationRectCrop = new RectangleF(
               5,
               5,
               scaleDown * dx,
               scaleDown * dy);

            // Draw a portion of the image. Scale that portion of the image

            using (Graphics g = Graphics.FromImage(imageCroped))
            {
                g.Clear(Color.FromArgb(0, 0, 0, 0));
                if (curMode >= 8)
                {
                    Matrix mat = new Matrix();
                    var p = new PointF(scaleDown * dx / 2f - 1, scaleDown * dy / 2f - 1);
                    mat.RotateAt(45, p);
                    g.Transform = mat;
                }

                g.InterpolationMode = InterpolationMode.High;
            
                RectangleF sourceRect = new RectangleF(dx * yS + 3, dy * xS + 3, dx - 6, dy - 6);
                g.DrawImage(
                    pictureBox1.Image,
                    destinationRectCrop,
                    sourceRect,
                    GraphicsUnit.Pixel);
            }

            imageCroped.RotateFlip((RotateFlipType)(curMode % 8));
            e.Graphics.DrawImage(imageCroped, targetX - (int)(scaleDown * dx / 2) + kx, targetY - (int)(scaleDown * dy / 2) + ky);
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            targetX = e.X;
            targetY = e.Y;
            pictureBox2.Refresh();
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                curMode++;

                if (curMode >= 16)
                    curMode = 0;

                Text = "Mode = " + curMode.ToString();
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Graphics g = Graphics.FromImage(image2);
                int dx = pictureBox1.Image.Width / 2;
                int dy = pictureBox1.Image.Height / 8;

                g.DrawImage(imageCroped, targetX - (int)(scaleDown * dx / 2) + kx, targetY - (int)(scaleDown * dy / 2) + ky);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            image2.Save("latestIdx" + (latestIdx++).ToString() + ".png");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            latestIdx = -1;

            while (File.Exists("latestIdx" + (++latestIdx).ToString() + ".png")) ;
            
            if(latestIdx > 0)
                image2 = new Bitmap("latestIdx" + (latestIdx - 1).ToString() + ".png");

        }

        private void button2_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        public bool PreFilterMessage(ref Message m)
        {
            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
            {
                lastKeyPressed = keyData;
                switch (keyData)
                {
                    case Keys.Down:
                        ky++;
                        this.Refresh();
                        break;

                    case Keys.Up:
                        ky--;
                        this.Refresh();
                        break;

                    case Keys.Left:
                        kx--;
                        this.Refresh();
                        break;

                    case Keys.Right:
                        kx++;
                        this.Refresh();
                        break;

                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
