﻿/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2024 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.117.1000.M2.201101-1440

This file is part of "多体系统模拟" (MultibodySystemSimulation)

"多体系统模拟" (MultibodySystemSimulation) is released under the GPLv3 license
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using AffineTransformation = Com.AffineTransformation;
using FrequencyCounter = Com.FrequencyCounter;
using Geometry = Com.Geometry;
using Painting2D = Com.Painting2D;
using PointD = Com.PointD;
using PointD3D = Com.PointD3D;
using Texting = Com.Text;
using VectorType = Com.Vector.Type;
using UIMessage = Com.WinForm.UIMessage;
using UIMessageProcessor = Com.WinForm.UIMessageProcessor;

namespace Multibody
{
    // 渲染器。
    internal sealed class Renderer : UIMessageProcessor
    {
        private SimulationData _SimulationData;

        #region 构造函数

        public Renderer(SimulationData simulationData, Control redrawControl, Action<Bitmap> redrawMethod, Point coordinateOffset, Size bitmapSize) : base()
        {
            if (simulationData is null || redrawControl is null || redrawMethod is null)
            {
                throw new ArgumentNullException();
            }

            //

            _SimulationData = simulationData;

            _RedrawControl = redrawControl;
            _RedrawMethod = redrawMethod;
            _CoordinateOffset = coordinateOffset;
            _BitmapSize = bitmapSize;
        }

        #endregion

        #region 消息处理器

        // 消息码。
        public enum MessageCode
        {
            SimulationStart,
            SimulationStop,

            SetFocalLength,
            SetSpaceMag,

            ViewOperationStart,
            ViewOperationUpdateParam,
            ViewOperationStop,

            UpdateCoordinateOffset,

            SetTimeMag,
            UpdateBitmapSize
        }

        protected override void ProcessMessage(UIMessage message)
        {
            switch (message.MessageCode)
            {
                case (int)MessageCode.SimulationStart:
                    _SimulationStart();
                    break;

                case (int)MessageCode.SimulationStop:
                    _SimulationStop();
                    break;

                //

                case (int)MessageCode.SetFocalLength:
                    _SetFocalLength((double)message.RequestData);
                    break;

                case (int)MessageCode.SetSpaceMag:
                    _SetSpaceMag((double)message.RequestData);
                    break;

                //

                case (int)MessageCode.ViewOperationStart:
                    _ViewOperationStart();
                    break;

                case (int)MessageCode.ViewOperationUpdateParam:
                    _ViewOperationUpdateParam(((ViewOperationType, double)[])message.RequestData);
                    break;

                case (int)MessageCode.ViewOperationStop:
                    _ViewOperationStop();
                    break;

                //

                case (int)MessageCode.UpdateCoordinateOffset:
                    _UpdateCoordinateOffset((Point)message.RequestData);
                    break;

                //

                case (int)MessageCode.SetTimeMag:
                    _SetTimeMag((double)message.RequestData);
                    break;

                case (int)MessageCode.UpdateBitmapSize:
                    _UpdateBitmapSize((Size)message.RequestData);
                    break;
            }
        }

        protected override void MessageLoop()
        {
            base.MessageLoop();

            //

            _RedrawBitmap();
        }

        #endregion

        #region 仿真

        private bool _SimulationIsRunning = false; // 是否正在运行仿真。

        // 仿真开始。
        private void _SimulationStart()
        {
            _LastGenerateTime = DateTime.MinValue;
            _LastSnapshotTime = 0;
            _GenerateCount = 0;

            //

            _SimulationIsRunning = true;
        }

        // 仿真停止。
        private void _SimulationStop()
        {
            _SimulationIsRunning = false;
        }

        #endregion

        #region 图形学与视图控制

        private AffineTransformation _AffineTransformation = AffineTransformation.Empty; // 当前使用的仿射变换。
        private AffineTransformation _AffineTransformationCopy = null; // 视图控制开始前使用的仿射变换的副本。
        private AffineTransformation _InverseAffineTransformation = AffineTransformation.Empty; // 当前使用的仿射变换的逆变换。

        private FrequencyCounter _TransformFrequencyCounter = new FrequencyCounter(); // 仿射变换的频率计数器。

        //

        private double _FocalLength = SimulationData.InitialFocalLength; // 投影变换使用的焦距。

        private double _SpaceMag = SimulationData.InitialSpaceMag; // 空间倍率（米/像素），指投影变换焦点附近每像素表示的长度。

        private void _SetFocalLength(double focalLength)
        {
            _FocalLength = focalLength;
            _SimulationData.FocalLength = focalLength;
        }

