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
        private Frame _FirstFrame;
        private List<Frame> _FrameHistory;

        public MultibodySystem(params Particle[] particles)
        {
            _FrameHistory = new List<Frame>();

            Reset(particles);
        }

        public MultibodySystem(List<Particle> particles)
        {
            _FrameHistory = new List<Frame>();

            Reset(particles);
        }

        // 获取此 MultibodySystem 对象的第一帧
        public Frame FirstFrame => _FirstFrame;

        // 获取此 MultibodySystem 对象的最后一帧
        public Frame LastFrame => _FrameHistory[_FrameHistory.Count - 1];

        // 获取此 MultibodySystem 对象的总帧数
        public int FrameCount => _FrameHistory.Count;

        // 获取此 MultibodySystem 对象的指定帧
        public Frame Frame(int index)
        {
            return _FrameHistory[index];
        }

        // 将此 MultibodySystem 对象运动指定的秒数
        public void NextFrame(double second)
        {
            Frame frame = LastFrame.Copy();

            frame.NextMoment(second);

            _FrameHistory.Add(frame);
        }

        // 将此 MultibodySystem 对象回到第一帧
        public void Restart()
        {
            _FrameHistory.Clear();
            _FrameHistory.Add(_FirstFrame.Copy());
        }

        // 重新设置此 MultibodySystem 对象的所有粒子
        public void Reset(params Particle[] particles)
        {
            _FirstFrame = new Frame(0, particles);

            Restart();
        }

        // 重新设置此 MultibodySystem 对象的所有粒子
        public void Reset(List<Particle> particles)
        {
            _FirstFrame = new Frame(0, particles);

            Restart();
        }
    }
}