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
using VectorType = Com.VectorType;
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

            PressedKeysChanged,

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

                case (int)MessageCode.PressedKeysChanged:
                    _UpdatePressedKeys((Keys[])message.RequestData);
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
            _SimulationStartTime = DateTime.UtcNow;
            _RenderCount = 0;

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
            _DisposeBackgroundBitmap();
        }

        private void _SetSpaceMag(double spaceMag)
        {
            _SpaceMag = spaceMag;
            _SimulationData.SpaceMag = spaceMag;

            _ViewParamChangedCount = Math.Max(0, _ViewParamChangedCount + 1);

            _UpdateGridDistance();

            _DisposeGridBitmap();
            _DisposeBackgroundBitmap();
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
                _DisposeBackgroundBitmap();
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
            _DisposeBackgroundBitmap();
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
                    PointD tmpPt = (pt1 - (pt1 - pt2) * (z1 / (z1 - z2))).XY;
                    if (tmpPt.IsZero && pt1.XY.IsZero)
                    {
                        scrPt1 = PointD.NaN;
                        scrPt2 = PointD.NaN;
                        return false;
                    }
                    else
                    {
                        scrPt1 = pt1.ProjectToXY(PointD3D.Zero, _FocalLength).ScaleCopy(1 / _SpaceMag).OffsetCopy(new PointD(_ViewSize) / 2);
                        if (tmpPt.IsZero)
                        {
                            tmpPt = pt1.XY;
                        }
                        scrPt2 = tmpPt.Normalize * (Math.Max(scrPt1.Module, ((PointD)_ViewSize).Module) * 1000);
                        return scrPt1 != scrPt2;
                    }
                }
                else
                {
                    PointD tmpPt = (pt2 - (pt2 - pt1) * (z2 / (z2 - z1))).XY;
                    if (tmpPt.IsZero && pt2.XY.IsZero)
                    {
                        scrPt1 = PointD.NaN;
                        scrPt2 = PointD.NaN;
                        return false;
                    }
                    else
                    {
                        scrPt2 = pt2.ProjectToXY(PointD3D.Zero, _FocalLength).ScaleCopy(1 / _SpaceMag).OffsetCopy(new PointD(_ViewSize) / 2);
                        if (tmpPt.IsZero)
                        {
                            tmpPt = pt2.XY;
                        }
                        scrPt1 = tmpPt.Normalize * (Math.Max(scrPt2.Module, ((PointD)_ViewSize).Module) * 1000);
                        return scrPt1 != scrPt2;
                    }
                }
            }
        }

        // 获取或缓存粒子的仿射变换结果。
        private bool _GetOrCacheTransformResult(Particle particle, out PointD scrPt, out double z)
        {
            TransformResultCache cache = particle.TransformResultCache;
            if (cache.TransformID == _ViewParamChangedCount)
            {
                scrPt = cache.ScreenLocation;
                z = cache.DistanceToScreen;
                return true;
            }
            else
            {
                _WorldToScreen(particle.Location, out scrPt, out z);
                cache.ScreenLocation = scrPt;
                cache.DistanceToScreen = z;
                cache.TransformID = _ViewParamChangedCount;
                return false;
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

        private HashSet<Keys> _PressedKeys = new HashSet<Keys>(); // 键盘正在按下的按键。

        // 更新按键。
        private void _UpdatePressedKeys(Keys[] keys)
        {
            _PressedKeys = new HashSet<Keys>(keys);

            _DisposeBackgroundBitmap();
        }

        private DateTime _SimulationStartTime = DateTime.MinValue; // 仿真开始的日期时间。
        private long _RenderCount = 0; // 自仿真开始以来的累计渲染次数。

        private double _CurrentTime = 0; // 多体系统的当前时间。
        private int _UsingFrameCount = 0; // 使用中的多体系统快照帧数。
        private long _LatestFrameDynamicsId = -1; // 最新的动力学帧ID。
        private long _LatestFrameKinematicsId = -1; // 最新的运动学帧ID。

        private int _TransformRequestNum = 0; // 仿射变换请求次数。
        private int _TransformCachedNum = 0; // 仿射变换缓存命中次数。

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

        private Bitmap _GridBitmap = null; // 坐标系网格位图。

        // 删除坐标系网格位图。
        private void _DisposeGridBitmap()
        {
            if (_GridBitmap != null)
            {
                _GridBitmap.Dispose();
                _GridBitmap = null;
            }
        }

        // 获取或生成坐标系网格位图。
        private Bitmap _GetOrCreateGridBitmap()
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

                using (Graphics graph = Graphics.FromImage(_GridBitmap))
                {
                    graph.SmoothingMode = SmoothingMode.AntiAlias;
                    graph.Clear(_RedrawControl.BackColor);

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
                                            if (Painting2D.PaintLine(graph, _GridBitmap.Size, scrPt1, scrPt2, cr, 1))
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

            return _GridBitmap;
        }

        //

        private Bitmap _BackgroundBitmap = null; // 背景（坐标系网格+提示信息）位图。

        private DateTime _LastCreateBackgroundBitmapTime = DateTime.MinValue;

        // 删除背景位图。
        private void _DisposeBackgroundBitmap()
        {
            if (_BackgroundBitmap != null)
            {
                _BackgroundBitmap.Dispose();
                _BackgroundBitmap = null;
            }
        }

        private static Font _FPSInfoFont = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);
        private static Brush _FPSInfoBrush = new SolidBrush(Color.Gray);

        private static Font _UnpressedKeyFont = new Font("微软雅黑", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
        private static Font _PressedKeyFont = new Font("微软雅黑", 12F, FontStyle.Bold, GraphicsUnit.Point, 134);
        private static Brush _UnpressedKeyBrush = new SolidBrush(Color.Gray);
        private static Brush _PressedKeyBrush = new SolidBrush(Color.Silver);
        private static Pen _UnpressedKeyPen = new Pen(Color.Gray, 1);
        private static Pen _PressedKeyPen = new Pen(Color.Silver, 2);

        // 获取或生成背景位图。
        private Bitmap _GetOrCreateBackgroundBitmap()
        {
            if (_BackgroundBitmap != null && (DateTime.UtcNow - _LastCreateBackgroundBitmapTime).TotalMilliseconds >= 100)
            {
                _DisposeBackgroundBitmap();
            }

            if (_BackgroundBitmap is null)
            {
                _BackgroundBitmap = (Bitmap)_GetOrCreateGridBitmap().Clone();

                using (Graphics graph = Graphics.FromImage(_BackgroundBitmap))
                {
                    graph.SmoothingMode = SmoothingMode.AntiAlias;

                    if (_SimulationIsRunning)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("性能:\n");
                        double dynamicsPFS = _SimulationData.DynamicsPFS;
                        double kinematicsPFS = _SimulationData.KinematicsPFS;
                        if (!_SimulationData.CacheIsFull)
                        {
                            if (dynamicsPFS < _TimeMag / _SimulationData.DynamicsResolution * 0.9)
                            {
                                sb.Append($"   动力学方程(D): {dynamicsPFS:N1} Hz  [性能不佳]\n");
                            }
                            else
                            {
                                sb.Append($"   动力学方程(D): {dynamicsPFS:N1} Hz\n");
                            }

                            if (kinematicsPFS < _TimeMag / _SimulationData.KinematicsResolution * 0.9)
                            {
                                sb.Append($"   轨迹(K): {kinematicsPFS:N1} Hz  [性能不佳]\n");
                            }
                            else
                            {
                                sb.Append($"   轨迹(K): {kinematicsPFS:N1} Hz\n");
                            }
                        }
                        else
                        {
                            sb.Append($"   动力学方程(D): {dynamicsPFS:N1} Hz\n");
                            sb.Append($"   轨迹(K): {kinematicsPFS:N1} Hz\n");
                        }
                        sb.Append($"   -  使用中/已缓存: {_UsingFrameCount}/{_SimulationData.CachedFrameCount} 帧\n");
                        sb.Append($"   仿射变换: {_TransformFrequencyCounter.Frequency:N1} Hz\n");
                        sb.Append($"   -  命中缓存/已提交: {_TransformCachedNum}/{_TransformRequestNum} 次\n");
                        sb.Append($"   直线: {_DrawLineFrequencyCounter.Frequency:N1} Hz\n");
                        double fps = _FrameRateCounter.Frequency;
                        if (fps < 10)
                        {
                            sb.Append($"   刷新率(G): {_FrameRateCounter.Frequency:N1} FPS  [性能不佳]\n\n");
                        }
                        else
                        {
                            sb.Append($"   刷新率(G): {_FrameRateCounter.Frequency:N1} FPS\n\n");
                        }
                        sb.Append($"最新帧: (D) {_SimulationData.LatestFrame.DynamicsId}, (K) {_SimulationData.LatestFrame.KinematicsId}\n");
                        sb.Append($"当前帧: (D) {_LatestFrameDynamicsId}, (K) {_LatestFrameKinematicsId}, (G) {_RenderCount}\n\n");
                        sb.Append($"时间: {Texting.GetLongTimeStringFromTimeSpan(TimeSpan.FromSeconds(_CurrentTime))}");
                        graph.DrawString(sb.ToString(), _FPSInfoFont, _FPSInfoBrush, new Point(5, _BackgroundBitmap.Height - 215));
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("性能:\n");
                        sb.Append($"   仿射变换: {_TransformFrequencyCounter.Frequency:N1} Hz\n");
                        sb.Append($"   刷新率(G): {_FrameRateCounter.Frequency:N1} FPS\n\n");
                        graph.DrawString(sb.ToString(), _FPSInfoFont, _FPSInfoBrush, new Point(5, _BackgroundBitmap.Height - 55));
                    }

                    bool pressedKeysAreLegal = false;
                    if (_PressedKeys.Count == 1)
                    {
                        if (_PressedKeys.Contains(Keys.X) || _PressedKeys.Contains(Keys.Y) || _PressedKeys.Contains(Keys.Z))
                        {
                            pressedKeysAreLegal = true;
                        }
                    }
                    else if (_PressedKeys.Count == 2)
                    {
                        if (_PressedKeys.Contains(Keys.X) && _PressedKeys.Contains(Keys.Y))
                        {
                            pressedKeysAreLegal = true;
                        }
                        else if (_PressedKeys.Contains(Keys.R))
                        {
                            if (_PressedKeys.Contains(Keys.X) || _PressedKeys.Contains(Keys.Y) || _PressedKeys.Contains(Keys.Z))
                            {
                                pressedKeysAreLegal = true;
                            }
                        }
                    }
                    Rectangle rectR = new Rectangle(10, 10, 30, 30);
                    Rectangle rectX = new Rectangle(50, 10, 30, 30);
                    Rectangle rectY = new Rectangle(90, 10, 30, 30);
                    Rectangle rectZ = new Rectangle(130, 10, 30, 30);
                    Action<Rectangle, Keys, string> DrawKey = (rect, key, str) =>
                    {
                        bool pressed = pressedKeysAreLegal && _PressedKeys.Contains(key);
                        Font font = pressed ? _PressedKeyFont : _UnpressedKeyFont;
                        font = Texting.GetSuitableFont(str, font, rect.Size);
                        SizeF size = graph.MeasureString(str, font);
                        PointF loc = new PointF(rect.X + (rect.Width - size.Width) / 2, rect.Y + (rect.Height - size.Height) / 2);
                        Pen pen = pressed ? _PressedKeyPen : _UnpressedKeyPen;
                        Brush br = pressed ? _PressedKeyBrush : _UnpressedKeyBrush;
                        graph.DrawRectangle(pen, rect);
                        graph.DrawString(str, font, br, loc);
                    };
                    DrawKey(rectR, Keys.R, "R");
                    DrawKey(rectX, Keys.X, "X");
                    DrawKey(rectY, Keys.Y, "Y");
                    DrawKey(rectZ, Keys.Z, "Z");
                }

                _LastCreateBackgroundBitmapTime = DateTime.UtcNow;
            }

            return _BackgroundBitmap;
        }

        //

        // 返回将多体系统的当前状态渲染得到的位图。
        private Bitmap _CreateBitmap()
        {
            Bitmap bitmap = (Bitmap)_GetOrCreateBackgroundBitmap().Clone();

            using (Graphics graph = Graphics.FromImage(bitmap))
            {
                graph.SmoothingMode = SmoothingMode.AntiAlias;

                if (_SimulationIsRunning)
                {
                    _CurrentTime = Math.Min(_SimulationData.LatestFrame.Time, (DateTime.UtcNow - _SimulationStartTime).TotalSeconds * _TimeMag);
                    Snapshot snapshot = _SimulationData.GetSnapshot(_CurrentTime - _SimulationData.TrackLength, _CurrentTime);

                    _TransformRequestNum = 0;
                    _TransformCachedNum = 0;

                    _LatestFrameDynamicsId = -1;
                    _LatestFrameKinematicsId = -1;

                    _UsingFrameCount = snapshot.FrameCount;
                    if (snapshot != null && _UsingFrameCount > 0)
                    {
                        Frame latestFrame = snapshot.LatestFrame;
                        _LatestFrameDynamicsId = latestFrame.DynamicsId;
                        _LatestFrameKinematicsId = latestFrame.KinematicsId;
                        int particleCount = latestFrame.ParticleCount;

                        RectangleF bitmapBounds = new RectangleF(new PointF(), bitmap.Size);

                        int transformNum = 0;
                        int lineNum = 0;

                        if (_UsingFrameCount > 1)
                        {
                            const double MinLineLength = 3;
                            for (int i = 0; i < particleCount; i++)
                            {
                                int j = _UsingFrameCount - 1, k = _UsingFrameCount - 2;

                                Particle particle1 = snapshot.GetFrame(j).GetParticle(i);
                                _TransformRequestNum++;
                                if (_GetOrCacheTransformResult(particle1, out PointD pt1, out _))
                                {
                                    _TransformCachedNum++;
                                }
                                else
                                {
                                    transformNum++;
                                }

                                Particle particle2 = snapshot.GetFrame(k).GetParticle(i);
                                _TransformRequestNum++;
                                if (_GetOrCacheTransformResult(particle2, out PointD pt2, out _))
                                {
                                    _TransformCachedNum++;
                                }
                                else
                                {
                                    transformNum++;
                                }

                                while (true)
                                {
                                    if (pt1.DistanceFrom(pt2) >= MinLineLength || k == 0)
                                    {
                                        int alpha = (int)Math.Round(255.0 * j / _UsingFrameCount);
                                        if (alpha >= 1 && Painting2D.PaintLine(graph, bitmap.Size, pt1, pt2, Color.FromArgb(Math.Min(255, alpha), latestFrame.GetParticle(i).Color), 1))
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
                                        _TransformRequestNum++;
                                        if (_GetOrCacheTransformResult(particle2, out pt2, out _))
                                        {
                                            _TransformCachedNum++;
                                        }
                                        else
                                        {
                                            transformNum++;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        int lowRadius = Math.Min(bitmap.Width, bitmap.Height) / 2;
                        int highRadius = Math.Max(lowRadius * 2, Math.Max(bitmap.Width, bitmap.Height) / 2);
                        for (int i = 0; i < particleCount; i++)
                        {
                            Particle particle = latestFrame.GetParticle(i);
                            _TransformRequestNum++;
                            if (_GetOrCacheTransformResult(particle, out PointD pt, out double z))
                            {
                                _TransformCachedNum++;
                            }
                            else
                            {
                                transformNum++;
                            }

                            float radius = Math.Max(1, (float)(particle.Radius * _FocalLength / z));

                            if (radius < highRadius && Geometry.CircleInnerIsVisibleInRectangle(pt, radius, bitmapBounds))
                            {
                                RectangleF ellipse = new RectangleF((float)pt.X - radius, (float)pt.Y - radius, radius * 2, radius * 2);
                                if (radius <= lowRadius)
                                {
                                    graph.FillEllipse(particle.Brush, ellipse);
                                }
                                else
                                {
                                    int alpha = (int)Math.Round(255 * (highRadius - radius) / (highRadius - lowRadius));
                                    if (alpha >= 1)
                                    {
                                        Color cr = Color.FromArgb(Math.Min(255, alpha), particle.Color);
                                        using (SolidBrush br = new SolidBrush(cr))
                                        {
                                            graph.FillEllipse(br, ellipse);
                                        }
                                    }
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

                    _RenderCount++;
                }
                else
                {
                    int particleCount = _SimulationData.ParticleCount;

                    if (particleCount > 0)
                    {
                        RectangleF bitmapBounds = new RectangleF(new PointF(), bitmap.Size);

                        int lowRadius = Math.Min(bitmap.Width, bitmap.Height) / 2;
                        int highRadius = Math.Max(lowRadius * 2, Math.Max(bitmap.Width, bitmap.Height) / 2);
                        for (int i = 0; i < particleCount; i++)
                        {
                            Particle particle = _SimulationData.GetParticle(i);

                            if (_WorldToScreen(particle.Location, out PointD pt, out double z))
                            {
                                float radius = Math.Max(1, (float)(particle.Radius * _FocalLength / z));

                                if (radius < highRadius && Geometry.CircleInnerIsVisibleInRectangle(pt, radius, bitmapBounds))
                                {
                                    RectangleF ellipse = new RectangleF((float)pt.X - radius, (float)pt.Y - radius, radius * 2, radius * 2);
                                    if (radius <= lowRadius)
                                    {
                                        graph.FillEllipse(particle.Brush, ellipse);
                                    }
                                    else
                                    {
                                        int alpha = (int)Math.Round(255 * (highRadius - radius) / (highRadius - lowRadius));
                                        if (alpha >= 1)
                                        {
                                            Color cr = Color.FromArgb(Math.Min(255, alpha), particle.Color);
                                            using (SolidBrush br = new SolidBrush(cr))
                                            {
                                                graph.FillEllipse(br, ellipse);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        _TransformFrequencyCounter.Update(particleCount);
                    }

                    StringBuilder sb = new StringBuilder();
                    sb.Append("性能:\n");
                    sb.Append($"   仿射变换: {_TransformFrequencyCounter.Frequency:N1} Hz\n");
                    sb.Append($"   刷新率(G): {_FrameRateCounter.Frequency:N1} FPS\n\n");
                    graph.DrawString(sb.ToString(), _FPSInfoFont, _FPSInfoBrush, new Point(5, bitmap.Height - 55));
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
            _RedrawControl.Invoke(_RedrawMethod, _CreateBitmap());

            _FrameRateCounter.Update();
        }

        #endregion
    }
}