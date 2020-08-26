﻿/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2020 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.0.0.DEV.200702-0000

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

namespace Multibody
{
    // 粒子，表示三维空间中的有体积的质点。
    internal sealed class Particle
    {
        private double _Mass; // 质量（千克）。
        private double _Radius; // 半径（米）。
        private PointD3D _Location; // 位置（米）。
        private PointD3D _Velocity; // 速度（米/秒）。
        private PointD3D _Force; // 作用力（牛顿）。
        private Color _Color; // 颜色。

        private Particle(double mass, double radius, PointD3D location, PointD3D velocity, PointD3D force, Color color)
        {
            if ((double.IsNaN(mass) || double.IsInfinity(mass) || mass <= 0) || (double.IsNaN(radius) || double.IsInfinity(radius) || radius <= 0) || location.IsNaNOrInfinity || velocity.IsNaNOrInfinity || force.IsNaNOrInfinity)
            {
                throw new ArgumentOutOfRangeException();
            }

            //

            _Mass = mass;
            _Radius = radius;
            _Location = location;
            _Velocity = velocity;
            _Force = force;
            _Color = color;
        }

        public Particle(double mass, double radius, PointD3D location, PointD3D velocity, Color color) : this(mass, radius, location, velocity, PointD3D.Zero, color)
        {
        }

        // 获取此 Particle 对象的质量（千克）。
        public double Mass => _Mass;

        // 获取此 Particle 对象的半径（米）。
        public double Radius => _Radius;

        // 获取此 Particle 对象的密度（千克/立方米）。
        private double Density => _Mass * 0.75 / Math.PI / (_Radius * _Radius * _Radius);

        // 获取此 Particle 对象的位置（米）。
        public PointD3D Location => _Location;

        // 获取此 Particle 对象的速度（米/秒）。
        public PointD3D Velocity => _Velocity;

        // 获取此 Particle 对象的加速度（米/平方秒）。
        public PointD3D Acceleration => _Force / _Mass;

        // 获取此 Particle 对象的颜色。
        public Color Color => _Color;

        // 返回此 Particle 对象的副本。
        public Particle Copy()
        {
            return new Particle(_Mass, _Radius, _Location, _Velocity, _Force, _Color);
        }

        // 将此 Particle 对象运动指定的时长（秒）。
        public void NextMoment(double seconds)
        {
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
            foreach (PointD3D force in forces)
            {
                if (force.IsNaNOrInfinity)
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            //

            foreach (PointD3D force in forces)
            {
                _Force += force;
            }
        }

        // 移除在此 Particle 对象上施加的所有作用力。
        public void RemoveForce()
        {
            _Force = PointD3D.Zero;
        }
    }
}