        private void _SetSpaceMag(double spaceMag)
        {
            _SpaceMag = spaceMag;
            _SimulationData.SpaceMag = spaceMag;
        }

        //

        // 视图控制开始。
        private void _ViewOperationStart() => _AffineTransformationCopy = _AffineTransformation.Copy();

        // 视图控制停止。
        private void _ViewOperationStop() => _AffineTransformationCopy = null;

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
        private void _ViewOperationUpdateParam((ViewOperationType type, double value)[] param)
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
                        switch (type)
                        {
                            case ViewOperationType.OffsetX: affineTransformation.Offset(0, value); break;
                            case ViewOperationType.OffsetY: affineTransformation.Offset(1, value); break;
                            case ViewOperationType.OffsetZ: affineTransformation.Offset(2, value); break;

                            case ViewOperationType.RotateX: affineTransformation.Rotate(1, 2, value); break;
                            case ViewOperationType.RotateY: affineTransformation.Rotate(2, 0, value); break;
                            case ViewOperationType.RotateZ: affineTransformation.Rotate(0, 1, value); break;
                        }
                    }
                }

                _AffineTransformation = affineTransformation.CompressCopy(VectorType.ColumnVector, 3);
                _InverseAffineTransformation = _AffineTransformation.InverseTransformCopy();
            }
        }

        //

        private Point _CoordinateOffset; // 坐标系偏移。

        // 更新坐标系偏移（用途：绘图时使原点位于视图中心）。
        private void _UpdateCoordinateOffset(Point coordinateOffset) => _CoordinateOffset = coordinateOffset;

        // 世界坐标系转换到屏幕坐标系（并叠加绘图偏移）。
        private PointD _WorldToScreen(PointD3D pt) => pt.AffineTransformCopy(_AffineTransformation).ProjectToXY(PointD3D.Zero, _FocalLength).ScaleCopy(1 / _SpaceMag).OffsetCopy(_CoordinateOffset);

        // 世界坐标系中的坐标到屏幕的距离。
        private double _DistanceToScreen(PointD3D pt) => pt.AffineTransformCopy(_AffineTransformation).Z;

        #endregion

        #region 渲染

        private double _TimeMag = SimulationData.InitialTimeMag; // 时间倍率（秒/秒），指仿真时间流逝速度与真实时间流逝速度的比值，表现为动画的播放速度。

        private void _SetTimeMag(double timeMag)
        {
            _TimeMag = timeMag;
            _SimulationData.TimeMag = timeMag;
        }

        //

        private Size _BitmapSize; // 位图大小。

        // 更新位图大小。
        private void _UpdateBitmapSize(Size bitmapSize) => _BitmapSize = bitmapSize;

        //

        private DateTime _LastGenerateTime = DateTime.MinValue; // 最近一次渲染位图的日期时间。
        private double _LastSnapshotTime = 0; // 最近一次获取快照的最新一帧的时刻。
        private long _GenerateCount = 0; // 自仿真开始以来的累计渲染次数。

        private Font _Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);

        // 绘制坐标系网格。
        private void _DrawGrid(Bitmap bitmap)
        {
            // 坐标系网格在视图内的可见部分，在世界坐标系中是一个顶点位于视图中心（或者，当不考虑绘图偏移时为原点）、高度无限大的四棱锥，
            // 其任一横截面与视图矩形相似，棱的斜率与投影变换的焦距成反比；考虑该四棱锥从顶点起、高度有限大的部分，
            // 将5个顶点逆变换到世界坐标系，再取其外接长方体，可容易地得到该长方体内与X、Y、Z坐标轴平行的直线段族，
            // 将这些线段放射变换到屏幕坐标系，并取其可见部分，即可用于绘制坐标系网格。

            PointD3D[] pts = new PointD3D[] {
                PointD3D.Zero,
                new PointD3D(-_BitmapSize.Width / 2, -_BitmapSize.Height / 2, _FocalLength),
                new PointD3D(-_BitmapSize.Width / 2, _BitmapSize.Height / 2, _FocalLength),
                new PointD3D(_BitmapSize.Width / 2, _BitmapSize.Height / 2, _FocalLength),
                new PointD3D(_BitmapSize.Width / 2, -_BitmapSize.Height / 2, _FocalLength)
            };
            const double deep = 3; // 绘制坐标系网格的最远距离是焦距的几倍
            for (int i = 0; i < pts.Length; i++)
            {
                pts[i] = (pts[i] * deep).ScaleCopy(_SpaceMag).AffineTransformCopy(_InverseAffineTransformation);
            }

            double minX = 0, maxX = 0, minY = 0, maxY = 0, minZ = 0, maxZ = 0;
            for (int i = 0; i < pts.Length; i++)
            {
                if (i == 0)
                {
                    minX = maxX = pts[i].X;
                    minY = maxY = pts[i].Y;
                    minZ = maxZ = pts[i].Z;
                }
                else
                {
                    minX = Math.Min(minX, pts[i].X);
                    maxX = Math.Max(maxX, pts[i].X);
                    minY = Math.Min(minY, pts[i].Y);
                    maxY = Math.Max(maxY, pts[i].Y);
                    minZ = Math.Min(minZ, pts[i].Z);
                    maxZ = Math.Max(maxZ, pts[i].Z);
                }
            }

            double delta = 500 * _SpaceMag; // 绘制坐标系网格的间距。
            minX = Math.Floor(minX / delta) * delta;
            maxX = Math.Floor(maxX / delta) * delta;
            minY = Math.Floor(minY / delta) * delta;
            maxY = Math.Floor(maxY / delta) * delta;
            minZ = Math.Floor(minZ / delta) * delta;
            maxZ = Math.Floor(maxZ / delta) * delta;

            int n = 0;
            for (double x = minX; x <= maxX; x += delta)
            {
                for (double y = minY; y <= maxY; y += delta)
                {
                    for (double z = minZ; z <= maxZ; z += delta)
                    {
                        PointD pt0 = _WorldToScreen(new PointD3D(x, y, z));
                        PointD pt1 = _WorldToScreen(new PointD3D(x + delta, y, z));
                        PointD pt2 = _WorldToScreen(new PointD3D(x, y + delta, z));
                        PointD pt3 = _WorldToScreen(new PointD3D(x, y, z + delta));
                        Color cr = Color.FromArgb((int)Math.Max(0, Math.Min(255, 255 * (1 - Math.Max(0, _DistanceToScreen(new PointD3D(x, y, z))) / _FocalLength / deep))), 64, 64, 64);
                        n += 5;

                        Painting2D.PaintLine(bitmap, pt0, pt1, cr, 1, true);
                        Painting2D.PaintLine(bitmap, pt0, pt2, cr, 1, true);
                        Painting2D.PaintLine(bitmap, pt0, pt3, cr, 1, true);
                    }
                }
            }

            _TransformFrequencyCounter.Update(n);
        }

        // 返回将多体系统的当前状态渲染得到的位图。
        private Bitmap _GenerateBitmap()
        {
            int bitmapWidth = _BitmapSize.Width;
            int bitmapHeight = _BitmapSize.Height;

            Bitmap bitmap = new Bitmap(Math.Max(1, bitmapWidth), Math.Max(1, bitmapHeight));

            using (Graphics grap = Graphics.FromImage(bitmap))
            {
                grap.SmoothingMode = SmoothingMode.AntiAlias;
                grap.Clear(_RedrawControl.BackColor);

                _DrawGrid(bitmap);

                if (_SimulationIsRunning)
                {
                    DateTime lastGenerateTime;

                    if (_LastGenerateTime == DateTime.MinValue)
                    {
                        lastGenerateTime = DateTime.Now;
                    }
                    else
                    {
                        lastGenerateTime = _LastGenerateTime;
                    }

                    _LastGenerateTime = DateTime.Now;

                    double time = _LastSnapshotTime + (DateTime.Now - lastGenerateTime).TotalSeconds * _TimeMag;

                    Snapshot snapshot = _SimulationData.GetSnapshot(time - _SimulationData.TrackLength, time);

                    _LastSnapshotTime = snapshot.LatestFrame.Time;

                    if (snapshot != null && snapshot.FrameCount > 0)
                    {
                        RectangleF bitmapBounds = new RectangleF(new PointF(), bitmap.Size);

                        Frame latestFrame = snapshot.LatestFrame;
                        latestFrame.GraphicsId = _GenerateCount;

                        int frameCount = snapshot.FrameCount;
                        int particleCount = latestFrame.ParticleCount;

                        for (int i = 0; i < particleCount; i++)
                        {
                            for (int j = frameCount - 1; j >= 1; j--)
                            {
                                PointD pt1 = _WorldToScreen(snapshot.GetFrame(j).GetParticle(i).Location);
                                PointD pt2 = _WorldToScreen(snapshot.GetFrame(j - 1).GetParticle(i).Location);

                                Painting2D.PaintLine(bitmap, pt1, pt2, Color.FromArgb(255 * j / frameCount, latestFrame.GetParticle(i).Color), 1, true);
                            }
                        }

                        for (int i = 0; i < particleCount; i++)
                        {
                            Particle particle = latestFrame.GetParticle(i);
                            PointD location = _WorldToScreen(particle.Location);

                            float radius = Math.Max(1, (float)(particle.Radius * _FocalLength / _DistanceToScreen(particle.Location)));

                            if (Geometry.CircleInnerIsVisibleInRectangle(location, radius, bitmapBounds))
                            {
                                using (Brush Br = new SolidBrush(particle.Color))
                                {
                                    grap.FillEllipse(Br, new RectangleF((float)location.X - radius, (float)location.Y - radius, radius * 2, radius * 2));
                                }
                            }
                        }

                        _TransformFrequencyCounter.Update(particleCount * 3);

                        using (Brush br = new SolidBrush(Color.Silver))
                        {
                            grap.DrawString("帧率:", _Font, br, new Point(5, bitmapHeight - 240));
                            grap.DrawString($"    动力学方程(D): {_SimulationData.DynamicsPFS:N1} Hz", _Font, br, new Point(5, bitmapHeight - 220));
                            grap.DrawString($"    轨迹(K): {_SimulationData.KinematicsPFS:N1} Hz", _Font, br, new Point(5, bitmapHeight - 200));
                            grap.DrawString($"    仿射变换(T): {_TransformFrequencyCounter.Frequency:N1} Hz", _Font, br, new Point(5, bitmapHeight - 180));
                            grap.DrawString($"    刷新率(G): {_FrameRateCounter.Frequency:N1} FPS", _Font, br, new Point(5, bitmapHeight - 160));

                            grap.DrawString($"已缓存(K): {_SimulationData.CachedFrameCount} 帧", _Font, br, new Point(5, bitmapHeight - 120));
                            grap.DrawString($"使用中(K): {snapshot.FrameCount} 帧", _Font, br, new Point(5, bitmapHeight - 100));
                            grap.DrawString($"最新帧: D {_SimulationData.LatestFrame.DynamicsId}, K {_SimulationData.LatestFrame.KinematicsId}", _Font, br, new Point(5, bitmapHeight - 80));
                            grap.DrawString($"当前帧: D {latestFrame.DynamicsId}, K {latestFrame.KinematicsId}, G {latestFrame.GraphicsId}", _Font, br, new Point(5, bitmapHeight - 60));

                            grap.DrawString($"时间:   {Texting.GetLongTimeStringFromTimeSpan(TimeSpan.FromSeconds(snapshot.LatestFrame.Time))}", _Font, br, new Point(5, bitmapHeight - 20));
                        }

                        _GenerateCount++;
                    }
                }
                else
                {
                    int particleCount = _SimulationData.ParticleCount;

                    if (particleCount > 0)
                    {
                        RectangleF bitmapBounds = new RectangleF(new PointF(), bitmap.Size);

                        for (int i = 0; i < particleCount; i++)
                        {
                            Particle particle = _SimulationData.GetParticle(i);
                            PointD location = _WorldToScreen(particle.Location);

                            float radius = Math.Max(1, (float)(particle.Radius * _FocalLength / _DistanceToScreen(particle.Location)));

                            if (Geometry.CircleInnerIsVisibleInRectangle(location, radius, bitmapBounds))
                            {
                                using (Brush Br = new SolidBrush(particle.Color))
                                {
                                    grap.FillEllipse(Br, new RectangleF((float)location.X - radius, (float)location.Y - radius, radius * 2, radius * 2));
                                }
                            }
                        }

                        _TransformFrequencyCounter.Update(particleCount * 2);

                        using (Brush br = new SolidBrush(Color.Silver))
                        {
                            grap.DrawString($"帧率: {_FrameRateCounter.Frequency:N1} FPS", _Font, br, new Point(5, bitmapHeight - 20));
                        }
                    }
                }
            }

            return bitmap;
        }

        #endregion

        #region 重绘

        private Control _RedrawControl; // 用于重绘的控件。
        private Action<Bitmap> _RedrawMethod; // 用于重绘的方法。

        private FrequencyCounter _FrameRateCounter = new FrequencyCounter(); // 重绘帧率（FPS）的频率计数器。

        // 重绘。
        private void _RedrawBitmap()
        {
            _RedrawControl.Invoke(_RedrawMethod, _GenerateBitmap());

            _FrameRateCounter.Update();
        }

        #endregion
    }
}