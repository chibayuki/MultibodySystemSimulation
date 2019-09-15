/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2019 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.0.0.DEV.190906-0000

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
    // 多体系统
    internal sealed class MultibodySystem
    {
        private double _DynamicResolution;
        private double _LocusResolution;
        private double _LocusLength;
        private Frame _InitialFrame;
        private FixedQueue<Frame> _FrameHistory;
        private FrameRateCounter _DynamicFrameRateCounter = new FrameRateCounter();
        private FrameRateCounter _LocusFrameRateCounter = new FrameRateCounter();

        public MultibodySystem(double dynamicResolution, double locusResolution, double locusLength, params Particle[] particles)
        {
            Reset(dynamicResolution, locusResolution, locusLength, particles);
        }

        public MultibodySystem(double dynamicResolution, double locusResolution, double locusLength, List<Particle> particles)
        {
            Reset(dynamicResolution, locusResolution, locusLength, particles);
        }

        // 获取此 MultibodySystem 对象的动力学分辨率（秒）
        private double DynamicResolution => _DynamicResolution;

        // 获取此 MultibodySystem 对象的轨迹分辨率（秒）
        private double LocusResolution => _LocusResolution;

        // 获取此 MultibodySystem 对象的轨迹长度（秒）
        private double LocusLength => _LocusLength;

        // 获取此 MultibodySystem 对象的初始帧
        public Frame InitialFrame => _InitialFrame;

        // 获取此 MultibodySystem 对象的最新一帧
        public Frame LatestFrame => _FrameHistory.Tail;

        // 获取此 MultibodySystem 对象的帧容量
        public int FrameCapacity => _FrameHistory.Capacity;

        // 获取此 MultibodySystem 对象的总帧数
        public int FrameCount => _FrameHistory.Count;

        // 获取此 MultibodySystem 对象的动力学帧率
        public FrameRateCounter DynamicFPS => _DynamicFrameRateCounter;

        // 获取此 MultibodySystem 对象的轨迹帧率
        public FrameRateCounter LocusFPS => _LocusFrameRateCounter;

        // 获取此 MultibodySystem 对象的指定帧
        public Frame Frame(int index)
        {
            return _FrameHistory[index];
        }

        // 将此 MultibodySystem 对象运动指定的时长（秒）
        public void NextMoment(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < _LocusResolution)
            {
                throw new ArgumentException();
            }

            //

            int countL = (int)Math.Round(seconds / _LocusResolution);
            int countD = (int)Math.Round(_LocusResolution / _DynamicResolution);
            int fpsDivL = Math.Max(1, countL / 10);
            int fpsDivD = Math.Max(1, countD / 10);

            for (int i = 1; i <= countL; i++)
            {
                Frame frame = LatestFrame.Copy();

                for (int j = 1; j <= countD; j++)
                {
                    frame.NextMoment(_DynamicResolution);

                    if (fpsDivD == 1 || j % fpsDivD == 0)
                    {
                        _DynamicFrameRateCounter.Update(fpsDivD);
                    }
                }

                if (fpsDivD > 1 && countD % fpsDivD > 0)
                {
                    _DynamicFrameRateCounter.Update(countD % fpsDivD);
                }

                _FrameHistory.Enqueue(frame);

                if (fpsDivL == 1 || i % fpsDivL == 0)
                {
                    _LocusFrameRateCounter.Update(fpsDivL);
                }
            }

            if (fpsDivL > 1 && countL % fpsDivL > 0)
            {
                _LocusFrameRateCounter.Update(countL % fpsDivL);
            }
        }

        // 将此 MultibodySystem 对象运动与轨迹分辨率相同的时长
        public void NextMoment()
        {
            int countD = (int)Math.Round(_LocusResolution / _DynamicResolution);
            int fpsDivD = Math.Max(1, countD / 10);

            Frame frame = LatestFrame.Copy();

            for (int i = 1; i <= countD; i++)
            {
                frame.NextMoment(_DynamicResolution);

                if (fpsDivD == 1 || i % fpsDivD == 0)
                {
                    _DynamicFrameRateCounter.Update(fpsDivD);
                }
            }

            if (fpsDivD > 1 && countD % fpsDivD > 0)
            {
                _DynamicFrameRateCounter.Update(countD % fpsDivD);
            }

            _FrameHistory.Enqueue(frame);

            _LocusFrameRateCounter.Update();
        }

        // 将此 MultibodySystem 对象回到初始帧
        public void Restart()
        {
            _FrameHistory.Clear();
            _FrameHistory.Enqueue(_InitialFrame.Copy());
            _DynamicFrameRateCounter.Reset();
            _LocusFrameRateCounter.Reset();
        }

        // 重新设置此 MultibodySystem 对象的参数与所有粒子
        public void Reset(double dynamicResolution, double locusResolution, double locusLength, params Particle[] particles)
        {
            if (double.IsNaN(dynamicResolution) || double.IsInfinity(dynamicResolution) || dynamicResolution <= 0)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(locusResolution) || double.IsInfinity(locusResolution) || locusResolution < dynamicResolution)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(locusLength) || double.IsInfinity(locusLength) || locusLength < 0)
            {
                throw new ArgumentException();
            }

            if (particles == null || particles.Length <= 0)
            {
                throw new ArgumentNullException();
            }

            //

            _DynamicResolution = dynamicResolution;
            _LocusResolution = locusResolution;
            _LocusLength = locusLength;
            _InitialFrame = new Frame(0, particles);
            _FrameHistory = new FixedQueue<Frame>(locusLength == 0 ? 1 : (int)Math.Ceiling(locusLength / locusResolution));
            _FrameHistory.Enqueue(_InitialFrame.Copy());
        }

        // 重新设置此 MultibodySystem 对象的参数与所有粒子
        public void Reset(double dynamicResolution, double locusResolution, double locusLength, List<Particle> particles)
        {
            if (double.IsNaN(dynamicResolution) || double.IsInfinity(dynamicResolution) || dynamicResolution <= 0)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(locusResolution) || double.IsInfinity(locusResolution) || locusResolution < dynamicResolution)
            {
                throw new ArgumentException();
            }

            if (double.IsNaN(locusLength) || double.IsInfinity(locusLength) || locusLength < 0)
            {
                throw new ArgumentException();
            }

            if (particles == null || particles.Count <= 0)
            {
                throw new ArgumentNullException();
            }

            //

            _DynamicResolution = dynamicResolution;
            _LocusResolution = locusResolution;
            _LocusLength = locusLength;
            _InitialFrame = new Frame(0, particles);
            _FrameHistory = new FixedQueue<Frame>(locusLength == 0 ? 1 : (int)Math.Ceiling(locusLength / locusResolution));
            _FrameHistory.Enqueue(_InitialFrame.Copy());
        }
    }
}