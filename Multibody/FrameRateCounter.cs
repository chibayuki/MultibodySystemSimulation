/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2019 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.0.0.DEV.190914-0000

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
    // FPS 计数器，用于计算事件的实时帧率
    internal sealed class FpsCounter
    {
        // 表示携带计数的计时周期数
        private struct _TicksWithCount
        {
            public long Ticks;
            public int Count;

            public _TicksWithCount(long ticks, int count)
            {
                Ticks = ticks;
                Count = count;
            }

            public _TicksWithCount(long ticks) : this(ticks, 1)
            {
            }
        }

        // 通过链表实现的队列
        private sealed class _LinkedQueue<T>
        {
            private int _BlockSize;
            private int _StartIndex;
            private int _Count;
            private T[] _TArray;
            private _LinkedQueue<T> _Next;

            public _LinkedQueue(int blockSize)
            {
                if (blockSize <= 0)
                {
                    throw new ArgumentException();
                }

                //

                _BlockSize = blockSize;
                _StartIndex = 0;
                _Count = 0;
                _TArray = new T[_BlockSize];
                _Next = null;
            }

            public _LinkedQueue() : this(32)
            {
            }

            // 获取或设置此 _LinkedQueue 对象的指定索引的元素
            public T this[int index]
            {
                get
                {
                    if (index < 0)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    //

                    if (index >= _Count)
                    {
                        if (_Next == null)
                        {
                            throw new IndexOutOfRangeException();
                        }
                        else
                        {
                            return _Next[index - _Count];
                        }
                    }
                    else
                    {
                        return _TArray[_StartIndex + index];
                    }
                }

                set
                {
                    if (index < 0)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    //

                    if (index >= _Count)
                    {
                        if (_Count == 0 || _Next == null)
                        {
                            throw new IndexOutOfRangeException();
                        }
                        else
                        {
                            _Next[index - _Count] = value;
                        }
                    }
                    else
                    {
                        _TArray[_StartIndex + index] = value;
                    }
                }
            }

            // 获取或设置此 _LinkedQueue 对象的队首元素
            public T Head
            {
                get
                {
                    if (_Count <= 0)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    //

                    return _TArray[_StartIndex];
                }

                set
                {
                    if (_Count <= 0)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    //

                    _TArray[_StartIndex] = value;
                }
            }

            // 获取或设置此 _LinkedQueue 对象的队尾元素
            public T Tail
            {
                get
                {
                    if (_Count <= 0)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    //

                    if (_Next == null)
                    {
                        return _TArray[_StartIndex + _Count - 1];
                    }
                    else
                    {
                        return _Next.Tail;
                    }
                }

                set
                {
                    if (_Count <= 0)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    //

                    if (_Next == null)
                    {
                        _TArray[_StartIndex + _Count - 1] = value;
                    }
                    else
                    {
                        _Next.Tail = value;
                    }
                }
            }

            // 获取此 _LinkedQueue 对象的元素数目
            public int Count => (_Next == null ? _Count : _Count + _Next.Count);

            // 向此 _LinkedQueue 对象的队尾添加一个元素
            public void Enqueue(T item)
            {
                int index = _StartIndex + _Count;

                if (index < _BlockSize)
                {
                    _TArray[index] = item;
                    _Count++;
                }
                else
                {
                    if (_Next == null)
                    {
                        _Next = new _LinkedQueue<T>(_BlockSize);
                    }

                    _Next.Enqueue(item);
                }
            }

            // 从此 _LinkedQueue 对象的队首取出一个元素
            public T Dequeue()
            {
                T result = _TArray[_StartIndex];

                _StartIndex++;
                _Count--;

                if (_Count <= 0 && _Next != null)
                {
                    _StartIndex = _Next._StartIndex;
                    _Count = _Next._Count;
                    _TArray = _Next._TArray;
                    _Next = _Next._Next;
                }

                return result;
            }

            // 删除此 _LinkedQueue 对象的所有元素
            public void Clear()
            {
                _StartIndex = 0;
                _Count = 0;
                _TArray = new T[_BlockSize];
                _Next = null;
            }
        }

        private long _DeltaTicks;
        private _LinkedQueue<_TicksWithCount> _TicksHistory;

        public FpsCounter(int maxFps, double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0)
            {
                throw new ArgumentException();
            }

            //

            _DeltaTicks = Math.Max(1, (long)Math.Round(seconds * 1E7));
            _TicksHistory = new _LinkedQueue<_TicksWithCount>((int)Math.Ceiling(maxFps * seconds));
        }

        public FpsCounter(int maxFps) : this(maxFps, 1)
        {
        }

        public FpsCounter() : this(1024, 1)
        {
        }

        // 获取此 FPS 对象的帧率（帧/秒 - 或 - 赫兹）
        public double FrameRate
        {
            get
            {
                int count = 0;

                for (int i = 0; i < _TicksHistory.Count; i++)
                {
                    count += _TicksHistory[i].Count;
                }

                if (count > 1)
                {
                    return (count * 1E7 / (_TicksHistory.Tail.Ticks - _TicksHistory.Head.Ticks));
                }
                else
                {
                    return 0;
                }
            }
        }

        // 获取此 FPS 对象的帧长度（秒）
        public double FrameLength
        {
            get
            {
                int count = 0;

                for (int i = 0; i < _TicksHistory.Count; i++)
                {
                    count += _TicksHistory[i].Count;
                }

                if (count > 1)
                {
                    return ((_TicksHistory.Tail.Ticks - _TicksHistory.Head.Ticks) * 1E-7 / count);
                }
                else
                {
                    return 0;
                }
            }
        }

        // 更新此 FPS 对象
        public void Update(int count)
        {
            long ticks = DateTime.UtcNow.Ticks;

            if (_TicksHistory.Count > 0 && ticks < _TicksHistory.Tail.Ticks)
            {
                Reset();
            }

            _TicksHistory.Enqueue(new _TicksWithCount(ticks, count));

            while (_TicksHistory.Count > 2 && ticks - _TicksHistory.Head.Ticks > _DeltaTicks)
            {
                _TicksHistory.Dequeue();
            }
        }

        // 更新此 FPS 对象
        public void Update()
        {
            Update(1);
        }

        // 重置此 FPS 对象
        public void Reset()
        {
            _TicksHistory = new _LinkedQueue<_TicksWithCount>(40960);
        }
    }
}
