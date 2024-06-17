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

using System.Threading;

namespace Multibody
{
    // 仿真数据。
    internal sealed class SimulationData
    {
        #region 构造函数

        public SimulationData()
        {
        }

        ~SimulationData()
        {
            _SimulationStateLock.Dispose();
            _StaticDataLock.Dispose();
            _ParticlesLock.Dispose();
            _MultibodySystemLock.Dispose();
            _GraphicsLock.Dispose();
            _RenderLock.Dispose();
        }

        #endregion

        #region 仿真

        private bool _SimulationIsRunning = false; // 是否正在运行仿真。

        private ReaderWriterLockSlim _SimulationStateLock = new ReaderWriterLockSlim();

        public bool SimulationIsRunning
        {
            get
            {
                bool result = false;

                _SimulationStateLock.EnterReadLock();

                try
                {
                    result = _SimulationIsRunning;
                }
                finally
                {
                    _SimulationStateLock.ExitReadLock();
                }

                return result;
            }
            set
            {
                _SimulationStateLock.EnterWriteLock();

                try
                {
                    _SimulationIsRunning = value;
                }
                finally
                {
                    _SimulationStateLock.ExitWriteLock();
                }
            }
        }

        #endregion

        #region 粒子与多体系统（静态数据）

        private double _DynamicsResolution = 1; // 动力学分辨率（秒），指期待每次求解动力学微分方程组的时间微元 dT，表现为仿真计算的精确程度。
        private double _KinematicsResolution = 200; // 运动学分辨率（秒），指期待每次抽取运动学状态的时间间隔 ΔT，表现为轨迹绘制的平滑程度。
        private double _CacheSize = 2000000; // 缓存大小（秒），指缓存运动学状态的最大时间跨度。
        private double _TrackLength = 1000000; // 轨迹长度（秒）。

        private ReaderWriterLockSlim _StaticDataLock = new ReaderWriterLockSlim();

        public double DynamicsResolution
        {
            get
            {
                double result = 0;

                _StaticDataLock.EnterReadLock();

                try
                {
                    result = _DynamicsResolution;
                }
                finally
                {
                    _StaticDataLock.ExitReadLock();
                }

                return result;
            }
            set
            {
                _StaticDataLock.EnterWriteLock();

                try
                {
                    _DynamicsResolution = value;
                }
                finally
                {
                    _StaticDataLock.ExitWriteLock();
                }
            }
        }

        public double KinematicsResolution
        {
            get
            {
                double result = 0;

                _StaticDataLock.EnterReadLock();

                try
                {
                    result = _KinematicsResolution;
                }
                finally
                {
                    _StaticDataLock.ExitReadLock();
                }

                return result;
            }
            set
            {
                _StaticDataLock.EnterWriteLock();

                try
                {
                    _KinematicsResolution = value;
                }
                finally
                {
                    _StaticDataLock.ExitWriteLock();
                }
            }
        }

        public double CacheSize
        {
            get
            {
                double result = 0;

                _StaticDataLock.EnterReadLock();

                try
                {
                    result = _CacheSize;
                }
                finally
                {
                    _StaticDataLock.ExitReadLock();
                }

                return result;
            }
            set
            {
                _StaticDataLock.EnterWriteLock();

                try
                {
                    _CacheSize = value;
                }
                finally
                {
                    _StaticDataLock.ExitWriteLock();
                }
            }
        }

        public double TrackLength
        {
            get
            {
                double result = 0;

                _StaticDataLock.EnterReadLock();

                try
                {
                    result = _TrackLength;
                }
                finally
                {
                    _StaticDataLock.ExitReadLock();
                }

                return result;
            }
            set
            {
                _StaticDataLock.EnterWriteLock();

                try
                {
                    _TrackLength = value;
                }
                finally
                {
                    _StaticDataLock.ExitWriteLock();
                }
            }
        }

        //

        private List<Particle> _Particles = new List<Particle>(); // 粒子列表。

        private ReaderWriterLockSlim _ParticlesLock = new ReaderWriterLockSlim();

