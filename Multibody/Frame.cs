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

using PointD3D = Com.PointD3D;

namespace Multibody
{
    // 帧，表示若干粒子的瞬时状态。
    internal sealed class Frame
    {
        private const double _GravitationalConstant = 6.67259E-11; // 万有引力常量（牛顿平方米/平方千克）。

        private bool _Frozen; // 是否已冻结。

        private long _DynamicsId; // 基于动力学的ID。
        private long _KinematicsId; // 基于运动学的ID。

        private double _Time; // 相对时刻（秒）。
        private Particle[] _Particles; // 所有粒子。

        public Frame(double time, params Particle[] particles)
        {
            if (double.IsNaN(time) || double.IsInfinity(time) || time < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (particles is null)
            {
                throw new ArgumentNullException();
            }

            //

            _Frozen = false;

            _DynamicsId = 0;
            _KinematicsId = 0;

            _Time = time;
            _Particles = new Particle[particles.Length];

            for (int i = 0; i < particles.Length; i++)
            {
                _Particles[i] = particles[i].Copy();
            }
        }

        public Frame(double time, IEnumerable<Particle> particles)
        {
            if (double.IsNaN(time) || double.IsInfinity(time) || time < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (particles is null)
            {
                throw new ArgumentNullException();
            }

            //

            Particle[] particleArray = particles.ToArray();

            _Frozen = false;

            _DynamicsId = 0;
            _KinematicsId = 0;

            _Time = time;
            _Particles = new Particle[particleArray.Length];

            for (int i = 0; i < particleArray.Length; i++)
            {
                _Particles[i] = particleArray[i].Copy();
            }
        }

        // 获取表示此 Frame 是否已冻结的布尔值。
        public bool Frozen => _Frozen;

        // 获取或设置此 Frame 对象基于动力学的 ID。
        public long DynamicsId
        {
            get => _DynamicsId;

            set => _DynamicsId = value;
        }

        // 获取或设置此 Frame 对象基于运动学的 ID。
        public long KinematicsId
        {
            get => _KinematicsId;

            set => _KinematicsId = value;
        }

        // 获取此 Frame 对象的相对时刻（秒）。
        public double Time => _Time;

        // 获取此 Frame 对象的粒子数。
        public int ParticleCount => _Particles.Length;

        // 获取此 Frame 对象的指定粒子。
        public Particle GetParticle(int index) => _Particles[index];

        // 获取此 Frame 对象的副本。
        public Frame Copy() => new Frame(_Time, _Particles)
        {
            _DynamicsId = this._DynamicsId,
            _KinematicsId = this._KinematicsId,
        };

        // 将此 Frame 对象运动指定的时长（秒）。
        public void NextMoment(double seconds)
        {
            if (_Frozen)
            {
                throw new InvalidOperationException();
            }

            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            //

            _Time += seconds;

            if (_Particles.Length > 0)
            {
                for (int i = 0; i < _Particles.Length; i++)
                {
                    _Particles[i].RemoveForce();
                }

                for (int i = 0; i < _Particles.Length; i++)
                {
                    for (int j = i + 1; j < _Particles.Length; j++)
                    {
                        PointD3D distance = _Particles[j].Location - _Particles[i].Location;

                        double distanceModule = distance.Module;

                        if (distanceModule > 0)
                        {
                            double radiusSum = _Particles[i].Radius + _Particles[j].Radius;
                            double dist = Math.Max(distanceModule, radiusSum);
                            double distSquared = dist * dist;

                            PointD3D force = (_GravitationalConstant * _Particles[i].Mass * _Particles[j].Mass / distSquared) * distance.Normalize;

                            if (distanceModule < radiusSum)
                            {
                                force *= distanceModule / radiusSum;
                            }

                            _Particles[i].AddForce(force);
                            _Particles[j].AddForce(force.Opposite);
                        }
                    }
                }

                for (int i = 0; i < _Particles.Length; i++)
                {
                    _Particles[i].NextMoment(seconds);
                }
            }
        }

        public void Freeze()
        {
            if (_Frozen)
            {
                throw new InvalidOperationException();
            }

            //

            _Frozen = true;

            for (int i = 0; i < _Particles.Length; i++)
            {
                _Particles[i].Freeze();
            }
        }
    }
}