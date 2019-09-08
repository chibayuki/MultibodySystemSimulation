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
    internal class MultibodySystem
    {
        private int _Capacity;
        private int _Count;
        private Frame _FirstFrame;
        private List<Frame> _FrameHistory_Part1;
        private List<Frame> _FrameHistory_Part2;

        public MultibodySystem(int capacity, params Particle[] particles)
        {
            Reset(capacity, particles);
        }

        public MultibodySystem(int capacity, List<Particle> particles)
        {
            Reset(capacity, particles);
        }

        // 获取此 MultibodySystem 对象的第一帧
        public Frame FirstFrame => _FirstFrame;

        // 获取此 MultibodySystem 对象的最后一帧
        public Frame LastFrame => Frame(_Count - 1);

        // 获取此 MultibodySystem 对象的帧容量
        public int FrameCapacity => _Capacity;

        // 获取此 MultibodySystem 对象的总帧数
        public int FrameCount => _Count;

        // 获取此 MultibodySystem 对象的指定帧
        public Frame Frame(int index)
        {
            if (index < 0 || index >= _Count)
            {
                throw new ArgumentException();
            }

            //

            if (_FrameHistory_Part1.Count < _Capacity)
            {
                return _FrameHistory_Part1[index];
            }
            else
            {
                int _index = _FrameHistory_Part1.Count + _FrameHistory_Part2.Count - _Capacity + index;

                if (_index < _Capacity)
                {
                    return _FrameHistory_Part1[_index];
                }
                else
                {
                    return _FrameHistory_Part2[_index - _Capacity];
                }
            }
        }

        // 将此 MultibodySystem 对象运动指定的秒数
        public void NextMoment(double second)
        {
            if (double.IsNaN(second) || double.IsInfinity(second) || second <= 0)
            {
                throw new ArgumentException();
            }

            //

            Frame frame = LastFrame.Copy();

            frame.NextMoment(second);

            if (_FrameHistory_Part1.Count < _Capacity)
            {
                _FrameHistory_Part1.Add(frame);
                _Count++;
            }
            else if (_FrameHistory_Part2.Count < _Capacity)
            {
                _FrameHistory_Part2.Add(frame);
            }
            else
            {
                _FrameHistory_Part1 = _FrameHistory_Part2;
                _FrameHistory_Part2 = new List<Frame>(_Capacity);
                _FrameHistory_Part2.Add(frame);
            }
        }

        // 将此 MultibodySystem 对象回到第一帧
        public void Restart()
        {
            _FrameHistory_Part1.Clear();
            _FrameHistory_Part2.Clear();
            _FrameHistory_Part1.Add(_FirstFrame.Copy());
            _Count = 1;
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

            _Capacity = capacity;
            _FirstFrame = new Frame(0, particles);
            _FrameHistory_Part1 = new List<Frame>(_Capacity);
            _FrameHistory_Part2 = new List<Frame>(_Capacity);
            _FrameHistory_Part1.Add(_FirstFrame.Copy());
            _Count = 1;
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

            _Capacity = capacity;
            _FirstFrame = new Frame(0, particles);
            _FrameHistory_Part1 = new List<Frame>(_Capacity);
            _FrameHistory_Part2 = new List<Frame>(_Capacity);
            _FrameHistory_Part1.Add(_FirstFrame.Copy());
            _Count = 1;
        }
    }
}