        // 获取粒子数量。
        public int ParticleCount
        {
            get
            {
                int result = 0;

                _ParticlesLock.EnterReadLock();

                try
                {
                    result = _Particles.Count;
                }
                finally
                {
                    _ParticlesLock.ExitReadLock();
                }

                return result;
            }
        }

        // 添加粒子。
        public void AddParticle(Particle particle)
        {
            _ParticlesLock.EnterWriteLock();

            try
            {
                _Particles.Add(particle.Copy());
            }
            finally
            {
                _ParticlesLock.ExitWriteLock();
            }
        }

        // 删除粒子。
        public void RemoveParticle(int index)
        {
            _ParticlesLock.EnterWriteLock();

            try
            {
                _Particles.RemoveAt(index);
            }
            finally
            {
                _ParticlesLock.ExitWriteLock();
            }
        }

        // 获取粒子。
        public Particle GetParticle(int index)
        {
            Particle result = null;

            _ParticlesLock.EnterReadLock();

            try
            {
                result = _Particles[index].Copy();
            }
            finally
            {
                _ParticlesLock.ExitReadLock();
            }

            return result;
        }

        // 设置粒子。
        public void SetParticle(int index, Particle particle)
        {
            _ParticlesLock.EnterWriteLock();

            try
            {
                _Particles[index] = particle.Copy();
            }
            finally
            {
                _ParticlesLock.ExitWriteLock();
            }
        }

        #endregion

        #region 粒子与多体系统（动态数据）

        private MultibodySystem _MultibodySystem = null; // 多体系统。

        private ReaderWriterLockSlim _MultibodySystemLock = new ReaderWriterLockSlim();

        // 获取多体系统的缓存是否已满。
        public bool CacheIsFull
        {
            get
            {
                bool result = false;

                _MultibodySystemLock.EnterReadLock();

                try
                {
                    result = _MultibodySystem.CacheIsFull;
                }
                finally
                {
                    _MultibodySystemLock.ExitReadLock();
                }

                return result;
            }
        }

        // 获取多体系统的缓存容量。
        public int CacheCapacity
        {
            get
            {
                int result = 0;

                _MultibodySystemLock.EnterReadLock();

                try
                {
                    result = _MultibodySystem.FrameCapacity;
                }
                finally
                {
                    _MultibodySystemLock.ExitReadLock();
                }

                return result;
            }
        }

        // 获取多体系统当前已缓存的总帧数。
        public int CachedFrameCount
        {
            get
            {
                int result = 0;

                _MultibodySystemLock.EnterReadLock();

                try
                {
                    result = _MultibodySystem.FrameCount;
                }
                finally
                {
                    _MultibodySystemLock.ExitReadLock();
                }

                return result;
            }
        }

        // 获取多体系统的最新一帧。
        public Frame LatestFrame
        {
            get
            {
                Frame result = null;

                _MultibodySystemLock.EnterReadLock();

                try
                {
                    result = _MultibodySystem.LatestFrame;
                }
                finally
                {
                    _MultibodySystemLock.ExitReadLock();
                }

                return result;
            }
        }

        // 获取多体系统当前的动力学频率。
        public double DynamicsPFS
        {
            get
            {
                double result = 0;

                _MultibodySystemLock.EnterReadLock();

                try
                {
                    result = _MultibodySystem.DynamicsFrequencyCounter.Frequency;
                }
                finally
                {
                    _MultibodySystemLock.ExitReadLock();
                }

                return result;
            }
        }

        // 获取多体系统当前的运动学频率。
        public double KinematicsPFS
        {
            get
            {
                double result = 0;

                _MultibodySystemLock.EnterReadLock();

                try
                {
                    result = _MultibodySystem.KinematicsFrequencyCounter.Frequency;
                }
                finally
                {
                    _MultibodySystemLock.ExitReadLock();
                }

                return result;
            }
        }

