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

using System.Diagnostics;

using UIMessage = Com.WinForm.UIMessage;
using UIMessageProcessor = Com.WinForm.UIMessageProcessor;

namespace Multibody
{
    // 仿真器。
    internal sealed class Simulator : UIMessageProcessor
    {
        private SimulationData _SimulationData;

        #region 构造函数

        public Simulator(SimulationData simulationData) : base()
        {
            if (simulationData is null)
            {
                throw new ArgumentNullException();
            }

            //

            _SimulationData = simulationData;
        }

        #endregion

        #region 消息处理器

        protected override void ProcessMessage(UIMessage message) { }

        protected override void MessageLoop()
        {
            base.MessageLoop();

            //

            if (!_SimulationData.CacheIsFull)
            {
                // 此Simulator线程与Renderer竞争读写锁，需要尽可能缩短当前线程占用写入锁的时长以避免降低刷新率；
                // 另一方面，仿真计算的速度应该尽可能总是快于渲染速度，否则也会降低刷新率。
                // 例如，下列代码期待：仿真计算每次持续不超过5毫秒，并且将多体系统的运行时间推进对应于至多0.1秒现实时长。
                Stopwatch sw = Stopwatch.StartNew();
                int num = Math.Max(1, (int)Math.Ceiling(_SimulationData.TimeMag * 0.1 / _SimulationData.KinematicsResolution));
                for (int i = 0; i < num; i++)
                {
                    _SimulationData.NextMoment();
                    if (_SimulationData.CacheIsFull || sw.ElapsedMilliseconds >= 5)
                    {
                        break;
                    }
                }
                sw.Stop();
            }
        }

        #endregion
    }
}