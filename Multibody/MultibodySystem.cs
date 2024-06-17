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

using FrequencyCounter = Com.FrequencyCounter;
using FrameQueue = Com.IndexableQueue<Multibody.Frame>;

namespace Multibody
{
    // 多体系统。
    internal sealed class MultibodySystem
    {
        private double _DynamicsResolution; // 动力学分辨率（秒）。
        private double _KinematicsResolution; // 运动学分辨率（秒）。
        private double _CacheSize; // 缓存大小（秒）。

        private Frame _InitialFrame; // 初始帧。
        private FrameQueue _FrameHistory; // 历史帧队列。

        private FrequencyCounter _DynamicsFrequencyCounter = new FrequencyCounter(); // 动力学频率计数器。
        private FrequencyCounter _KinematicsFrequencyCounter = new FrequencyCounter(); // 运动学频率计数器。

        public MultibodySystem(double dynamicsResolution, double kinematicsResolution, double cacheSize, params Particle[] particles)
        {
            Reset(dynamicsResolution, kinematicsResolution, cacheSize, particles);
        }

        public MultibodySystem(double dynamicsResolution, double kinematicsResolution, double cacheSize, IEnumerable<Particle> particles)
        {
            Reset(dynamicsResolution, kinematicsResolution, cacheSize, particles);
        }

        // 获取此 MultibodySystem 对象的动力学分辨率（秒）。
        public double DynamicsResolution => _DynamicsResolution;

        // 获取此 MultibodySystem 对象的运动学分辨率（秒）。
        public double KinematicsResolution => _KinematicsResolution;

        // 获取此 MultibodySystem 对象的缓存大小（秒）。
        public double CacheSize => _CacheSize;

        // 获取此 MultibodySystem 对象的缓存是否已满。
        public bool CacheIsFull => _FrameHistory.IsFull;

        // 获取此 MultibodySystem 对象的帧容量。
        public int FrameCapacity => _FrameHistory.Capacity;

        // 获取此 MultibodySystem 对象的总帧数。
        public int FrameCount => _FrameHistory.Count;

        // 获取此 MultibodySystem 对象的初始帧。
        public Frame InitialFrame => _InitialFrame;

        // 获取此 MultibodySystem 对象的最旧一帧。
        public Frame OldestFrame => _FrameHistory.Tail;

        // 获取此 MultibodySystem 对象的最新一帧。
        public Frame LatestFrame => _FrameHistory.Tail;

        // 获取此 MultibodySystem 对象的动力学频率计数器。
        public FrequencyCounter DynamicsFrequencyCounter => _DynamicsFrequencyCounter;

        // 获取此 MultibodySystem 对象的运动学频率计数器。
        public FrequencyCounter KinematicsFrequencyCounter => _KinematicsFrequencyCounter;

        // 获取此 MultibodySystem 对象的指定帧。
        public Frame GetFrame(int index)
        {
            return _FrameHistory[index];
        }

        // 将此 MultibodySystem 对象运动指定的时长（秒）。
        public void NextMoment(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < _KinematicsResolution)
            {
                throw new ArgumentOutOfRangeException();
            }

            //

            int countK = (int)Math.Round(seconds / _KinematicsResolution);
            int countD = (int)Math.Round(_KinematicsResolution / _DynamicsResolution);
            int countKdiv10 = Math.Max(1, countK / 10);
            int countDdiv10 = Math.Max(1, countD / 10);

            for (int k = 1; k <= countK; k++)
            {
                Frame frame = _FrameHistory.Tail.Copy();
                frame.KinematicsId++;
                frame.DynamicsId += countD;

                for (int d = 1; d <= countD; d++)
                {
                    frame.NextMoment(_DynamicsResolution);

                    if (countDdiv10 == 1 || d % countDdiv10 == 0)
                    {
                        _DynamicsFrequencyCounter.Update(countDdiv10);
                    }
                }

                if (countDdiv10 > 1 && countD % countDdiv10 > 0)
                {
                    _DynamicsFrequencyCounter.Update(countD % countDdiv10);
                }

                frame.Freeze();
                _FrameHistory.Enqueue(frame);

                if (countKdiv10 == 1 || k % countKdiv10 == 0)
                {
                    _KinematicsFrequencyCounter.Update(countKdiv10);
                }
            }

            if (countKdiv10 > 1 && countK % countKdiv10 > 0)
            {
                _KinematicsFrequencyCounter.Update(countK % countKdiv10);
            }
        }

        // 将此 MultibodySystem 对象运动与轨迹分辨率相同的时长。
        public void NextMoment()
        {
            int countD = (int)Math.Round(_KinematicsResolution / _DynamicsResolution);
            int countDdiv10 = Math.Max(1, countD / 10);

            Frame frame = _FrameHistory.Tail.Copy();
            frame.KinematicsId++;
            frame.DynamicsId += countD;

            for (int d = 1; d <= countD; d++)
            {
                frame.NextMoment(_DynamicsResolution);

                if (countDdiv10 == 1 || d % countDdiv10 == 0)
                {
                    _DynamicsFrequencyCounter.Update(countDdiv10);
                }
            }

            if (countDdiv10 > 1 && countD % countDdiv10 > 0)
            {
                _DynamicsFrequencyCounter.Update(countD % countDdiv10);
            }

            frame.Freeze();
            _FrameHistory.Enqueue(frame);

            _KinematicsFrequencyCounter.Update();
        }

