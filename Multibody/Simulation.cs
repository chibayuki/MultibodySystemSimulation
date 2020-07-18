/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2020 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.0.0.DEV.200718-0000

This file is part of "多体系统模拟" (MultibodySystemSimulation)

"多体系统模拟" (MultibodySystemSimulation) is released under the GPLv3 license
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

using AffineTransformation = Com.AffineTransformation;
using FrequencyCounter = Com.FrequencyCounter;
using Geometry = Com.Geometry;
using Painting2D = Com.Painting2D;
using PointD = Com.PointD;
using PointD3D = Com.PointD3D;
using Texting = Com.Text;
using VectorType = Com.Vector.Type;

namespace Multibody
{
    // 仿真。
    class Simulation
    {
        #region 构造函数

        public Simulation(Control redrawControl, Action redrawMethod, Func<Point> getOffsetMethod, Func<Size> getBitmapSizeMethod)
        {
            _RedrawControl = redrawControl;
            _RedrawMethod = redrawMethod;
            _GetOffsetMethod = getOffsetMethod;
            _GetBitmapSizeMethod = getBitmapSizeMethod;
        }

        #endregion

        #region 粒子和多体系统定义

        private double _DynamicsResolution = 1; // 动力学分辨率（秒），指期待每次求解动力学微分方程组的时间微元 dT，表现为仿真计算的精确程度。
        private double _KinematicsResolution = 1000; // 运动学分辨率（秒），指期待每次抽取运动学状态的时间间隔 ΔT，表现为轨迹绘制的平滑程度。
        private double _CacheSize = 1000000; // 缓存大小（秒），指缓存运动学状态的最大时间跨度，表现为轨迹长度。

        private double _TimeMag = 100000; // 时间倍率（秒/秒），指仿真时间流逝速度与真实时间流逝速度的比值，表现为动画的播放速度。

        private List<Particle> _Particles = new List<Particle>(); // 粒子列表。
        private MultibodySystem _MultibodySystem = null; // 多体系统。

        public double DynamicsResolution
        {
            get
            {
                return _DynamicsResolution;
            }

            set
            {
                _DynamicsResolution = value;
            }
        }

        public double KinematicsResolution
        {
            get
            {
                return _KinematicsResolution;
            }

            set
            {
                _KinematicsResolution = value;
            }
        }

        public double CacheSize
        {
            get
            {
                return _CacheSize;
            }

            set
            {
                _CacheSize = value;
            }
        }

        public double TimeMag
        {
            get
            {
                return _TimeMag;
            }

            set
            {
                _TimeMag = value;
            }
        }

        public List<Particle> Particles => _Particles;

        #endregion

        #region 仿射、投影和视图控制

        private AffineTransformation _AffineTransformation = null; // 当前使用的仿射变换。
        private AffineTransformation _AffineTransformationCopy = null; // 视图控制开始前使用的仿射变换的副本。

        private double _FocalLength = 1000; // 投影变换使用的焦距。

        private double _SpaceMag = 1; // 空间倍率（米/像素），指投影变换焦点附近每像素表示的长度。

        public double FocalLength
        {
            get
            {
                return _FocalLength;
            }

            set
            {
                _FocalLength = value;
            }
        }

        public double SpaceMag
        {
            get
            {
                return _SpaceMag;
            }

            set
            {
                _SpaceMag = value;
            }
        }

        // 世界坐标系转换到屏幕坐标系。
        private PointD WorldToScreen(PointD3D pt)
        {
            return pt.AffineTransformCopy(_AffineTransformation).ProjectToXY(PointD3D.Zero, _FocalLength).ScaleCopy(1 / _SpaceMag).OffsetCopy(_GetOffsetMethod());
        }

        // 视图控制开始。
        public void ViewOperationStart()
        {
            _AffineTransformationCopy = _AffineTransformation.Copy();
        }

        // 视图控制停止。
        public void ViewOperationStop()
        {
            _AffineTransformationCopy = null;
        }

        // 视图控制类型。
        public enum ViewOperationType
        {
            OffsetX,
            OffsetY,
            OffsetZ,
            RotateX,
            RotateY,
            RotateZ
        }

        // 视图控制更新参数。
        public void ViewOperationUpdateParam(ViewOperationType type, double value)
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

