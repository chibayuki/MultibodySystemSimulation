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
        private sealed class Queue<T>
        {
            private int _Capacity;
            private int _Count;
            private List<T> _List1;
            private List<T> _List2;

            public Queue(int capacity)
            {
                _Capacity = capacity;
                _Count = 0;
                _List1 = new List<T>(_Capacity);
                _List2 = new List<T>(_Capacity);
            }

            public Queue() : this(0)
            {
            }

            // 获取或设置此 Queue 对象的指定索引的元素
            public T this[int index]
            {
                get
                {
                    if (index < 0 || index >= _Count)
                    {
                        throw new ArgumentException();
                    }

                    //

                    if (_List1.Count < _Capacity)
                    {
                        return _List1[index];
                    }
                    else
                    {
                        int _index = _List1.Count + _List2.Count - _Capacity + index;

                        if (_index < _Capacity)
                        {
                            return _List1[_index];
                        }
                        else
                        {
                            return _List2[_index - _Capacity];
                        }
                    }
                }

                set
                {
                    if (index < 0 || index >= _Count)
                    {
                        throw new ArgumentException();
                    }

                    //

                    if (_List1.Count < _Capacity)
                    {
                        _List1[index] = value;
                    }
                    else
                    {
                        int _index = _List1.Count + _List2.Count - _Capacity + index;

                        if (_index < _Capacity)
                        {
                            _List1[_index] = value;
                        }
                        else
                        {
                            _List2[_index - _Capacity] = value;
                        }
                    }
                }
            }

            // 获取此 Queue 对象的容量
            public int Capacity => _Capacity;

            // 获取此 Queue 对象的元素数目
            public int Count => _Count;

            // 向此 Queue 对象的队尾添加一个元素
            public void Add(T item)
            {
                if (_List1.Count < _Capacity)
                {
                    _List1.Add(item);
                    _Count++;
                }
                else
                {
                    if (_List2.Count >= _Capacity)
                    {
                        List<T> temp = _List1;
                        _List1 = _List2;
                        _List2 = temp;
                        _List2.Clear();
                    }

                    _List2.Add(item);
                }
            }

            // 删除此 Queue 对象的所有元素
            public void Clear()
            {
                _Count = 0;
                _List1.Clear();
                _List2.Clear();
            }
        }

        private Frame _InitialFrame;
        private Queue<Frame> _FrameHistory;

        public MultibodySystem(int capacity, params Particle[] particles)
        {
            Reset(capacity, particles);
        }

        public MultibodySystem(int capacity, List<Particle> particles)
        {
            Reset(capacity, particles);
        }

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
            if (double.IsNaN(second) || double.IsInfinity(second) || second <= 0)
            {
                throw new ArgumentException();
            }

            //

            Frame frame = LatestFrame.Copy();

            frame.NextMoment(second);

            _FrameHistory.Add(frame);
        }

        // 将此 MultibodySystem 对象回到第一帧
        public void Restart()
        {
            _FrameHistory.Clear();
            _FrameHistory.Add(_InitialFrame.Copy());
        }

        // 重新设置此 MultibodySystem 对象的所有粒子
        public void Reset(int capacity, params Particle[] particles)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException();
            }

            if (particles == null || particles.Length <= 0)
            {
                throw new ArgumentNullException();
            }

            //

            _InitialFrame = new Frame(0, particles);

            _FrameHistory = new Queue<Frame>(capacity);
            _FrameHistory.Add(_InitialFrame.Copy());
        }

        // 重新设置此 MultibodySystem 对象的所有粒子
        public void Reset(int capacity, List<Particle> particles)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException();
            }

            if (particles == null || particles.Count <= 0)
            {
                throw new ArgumentNullException();
            }

            //

            _InitialFrame = new Frame(0, particles);

            _FrameHistory = new Queue<Frame>(capacity);
            _FrameHistory.Add(_InitialFrame.Copy());
        }
    }
}