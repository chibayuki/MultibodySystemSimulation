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
    // 频率计数器，用于实时计算在过去一小段时间间隔内某一事件发生的平均频率
    internal sealed class FrequencyCounter
    {
        // 表示携带计数的计时周期数
        private sealed class _TicksWithCount
        {
            private long _Ticks;
            private long _Count;

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

            // 获取或设置此 _TicksWithCount 对象的计时周期数
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

            // 获取或设置此 _TicksWithCount 对象的计数
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

        private long _TypicalMeasurementPeriodTicks;
        private FixedQueue<_TicksWithCount> _TicksHistory;

        public FrequencyCounter(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0)
            {
                throw new ArgumentException();
            }

            //

            _TypicalMeasurementPeriodTicks = Math.Max(1, (long)Math.Round(seconds * 1E7));
            _TicksHistory = new FixedQueue<_TicksWithCount>(32);
        }

        public FrequencyCounter() : this(1)
        {
        }

        // 获取此 FrequencyCounter 对象的频率（赫兹）
        public double Frequency
        {
            get
            {
                long count = 0;

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

        // 获取此 FrequencyCounter 对象的周期（秒）
        public double Period => 1 / Frequency;

        // 更新此 FrequencyCounter 对象指定次计数
        public void Update(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            //

            long ticks = DateTime.UtcNow.Ticks;

            if (_TicksHistory.Count > 0)
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
                    if (_TicksHistory.Count == _TicksHistory.Capacity && ticks - _TicksHistory.Head.Ticks <= _TypicalMeasurementPeriodTicks)
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

            while (_TicksHistory.Count > 2 && ticks - _TicksHistory.Head.Ticks > _TypicalMeasurementPeriodTicks)
            {
                _TicksHistory.Dequeue();
            }
        }

        // 更新此 FrequencyCounter 对象一次计数
        public void Update()
        {
            Update(1);
        }

        // 重置此 FrequencyCounter 对象
        public void Reset()
        {
            _TicksHistory.Clear();
        }
    }
}