/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2019 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.0.0.DEV.190906-0000

This file is part of "多体系统模拟" (MultibodySystemSimulation)

"多体系统模拟" (MultibodySystemSimulation) is released under the GPLv3 license
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing.Drawing2D;

namespace Multibody
{
    public partial class MainForm : Form
    {
        private Com.WinForm.FormManager Me;

        public Com.WinForm.FormManager FormManager
        {
            get
            {
                return Me;
            }
        }

        private void _Ctor(Com.WinForm.FormManager owner)
        {
            InitializeComponent();

            //

            if (owner != null)
            {
                Me = new Com.WinForm.FormManager(this, owner);
            }
            else
            {
                Me = new Com.WinForm.FormManager(this);
            }

            //

            FormDefine();
        }

        public MainForm()
        {
            _Ctor(null);
        }

        public MainForm(Com.WinForm.FormManager owner)
        {
            _Ctor(owner);
        }

        private void FormDefine()
        {
            Me.Caption = Application.ProductName;
            Me.ShowCaptionBarColor = false;
            Me.EnableCaptionBarTransparent = false;
            Me.Theme = Com.WinForm.Theme.Black;
            Me.ThemeColor = Com.ColorManipulation.GetRandomColorX();
            Me.Location = new Point(0, 0);
            Me.Size = new Size(1500, 1000);

            Me.Loading += Me_Loading;
            Me.Loaded += Me_Loaded;
        }

        private void Me_Loading(object sender, EventArgs e)
        {
            List<Particle> particles = new List<Particle>();

            int h = Com.Statistics.RandomInteger(360);
            const int s = 100;
            const int v = 70;
            int i = 0;
            const int d = 37;

            particles.Add(new Particle(1E7, 7.815926418, new Com.PointD3D(700, 500, 4000), new Com.PointD3D(0, 0.0012, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            particles.Add(new Particle(5E6, 6.203504909, new Com.PointD3D(780, 500, 4000), new Com.PointD3D(0, -0.0021, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            particles.Add(new Particle(1E6, 3.627831679, new Com.PointD3D(440, 500, 4000), new Com.PointD3D(0, -0.0016, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            particles.Add(new Particle(5E4, 1.336504618, new Com.PointD3D(420, 500, 4000), new Com.PointD3D(0, -0.0029, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            particles.Add(new Particle(2E5, 2.121568836, new Com.PointD3D(1150, 500, 4000), new Com.PointD3D(0, 0.0017, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            particles.Add(new Particle(1E4, 0.781592642, new Com.PointD3D(1170, 500, 4000), new Com.PointD3D(0, 0.0024, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            particles.Add(new Particle(2E4, 0.984745022, new Com.PointD3D(320, 500, 4000), new Com.PointD3D(0, 0.0017, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));

            _MultibodySystem = new MultibodySystem(1, 1000, 1000000, particles);
        }

        private void Me_Loaded(object sender, EventArgs e)
        {
            Timer_Graph.Enabled = true;
        }

        private MultibodySystem _MultibodySystem = null;
        private FpsCounter _FpsCounter = new FpsCounter(256);

        private Bitmap _MultibodyBitmap = null;

        private void _UpdateMultibodyBitmap()
        {
            if (_MultibodyBitmap != null)
            {
                _MultibodyBitmap.Dispose();
            }

            _MultibodyBitmap = new Bitmap(Math.Max(1, Me.Width), Math.Max(1, Me.Height));

            using (Graphics Grap = Graphics.FromImage(_MultibodyBitmap))
            {
                Grap.SmoothingMode = SmoothingMode.AntiAlias;
                Grap.Clear(Color.Black);

                RectangleF bitmapBounds = new RectangleF(new PointF(), _MultibodyBitmap.Size);

                List<Particle> particles = _MultibodySystem.LatestFrame.Particles;

                int FrameCount = _MultibodySystem.FrameCount;

                for (int i = 0; i < particles.Count; i++)
                {
                    Com.PointD location = CoordinateTransform(particles[i].Location);

                    for (int j = FrameCount - 1; j >= 1; j--)
                    {
                        Com.PointD pt1 = CoordinateTransform(_MultibodySystem.Frame(j).Particles[i].Location);
                        Com.PointD pt2 = CoordinateTransform(_MultibodySystem.Frame(j - 1).Particles[i].Location);

                        if (Com.Geometry.LineIsVisibleInRectangle(pt1, pt2, bitmapBounds))
                        {
                            Com.Painting2D.PaintLine(_MultibodyBitmap, pt1, pt2, Color.FromArgb(255 * j / FrameCount, particles[i].Color), 1, true);
                        }
                    }
                }

                for (int i = 0; i < particles.Count; i++)
                {
                    Com.PointD location = CoordinateTransform(particles[i].Location);

                    if (Com.Geometry.PointIsVisibleInRectangle(location, bitmapBounds))
                    {
                        using (Brush Br = new SolidBrush(particles[i].Color))
                        {
                            float r = Math.Max(1, (float)(new Com.PointD(Screen.PrimaryScreen.Bounds.Size).Module * particles[i].Radius / particles[i].Location.Z));

                            Grap.FillEllipse(Br, new RectangleF((float)location.X - r, (float)location.Y - r, r * 2, r * 2));
                        }
                    }
                }

                Grap.DrawString("D: " + _MultibodySystem.DynamicFPS.FrameRate.ToString("N1") + " FPS", new Font("微软雅黑", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 134), new SolidBrush(Color.Silver), new Point(0, Me.CaptionBarHeight));
                Grap.DrawString("L: " + _MultibodySystem.LocusFPS.FrameRate.ToString("N1") + " FPS", new Font("微软雅黑", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 134), new SolidBrush(Color.Silver), new Point(0, Me.CaptionBarHeight + 25));
                Grap.DrawString("A: " + _FpsCounter.FrameRate.ToString("N1") + " FPS", new Font("微软雅黑", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 134), new SolidBrush(Color.Silver), new Point(0, Me.CaptionBarHeight + 50));

                _FpsCounter.Update();
            }
        }

        private void _RepaintMultibodyBitmap()
        {
            _UpdateMultibodyBitmap();

            if (_MultibodyBitmap != null)
            {
                Me.CaptionBarBackgroundImage = _MultibodyBitmap;

                Panel_Main.CreateGraphics().DrawImage(_MultibodyBitmap, new Point(0, -Me.CaptionBarHeight));
            }
        }

        private void Panel_Main_Paint(object sender, PaintEventArgs e)
        {
            if (_MultibodyBitmap == null)
            {
                _UpdateMultibodyBitmap();
            }

            if (_MultibodyBitmap != null)
            {
                Me.CaptionBarBackgroundImage = _MultibodyBitmap;

                e.Graphics.DrawImage(_MultibodyBitmap, new Point(0, -Me.CaptionBarHeight));
            }
        }

        private void Timer_Graph_Tick(object sender, EventArgs e)
        {
            _MultibodySystem.NextMoment(10000);

            _RepaintMultibodyBitmap();
        }

        private Com.PointD CoordinateTransform(Com.PointD3D pt)
        {
            return pt.ProjectToXY(new Com.PointD3D(Me.Width / 2, Me.Height / 2, 0), new Com.PointD(Screen.PrimaryScreen.Bounds.Size).Module);
        }
    }
}