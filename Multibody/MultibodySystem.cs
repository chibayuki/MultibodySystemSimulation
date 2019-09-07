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

        public void NextFrame(double second)
        {
            Frame frame = _FrameHistory[_FrameHistory.Count - 1].Copy();

            frame.NextMoment(second);

            _FrameHistory.Add(frame);
        }

        public void Restart()
        {
            _FrameHistory.Clear();
            _FrameHistory.Add(_FirstFrame.Copy());
        }

        public void Reset(params Particle[] particles)
        {
            _FirstFrame = new Frame(0, particles);

            Restart();
        }

        public void Reset(List<Particle> particles)
        {
            _FirstFrame = new Frame(0, particles);

            Restart();
        }
    }
}