                _AffineTransformation = affineTransformation.CompressCopy(VectorType.ColumnVector, 3);
            }
        }

        // 视图控制更新参数。
        public void ViewOperationUpdateParam(params (ViewOperationType type, double value)[] param)
        {
            if (param != null && param.Length > 0)
            {
                AffineTransformation affineTransformation = _AffineTransformationCopy.Copy();

                for (int i = 0; i < param.Length; i++)
                {
                    ViewOperationType type = param[i].type;
                    double value = param[i].value;

                    if (value != 0)
                    {
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
                    }
                }

                _AffineTransformation = affineTransformation.CompressCopy(VectorType.ColumnVector, 3);
            }
        }

        #endregion

        #region 渲染

        private Control _RedrawControl; // 用于重绘的控件。
        private Action _RedrawMethod; // 用于重绘的方法。
        private Func<Point> _GetOffsetMethod; // 用于获取坐标系偏移的方法。
        private Func<Size> _GetBitmapSizeMethod; // 用于获取位图大小的方法。

        private Bitmap _MultibodyBitmap = null; // 多体系统当前渲染的位图。

        // 将多体系统的当前状态渲染到位图。
        private void UpdateMultibodyBitmap()
        {
            Size bitmapSize = _GetBitmapSizeMethod();

            Bitmap multibodyBitmap = new Bitmap(Math.Max(1, bitmapSize.Width), Math.Max(1, bitmapSize.Height));

            if (_MultibodySystem != null)
            {
                using (Graphics Grap = Graphics.FromImage(multibodyBitmap))
                {
                    Grap.SmoothingMode = SmoothingMode.AntiAlias;
                    Grap.Clear(_RedrawControl.BackColor);

                    RectangleF bitmapBounds = new RectangleF(new PointF(), multibodyBitmap.Size);

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
                                Painting2D.PaintLine(multibodyBitmap, pt1, pt2, Color.FromArgb(255 * j / FrameCount, particles[i].Color), 1, true);
                            }
                        }
                    }

                    for (int i = 0; i < particles.Count; i++)
                    {
                        PointD location = WorldToScreen(particles[i].Location);

                        float radius = Math.Max(1, (float)(particles[i].Radius * _FocalLength / particles[i].Location.Z));

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

                        Grap.DrawString("Dynamics:   " + _MultibodySystem.DynamicFrequencyCounter.Frequency.ToString("N1") + " Hz", ft, Br, new Point(5, bitmapSize.Height - 100));
                        Grap.DrawString("Kinematics: " + _MultibodySystem.KinematicsFrequencyCounter.Frequency.ToString("N1") + " Hz", ft, Br, new Point(5, bitmapSize.Height - 75));
                        Grap.DrawString("Graphics:    " + _FrameRateCounter.Frequency.ToString("N1") + " FPS", ft, Br, new Point(5, bitmapSize.Height - 50));
                        Grap.DrawString("Time:           " + Texting.GetLongTimeStringFromTimeSpan(TimeSpan.FromSeconds(_MultibodySystem.LatestFrame.Time)), ft, Br, new Point(5, bitmapSize.Height - 25));
                    }

                    _FrameRateCounter.Update();
                }
            }

            if (_MultibodyBitmap != null)
            {
                _MultibodyBitmap.Dispose();
            }

            _MultibodyBitmap = multibodyBitmap;
        }

        // 获取多体系统当前渲染的位图。
        public Bitmap CurrentBitmap
        {
            get
            {
                if (_MultibodyBitmap == null)
                {
                    UpdateMultibodyBitmap();
                }

                return _MultibodyBitmap;
            }
        }

        #endregion

        #region 重绘线程和帧率控制

        private bool _Redrawing = false; // 是否正在运行重绘线程。

        private Thread _RedrawThread; // 重绘线程。

        private FrequencyCounter _FrameRateCounter = new FrequencyCounter(); // 重绘帧率（FPS）的频率计数器。

        // 动力学刷新率。
        public double DynamicsFPS => _MultibodySystem.DynamicFrequencyCounter.Frequency;

        // 运动学刷新率。
        public double KinematicsFPS => _MultibodySystem.KinematicsFrequencyCounter.Frequency;

        // 图形刷新率。
        public double GraphicsFPS => _FrameRateCounter.Frequency;

        // 重绘线程开始。
        public void RedrawThreadStart()
        {
            if (!_Redrawing)
            {
                _Redrawing = true;

                _MultibodySystem = new MultibodySystem(_DynamicsResolution, _KinematicsResolution, _CacheSize, _Particles);

                _AffineTransformation = AffineTransformation.Empty;

                _RedrawThread = new Thread(new ThreadStart(RedrawThreadEvent));
                _RedrawThread.IsBackground = true;
                _RedrawThread.Start();
            }
        }

        // 重绘线程停止。
        public void RedrawThreadStop()
        {
            if (_Redrawing)
            {
                if (_RedrawThread != null && _RedrawThread.IsAlive)
                {
                    _RedrawThread.Abort();
                }

                _Redrawing = false;
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

                UpdateMultibodyBitmap();

                _RedrawControl.Invoke(_RedrawMethod);

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