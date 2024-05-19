/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
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
using Real = Com.Real;
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

        public Renderer(SimulationData simulationData, Control redrawControl, Action<Bitmap> redrawMethod, Size viewSize) : base()
        {
            if (simulationData is null || redrawControl is null || redrawMethod is null)
            {
                throw new ArgumentNullException();
            }

            //

            _SimulationData = simulationData;

            _RedrawControl = redrawControl;
            _RedrawMethod = redrawMethod;
            _ViewSize = viewSize;

            _UpdateGridDistance();
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

            UpdateViewSize,

            SetTimeMag
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

                case (int)MessageCode.UpdateViewSize:
                    _UpdateViewSize((Size)message.RequestData);
                    break;

                //

                case (int)MessageCode.SetTimeMag:
                    _SetTimeMag((double)message.RequestData);
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

        private long _ViewParamChangedCount = 0; // 视图参数改变的次数。

        //

        private double _FocalLength = SimulationData.InitialFocalLength; // 投影变换使用的焦距。

        private double _SpaceMag = SimulationData.InitialSpaceMag; // 空间倍率（米/像素），指投影变换焦点附近每像素表示的长度。

        private void _SetFocalLength(double focalLength)
        {
            _FocalLength = focalLength;
            _SimulationData.FocalLength = focalLength;

            _ViewParamChangedCount = Math.Max(0, _ViewParamChangedCount + 1);

            _DisposeGridBitmap();
        }

        private void _SetSpaceMag(double spaceMag)
        {
            _SpaceMag = spaceMag;
            _SimulationData.SpaceMag = spaceMag;

            _ViewParamChangedCount = Math.Max(0, _ViewParamChangedCount + 1);

            _UpdateGridDistance();

            _DisposeGridBitmap();
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
                _InverseAffineTransformation = _AffineTransformation.InverseTransformCopy().CompressCopy(VectorType.ColumnVector, 3);

                _ViewParamChangedCount = Math.Max(0, _ViewParamChangedCount + 1);

                _DisposeGridBitmap();
            }
        }

        //

        private Size _ViewSize; // 视图大小。

        // 更新视图大小。
        private void _UpdateViewSize(Size viewSize)
        {
            _ViewSize = viewSize;

            _ViewParamChangedCount = Math.Max(0, _ViewParamChangedCount + 1);

            _DisposeGridBitmap();
        }

        // 将世界坐标系坐标转换到屏幕坐标系（并将原点平移至视图中心），输出世界坐标系中的坐标到屏幕的距离。
        private bool _WorldToScreen(PointD3D pt, out PointD scrPt, out double z)
        {
            pt.AffineTransform(_AffineTransformation);
            z = pt.Z;
            if (z <= 0)
            {
                scrPt = PointD.NaN;
                return false;
            }
            else
            {
                scrPt = pt.ProjectToXY(PointD3D.Zero, _FocalLength).ScaleCopy(1 / _SpaceMag).OffsetCopy(new PointD(_ViewSize) / 2);
                return true;
            }
        }

        // 将世界坐标系直线段转换到屏幕坐标系（并将原点平移至视图中心），输出世界坐标系中的坐标到屏幕的距离。
        private bool _WorldToScreen(PointD3D pt1, PointD3D pt2, out PointD scrPt1, out PointD scrPt2, out double z1, out double z2)
        {
            pt1.AffineTransform(_AffineTransformation);
            pt2.AffineTransform(_AffineTransformation);
            z1 = pt1.Z;
            z2 = pt2.Z;
            if (z1 <= 0 && z2 <= 0)
            {
                scrPt1 = PointD.NaN;
                scrPt2 = PointD.NaN;
                return false;
            }
            else if (z1 > 0 && z2 > 0)
            {
                scrPt1 = pt1.ProjectToXY(PointD3D.Zero, _FocalLength).ScaleCopy(1 / _SpaceMag).OffsetCopy(new PointD(_ViewSize) / 2);
                scrPt2 = pt2.ProjectToXY(PointD3D.Zero, _FocalLength).ScaleCopy(1 / _SpaceMag).OffsetCopy(new PointD(_ViewSize) / 2);
                return scrPt1 != scrPt2;
            }
            else
            {
                if (z1 > z2)
                {
                    scrPt1 = pt1.ProjectToXY(PointD3D.Zero, _FocalLength).ScaleCopy(1 / _SpaceMag).OffsetCopy(new PointD(_ViewSize) / 2);
                    scrPt2 = (pt1 - (pt1 - pt2) * (z1 / (z1 - z2))).XY.Normalize * (Math.Max(scrPt1.Module, ((PointD)_ViewSize).Module) * 1000);
                    // TODO: (pt1 - (pt1 - pt2) * (z1 / (z1 - z2))).XY 是零向量？
                }
                else
                {
                    scrPt2 = pt2.ProjectToXY(PointD3D.Zero, _FocalLength).ScaleCopy(1 / _SpaceMag).OffsetCopy(new PointD(_ViewSize) / 2);
                    scrPt1 = (pt2 - (pt2 - pt1) * (z2 / (z2 - z1))).XY.Normalize * (Math.Max(scrPt2.Module, ((PointD)_ViewSize).Module) * 1000);
                }
                return scrPt1 != scrPt2;
            }
        }

        #endregion

        #region 渲染

        private double _TimeMag = SimulationData.InitialTimeMag; // 时间倍率（秒/秒），指仿真时间流逝速度与真实时间流逝速度的比值，表现为动画的播放速度。

        // 更新时间倍率。
        private void _SetTimeMag(double timeMag)
        {
            _TimeMag = timeMag;
            _SimulationData.TimeMag = timeMag;
        }

        //

        private double _GridDistance = 500 * SimulationData.InitialSpaceMag; // 绘制坐标系网格的间距（米）。

        // 更新坐标系网格间距。
        private void _UpdateGridDistance()
        {
            Real gridDist = 500 * _SpaceMag;

            if (gridDist.Value > 1 && gridDist.Value < 1.5)
            {
                gridDist.Value = 1;
            }
            else if (gridDist.Value >= 1.5 && gridDist.Value < 3.5)
            {
                gridDist.Value = 2;
            }
            else if (gridDist.Value >= 3.5 && gridDist.Value < 7.5)
            {
                gridDist.Value = 5;
            }
            else if (gridDist.Value >= 7.5)
            {
                gridDist.Value = 10;
            }

            _GridDistance = (double)gridDist;
        }

        private FrequencyCounter _DrawLineFrequencyCounter = new FrequencyCounter(); // 绘制直线的频率计数器。

        Bitmap _GridBitmap = null; // 坐标系网格位图。

        // 删除坐标系网格位图。
        private void _DisposeGridBitmap()
        {
            if (_GridBitmap != null)
            {
                _GridBitmap.Dispose();
                _GridBitmap = null;
            }
        }

        // 生成坐标系网格位图。
        private void _GenerateGridBitmap()
        {
            // 坐标系网格在视图内的可见部分，在世界坐标系中是一个顶点位于视图中心（或者，当不考虑绘图偏移时为原点）、高度无限大的四棱锥，
            // 其任一横截面与视图矩形相似，棱的斜率与投影变换的焦距成反比；考虑该四棱锥从顶点起、高度有限大的部分，
            // 将5个顶点逆变换到世界坐标系，再取其外接长方体，可容易地得到该长方体内与X、Y、Z坐标轴平行的直线段族，
            // 将这些线段放射变换到屏幕坐标系，并取其可见部分，即可用于绘制坐标系网格。

            if (_GridBitmap is null)
            {
                int bitmapWidth = _ViewSize.Width;
                int bitmapHeight = _ViewSize.Height;

                _GridBitmap = new Bitmap(Math.Max(1, bitmapWidth), Math.Max(1, bitmapHeight));

                using (Graphics grap = Graphics.FromImage(_GridBitmap))
                {
                    grap.SmoothingMode = SmoothingMode.AntiAlias;
                    grap.Clear(_RedrawControl.BackColor);

                    double gridDepth = 5000; // 绘制坐标系网格的最远距离（像素）
                    PointD3D[] pts;
                    if (_FocalLength > 0)
                    {
                        pts = new PointD3D[] {
                            PointD3D.Zero,
                            new PointD3D(-bitmapWidth, -bitmapHeight, _FocalLength * 2 / _SpaceMag) / 2,
                            new PointD3D(-bitmapWidth, bitmapHeight, _FocalLength * 2 / _SpaceMag) / 2,
                            new PointD3D(bitmapWidth, bitmapHeight, _FocalLength * 2 / _SpaceMag) / 2,
                            new PointD3D(bitmapWidth, -bitmapHeight, _FocalLength * 2 / _SpaceMag) / 2
                        };
                        for (int i = 0; i < pts.Length; i++)
                        {
                            pts[i] = (pts[i] * gridDepth / (_FocalLength / _SpaceMag)).ScaleCopy(_SpaceMag).AffineTransformCopy(_InverseAffineTransformation);
                        }
                    }
                    else
                    {
                        pts = new PointD3D[] {
                            PointD3D.Zero,
                            new PointD3D(-bitmapWidth, -bitmapHeight, gridDepth * 2) / 2,
                            new PointD3D(-bitmapWidth, bitmapHeight, gridDepth * 2) / 2,
                            new PointD3D(bitmapWidth, bitmapHeight, gridDepth * 2) / 2,
                            new PointD3D(bitmapWidth, -bitmapHeight, gridDepth * 2) / 2
                        };
                        for (int i = 0; i < pts.Length; i++)
                        {
                            pts[i] = pts[i].ScaleCopy(_SpaceMag).AffineTransformCopy(_InverseAffineTransformation);
                        }
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

                    minX = Math.Floor(minX / _GridDistance) * _GridDistance;
                    maxX = Math.Floor(maxX / _GridDistance) * _GridDistance;
                    minY = Math.Floor(minY / _GridDistance) * _GridDistance;
                    maxY = Math.Floor(maxY / _GridDistance) * _GridDistance;
                    minZ = Math.Floor(minZ / _GridDistance) * _GridDistance;
                    maxZ = Math.Floor(maxZ / _GridDistance) * _GridDistance;

                    int transformNum = 0;
                    int lineNum = 0;
                    for (double x = minX; x <= maxX; x += _GridDistance)
                    {
                        for (double y = minY; y <= maxY; y += _GridDistance)
                        {
                            for (double z = minZ; z <= maxZ; z += _GridDistance)
                            {
                                Action<PointD3D, PointD3D> DrawLine = (pt1, pt2) =>
                                {
                                    if (_WorldToScreen(pt1, pt2, out PointD scrPt1, out PointD scrPt2, out double z1, out double z2))
                                    {
                                        int alpha = (int)Math.Round(255 * Math.Pow(2, -(Math.Min(z1, z2) / (gridDepth * _SpaceMag / 5))));
                                        if (alpha >= 1)
                                        {
                                            Color cr = Color.FromArgb(Math.Min(255, alpha), 64, 64, 64);
                                            if (Painting2D.PaintLine(_GridBitmap, scrPt1, scrPt2, cr, 1, true))
                                            {
                                                lineNum++;
                                            }
                                        }
                                    }
                                };

                                PointD3D pt0 = new PointD3D(x, y, z);
                                PointD3D ptX = new PointD3D(x + _GridDistance, y, z);
                                PointD3D ptY = new PointD3D(x, y + _GridDistance, z);
                                PointD3D ptZ = new PointD3D(x, y, z + _GridDistance);
                                DrawLine(pt0, ptX);
                                DrawLine(pt0, ptY);
                                DrawLine(pt0, ptZ);
                                transformNum += 6;
                            }
                        }
                    }

                    if (transformNum > 0)
                    {
                        _TransformFrequencyCounter.Update(transformNum);
                    }
                    if (lineNum > 0)
                    {
                        _DrawLineFrequencyCounter.Update(lineNum);
                    }
                }
            }
        }

        private DateTime _LastGenerateTime = DateTime.MinValue; // 最近一次渲染的日期时间。
        private double _LastSnapshotTime = 0; // 最近一次获取的快照的最新一帧的时刻。
        private long _GenerateCount = 0; // 自仿真开始以来的累计渲染次数。

        private Font _Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);

        // 返回将多体系统的当前状态渲染得到的位图。
        private Bitmap _GenerateBitmap()
        {
            _GenerateGridBitmap();

            Bitmap bitmap = (Bitmap)_GridBitmap.Clone();

            using (Graphics grap = Graphics.FromImage(bitmap))
            {
                grap.SmoothingMode = SmoothingMode.AntiAlias;

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

                    long latestFrameDynamicsId = -1, latestFrameKinematicsId = -1;
                    if (snapshot != null && snapshot.FrameCount > 0)
                    {
                        int frameCount = snapshot.FrameCount;

                        Frame latestFrame = snapshot.LatestFrame;
                        _LastSnapshotTime = latestFrame.Time;
                        latestFrameDynamicsId = latestFrame.DynamicsId;
                        latestFrameKinematicsId = latestFrame.KinematicsId;
                        int particleCount = latestFrame.ParticleCount;

                        RectangleF bitmapBounds = new RectangleF(new PointF(), bitmap.Size);

                        int transformNum = 0;
                        int lineNum = 0;

                        if (frameCount > 1)
                        {
                            for (int i = 0; i < particleCount; i++)
                            {
                                int j = frameCount - 1, k = frameCount - 2;

                                Particle particle1 = snapshot.GetFrame(j).GetParticle(i);
                                TransformResultCache cache1 = particle1.TransformResultCache;
                                if (cache1.TransformID != _ViewParamChangedCount)
                                {
                                    _WorldToScreen(particle1.Location, out PointD pt, out double z);
                                    cache1.ScreenLocation = pt;
                                    cache1.DistanceToScreen = z;
                                    cache1.TransformID = _ViewParamChangedCount;
                                    transformNum++;
                                }
                                PointD pt1 = cache1.ScreenLocation;

                                Particle particle2 = snapshot.GetFrame(k).GetParticle(i);
                                TransformResultCache cache2 = particle2.TransformResultCache;
                                if (cache2.TransformID != _ViewParamChangedCount)
                                {
                                    _WorldToScreen(particle2.Location, out PointD pt, out double z);
                                    cache2.ScreenLocation = pt;
                                    cache2.DistanceToScreen = z;
                                    cache2.TransformID = _ViewParamChangedCount;
                                    transformNum++;
                                }
                                PointD pt2 = cache2.ScreenLocation;

                                while (true)
                                {
                                    if (pt1.DistanceFrom(pt2) >= 2 || k == 0)
                                    {
                                        int alpha = (int)Math.Round(255.0 * j / frameCount);
                                        if (alpha >= 1 && Painting2D.PaintLine(bitmap, pt1, pt2, Color.FromArgb(Math.Min(255, alpha), latestFrame.GetParticle(i).Color), 1, true))
                                        {
                                            lineNum++;
                                        }
                                        j = k;
                                        pt1 = pt2;
                                    }

                                    if (k > 0)
                                    {
                                        k--;
                                        particle2 = snapshot.GetFrame(k).GetParticle(i);
                                        cache2 = particle2.TransformResultCache;
                                        if (cache2.TransformID != _ViewParamChangedCount)
                                        {
                                            _WorldToScreen(particle2.Location, out PointD pt, out double z);
                                            cache2.ScreenLocation = pt;
                                            cache2.DistanceToScreen = z;
                                            cache2.TransformID = _ViewParamChangedCount;
                                            transformNum++;
                                        }
                                        pt2 = cache2.ScreenLocation;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < particleCount; i++)
                        {
                            Particle particle = latestFrame.GetParticle(i);

                            TransformResultCache cache = particle.TransformResultCache;
                            if (cache.TransformID != _ViewParamChangedCount)
                            {
                                _WorldToScreen(particle.Location, out PointD pt, out double z);
                                cache.ScreenLocation = pt;
                                cache.DistanceToScreen = z;
                                cache.TransformID = _ViewParamChangedCount;

                                transformNum++;
                            }

                            PointD location = cache.ScreenLocation;
                            double distance = cache.DistanceToScreen;

                            float radius = Math.Max(1, (float)(particle.Radius * _FocalLength / distance));

                            if (Geometry.CircleInnerIsVisibleInRectangle(location, radius, bitmapBounds))
                            {
                                using (Brush Br = new SolidBrush(particle.Color))
                                {
                                    grap.FillEllipse(Br, new RectangleF((float)location.X - radius, (float)location.Y - radius, radius * 2, radius * 2));
                                }
                            }
                        }

                        if (transformNum > 0)
                        {
                            _TransformFrequencyCounter.Update(transformNum);
                        }
                        if (lineNum > 0)
                        {
                            _DrawLineFrequencyCounter.Update(lineNum);
                        }
                    }

                    using (Brush br = new SolidBrush(Color.Silver))
                    {
                        int bitmapHeight = bitmap.Height;

                        grap.DrawString("帧率:", _Font, br, new Point(5, bitmapHeight - 240));
                        grap.DrawString($"    动力学方程(D): {_SimulationData.DynamicsPFS:N1} Hz", _Font, br, new Point(5, bitmapHeight - 220));
                        grap.DrawString($"    轨迹(K): {_SimulationData.KinematicsPFS:N1} Hz", _Font, br, new Point(5, bitmapHeight - 200));
                        grap.DrawString($"    仿射变换: {_TransformFrequencyCounter.Frequency:N1} Hz, 直线: {_DrawLineFrequencyCounter.Frequency:N1} Hz", _Font, br, new Point(5, bitmapHeight - 180));
                        grap.DrawString($"    刷新率(G): {_FrameRateCounter.Frequency:N1} FPS", _Font, br, new Point(5, bitmapHeight - 160));

                        grap.DrawString($"已缓存(K): {_SimulationData.CachedFrameCount} 帧", _Font, br, new Point(5, bitmapHeight - 120));
                        grap.DrawString($"使用中(K): {snapshot.FrameCount} 帧", _Font, br, new Point(5, bitmapHeight - 100));
                        grap.DrawString($"最新帧: D {_SimulationData.LatestFrame.DynamicsId}, K {_SimulationData.LatestFrame.KinematicsId}", _Font, br, new Point(5, bitmapHeight - 80));
                        grap.DrawString($"当前帧: D {latestFrameDynamicsId}, K {latestFrameKinematicsId}, G {_GenerateCount}", _Font, br, new Point(5, bitmapHeight - 60));

                        grap.DrawString($"时间:   {Texting.GetLongTimeStringFromTimeSpan(TimeSpan.FromSeconds(snapshot.LatestFrame.Time))}", _Font, br, new Point(5, bitmapHeight - 20));
                    }

                    _GenerateCount++;
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

                            if (_WorldToScreen(particle.Location, out PointD pt, out double z))
                            {
                                float radius = Math.Max(1, (float)(particle.Radius * _FocalLength / z));

                                if (Geometry.CircleInnerIsVisibleInRectangle(pt, radius, bitmapBounds))
                                {
                                    using (Brush Br = new SolidBrush(particle.Color))
                                    {
                                        grap.FillEllipse(Br, new RectangleF((float)pt.X - radius, (float)pt.Y - radius, radius * 2, radius * 2));
                                    }
                                }
                            }
                        }

                        _TransformFrequencyCounter.Update(particleCount);
                    }

                    using (Brush br = new SolidBrush(Color.Silver))
                    {
                        int bitmapHeight = bitmap.Height;

                        grap.DrawString("帧率:", _Font, br, new Point(5, bitmapHeight - 60));
                        grap.DrawString($"    仿射变换: {_TransformFrequencyCounter.Frequency:N1} Hz", _Font, br, new Point(5, bitmapHeight - 40));
                        grap.DrawString($"    刷新率(G): {_FrameRateCounter.Frequency:N1} FPS", _Font, br, new Point(5, bitmapHeight - 20));
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