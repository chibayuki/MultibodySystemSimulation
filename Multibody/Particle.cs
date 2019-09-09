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
    // 粒子，表示三维空间中的质点
    internal sealed class Particle
    {
        private double _Mass;
        private double _Radius;
        private Com.PointD3D _Location;
        private Com.PointD3D _Velocity;
        private Com.PointD3D _Force;

        private Particle(double mass, double radius, Com.PointD3D location, Com.PointD3D velocity, Com.PointD3D force)
        {
            if ((double.IsNaN(mass) || double.IsInfinity(mass) || mass <= 0) || (double.IsNaN(radius) || double.IsInfinity(radius) || radius <= 0) || location.IsNaNOrInfinity || velocity.IsNaNOrInfinity || force.IsNaNOrInfinity)
            {
                throw new ArgumentException();
            }

            //

            _Mass = mass;
            _Radius = radius;
            _Location = location;
            _Velocity = velocity;
            _Force = force;
        }

        public Particle(double mass, double radius, Com.PointD3D location, Com.PointD3D velocity) : this(mass, radius, location, velocity, Com.PointD3D.Zero)
        {
        }

        // 获取此 Particle 对象的质量（千克）
        public double Mass => _Mass;

        // 获取此 Particle 对象的半径（米）
        public double Radius => _Radius;

        // 获取此 Particle 对象的位置（米）
        public Com.PointD3D Location => _Location;

        // 获取此 Particle 对象的速度（米/秒）
        public Com.PointD3D Velocity => _Velocity;

        // 获取此 Particle 对象的加速度（米/平方秒）
        public Com.PointD3D Acceleration => _Force / _Mass;

        // 返回此 Particle 对象的副本
        public Particle Copy()
        {
            return new Particle(_Mass, _Radius, _Location, _Velocity, _Force);
        }

        // 将此 Particle 对象运动指定的时长（秒）
        public void NextMoment(double second)
        {
            if (double.IsNaN(second) || double.IsInfinity(second) || second <= 0)
            {
                throw new ArgumentException();
            }

            //

            Com.PointD3D acceleration = Acceleration;

            _Location += (_Velocity + acceleration * (second / 2)) * second;
            _Velocity += acceleration * second;
        }

        // 在此 Particle 对象上施加一个作用力（牛顿）
        public void AddForce(Com.PointD3D force)
        {
            if (force.IsNaNOrInfinity)
            {
                throw new ArgumentException();
            }

            //

            _Force += force;
        }

        // 在此 Particle 对象上施加若干个作用力（牛顿）
        public void AddForce(params Com.PointD3D[] forces)
        {
            foreach (Com.PointD3D force in forces)
            {
                if (force.IsNaNOrInfinity)
                {
                    throw new ArgumentException();
                }
            }

            //

            foreach (Com.PointD3D force in forces)
            {
                _Force += force;
            }
        }

        // 移除在此 Particle 对象上施加的所有作用力
        public void RemoveForce()
        {
            _Force = Com.PointD3D.Zero;
        }
    }
}