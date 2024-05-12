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

using System.Drawing;

using PointD3D = Com.PointD3D;
using Com;

namespace Multibody
{
    // 粒子的固定属性。
    internal sealed class ParticleConstantAttr
    {
        public ParticleConstantAttr(int id, double mass, double radius, Color color)
        {
            Id = id;

            Mass = mass;
            Radius = radius;
            Density = mass * 0.75 / Math.PI / (radius * radius * radius);

            Color = color;
        }

        public int Id { get; private set; } // ID。

        public double Mass { get; private set; } // 质量（千克）。

        public double Radius { get; private set; } // 半径（米）。

        public double Density { get; private set; } // 密度。

        public Color Color { get; private set; } // 颜色。
    }

    // 粒子，表示三维空间中的有体积的质点。
    internal sealed class Particle
    {
        private bool _Frozen; // 是否已冻结。

        private ParticleConstantAttr _ConstantAttr; // 固定属性。

        private PointD3D _Location; // 位置（米）。
        private PointD3D _Velocity; // 速度（米/秒）。
        private PointD3D _Force; // 作用力（牛顿）。

        private Particle(ParticleConstantAttr constantAttr, PointD3D location, PointD3D velocity, PointD3D force)
        {
            if (constantAttr is null)
            {
                throw new ArgumentNullException();
            }

            if (location.IsNaNOrInfinity || velocity.IsNaNOrInfinity || force.IsNaNOrInfinity)
            {
                throw new ArgumentOutOfRangeException();
            }

            //

            _Frozen = false;

            _ConstantAttr = constantAttr;

            _Location = location;
            _Velocity = velocity;
            _Force = force;
        }

        private Particle(int id, double mass, double radius, Color color, PointD3D location, PointD3D velocity, PointD3D force)
        {
            if ((double.IsNaN(mass) || double.IsInfinity(mass) || mass <= 0) || (double.IsNaN(radius) || double.IsInfinity(radius) || radius <= 0) || location.IsNaNOrInfinity || velocity.IsNaNOrInfinity || force.IsNaNOrInfinity)
            {
                throw new ArgumentOutOfRangeException();
            }

            //

            _Frozen = false;

            _ConstantAttr = new ParticleConstantAttr(id, mass, radius, color);

            _Location = location;
            _Velocity = velocity;
            _Force = force;
        }

        public Particle(int id, double mass, double radius, Color color, PointD3D location, PointD3D velocity) : this(id, mass, radius, color, location, velocity, PointD3D.Zero)
        {
        }

        // 获取此 Particle 对象的ID。
        public int Id => _ConstantAttr.Id;

        // 获取此 Particle 对象的质量（千克）。
        public double Mass => _ConstantAttr.Mass;

        // 获取此 Particle 对象的半径（米）。
        public double Radius => _ConstantAttr.Radius;

        // 获取此 Particle 对象的密度（千克/立方米）。
        public double Density => _ConstantAttr.Density;

        // 获取此 Particle 对象的颜色。
        public Color Color => _ConstantAttr.Color;

        // 获取此 Particle 对象的位置（米）。
        public PointD3D Location => _Location;

        // 获取此 Particle 对象的速度（米/秒）。
        public PointD3D Velocity => _Velocity;

        // 获取此 Particle 对象受到的的作用力（牛顿）。
        public PointD3D Force => _Force;

        // 获取此 Particle 对象的加速度（米/平方秒）。
        public PointD3D Acceleration => _Force / _ConstantAttr.Mass;

        // 返回此 Particle 对象的副本。
        public Particle Copy()
        {
            return new Particle(_ConstantAttr, _Location, _Velocity, _Force);
        }

        // 将此 Particle 对象运动指定的时长（秒）。
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

            PointD3D acceleration = Acceleration;

            _Location += (_Velocity + acceleration * (seconds / 2)) * seconds;
            _Velocity += acceleration * seconds;
        }

        // 在此 Particle 对象上施加一个作用力（牛顿）。
        public void AddForce(PointD3D force)
        {
            if (_Frozen)
            {
                throw new InvalidOperationException();
            }

            if (force.IsNaNOrInfinity)
            {
                throw new ArgumentOutOfRangeException();
            }

            //

            _Force += force;
        }

        // 在此 Particle 对象上施加若干个作用力（牛顿）。
        public void AddForce(params PointD3D[] forces)
        {
            if (_Frozen)
            {
                throw new InvalidOperationException();
            }

            for (int i = 0; i < forces.Length; i++)
            {
                if (forces[i].IsNaNOrInfinity)
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            //

            for (int i = 0; i < forces.Length; i++)
            {
                _Force += forces[i];
            }
        }

        // 移除在此 Particle 对象上施加的所有作用力。
        public void RemoveForce()
        {
            if (_Frozen)
            {
                throw new InvalidOperationException();
            }

            //

            _Force = PointD3D.Zero;
        }

        public void Freeze()
        {
            _Frozen = true;
        }
    }
}