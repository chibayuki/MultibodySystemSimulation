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
    internal class Frame
    {
        private const double GravitationalConstant = 6.67259E-11;

        public double _Time;
        public List<Particle> _Particles;

        public Frame(double time, params Particle[] particles)
        {
            _Time = time;
            _Particles = new List<Particle>(0);

            foreach (Particle particle in particles)
            {
                _Particles.Add(particle.Copy());
            }
        }

        public Frame(double time, List<Particle> particles)
        {
            _Time = time;
            _Particles = new List<Particle>(0);

            foreach (Particle particle in particles)
            {
                _Particles.Add(particle.Copy());
            }
        }

        public double Time => _Time;

        public List<Particle> Particles => _Particles;

        public Frame Copy()
        {
            return new Frame(_Time, _Particles);
        }

        public void NextMoment(double second)
        {
            _Time += second;

            for (int i = 0; i < _Particles.Count; i++)
            {
                for (int j = i + 1; j < _Particles.Count; j++)
                {
                    Com.PointD3D distance = _Particles[j].Location - _Particles[i].Location;
                    Com.PointD3D acceleration = (GravitationalConstant / distance.ModuleSquared) * distance.Normalize;

                    _Particles[i].AddForce(_Particles[j].Mass * acceleration);
                    _Particles[j].AddForce(_Particles[i].Mass * acceleration.Opposite);
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