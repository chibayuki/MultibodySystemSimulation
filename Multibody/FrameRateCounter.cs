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
    // 帧率计数器，用于实时计算事件在过去一小段时间间隔内发生的平均频率
    internal sealed class FrameRateCounter
    {
        // 表示携带计数的计时周期数
        private sealed class _TicksWithCount
        {
            public long Ticks;
            public int Count;

            public _TicksWithCount(long ticks, int count)
            {
                if (ticks <= 0 || count <= 0)
                {
                    throw new ArgumentException();
                }

                //

                Ticks = ticks;
                Count = count;
            }

            public _TicksWithCount(long ticks) : this(ticks, 1)
            {
            }
        }

        private long _DeltaTicks;
        private FixedQueue<_TicksWithCount> _TicksHistory;

        public FrameRateCounter(int maxFps, double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0)
            {
                throw new ArgumentException();
            }

            //

            _DeltaTicks = Math.Max(1, (long)Math.Round(seconds * 1E7));
            _TicksHistory = new FixedQueue<_TicksWithCount>(1 + (int)Math.Ceiling(maxFps * seconds));
        }

        public FrameRateCounter(int maxFps) : this(maxFps, 1)
        {
        }

        public FrameRateCounter() : this(1024, 1)
        {
        }

        // 获取此 FrameRateCounter 对象的帧率（帧/秒 - 或 - 赫兹）
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

        // 更新此 FrameRateCounter 对象
        public void Update(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentException();
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
                    _TicksHistory.Tail.Count += count;
                }
                else
                {
                    _TicksHistory.Enqueue(new _TicksWithCount(ticks, count));
                }
            }
            else
            {
                _TicksHistory.Enqueue(new _TicksWithCount(ticks, count));
            }

            while (_TicksHistory.Count > 2 && ticks - _TicksHistory.Head.Ticks > _DeltaTicks)
            {
                _TicksHistory.Dequeue();
            }
        }

        // 更新此 FrameRateCounter 对象
        public void Update()
        {
            Update(1);
        }

        // 重置此 FrameRateCounter 对象
        public void Reset()
        {
            _TicksHistory.Clear();
        }
    }
}