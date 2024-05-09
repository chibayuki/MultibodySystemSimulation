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
            }
        }

        //

        private Point _CoordinateOffset; // 坐标系偏移。

        // 更新坐标系偏移。
        private void _UpdateCoordinateOffset(Point coordinateOffset) => _CoordinateOffset = coordinateOffset;

        // 世界坐标系转换到屏幕坐标系。
        private PointD _WorldToScreen(PointD3D pt) => pt.AffineTransformCopy(_AffineTransformation).ProjectToXY(PointD3D.Zero, _FocalLength).ScaleCopy(1 / _SpaceMag).OffsetCopy(_CoordinateOffset);

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

        private Font _Font = new Font("微软雅黑", 9F, FontStyle.Bold, GraphicsUnit.Point, 134);

        // 返回将多体系统的当前状态渲染得到的位图。
        private Bitmap _GenerateBitmap()
        {
            Size bitmapSize = _BitmapSize;

            Bitmap bitmap = new Bitmap(Math.Max(1, bitmapSize.Width), Math.Max(1, bitmapSize.Height));

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
                    using (Graphics Grap = Graphics.FromImage(bitmap))
                    {
                        Grap.SmoothingMode = SmoothingMode.AntiAlias;
                        Grap.Clear(_RedrawControl.BackColor);

                        RectangleF bitmapBounds = new RectangleF(new PointF(), bitmap.Size);

                        Frame latestFrame = snapshot.LatestFrame;
                        latestFrame.GraphicsId = _GenerateCount;

                        int FrameCount = snapshot.FrameCount;

                        for (int i = 0; i < latestFrame.ParticleCount; i++)
                        {
                            PointD location = _WorldToScreen(latestFrame.GetParticle(i).Location);

                            for (int j = FrameCount - 1; j >= 1; j--)
                            {
                                PointD pt1 = _WorldToScreen(snapshot.GetFrame(j).GetParticle(i).Location);
                                PointD pt2 = _WorldToScreen(snapshot.GetFrame(j - 1).GetParticle(i).Location);

                                if (Geometry.LineIsVisibleInRectangle(pt1, pt2, bitmapBounds))
                                {
                                    Painting2D.PaintLine(bitmap, pt1, pt2, Color.FromArgb(255 * j / FrameCount, latestFrame.GetParticle(i).Color), 1, true);
                                }
                            }
                        }

                        for (int i = 0; i < latestFrame.ParticleCount; i++)
                        {
                            PointD location = _WorldToScreen(latestFrame.GetParticle(i).Location);

                            float radius = Math.Max(1, (float)(latestFrame.GetParticle(i).Radius * _FocalLength / latestFrame.GetParticle(i).Location.Z));

                            if (Geometry.CircleInnerIsVisibleInRectangle(location, radius, bitmapBounds))
                            {
                                using (Brush Br = new SolidBrush(latestFrame.GetParticle(i).Color))
                                {
                                    Grap.FillEllipse(Br, new RectangleF((float)location.X - radius, (float)location.Y - radius, radius * 2, radius * 2));
                                }
                            }
                        }

                        using (Brush Br = new SolidBrush(Color.Silver))
                        {
                            Grap.DrawString("帧率:", _Font, Br, new Point(5, bitmapSize.Height - 220));
                            Grap.DrawString($"    动力学(D): {_SimulationData.DynamicsPFS:N1} Hz", _Font, Br, new Point(5, bitmapSize.Height - 200));
                            Grap.DrawString($"    运动学(K): {_SimulationData.KinematicsPFS:N1} Hz", _Font, Br, new Point(5, bitmapSize.Height - 180));
                            Grap.DrawString($"    图形学(G): {_FrameRateCounter.Frequency:N1} FPS", _Font, Br, new Point(5, bitmapSize.Height - 160));

                            Grap.DrawString($"已缓存: {_SimulationData.CachedFrameCount} 帧", _Font, Br, new Point(5, bitmapSize.Height - 120));
                            Grap.DrawString($"使用中: {snapshot.FrameCount} 帧", _Font, Br, new Point(5, bitmapSize.Height - 100));
                            Grap.DrawString($"最新帧: D {_SimulationData.LatestFrame.DynamicsId}, K {_SimulationData.LatestFrame.KinematicsId}", _Font, Br, new Point(5, bitmapSize.Height - 80));
                            Grap.DrawString($"当前帧: D {latestFrame.DynamicsId}, K {latestFrame.KinematicsId}, G {latestFrame.GraphicsId}", _Font, Br, new Point(5, bitmapSize.Height - 60));

                            Grap.DrawString($"时间:   {Texting.GetLongTimeStringFromTimeSpan(TimeSpan.FromSeconds(snapshot.LatestFrame.Time))}", _Font, Br, new Point(5, bitmapSize.Height - 20));
                        }
                    }

                    _GenerateCount++;
                }
            }
            else
            {
                if (_SimulationData.ParticleCount > 0)
                {

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
            using (Bitmap bitmap = _GenerateBitmap())
            {
                _RedrawControl.Invoke(_RedrawMethod, (Bitmap)bitmap.Clone());
            }

            _FrameRateCounter.Update();
        }

        //

        // 获取当前的重绘刷新率（图形学频率）。
        public double GraphicsFPS => _FrameRateCounter.Frequency;

        #endregion
    }
}