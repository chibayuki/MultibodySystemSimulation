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
        private sealed class FixedQueue<T>
        {
            private int _Capacity;
            private int _StartIndex;
            private int _Count;
            private T[] _TArray;

            public FixedQueue(int capacity)
            {
                _Capacity = capacity;
                _StartIndex = 0;
                _Count = 0;
                _TArray = new T[_Capacity];
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
                        throw new ArgumentException();
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

            // 获取此 Queue 对象的容量
            public int Capacity => _Capacity;

            // 获取此 Queue 对象的元素数目
            public int Count => _Count;

            // 向此 Queue 对象的队尾添加一个元素
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

            // 从此 Queue 对象的队首取出一个元素
            public T Pop()
            {
                T result = this[0];

                _StartIndex++;
                _Count--;

                return result;
            }

            // 删除此 Queue 对象的所有元素
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

        private Frame _InitialFrame;
        private FixedQueue<Frame> _FrameHistory;

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

            _FrameHistory.Push(frame);
        }

        // 将此 MultibodySystem 对象回到第一帧
        public void Restart()
        {
            _FrameHistory.Clear();
            _FrameHistory.Push(_InitialFrame.Copy());
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

            _FrameHistory = new FixedQueue<Frame>(capacity);
            _FrameHistory.Push(_InitialFrame.Copy());
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

            _FrameHistory = new FixedQueue<Frame>(capacity);
            _FrameHistory.Push(_InitialFrame.Copy());
        }
    }
}