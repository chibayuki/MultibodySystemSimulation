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
            Me.Theme = Com.WinForm.Theme.Black;

            Me.Location = new Point(0, 0);
            Me.Size = new Size(1500, 1000);

            Me.Loading += Me_Loading;
            Me.Loaded += Me_Loaded;
        }

        private void Me_Loading(object sender, EventArgs e)
        {
            List<Particle> particles = new List<Particle>();

            /*for (int i = 0; i < 3; i++)
            {
                particles.Add(new Particle(
                    Com.Statistics.RandomDouble(1E6, 1E7),
                    new Com.PointD3D(Com.Statistics.RandomInteger(500, 1000), Com.Statistics.RandomInteger(300, 700), 0),
                    new Com.PointD3D(Com.Statistics.RandomDouble(-0.001, 0.001), Com.Statistics.RandomDouble(-0.001, 0.001), 0)
                    ));
            }*/

            particles.Add(new Particle(1E7, new Com.PointD3D(700, 500, 0), new Com.PointD3D(0, 0.0012, 0)));
            particles.Add(new Particle(5E6, new Com.PointD3D(800, 500, 0), new Com.PointD3D(0, -0.0021, 0)));
            particles.Add(new Particle(1E6, new Com.PointD3D(400, 500, 0), new Com.PointD3D(0, -0.0014, 0)));

            _MultibodySystem = new MultibodySystem(1000000, particles);
        }

        private void Me_Loaded(object sender, EventArgs e)
        {
            Timer_Graph.Enabled = true;
        }

        MultibodySystem _MultibodySystem = null;

        Bitmap _MultibodyBitmap = null;

        private void _UpdateMultibodyBitmap()
        {
            if (_MultibodyBitmap != null)
            {
                _MultibodyBitmap.Dispose();
            }

            _MultibodyBitmap = new Bitmap(Math.Max(1, Panel_Main.Width), Math.Max(1, Panel_Main.Height));

            using (Graphics Grap = Graphics.FromImage(_MultibodyBitmap))
            {
                Grap.SmoothingMode = SmoothingMode.AntiAlias;
                Grap.Clear(Color.Black);

                RectangleF bitmapBounds = new RectangleF(new PointF(), _MultibodyBitmap.Size);

                List<Particle> particles = _MultibodySystem.LastFrame.Particles;

                int FrameCount = _MultibodySystem.FrameCount;

                for (int i = 0; i < particles.Count; i++)
                {
                    Com.PointD location = CoordinateTransform(particles[i].Location);
                    Com.ColorX color = Com.ColorX.FromHSL((31 * i) % 360, 100, 70);

                    if (Com.Geometry.PointIsVisibleInRectangle(location, bitmapBounds))
                    {
                        using (Brush Br = new SolidBrush(color.ToColor()))
                        {
                            Grap.FillEllipse(Br, new RectangleF((float)location.X - 2.5F, (float)location.Y - 2.5F, 5, 5));
                        }
                    }

                    for (int j = FrameCount - 1; j >= 1000; j -= 1000)
                    {
                        Com.PointD pt1 = CoordinateTransform(_MultibodySystem.Frame(j).Particles[i].Location);
                        Com.PointD pt2 = CoordinateTransform(_MultibodySystem.Frame(j - 1000).Particles[i].Location);

                        if (Com.Geometry.LineIsVisibleInRectangle(pt1, pt2, bitmapBounds))
                        {
                            Com.Painting2D.PaintLine(_MultibodyBitmap, pt1, pt2, color.AtOpacity(100 * j / FrameCount).ToColor(), 1, true);
                        }
                    }
                }
            }
        }

        private void _RepaintMultibodyBitmap()
        {
            _UpdateMultibodyBitmap();

            if (_MultibodyBitmap != null)
            {
                Panel_Main.CreateGraphics().DrawImage(_MultibodyBitmap, new Point(0, 0));
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
                e.Graphics.DrawImage(_MultibodyBitmap, new Point(0, 0));
            }
        }

        private void Timer_Graph_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 20000; i++)
            {
                _MultibodySystem.NextMoment(1);
            }

            _RepaintMultibodyBitmap();
        }

        private Com.PointD CoordinateTransform(Com.PointD3D pt)
        {
            return pt.XY;
        }
    }
}