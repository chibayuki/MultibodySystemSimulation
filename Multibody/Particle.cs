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
    internal class Particle
    {
        private double _Mass;
        private Com.PointD3D _Location;
        private Com.PointD3D _Velocity;
        private Com.PointD3D _Force;

        private Particle(double mass, Com.PointD3D location, Com.PointD3D velocity, Com.PointD3D force)
        {
            _Mass = mass;
            _Location = location;
            _Velocity = velocity;
            _Force = force;
        }

        public Particle(double mass, Com.PointD3D location, Com.PointD3D velocity) : this(mass, location, velocity, Com.PointD3D.Zero)
        {
        }

        public double Mass => _Mass;

        public Com.PointD3D Location => _Location;

        public Com.PointD3D Velocity => _Velocity;

        public Com.PointD3D Acceleration => _Force / _Mass;

        public Particle Copy()
        {
            return new Particle(_Mass, _Location, _Velocity, _Force);
        }

        public void NextMoment(double second)
        {
            Com.PointD3D acceleration = Acceleration;

            _Location += (_Velocity + acceleration * (second / 2)) * second;
            _Velocity += acceleration * second;
        }

        public void AddForce(Com.PointD3D force)
        {
            _Force += force;
        }

        public void RemoveForce()
        {
            _Force = Com.PointD3D.Zero;
        }
    }
}