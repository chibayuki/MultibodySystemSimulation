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

using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Threading;

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
            Me.Closed += Me_Closed;
        }

        private void Me_Loading(object sender, EventArgs e)
        {
            int h = Com.Statistics.RandomInteger(360);
            const int s = 100;
            const int v = 70;
            const int d = 37;
            int i = 0;

            _Particles.Add(new Particle(1E7, 7.815926418, new Com.PointD3D(700, 500, 4000), new Com.PointD3D(0, 0.0012, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(5E6, 6.203504909, new Com.PointD3D(780, 500, 4000), new Com.PointD3D(0, -0.0021, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(1E6, 3.627831679, new Com.PointD3D(440, 500, 4000), new Com.PointD3D(0, -0.0016, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(5E4, 1.336504618, new Com.PointD3D(420, 500, 4000), new Com.PointD3D(0, -0.0029, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(2E5, 2.121568836, new Com.PointD3D(1150, 500, 4000), new Com.PointD3D(0, 0.0017, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(1E4, 0.781592642, new Com.PointD3D(1170, 500, 4000), new Com.PointD3D(0, 0.0024, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(2E4, 0.984745022, new Com.PointD3D(320, 500, 4000), new Com.PointD3D(0, 0.0017, 0), Com.ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
        }

        private void Me_Loaded(object sender, EventArgs e)
        {
            RedrawThreadStart();
        }

        private void Me_Closed(object sender, EventArgs e)
        {
            RedrawThreadStop();
        }

        List<Particle> _Particles = new List<Particle>();
        private MultibodySystem _MultibodySystem = null;

        private FrequencyCounter _FrameRateCounter = new FrequencyCounter();

        private double _TimeMag = 100000; // 时间倍率（秒/秒）。
        private double _DynamicsResolution = 1; // 动力学分辨率（秒）。
        private double _KinematicsResolution = 1000; // 运动学分辨率（秒）。
        private double _CacheSize = 1000000; // 缓存大小（秒）。

        private Bitmap _MultibodyBitmap = null;

        private void _UpdateMultibodyBitmap()
        {
            if (_MultibodyBitmap != null)
            {
                _MultibodyBitmap.Dispose();
            }

            _MultibodyBitmap = new Bitmap(Math.Max(1, Me.Width), Math.Max(1, Me.Height));

            if (_MultibodySystem != null)
            {
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

                    using (Brush Br = new SolidBrush(Color.Silver))
                    {
                        Font ft = new Font("微软雅黑", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 134);

                        Grap.DrawString("Dynamics:   " + _MultibodySystem.DynamicFrequencyCounter.Frequency.ToString("N1") + " FPS", ft, Br, new Point(0, Me.CaptionBarHeight));
                        Grap.DrawString("Kinematics: " + _MultibodySystem.KinematicsFrequencyCounter.Frequency.ToString("N1") + " FPS", ft, Br, new Point(0, Me.CaptionBarHeight + 25));
                        Grap.DrawString("Graphics:    " + _FrameRateCounter.Frequency.ToString("N1") + " FPS", ft, Br, new Point(0, Me.CaptionBarHeight + 50));
                        Grap.DrawString("Time:           " + Com.Text.GetLongTimeStringFromTimeSpan(TimeSpan.FromSeconds(_MultibodySystem.LatestFrame.Time)), ft, Br, new Point(0, Me.CaptionBarHeight + 75));
                    }

                    _FrameRateCounter.Update();
                }
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

        private Thread RedrawThread;

        private void RedrawThreadStart()
        {
            _MultibodySystem = new MultibodySystem(_DynamicsResolution, _KinematicsResolution, _CacheSize, _Particles);

            RedrawThread = new Thread(new ThreadStart(RedrawThreadEvent));
            RedrawThread.IsBackground = true;
            RedrawThread.Start();
        }

        private void RedrawThreadStop()
        {
            if (RedrawThread != null && RedrawThread.IsAlive)
            {
                RedrawThread.Abort();
            }
        }

        private void RedrawThreadEvent()
        {
            int KCount = 1;
            int SleepMS = 0;

            DateTime LastAdjust = DateTime.MinValue;
            Stopwatch Watch = new Stopwatch();

            while (true)
            {
                Watch.Restart();

                _MultibodySystem.NextMoment(_KinematicsResolution * KCount);

                Watch.Stop();

                double KSec = Math.Max(0.000001, Watch.ElapsedMilliseconds * 0.001) / KCount;

                Watch.Restart();

                this.Invoke(new Action(_RepaintMultibodyBitmap));

                Watch.Stop();

                double GSec = Math.Max(0.001, Watch.ElapsedMilliseconds * 0.001);

                double DFpsActual = _MultibodySystem.DynamicFrequencyCounter.Frequency;

                if ((DateTime.UtcNow - LastAdjust).TotalSeconds >= 1 && DFpsActual > 0)
                {
                    double DFpsExpect = _TimeMag / _DynamicsResolution;
                    double DFpsRatio = DFpsActual / DFpsExpect;

                    if (DFpsRatio > 1.1 || DFpsRatio < 0.9)
                    {
                        double KFpsExpect = _TimeMag / _KinematicsResolution;
                        double KTime = KFpsExpect * KSec;
                        double GFpsExpect = Math.Min(KFpsExpect, (1 - KTime) / GSec);

                        if (GFpsExpect > 0)
                        {
                            double GTime = GFpsExpect * GSec;
                            double SleepTime = 1 - KTime - GTime;

                            if (SleepTime > 0.001)
                            {
                                KCount = 1;
                                SleepMS = (int)Math.Round(SleepTime * 1000 / GFpsExpect);
                            }
                            else
                            {
                                KCount = (int)Math.Round(Math.Min(KFpsExpect, KFpsExpect / GFpsExpect));
                                SleepMS = 0;
                            }
                        }
                        else
                        {
                            KCount = (int)Math.Round(KFpsExpect);
                            SleepMS = 0;
                        }
                    }
                    else
                    {
                        if (DFpsRatio > 1.01)
                        {
                            if (KCount > 1)
                            {
                                KCount--;
                            }
                            else if (SleepMS < 1000)
                            {
                                SleepMS++;
                            }
                        }
                        else if (DFpsRatio < 0.99)
                        {
                            if (SleepMS > 0)
                            {
                                SleepMS--;
                            }
                            else if (_FrameRateCounter.Frequency > 1)
                            {
                                KCount++;
                            }
                        }
                    }

                    LastAdjust = DateTime.UtcNow;
                }

                if (SleepMS > 0)
                {
                    Thread.Sleep(SleepMS);
                }
            }
        }

        private Com.PointD CoordinateTransform(Com.PointD3D pt)
        {
            return pt.ProjectToXY(new Com.PointD3D(Me.Width / 2, Me.Height / 2, 0), new Com.PointD(Screen.PrimaryScreen.Bounds.Size).Module);
        }
    }
}