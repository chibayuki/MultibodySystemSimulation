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
        // 通过自动弹出队首元素实现固定容量的队列
        private sealed class _FixedQueue<T>
        {
            private int _Capacity;
            private int _StartIndex;
            private int _Count;
            private T[] _TArray;

            public _FixedQueue(int capacity)
            {
                if (capacity < 0)
                {
                    throw new ArgumentException();
                }

                //

                _Capacity = capacity;
                _StartIndex = 0;
                _Count = 0;
                _TArray = new T[_Capacity];
            }

            // 获取或设置此 _FixedQueue 对象的指定索引的元素
            public T this[int index]
            {
                get
                {
                    if (index < 0 || index >= _Count)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    //

                    int _index = _StartIndex + index;

                    if (_index >= _Capacity)
                    {
                        _index -= _Capacity;
                    }

                    return _TArray[_index];
                }

                set
                {
                    if (index < 0 || index >= _Count)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    //

                    int _index = _StartIndex + index;

                    if (_index >= _Capacity)
                    {
                        _index -= _Capacity;
                    }

                    _TArray[_index] = value;
                }
            }

            // 获取此 _FixedQueue 对象的容量
            public int Capacity => _Capacity;

            // 获取此 _FixedQueue 对象的元素数目
            public int Count => _Count;

            // 向此 _FixedQueue 对象的队尾添加一个元素
            public void Push(T item)
            {
                if (_Count < _Capacity)
                {
                    _Count++;
                }
                else if (_StartIndex < _Capacity)
                {
                    _StartIndex++;
                }
                else
                {
                    _StartIndex = 0;
                }

                this[_Count - 1] = item;
            }

            // 从此 _FixedQueue 对象的队首取出一个元素
            public T Pop()
            {
                T result = this[0];

                _StartIndex++;
                _Count--;

                return result;
            }

            // 删除此 _FixedQueue 对象的所有元素
            public void Clear()
            {
                _StartIndex = 0;
                _Count = 0;

                for (int i = 0; i < _Capacity; i++)
                {
                    _TArray[i] = default(T);
                }
            }
        }

        private double _DynamicResolution;
        private double _LocusResolution;
        private double _LocusLength;
        private Frame _InitialFrame;
        private _FixedQueue<Frame> _FrameHistory;

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
        public Frame LatestFrame => _FrameHistory[_FrameHistory.Count - 1];

        // 获取此 MultibodySystem 对象的帧容量
        public int FrameCapacity => _FrameHistory.Capacity;

        // 获取此 MultibodySystem 对象的总帧数
        public int FrameCount => _FrameHistory.Count;

        // 获取此 MultibodySystem 对象的指定帧
        public Frame Frame(int index)
        {
            return _FrameHistory[index];
        }

        // 将此 MultibodySystem 对象运动指定的时长（秒）
        public void NextMoment(double second)
        {
            if (double.IsNaN(second) || double.IsInfinity(second) || second < _LocusResolution)
            {
                throw new ArgumentException();
            }

            //

            int countL = (int)Math.Round(second / _LocusResolution);
            int countD = (int)Math.Round(_LocusResolution / _DynamicResolution);

            for (int i = 0; i < countL; i++)
            {
                Frame frame = LatestFrame.Copy();

                for (int j = 0; j < countD; j++)
                {
                    frame.NextMoment(_DynamicResolution);
                }

                _FrameHistory.Push(frame);
            }
        }

        // 将此 MultibodySystem 对象运动与轨迹分辨率相同的时长
        public void NextMoment()
        {
            int countD = (int)Math.Round(_LocusResolution / _DynamicResolution);

            Frame frame = LatestFrame.Copy();

            for (int i = 0; i < countD; i++)
            {
                frame.NextMoment(_DynamicResolution);
            }

            _FrameHistory.Push(frame);
        }

        // 将此 MultibodySystem 对象回到第一帧
        public void Restart()
        {
            _FrameHistory.Clear();
            _FrameHistory.Push(_InitialFrame.Copy());
        }

        // 重新设置此 MultibodySystem 对象的所有粒子
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
            _FrameHistory = new _FixedQueue<Frame>(locusLength == 0 ? 1 : (int)Math.Ceiling(locusLength / locusResolution));
            _FrameHistory.Push(_InitialFrame.Copy());
        }

        // 重新设置此 MultibodySystem 对象的所有粒子
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
            _FrameHistory = new _FixedQueue<Frame>(locusLength == 0 ? 1 : (int)Math.Ceiling(locusLength / locusResolution));
            _FrameHistory.Push(_InitialFrame.Copy());
        }
    }
}