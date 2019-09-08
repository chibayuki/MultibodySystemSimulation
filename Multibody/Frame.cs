/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2019 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.0.0.DEV.190907-0000

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
    // 帧，表示若干粒子的瞬时状态
    internal class Frame
    {
        private const double GravitationalConstant = 6.67259E-11; // 万有引力常量（牛顿平方米/平方千克）
        private const double MinimizeDistanceSquared = 1.0; // 计算万有引力的最小距离（米）

        public double _Time;
        public List<Particle> _Particles;

        public Frame(double time, params Particle[] particles)
        {
            if (double.IsNaN(time) || double.IsInfinity(time) || time < 0)
            {
                throw new ArgumentException();
            }

            if(particles==null|| particles.Length<=0)
            {
                throw new ArgumentNullException();
            }

            //

            _Time = time;
            _Particles = new List<Particle>(0);

            foreach (Particle particle in particles)
            {
                _Particles.Add(particle.Copy());
            }
        }

        public Frame(double time, List<Particle> particles)
        {
            if (double.IsNaN(time) || double.IsInfinity(time) || time < 0)
            {
                throw new ArgumentException();
            }

            if (particles == null || particles.Count <= 0)
            {
                throw new ArgumentNullException();
            }

            //

            _Time = time;
            _Particles = new List<Particle>(0);

            foreach (Particle particle in particles)
            {
                _Particles.Add(particle.Copy());
            }
        }

        // 获取此 Frame 对象的相对时刻（秒）
        public double Time => _Time;

        // 获取此 Frame 对象的所有粒子
        public List<Particle> Particles => _Particles;

        // 获取此 Frame 对象的副本
        public Frame Copy()
        {
            return new Frame(_Time, _Particles);
        }

        // 将此 Frame 对象运动指定的秒数
        public void NextMoment(double second)
        {
            if (double.IsNaN(second) || double.IsInfinity(second) || second <= 0)
            {
                throw new ArgumentException();
            }

            //

            _Time += second;

            for (int i = 0; i < _Particles.Count; i++)
            {
                for (int j = i + 1; j < _Particles.Count; j++)
                {
                    Com.PointD3D distance = _Particles[j].Location - _Particles[i].Location;
                    double distanceSquared = distance.ModuleSquared;

                    if (distanceSquared >= MinimizeDistanceSquared)
                    {
                        Com.PointD3D force = (GravitationalConstant * _Particles[i].Mass * _Particles[j].Mass / distanceSquared) * distance.Normalize;

                        _Particles[i].AddForce(force);
                        _Particles[j].AddForce(force.Opposite);
                    }
                }
            }

            foreach (Particle particle in _Particles)
            {
                particle.NextMoment(second);
            }

            foreach (Particle particle in _Particles)
            {
                particle.RemoveForce();
            }
        }
    }
}