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

        protected override void ProcessMessage(UIMessage message)
        {
        }

        protected override void MessageLoop()
        {
            base.MessageLoop();

            //

            if (!_SimulationData.CacheIsFull)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int num = Math.Max(1, (int)Math.Ceiling(_SimulationData.TimeMag * 0.03 / _SimulationData.KinematicsResolution));
                for (int i = 0; i < num; i++)
                {
                    _SimulationData.NextMoment();
                    if (sw.ElapsedMilliseconds >= 15 || _SimulationData.CacheIsFull)
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