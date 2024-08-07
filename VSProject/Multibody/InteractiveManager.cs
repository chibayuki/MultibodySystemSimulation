﻿/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2024 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.228.1000.M3.240721-1100

This file is part of "多体系统模拟" (MultibodySystemSimulation)

"多体系统模拟" (MultibodySystemSimulation) is released under the GPLv3 license
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms;

using PointD = Com.PointD;
using UIMessage = Com.WinForm.UIMessage;

namespace Multibody
{
    // 交互管理器。
    internal sealed class InteractiveManager
    {
        private SimulationData _SimulationData;

        private Renderer _Renderer;
        private Simulator _Simulator;

        #region 构造函数

        public InteractiveManager(Control redrawControl, Action<Bitmap> redrawMethod, Size viewSize)
        {
            if (redrawControl is null || redrawMethod is null)
            {
                throw new ArgumentNullException();
            }

            //

            _SimulationData = new SimulationData();

            _Renderer = new Renderer(_SimulationData, redrawControl, redrawMethod, viewSize);
            _Simulator = new Simulator(_SimulationData);

            _Renderer.Start();
        }

        #endregion

        #region 仿真

        public bool SimulationIsRunning => _SimulationData.SimulationIsRunning;

        // 仿真开始。
        public void SimulationStart()
        {
            if (!_SimulationData.SimulationIsRunning)
            {
                _SimulationData.SimulationIsRunning = true;
                _SimulationData.InitializeMultibodySystem();

                _Simulator.Start();
                _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.SimulationStart));
            }
        }

        // 仿真停止。
        public void SimulationStop()
        {
            if (_SimulationData.SimulationIsRunning)
            {
                _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.SimulationStop));
                _Simulator.Stop();

                _SimulationData.SimulationIsRunning = false;
            }
        }

        #endregion

        #region 粒子与多体系统（静态数据）

        public double DynamicsResolution
        {
            get => _SimulationData.DynamicsResolution;

            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (_SimulationData.SimulationIsRunning)
                {
                    throw new InvalidOperationException();
                }

                //

                _SimulationData.DynamicsResolution = value;
            }
        }

        public double KinematicsResolution
        {
            get => _SimulationData.KinematicsResolution;

            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (_SimulationData.SimulationIsRunning)
                {
                    throw new InvalidOperationException();
                }

                //

                _SimulationData.KinematicsResolution = value;
            }
        }

        public double CacheSize
        {
            get => _SimulationData.CacheSize;

            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (_SimulationData.SimulationIsRunning)
                {
                    throw new InvalidOperationException();
                }

                //

                _SimulationData.CacheSize = value;
            }
        }

        public double TrackLength
        {
            get => _SimulationData.TrackLength;

            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (_SimulationData.SimulationIsRunning)
                {
                    throw new InvalidOperationException();
                }

                //

                _SimulationData.TrackLength = value;
            }
        }

        //

        // 获取粒子数量。
        public int ParticleCount => _SimulationData.ParticleCount;

        // 添加粒子。
        public void AddParticles(IEnumerable<Particle> particles)
        {
            if (particles is null)
            {
                throw new ArgumentNullException();
            }

            if (_SimulationData.SimulationIsRunning)
            {
                throw new InvalidOperationException();
            }

            //

            _SimulationData.AddParticles(particles);
        }

        // 添加粒子。
        public void AddParticle(Particle particle)
        {
            if (particle is null)
            {
                throw new ArgumentNullException();
            }

            if (_SimulationData.SimulationIsRunning)
            {
                throw new InvalidOperationException();
            }

            //

            _SimulationData.AddParticle(particle);
        }

        // 删除粒子。
        public void RemoveAllParticles()
        {
            if (_SimulationData.SimulationIsRunning)
            {
                throw new InvalidOperationException();
            }

            //

            _SimulationData.RemoveAllParticles();
        }

        // 删除粒子。
        public void RemoveParticle(int index)
        {
            if (_SimulationData.SimulationIsRunning)
            {
                throw new InvalidOperationException();
            }

            if (index < 0 || index >= _SimulationData.ParticleCount)
            {
                throw new IndexOutOfRangeException();
            }

            //

            _SimulationData.RemoveParticle(index);
        }

        // 获取粒子。
        public Particle GetParticle(int index) => _SimulationData.GetParticle(index);

        // 设置粒子。
        public void SetParticle(int index, Particle particle)
        {
            if (particle is null)
            {
                throw new ArgumentNullException();
            }

            if (_SimulationData.SimulationIsRunning)
            {
                throw new InvalidOperationException();
            }

            if (index < 0 || index >= _SimulationData.ParticleCount)
            {
                throw new IndexOutOfRangeException();
            }

            //

            _SimulationData.SetParticle(index, particle);
        }

        #endregion

        #region 粒子与多体系统（动态数据）

        // 获取多体系统当前的动力学频率。
        public double DynamicsPFS => _SimulationData.DynamicsFPS;

        // 获取多体系统当前的运动学频率。
        public double KinematicsPFS => _SimulationData.KinematicsFPS;

        #endregion

        #region 图形学与视图控制

        public double FocalLength
        {
            get => _SimulationData.FocalLength;

            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.SetFocalLength) { RequestData = value });
            }
        }

        public double SpaceMag
        {
            get => _SimulationData.SpaceMag;

            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.SetSpaceMag) { RequestData = value });
            }
        }

        //

        // 视图控制开始。
        public void ViewOperationStart() => _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.ViewOperationStart));

        // 视图控制停止。
        public void ViewOperationStop() => _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.ViewOperationStop));

        // 视图控制。
        public void ViewOperationOffsetX(double offset) => _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.ViewOperationUpdateParam) { RequestData = new (Renderer.ViewOperationType, double)[] { (Renderer.ViewOperationType.OffsetX, offset) } });

        public void ViewOperationOffsetY(double offset) => _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.ViewOperationUpdateParam) { RequestData = new (Renderer.ViewOperationType, double)[] { (Renderer.ViewOperationType.OffsetY, offset) } });

        public void ViewOperationOffsetXY(PointD offset) => _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.ViewOperationUpdateParam) { RequestData = new (Renderer.ViewOperationType, double)[] { (Renderer.ViewOperationType.OffsetX, offset.X), (Renderer.ViewOperationType.OffsetY, offset.Y) } });

        public void ViewOperationOffsetZ(double offset) => _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.ViewOperationUpdateParam) { RequestData = new (Renderer.ViewOperationType, double)[] { (Renderer.ViewOperationType.OffsetZ, offset) } });

        public void ViewOperationRotateX(double rotate) => _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.ViewOperationUpdateParam) { RequestData = new (Renderer.ViewOperationType, double)[] { (Renderer.ViewOperationType.RotateX, rotate) } });

        public void ViewOperationRotateY(double rotate) => _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.ViewOperationUpdateParam) { RequestData = new (Renderer.ViewOperationType, double)[] { (Renderer.ViewOperationType.RotateY, rotate) } });

        public void ViewOperationRotateZ(double rotate) => _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.ViewOperationUpdateParam) { RequestData = new (Renderer.ViewOperationType, double)[] { (Renderer.ViewOperationType.RotateZ, rotate) } });

        //

        // 更新按键。
        public void PressedKeysChanged(HashSet<Keys> pressedKeys)
        {
            Keys[] data = Array.Empty<Keys>();
            if (pressedKeys.Count > 0)
            {
                data = new Keys[pressedKeys.Count];
                pressedKeys.CopyTo(data);
            }
            _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.PressedKeysChanged) { RequestData = data });
        }

        //

        // 更新视图大小。
        public void UpdateViewSize(Size viewSize) => _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.UpdateViewSize) { RequestData = viewSize });

        #endregion

        #region 渲染

        public double TimeMag
        {
            get => _SimulationData.TimeMag;

            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _Renderer.PushMessage(new UIMessage((int)Renderer.MessageCode.SetTimeMag) { RequestData = value });
            }
        }

        #endregion
    }
}