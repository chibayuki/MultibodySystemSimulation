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
        private Com.PointD3D _Acceleration;

        public Particle(double mass, Com.PointD3D location, Com.PointD3D velocity)
        {
            _Mass = mass;
            _Location = location;
            _Velocity = velocity;
            _Acceleration = Com.PointD3D.Zero;
        }

        public double Mass
        {
            get
            {
                return _Mass;
            }
            set
            {
                _Mass = value;
            }
        }

        public Com.PointD3D Location
        {
            get
            {
                return _Location;
            }
        }

        public Com.PointD3D Velocity
        {
            get
            {
                return _Velocity;
            }
        }

        public Com.PointD3D Acceleration
        {
            get
            {
                return _Acceleration;
            }
        }

        public void AddForce(Com.PointD3D force)
        {
            _Acceleration += force / _Mass;
        }

        public void RemoveForce()
        {
            _Acceleration = Com.PointD3D.Zero;
        }

        public void Move(double second)
        {
            _Location += (_Velocity + _Acceleration * (second / 2)) * second;
            _Velocity += _Acceleration * second;
        }

        public Particle Copy()
        {
            Particle particle = new Particle(_Mass, _Location, _Velocity)
            {
                _Acceleration = Com.PointD3D.Zero
            };

            return particle;
        }
    }
}