/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2020 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.0.0.DEV.200702-0000

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

using AffineTransformation = Com.AffineTransformation;
using ColorManipulation = Com.ColorManipulation;
using ColorX = Com.ColorX;
using FrequencyCounter = Com.FrequencyCounter;
using Geometry = Com.Geometry;
using Painting2D = Com.Painting2D;
using PointD = Com.PointD;
using PointD3D = Com.PointD3D;
using Statistics = Com.Statistics;
using Texting = Com.Text;
using VectorType = Com.Vector.Type;
using FormManager = Com.WinForm.FormManager;
using Theme = Com.WinForm.Theme;

namespace Multibody
{
    public partial class MainForm : Form
    {
        #region 窗口定义

        private FormManager Me;

        public FormManager FormManager
        {
            get
            {
                return Me;
            }
        }

        private void _Ctor(FormManager owner)
        {
            InitializeComponent();

            //

            if (owner != null)
            {
                Me = new FormManager(this, owner);
            }
            else
            {
                Me = new FormManager(this);
            }

            //

            FormDefine();
        }

        public MainForm()
        {
            _Ctor(null);
        }

        public MainForm(FormManager owner)
        {
            _Ctor(owner);
        }

        private void FormDefine()
        {
            Me.Caption = Application.ProductName;
            Me.ShowCaptionBarColor = false;
            Me.EnableCaptionBarTransparent = false;
            Me.Theme = Theme.Black;
            Me.ThemeColor = ColorManipulation.GetRandomColorX();

            Me.Loading += Me_Loading;
            Me.Loaded += Me_Loaded;
            Me.Closed += Me_Closed;
            Me.Resize += Me_Resize;
            Me.SizeChanged += Me_SizeChanged;
            Me.ThemeChanged += Me_ThemeChanged;
            Me.ThemeColorChanged += Me_ThemeChanged;
        }

        #endregion

        #region 窗口事件回调

