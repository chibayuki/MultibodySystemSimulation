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
    // 频率计数器，用于实时计算在过去一小段时间间隔内某一事件发生的平均频率。
    internal sealed class FrequencyCounter
    {
        // 表示携带计数的计时周期数。
        private sealed class _TicksWithCount
        {
            private long _Ticks; // 计时周期数。
            private long _Count; // 计数。

            public _TicksWithCount(long ticks, int count)
            {
                if ((ticks < DateTime.MinValue.Ticks|| ticks > DateTime.MaxValue.Ticks) || count <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                //

                Ticks = ticks;
                Count = count;
            }

            public _TicksWithCount(long ticks) : this(ticks, 1)
            {
            }

            // 获取或设置此 _TicksWithCount 对象的计时周期数。
            public long Ticks
            {
                get
                {
                    return _Ticks;
                }

                set
                {
                    _Ticks = value;
                }
            }

            // 获取或设置此 _TicksWithCount 对象的计数。
            public long Count
            {
                get
                {
                    return _Count;
                }

                set
                {
                    _Count = value;
                }
            }
        }

        private const double _TicksPerSecond = 1E7; // 每秒的计时周期数。

        private long _DeltaTTicks; // ΔT 的计时周期数。
        private FixedQueue<_TicksWithCount> _TicksHistory; // 历史计时计数。

        public FrequencyCounter(double deltaTSeconds)
        {
            if (double.IsNaN(deltaTSeconds) || double.IsInfinity(deltaTSeconds) || deltaTSeconds <= 0)
            {
                throw new ArgumentException();
            }

            //

            _DeltaTTicks = Math.Max(1, (long)Math.Round(deltaTSeconds * _TicksPerSecond));
            _TicksHistory = new FixedQueue<_TicksWithCount>(32);
        }

        public FrequencyCounter() : this(1)
        {
        }

        // 获取此 FrequencyCounter 对象的频率（赫兹）。
        public double Frequency
        {
            get
            {
                if (_TicksHistory.Count >= 2)
                {
                    long ticks = DateTime.UtcNow.Ticks;

                    if (_TicksHistory.Count == 2)
                    {
                        return (_TicksHistory.Tail.Count * _TicksPerSecond / (ticks - _TicksHistory.Head.Ticks));
                    }
                    else
                    {
                        long count = 0;

                        _TicksWithCount head = _TicksHistory.Head;

                        for (int i = _TicksHistory.Count - 1; i >= 1; i--)
                        {
                            head = _TicksHistory[i - 1];

                            if (ticks - head.Ticks <= _DeltaTTicks)
                            {
                                count += _TicksHistory[i].Count;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (count <= 0)
                        {
                            count = _TicksHistory.Tail.Count;
                            head = _TicksHistory[_TicksHistory.Count - 2];
                        }

                        return (count * _TicksPerSecond / (ticks - head.Ticks));
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        // 获取此 FrequencyCounter 对象的周期（秒）。
        public double Period => 1 / Frequency;

        // 更新此 FrequencyCounter 对象指定次计数。
        public void Update(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            //

            long ticks = DateTime.UtcNow.Ticks;

            if (!_TicksHistory.IsEmpty)
            {
                _TicksWithCount tail = _TicksHistory.Tail;

                if (ticks < tail.Ticks)
                {
                    Reset();
                }
                else if (ticks == tail.Ticks)
                {
                    tail.Count += count;
                }
                else
                {
                    if (_TicksHistory.Count == _TicksHistory.Capacity && ticks - _TicksHistory.Head.Ticks <= _DeltaTTicks)
                    {
                        _TicksHistory.Resize(_TicksHistory.Capacity * 2);
                    }

                    _TicksHistory.Enqueue(new _TicksWithCount(ticks, count));
                }
            }
            else
            {
                _TicksHistory.Enqueue(new _TicksWithCount(ticks, count));
            }

            while (_TicksHistory.Count > 2 && ticks - _TicksHistory.Head.Ticks > _DeltaTTicks)
            {
                _TicksHistory.Dequeue();
            }
        }

        // 更新此 FrequencyCounter 对象一次计数。
        public void Update()
        {
            Update(1);
        }

        // 重置此 FrequencyCounter 对象。
        public void Reset()
        {
            _TicksHistory.Clear();
        }
    }
}