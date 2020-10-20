/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2020 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.0.0.DEV.200818-0000

This file is part of "多体系统模拟" (MultibodySystemSimulation)

"多体系统模拟" (MultibodySystemSimulation) is released under the GPLv3 license
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multibody
{
    // 仿真数据。
    internal sealed class SimulationData
    {
        #region 构造函数

        public SimulationData()
        {

        }

        #endregion

        #region 仿真

        private bool _SimulationIsRunning = false; // 是否正在运行仿真。

        public bool SimulationIsRunning
        {
            get
            {
                return _SimulationIsRunning;
            }

            set
            {
                _SimulationIsRunning = value;
            }
        }

        #endregion

        #region 粒子与多体系统（静态数据）

        private double _DynamicsResolution = 1; // 动力学分辨率（秒），指期待每次求解动力学微分方程组的时间微元 dT，表现为仿真计算的精确程度。
        private double _KinematicsResolution = 1000; // 运动学分辨率（秒），指期待每次抽取运动学状态的时间间隔 ΔT，表现为轨迹绘制的平滑程度。
        private double _CacheSize = 2000000; // 缓存大小（秒），指缓存运动学状态的最大时间跨度。
        private double _TrackLength = 1000000; // 轨迹长度（秒）。

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

        public double TrackLength
        {
            get
            {
                return _TrackLength;
            }

            set
            {
                _TrackLength = value;
            }
        }

        //

        private List<Particle> _Particles = new List<Particle>(); // 粒子列表。

        // 获取粒子数量。
        public int ParticleCount => _Particles.Count;

        // 添加粒子。
        public void AddParticle(Particle particle)
        {
            if (_SimulationIsRunning)
            {
                throw new InvalidOperationException();
            }

            //

            _Particles.Add(particle.Copy());
        }

        // 删除粒子。
        public void RemoveParticle(int index)
        {
            if (_SimulationIsRunning)
            {
                throw new InvalidOperationException();
            }

            //

            _Particles.RemoveAt(index);
        }

        // 获取粒子。
        public Particle GetParticle(int index)
        {
            return _Particles[index].Copy();
        }

        // 设置粒子。
        public void SetParticle(int index, Particle particle)
        {
            if (_SimulationIsRunning)
            {
                throw new InvalidOperationException();
            }

            //

            _Particles[index] = particle.Copy();
        }

        #endregion

        #region 粒子与多体系统（动态数据）

        private MultibodySystem _MultibodySystem = null; // 多体系统。

        // 获取多体系统的缓存是否已满。
        public bool CacheIsFull => _MultibodySystem.CacheIsFull;

        // 获取多体系统当前已缓存的总帧数。
        public int CachedFrameCount => _MultibodySystem.FrameCount;

        // 获取多体系统的最新一帧。
        public Frame LatestFrame => _MultibodySystem.LatestFrame;

        // 获取多体系统当前的动力学频率。
        public double DynamicsPFS => _MultibodySystem.DynamicsFrequencyCounter.Frequency;

        // 获取多体系统当前的运动学频率。
        public double KinematicsPFS => _MultibodySystem.KinematicsFrequencyCounter.Frequency;

        // 初始化多体系统。
        public void InitializeMultibodySystem()
        {
            _MultibodySystem = new MultibodySystem(_DynamicsResolution, _KinematicsResolution, _CacheSize, _Particles);
        }

        // 将多体系统运动指定的时长（秒）。
        public void NextMoment(double seconds)
        {
            _MultibodySystem.NextMoment(seconds);
        }

        // 将多体系统运动与轨迹分辨率相同的时长。
        public void NextMoment()
        {
            _MultibodySystem.NextMoment();
        }

        // 获取多体系统在指定时间区间内的快照。
        public Snapshot GetSnapshot(double startTime, double endTime)
        {
            Snapshot snapshot = _MultibodySystem.GetSnapshot(startTime, endTime);

            if (snapshot.FrameCount > 0)
            {
                _MultibodySystem.DiscardCache(snapshot.OldestFrame.Time);
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

        #endregion

        #region 渲染

        internal const double InitialTimeMag = 100000; // 初始值。
        private double _TimeMag = InitialTimeMag; // 时间倍率（秒/秒），指仿真时间流逝速度与真实时间流逝速度的比值，表现为动画的播放速度。

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

        #endregion
    }
}