        private void Me_Loading(object sender, EventArgs e)
        {
            int h = Statistics.RandomInteger(360);
            const int s = 100;
            const int v = 70;
            const int d = 37;
            int i = 0;

            /*_Particles.Add(new Particle(1E7, 7.815926418, new PointD3D(700, 500, 4000), new PointD3D(0, 0.0012, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(5E6, 6.203504909, new PointD3D(780, 500, 4000), new PointD3D(0, -0.0021, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(1E6, 3.627831679, new PointD3D(440, 500, 4000), new PointD3D(0, -0.0016, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(5E4, 1.336504618, new PointD3D(420, 500, 4000), new PointD3D(0, -0.0029, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(2E5, 2.121568836, new PointD3D(1150, 500, 4000), new PointD3D(0, 0.0017, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(1E4, 0.781592642, new PointD3D(1170, 500, 4000), new PointD3D(0, 0.0024, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(2E4, 0.984745022, new PointD3D(320, 500, 4000), new PointD3D(0, 0.0017, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));*/

            /*for (int i = 0; i < 10; i++)
            {
                _Particles.Add(new Particle(Statistics.RandomDouble(2E5, 5E6), Statistics.RandomDouble(2, 5), new PointD3D(Statistics.RandomDouble(200, 1000), Statistics.RandomDouble(200, 600), Statistics.RandomDouble(-1000, 1000)), new PointD3D(Statistics.RandomDouble(0.0005, 0.001), Statistics.RandomDouble(2 * Math.PI), Statistics.RandomDouble(2 * Math.PI)).ToCartesian(), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            }*/

            _Particles.Add(new Particle(1E8, 5, new PointD3D(0, 0, 1000), new PointD3D(0, 0, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(1E3, 2, new PointD3D(0, -200, 1400), new PointD3D(0.001, 0.001, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
            _Particles.Add(new Particle(1E1, 2, new PointD3D(-200, 0, 2000), new PointD3D(0.0007, -0.0007, 0), ColorX.FromHSL((h + d * (i++)) % 360, s, v).ToColor()));
        }

        private void Me_Loaded(object sender, EventArgs e)
        {
            Me.OnThemeChanged();
            Me.OnSizeChanged();

            Label_OffsetX.MouseEnter += Label_ViewOperation_MouseEnter;
            Label_OffsetY.MouseEnter += Label_ViewOperation_MouseEnter;
            Label_OffsetZ.MouseEnter += Label_ViewOperation_MouseEnter;
            Label_RotateX.MouseEnter += Label_ViewOperation_MouseEnter;
            Label_RotateY.MouseEnter += Label_ViewOperation_MouseEnter;
            Label_RotateZ.MouseEnter += Label_ViewOperation_MouseEnter;

            Label_OffsetX.MouseLeave += Label_ViewOperation_MouseLeave;
            Label_OffsetY.MouseLeave += Label_ViewOperation_MouseLeave;
            Label_OffsetZ.MouseLeave += Label_ViewOperation_MouseLeave;
            Label_RotateX.MouseLeave += Label_ViewOperation_MouseLeave;
            Label_RotateY.MouseLeave += Label_ViewOperation_MouseLeave;
            Label_RotateZ.MouseLeave += Label_ViewOperation_MouseLeave;

            Label_OffsetX.MouseDown += Label_ViewOperation_MouseDown;
            Label_OffsetY.MouseDown += Label_ViewOperation_MouseDown;
            Label_OffsetZ.MouseDown += Label_ViewOperation_MouseDown;
            Label_RotateX.MouseDown += Label_ViewOperation_MouseDown;
            Label_RotateY.MouseDown += Label_ViewOperation_MouseDown;
            Label_RotateZ.MouseDown += Label_ViewOperation_MouseDown;

            Label_OffsetX.MouseUp += Label_ViewOperation_MouseUp;
            Label_OffsetY.MouseUp += Label_ViewOperation_MouseUp;
            Label_OffsetZ.MouseUp += Label_ViewOperation_MouseUp;
            Label_RotateX.MouseUp += Label_ViewOperation_MouseUp;
            Label_RotateY.MouseUp += Label_ViewOperation_MouseUp;
            Label_RotateZ.MouseUp += Label_ViewOperation_MouseUp;

            Label_OffsetX.MouseMove += Label_OffsetX_MouseMove;
            Label_OffsetY.MouseMove += Label_OffsetY_MouseMove;
            Label_OffsetZ.MouseMove += Label_OffsetZ_MouseMove;
            Label_RotateX.MouseMove += Label_RotateX_MouseMove;
            Label_RotateY.MouseMove += Label_RotateY_MouseMove;
            Label_RotateZ.MouseMove += Label_RotateZ_MouseMove;

            RedrawThreadStart();
        }

        private void Me_Closed(object sender, EventArgs e)
        {
            RedrawThreadStop();
        }

        private void Me_Resize(object sender, EventArgs e)
        {
            Panel_SideBar.Height = Panel_Main.Height;
            Panel_SideBar.Left = Panel_Main.Width - Panel_SideBar.Width;

            Panel_View.Size = Panel_Main.Size;
        }

        private void Me_SizeChanged(object sender, EventArgs e)
        {
            Me.OnResize();
        }

        private void Me_ThemeChanged(object sender, EventArgs e)
        {
            this.BackColor = Me.RecommendColors.FormBackground.ToColor();

            Panel_View.BackColor = Me.RecommendColors.FormBackground.ToColor();

            Panel_SideBar.BackColor = Me.RecommendColors.Background_DEC.ToColor();

            Label_OffsetX.ForeColor = Label_OffsetY.ForeColor = Label_OffsetZ.ForeColor = Label_RotateX.ForeColor = Label_RotateY.ForeColor = Label_RotateZ.ForeColor = Me.RecommendColors.Text_INC.ToColor();
            Label_OffsetX.BackColor = Label_OffsetY.BackColor = Label_OffsetZ.BackColor = Label_RotateX.BackColor = Label_RotateY.BackColor = Label_RotateZ.BackColor = Me.RecommendColors.Background_INC.ToColor();
        }

        #endregion

        #region 粒子和多体系统定义

        private double _DynamicsResolution = 1; // 动力学分辨率（秒），指期待每次求解动力学微分方程组的时间微元 dT，表现为仿真计算的精确程度。
        private double _KinematicsResolution = 1000; // 运动学分辨率（秒），指期待每次抽取运动学状态的时间间隔 ΔT，表现为轨迹绘制的平滑程度。
        private double _CacheSize = 1000000; // 缓存大小（秒），指缓存运动学状态的最大时间跨度，表现为轨迹长度。

        private double _TimeMag = 100000; // 时间倍率（秒/秒），指仿真时间流逝速度与真实时间流逝速度的比值，表现为动画的播放速度。

        private List<Particle> _Particles = new List<Particle>(); // 粒子列表。
        private MultibodySystem _MultibodySystem = null; // 多体系统。

        #endregion

        #region 仿射、投影和视图控制

        private AffineTransformation _AffineTransformation = null; // 当前使用的仿射变换。
        private AffineTransformation _AffineTransformationCopy = null; // 视图控制开始前使用的仿射变换的副本。

        private double _SpaceMag = 1; // 空间倍率（米/像素），指投影变换焦点附近每像素表示的长度。

        // 投影变换使用的焦距。
        //private double FocalLength => new PointD(Screen.PrimaryScreen.Bounds.Size).Module;
        private double FocalLength => 1000;

        // 视图中心（屏幕坐标系）。
        private PointD ViewCenter => new PointD(Panel_View.Width / 2, Panel_View.Height / 2);

        // 视图控制开始。
        private void ViewOperationStart()
        {
            _AffineTransformationCopy = _AffineTransformation.Copy();
        }

        // 视图控制停止。
        private void ViewOperationStop()
        {
            _AffineTransformationCopy = null;
        }

        // 视图控制类型。
        private enum ViewOperationType
        {
            OffsetX,
            OffsetY,
            OffsetZ,
            RotateX,
            RotateY,
            RotateZ
        }

        // 视图控制更新参数。
        private void ViewOperationUpdateParam(ViewOperationType type, double value)
        {
            if (value != 0)
            {
                AffineTransformation affineTransformation = _AffineTransformationCopy.Copy();

                if (type <= ViewOperationType.OffsetZ)
                {
                    switch (type)
                    {
                        case ViewOperationType.OffsetX: affineTransformation.Offset(0, value); break;
                        case ViewOperationType.OffsetY: affineTransformation.Offset(1, value); break;
                        case ViewOperationType.OffsetZ: affineTransformation.Offset(2, value); break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case ViewOperationType.RotateX: affineTransformation.Rotate(1, 2, value); break;
                        case ViewOperationType.RotateY: affineTransformation.Rotate(2, 0, value); break;
                        case ViewOperationType.RotateZ: affineTransformation.Rotate(0, 1, value); break;
                    }
                }

                _AffineTransformation = AffineTransformation.FromMatrixTransform(affineTransformation.ToMatrix(VectorType.ColumnVector, 3));
            }
        }

        // 世界坐标系转换到屏幕坐标系。
        private PointD WorldToScreen(PointD3D pt)
        {
            return pt.AffineTransformCopy(_AffineTransformation).ProjectToXY(PointD3D.Zero, FocalLength).ScaleCopy(1 / _SpaceMag).OffsetCopy(ViewCenter);
        }

        private const double _ShiftPerPixel = 1; // 每像素的偏移量。
        private const double _RadPerPixel = Math.PI / 180; // 每像素的旋转弧度。

        private int _CursorX = 0; // 鼠标指针 X 坐标。
        private bool _AdjustNow = false; // 是否正在调整。

        private void Label_ViewOperation_MouseEnter(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Me.RecommendColors.Button_DEC.ToColor();
        }

        private void Label_ViewOperation_MouseLeave(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Me.RecommendColors.Background_INC.ToColor();
        }

        private void Label_ViewOperation_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ((Label)sender).BackColor = Me.RecommendColors.Button_INC.ToColor();
                ((Label)sender).Cursor = Cursors.SizeWE;

                ViewOperationStart();

                _CursorX = e.X;
                _AdjustNow = true;
            }
        }

        private void Label_ViewOperation_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _AdjustNow = false;

                ViewOperationStop();

                ((Label)sender).BackColor = (Geometry.CursorIsInControl((Label)sender) ? Me.RecommendColors.Button_DEC.ToColor() : Me.RecommendColors.Background_INC.ToColor());
                ((Label)sender).Cursor = Cursors.Default;
            }
        }

        private void Label_OffsetX_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double off = (e.X - _CursorX) * _ShiftPerPixel;

                ViewOperationUpdateParam(ViewOperationType.OffsetX, off);
            }
        }

