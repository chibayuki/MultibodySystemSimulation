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
        private const double GravitationalConstant = 6.67259E-11;

        private class Frame
        {
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

            public double Time
            {
                get
                {
                    return _Time;
                }

                set
                {
                    _Time = value;
                }
            }

            public List<Particle> Particles
            {
                get
                {
                    return _Particles;
                }

                set
                {
                    _Particles = value;
                }
            }
        }

        private List<Frame> _Frames;

        public MultibodySystem(params Particle[] particles)
        {
            Reset(particles);
        }

        public MultibodySystem(List<Particle> particles)
        {
            Reset(particles);
        }

        public void Reset(params Particle[] particles)
        {
            _Frames = new List<Frame>();
            _Frames.Add(new Frame(0, particles));
        }

        public void Reset(List<Particle> particles)
        {
            _Frames = new List<Frame>();
            _Frames.Add(new Frame(0, particles));
        }

        public void BackToBeginning()
        {
            Frame frame = _Frames[0];

            _Frames.Clear();
            _Frames.Add(frame);
        }

        public void NextFrame(double second)
        {
            Frame frame = _Frames[_Frames.Count - 1];
            Frame newFrame = new Frame(frame.Time + second, frame.Particles);

            List<Particle> particles = newFrame.Particles;

            for (int i = 0; i < particles.Count; i++)
            {
                for (int j = i + 1; j < particles.Count; j++)
                {
                    Com.PointD3D distance = particles[j].Location - particles[i].Location;
                    Com.PointD3D force = (GravitationalConstant * particles[j].Mass / distance.ModuleSquared) * distance.Normalize;

                    particles[i].AddForce(force);
                }
            }

            foreach (Particle particle in particles)
            {
                particle.Move(second);
            }

            foreach (Particle particle in particles)
            {
                particle.RemoveForce();
            }

            _Frames.Add(newFrame);
        }
    }
}