/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2024 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.117.1000.M2.201101-1440

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
    // 快照。
    internal sealed class Snapshot
    {
        private Frame[] _Frames;

        public Snapshot(IEnumerable<Frame> frames)
        {
            if (frames is null || !frames.Any())
            {
                throw new ArgumentNullException();
            }

            //

            _Frames = frames.ToArray();
        }

        // 获取此 MultibodySystem 对象的总帧数。
        public int FrameCount => _Frames.Length;

        // 获取此 Snapshot 对象的最旧一帧。
        public Frame OldestFrame => _Frames[0];

        // 获取此 Snapshot 对象的最新一帧。
        public Frame LatestFrame => _Frames[_Frames.Length - 1];

        // 获取此 Snapshot 对象的指定帧。
        public Frame GetFrame(int index) => _Frames[index];
    }
}