        private void Label_OffsetY_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double off = (e.X - _CursorX) * _ShiftPerPixel;

                ViewOperationUpdateParam(ViewOperationType.OffsetY, off);
            }
        }

        private void Label_OffsetZ_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double off = (e.X - _CursorX) * _ShiftPerPixel;

                ViewOperationUpdateParam(ViewOperationType.OffsetZ, off);
            }
        }

        private void Label_RotateX_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double rot = (e.X - _CursorX) * _RadPerPixel;

                ViewOperationUpdateParam(ViewOperationType.RotateX, rot);
            }
        }

        private void Label_RotateY_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double rot = (e.X - _CursorX) * _RadPerPixel;

                ViewOperationUpdateParam(ViewOperationType.RotateY, rot);
            }
        }

        private void Label_RotateZ_MouseMove(object sender, MouseEventArgs e)
        {
            if (_AdjustNow)
            {
                double rot = (e.X - _CursorX) * _RadPerPixel;

                ViewOperationUpdateParam(ViewOperationType.RotateZ, rot);
            }
        }

        #endregion

        #region 渲染

        private Bitmap _MultibodyBitmap = null; // 多体系统当前渲染的位图。

        // 将多体系统的当前状态渲染到位图。
        private void UpdateMultibodyBitmap()
        {
            if (_MultibodyBitmap != null)
            {
                _MultibodyBitmap.Dispose();
            }

            _MultibodyBitmap = new Bitmap(Math.Max(1, Panel_View.Width), Math.Max(1, Panel_View.Height + Me.CaptionBarHeight));

            if (_MultibodySystem != null)
            {
                using (Graphics Grap = Graphics.FromImage(_MultibodyBitmap))
                {
                    Grap.SmoothingMode = SmoothingMode.AntiAlias;
                    Grap.Clear(Panel_View.BackColor);

                    RectangleF bitmapBounds = new RectangleF(new PointF(), _MultibodyBitmap.Size);

                    List<Particle> particles = _MultibodySystem.LatestFrame.Particles;

                    int FrameCount = _MultibodySystem.FrameCount;

                    for (int i = 0; i < particles.Count; i++)
                    {
                        PointD location = WorldToScreen(particles[i].Location);

                        for (int j = FrameCount - 1; j >= 1; j--)
                        {
                            PointD pt1 = WorldToScreen(_MultibodySystem.Frame(j).Particles[i].Location);
                            PointD pt2 = WorldToScreen(_MultibodySystem.Frame(j - 1).Particles[i].Location);

                            if (Geometry.LineIsVisibleInRectangle(pt1, pt2, bitmapBounds))
                            {
                                Painting2D.PaintLine(_MultibodyBitmap, pt1, pt2, Color.FromArgb(255 * j / FrameCount, particles[i].Color), 1, true);
                            }
                        }
                    }

                    for (int i = 0; i < particles.Count; i++)
                    {
                        PointD location = WorldToScreen(particles[i].Location);

                        float radius = Math.Max(1, (float)(particles[i].Radius * FocalLength / particles[i].Location.Z));

                        if (Geometry.CircleInnerIsVisibleInRectangle(location, radius, bitmapBounds))
                        {
                            using (Brush Br = new SolidBrush(particles[i].Color))
                            {
                                Grap.FillEllipse(Br, new RectangleF((float)location.X - radius, (float)location.Y - radius, radius * 2, radius * 2));
                            }
                        }
                    }

                    using (Brush Br = new SolidBrush(Color.Silver))
                    {
                        Font ft = new Font("微软雅黑", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 134);

                        Grap.DrawString("Dynamics:   " + _MultibodySystem.DynamicFrequencyCounter.Frequency.ToString("N1") + " Hz", ft, Br, new Point(0, Me.CaptionBarHeight));
                        Grap.DrawString("Kinematics: " + _MultibodySystem.KinematicsFrequencyCounter.Frequency.ToString("N1") + " Hz", ft, Br, new Point(0, Me.CaptionBarHeight + 25));
                        Grap.DrawString("Graphics:    " + _FrameRateCounter.Frequency.ToString("N1") + " FPS", ft, Br, new Point(0, Me.CaptionBarHeight + 50));
                        Grap.DrawString("Time:           " + Texting.GetLongTimeStringFromTimeSpan(TimeSpan.FromSeconds(_MultibodySystem.LatestFrame.Time)), ft, Br, new Point(0, Me.CaptionBarHeight + 75));
                    }

                    _FrameRateCounter.Update();
                }
            }
        }

        // 渲染位图并重绘。
        private void RepaintMultibodyBitmap()
        {
            UpdateMultibodyBitmap();

            if (_MultibodyBitmap != null)
            {
                Me.CaptionBarBackgroundImage = _MultibodyBitmap;

                Panel_View.CreateGraphics().DrawImage(_MultibodyBitmap, new Point(0, -Me.CaptionBarHeight));
            }
        }

        private void Panel_View_Paint(object sender, PaintEventArgs e)
        {
            if (_MultibodyBitmap == null)
            {
                UpdateMultibodyBitmap();
            }

            if (_MultibodyBitmap != null)
            {
                Me.CaptionBarBackgroundImage = _MultibodyBitmap;

                e.Graphics.DrawImage(_MultibodyBitmap, new Point(0, -Me.CaptionBarHeight));
            }
        }

        #endregion

        #region 重绘线程和帧率控制

        private Thread _RedrawThread; // 重绘线程。

        private FrequencyCounter _FrameRateCounter = new FrequencyCounter(); // 重绘帧率（FPS）的频率计数器。

        // 重绘线程开始。
        private void RedrawThreadStart()
        {
            _MultibodySystem = new MultibodySystem(_DynamicsResolution, _KinematicsResolution, _CacheSize, _Particles);

            _AffineTransformation = AffineTransformation.Empty;

            _RedrawThread = new Thread(new ThreadStart(RedrawThreadEvent));
            _RedrawThread.IsBackground = true;
            _RedrawThread.Start();
        }

        // 重绘线程停止。
        private void RedrawThreadStop()
        {
            if (_RedrawThread != null && _RedrawThread.IsAlive)
            {
                _RedrawThread.Abort();
            }
        }

        // 重绘线程执行的事件。
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

                double KSecEachActual = Math.Max(0.000001, Watch.ElapsedMilliseconds * 0.001) / KCount;

                Watch.Restart();

                this.Invoke(new Action(RepaintMultibodyBitmap));

                Watch.Stop();

                double GSecEachActual = Math.Max(0.001, Watch.ElapsedMilliseconds * 0.001);

                double DFpsActual = _MultibodySystem.DynamicFrequencyCounter.Frequency;

                if ((DateTime.UtcNow - LastAdjust).TotalSeconds >= 1 && DFpsActual > 0)
                {
                    double DFpsExpect = _TimeMag / _DynamicsResolution;
                    double DFpsRatio = DFpsActual / DFpsExpect;

                    if (DFpsRatio > 1.1 || DFpsRatio < 0.9)
                    {
                        double KFpsExpect = _TimeMag / _KinematicsResolution;
                        double KSecTotalExpect = KFpsExpect * KSecEachActual;
                        double GFpsExpect = Math.Min(KFpsExpect, (1 - KSecTotalExpect) / GSecEachActual);

                        if (GFpsExpect > 0)
                        {
                            double GSecTotalExpect = GFpsExpect * GSecEachActual;
                            double SleepSecExpect = 1 - KSecTotalExpect - GSecTotalExpect;

                            if (SleepSecExpect > 0.001)
                            {
                                KCount = 1;
                                SleepMS = (int)Math.Round(SleepSecExpect * 1000 / GFpsExpect);
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

        #endregion
    }
}