        // 初始化多体系统。
        public void InitializeMultibodySystem()
        {
            _MultibodySystemLock.EnterWriteLock();

            try
            {
                _MultibodySystem = new MultibodySystem(_DynamicsResolution, _KinematicsResolution, _CacheSize, _Particles);
            }
            finally
            {
                _MultibodySystemLock.ExitWriteLock();
            }
        }

        // 将多体系统运动指定的时长（秒）。
        public void NextMoment(double seconds)
        {
            _MultibodySystemLock.EnterWriteLock();

            try
            {
                _MultibodySystem.NextMoment(seconds);
            }
            finally
            {
                _MultibodySystemLock.ExitWriteLock();
            }
        }

        // 将多体系统运动与轨迹分辨率相同的时长。
        public void NextMoment()
        {
            _MultibodySystemLock.EnterWriteLock();

            try
            {
                _MultibodySystem.NextMoment();
            }
            finally
            {
                _MultibodySystemLock.ExitWriteLock();
            }
        }

        // 获取多体系统在指定时间区间内的快照。
        public Snapshot GetSnapshot(double startTime, double endTime)
        {
            Snapshot snapshot = null;

            _MultibodySystemLock.EnterReadLock();

            try
            {
                snapshot = _MultibodySystem.GetSnapshot(startTime, endTime);
            }
            finally
            {
                _MultibodySystemLock.ExitReadLock();
            }

            if (snapshot != null && snapshot.FrameCount > 0)
            {
                _MultibodySystemLock.EnterWriteLock();

                try
                {
                    _MultibodySystem.DiscardCache(Math.Min(startTime, snapshot.OldestFrame.Time));
                }
                finally
                {
                    _MultibodySystemLock.ExitWriteLock();
                }
            }

            return snapshot;
        }

        #endregion

        #region 图形学与视图控制

        //

        internal const double InitialFocalLength = 1000; // 初始值。
        private double _FocalLength = InitialFocalLength; // 投影变换使用的焦距。

        internal const double InitialSpaceMag = 1; // 初始值。
        private double _SpaceMag = InitialSpaceMag; // 空间倍率（米/像素），指投影变换焦点附近每像素表示的长度。

        private ReaderWriterLockSlim _GraphicsLock = new ReaderWriterLockSlim();

        public double FocalLength
        {
            get
            {
                double result = 0;

                _GraphicsLock.EnterReadLock();

                try
                {
                    result = _FocalLength;
                }
                finally
                {
                    _GraphicsLock.ExitReadLock();
                }

                return result;
            }
            set
            {
                _GraphicsLock.EnterWriteLock();

                try
                {
                    _FocalLength = value;
                }
                finally
                {
                    _GraphicsLock.ExitWriteLock();
                }
            }
        }

        public double SpaceMag
        {
            get
            {
                double result = 0;

                _GraphicsLock.EnterReadLock();

                try
                {
                    result = _SpaceMag;
                }
                finally
                {
                    _GraphicsLock.ExitReadLock();
                }

                return result;
            }
            set
            {
                _GraphicsLock.EnterWriteLock();

                try
                {
                    _SpaceMag = value;
                }
                finally
                {
                    _GraphicsLock.ExitWriteLock();
                }
            }
        }

        #endregion

        #region 渲染

        internal const double InitialTimeMag = 100000; // 初始值。
        private double _TimeMag = InitialTimeMag; // 时间倍率（秒/秒），指仿真时间流逝速度与真实时间流逝速度的比值，表现为动画的播放速度。

        private ReaderWriterLockSlim _RenderLock = new ReaderWriterLockSlim();

        public double TimeMag
        {
            get
            {
                double result = 0;

                _RenderLock.EnterReadLock();

                try
                {
                    result = _TimeMag;
                }
                finally
                {
                    _RenderLock.ExitReadLock();
                }

                return result;
            }
            set
            {
                _RenderLock.EnterWriteLock();

                try
                {
                    _TimeMag = value;
                }
                finally
                {
                    _RenderLock.ExitWriteLock();
                }
            }
        }

        #endregion
    }
}