        // 将此 MultibodySystem 对象回到初始帧。
        public void Restart()
        {
            _FrameHistory.Clear();
            _FrameHistory.Enqueue(_InitialFrame);

            _DynamicsFrequencyCounter.Reset();
            _KinematicsFrequencyCounter.Reset();
        }

        // 重新设置此 MultibodySystem 对象的参数与所有粒子。
        public void Reset(double dynamicsResolution, double kinematicsResolution, double cacheSize, params Particle[] particles)
        {
            if (double.IsNaN(dynamicsResolution) || double.IsInfinity(dynamicsResolution) || dynamicsResolution <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(kinematicsResolution) || double.IsInfinity(kinematicsResolution) || kinematicsResolution < dynamicsResolution)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (double.IsNaN(cacheSize) || double.IsInfinity(cacheSize) || cacheSize < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (particles is null)
            {
                throw new ArgumentNullException();
            }

            //

            _DynamicsResolution = dynamicsResolution;
            _KinematicsResolution = kinematicsResolution;
            _CacheSize = cacheSize;

            _InitialFrame = new Frame(0, particles);
            _InitialFrame.Freeze();

            _FrameHistory = new FrameQueue(cacheSize == 0 ? 1 : (int)Math.Ceiling(cacheSize / kinematicsResolution), false);
            _FrameHistory.Enqueue(_InitialFrame);

            _DynamicsFrequencyCounter.Reset();
            _KinematicsFrequencyCounter.Reset();
        }

        // 重新设置此 MultibodySystem 对象的参数与所有粒子。
        public void Reset(double dynamicsResolution, double kinematicsResolution, double cacheSize, IEnumerable<Particle> particles)
        {
            if (particles is null)
            {
                throw new ArgumentNullException();
            }

            //

            Reset(dynamicsResolution, kinematicsResolution, cacheSize, particles.ToArray());
        }

        // 获取此 MultibodySystem 对象在指定时间区间内的快照。
        public Snapshot GetSnapshot(double startTime, double endTime)
        {
            int L = 0, R = _FrameHistory.Count - 1;

            if (startTime > endTime || startTime > _FrameHistory[R].Time || endTime < _FrameHistory[L].Time)
            {
                return null;
            }

            if (startTime == _FrameHistory[R].Time)
            {
                return new Snapshot(new Frame[] { _FrameHistory[R] });
            }

            if (endTime == _FrameHistory[L].Time)
            {
                return new Snapshot(new Frame[] { _FrameHistory[L] });
            }

            if (endTime < _FrameHistory[R].Time)
            {
                int endL = L, endR = R;

                while (endL < endR)
                {
                    if (endR - endL >= 2)
                    {
                        int m = (endL + endR) / 2;

                        if (endTime == _FrameHistory[m].Time)
                        {
                            R = m;
                            break;
                        }
                        else if (endTime > _FrameHistory[m].Time)
                        {
                            endL = m;
                        }
                        else
                        {
                            endR = m;
                        }
                    }
                    else
                    {
                        if (endTime - _FrameHistory[endL].Time < _FrameHistory[endR].Time - endTime)
                        {
                            R = endL;
                        }
                        else
                        {
                            R = endR;
                        }
                        break;
                    }
                }
            }

            if (startTime > _FrameHistory[L].Time)
            {
                int startL = L, startR = R;

                while (startL < startR)
                {
                    if (startR - startL >= 2)
                    {
                        int m = (startL + startR) / 2;

                        if (startTime == _FrameHistory[m].Time)
                        {
                            L = m;

                            break;
                        }
                        else if (startTime > _FrameHistory[m].Time)
                        {
                            startL = m;
                        }
                        else
                        {
                            startR = m;
                        }
                    }
                    else
                    {
                        if (startTime - _FrameHistory[startL].Time < _FrameHistory[startR].Time - startTime)
                        {
                            L = startL;
                        }
                        else
                        {
                            L = startR;
                        }
                        break;
                    }
                }
            }

            if (R > L)
            {
                List<Frame> frames = new List<Frame>();
                for (int i = L; i < R; i++)
                {
                    frames.Add(_FrameHistory[i]);
                }
                return new Snapshot(frames);
            }
            else
            {
                return new Snapshot(new Frame[] { _FrameHistory[R] });
            }
        }

        // 丢弃此 MultibodySystem 对象在指定时刻之前的缓存。
        public void DiscardCache(double time)
        {
            while (_FrameHistory.Head.Time < time)
            {
                _FrameHistory.Dequeue();
            }
        }
    }
}