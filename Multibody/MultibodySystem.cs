/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2020 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.0.0.DEV.200702-0000

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
    // 多体系统。
    internal sealed class MultibodySystem
    {
        private double _DynamicsResolution; // 动力学分辨率（秒）。
        private double _KinematicsResolution; // 运动学分辨率（秒）。
        private double _CacheSize; // 缓存大小（秒）。
        private Frame _InitialFrame; // 初始帧。
        private FixedQueue<Frame> _FrameHistory; // 历史帧。
        private FrequencyCounter _DynamicsFrequencyCounter = new FrequencyCounter(); // 动力学频率计数器。
        private FrequencyCounter _KinematicsFrequencyCounter = new FrequencyCounter(); // 运动学频率计数器。

        public MultibodySystem(double dynamicsResolution, double kinematicsResolution, double cacheSize, params Particle[] particles)
        {
            Reset(dynamicsResolution, kinematicsResolution, cacheSize, particles);
        }

        public MultibodySystem(double dynamicsResolution, double kinematicsResolution, double cacheSize, List<Particle> particles)
        {
            Reset(dynamicsResolution, kinematicsResolution, cacheSize, particles);
        }

        // 获取此 MultibodySystem 对象的动力学分辨率（秒）。
        public double DynamicsResolution => _DynamicsResolution;

        // 获取此 MultibodySystem 对象的运动学分辨率（秒）。
        public double KinematicsResolution => _KinematicsResolution;

        // 获取此 MultibodySystem 对象的缓存大小（秒）。
        public double CacheSize => _CacheSize;

        // 获取此 MultibodySystem 对象的初始帧。
        public Frame InitialFrame => _InitialFrame;

        // 获取此 MultibodySystem 对象的最新一帧。
        public Frame LatestFrame => _FrameHistory.Tail;

        // 获取此 MultibodySystem 对象的帧容量。
        public int FrameCapacity => _FrameHistory.Capacity;

        // 获取此 MultibodySystem 对象的总帧数。
        public int FrameCount => _FrameHistory.Count;

        // 获取此 MultibodySystem 对象的动力学频率计数器。
        public FrequencyCounter DynamicFrequencyCounter => _DynamicsFrequencyCounter;

        // 获取此 MultibodySystem 对象的运动学频率计数器。
        public FrequencyCounter KinematicsFrequencyCounter => _KinematicsFrequencyCounter;

        // 获取此 MultibodySystem 对象的指定帧。
        public Frame Frame(int index)
        {
            return _FrameHistory[index];
        }

        // 将此 MultibodySystem 对象运动指定的时长（秒）。
        public void NextMoment(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < _KinematicsResolution)
            {
                throw new ArgumentException();
            }

            //

            int countK = (int)Math.Round(seconds / _KinematicsResolution);
            int countD = (int)Math.Round(_KinematicsResolution / _DynamicsResolution);
            int fpsDivK = Math.Max(1, countK / 10);
            int fpsDivD = Math.Max(1, countD / 10);

            for (int k = 1; k <= countK; k++)
            {
                Frame frame = LatestFrame.Copy();

                for (int d = 1; d <= countD; d++)
                {
                    frame.NextMoment(_DynamicsResolution);

                    if (fpsDivD == 1 || d % fpsDivD == 0)
                    {
                        _DynamicsFrequencyCounter.Update(fpsDivD);
                    }
                }

                if (fpsDivD > 1 && countD % fpsDivD > 0)
                {
                    _DynamicsFrequencyCounter.Update(countD % fpsDivD);
                }

                _FrameHistory.Enqueue(frame);

                if (fpsDivK == 1 || k % fpsDivK == 0)
                {
                    _KinematicsFrequencyCounter.Update(fpsDivK);
                }
            }

            if (fpsDivK > 1 && countK % fpsDivK > 0)
            {
                _KinematicsFrequencyCounter.Update(countK % fpsDivK);
            }
        }

        // 将此 MultibodySystem 对象运动与轨迹分辨率相同的时长。
        public void NextMoment()
        {
            int countD = (int)Math.Round(_KinematicsResolution / _DynamicsResolution);
            int fpsDivD = Math.Max(1, countD / 10);

            Frame frame = LatestFrame.Copy();

            for (int d = 1; d <= countD; d++)
            {
                frame.NextMoment(_DynamicsResolution);

                if (fpsDivD == 1 || d % fpsDivD == 0)
                {
                    _DynamicsFrequencyCounter.Update(fpsDivD);
                }
            }

            if (fpsDivD > 1 && countD % fpsDivD > 0)
            {
                _DynamicsFrequencyCounter.Update(countD % fpsDivD);
            }

            _FrameHistory.Enqueue(frame);

            _KinematicsFrequencyCounter.Update();
        }

        // 将此 MultibodySystem 对象回到初始帧。
        public void Restart()
        {
            _FrameHistory.Clear();
            _FrameHistory.Enqueue(_InitialFrame.Copy());
            _DynamicsFrequencyCounter.Reset();
            _KinematicsFrequencyCounter.Reset();
        }

        // 重新设置此 MultibodySystem 对象的参数与所有粒子。
        public void Reset(double dynamicsResolution, double kinematicsResolution, double cacheSize, params Particle[] particles)
        {
            if (double.IsNaN(dynamicsResolution) || double.IsInfinity(dynamicsResolution) || dynamicsResolution <= 0)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(kinematicsResolution) || double.IsInfinity(kinematicsResolution) || kinematicsResolution < dynamicsResolution)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(cacheSize) || double.IsInfinity(cacheSize) || cacheSize < 0)
            {
                throw new ArgumentException();
            }

            if (particles == null || particles.Length <= 0)
            {
                throw new ArgumentNullException();
            }

            //

            _DynamicsResolution = dynamicsResolution;
            _KinematicsResolution = kinematicsResolution;
            _CacheSize = cacheSize;
            _InitialFrame = new Frame(0, particles);
            _FrameHistory = new FixedQueue<Frame>(cacheSize == 0 ? 1 : (int)Math.Ceiling(cacheSize / kinematicsResolution));
            _FrameHistory.Enqueue(_InitialFrame.Copy());
        }

        // 重新设置此 MultibodySystem 对象的参数与所有粒子。
        public void Reset(double dynamicsResolution, double kinematicsResolution, double cacheSize, List<Particle> particles)
        {
            if (double.IsNaN(dynamicsResolution) || double.IsInfinity(dynamicsResolution) || dynamicsResolution <= 0)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(kinematicsResolution) || double.IsInfinity(kinematicsResolution) || kinematicsResolution < dynamicsResolution)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(cacheSize) || double.IsInfinity(cacheSize) || cacheSize < 0)
            {
                throw new ArgumentException();
            }

            if (particles == null || particles.Count <= 0)
            {
                throw new ArgumentNullException();
            }

            //

            _DynamicsResolution = dynamicsResolution;
            _KinematicsResolution = kinematicsResolution;
            _CacheSize = cacheSize;
            _InitialFrame = new Frame(0, particles);
            _FrameHistory = new FixedQueue<Frame>(cacheSize == 0 ? 1 : (int)Math.Ceiling(cacheSize / kinematicsResolution));
            _FrameHistory.Enqueue(_InitialFrame.Copy());
        }
    }
}