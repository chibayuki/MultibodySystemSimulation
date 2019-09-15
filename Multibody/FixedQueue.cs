/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
Copyright © 2019 chibayuki@foxmail.com

多体系统模拟 (MultibodySystemSimulation)
Version 1.0.0.0.DEV.190914-0000

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
    // 通过自动弹出队首元素实现固定容量的队列
    internal sealed class FixedQueue<T>
    {
        private int _Capacity;
        private int _StartIndex;
        private int _Count;
        private T[] _TArray;

        public FixedQueue(int capacity)
        {
            if (capacity < 0)
            {
                throw new OverflowException();
            }

            //

            _Capacity = capacity;
            _StartIndex = 0;
            _Count = 0;
            _TArray = new T[_Capacity];
        }

        // 获取或设置此 _FixedQueue 对象的指定索引的元素
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _Count)
                {
                    throw new IndexOutOfRangeException();
                }

                //

                int _index = _StartIndex + index;

                if (_index >= _Capacity)
                {
                    _index -= _Capacity;
                }

                return _TArray[_index];
            }

            set
            {
                if (index < 0 || index >= _Count)
                {
                    throw new IndexOutOfRangeException();
                }

                //

                int _index = _StartIndex + index;

                if (_index >= _Capacity)
                {
                    _index -= _Capacity;
                }

                _TArray[_index] = value;
            }
        }

        // 获取或设置此 _FixedQueue 对象的队首元素
        public T Head
        {
            get
            {
                return this[0];
            }

            set
            {
                this[0] = value;
            }
        }

        // 获取或设置此 _FixedQueue 对象的队尾元素
        public T Tail
        {
            get
            {
                return this[_Count - 1];
            }

            set
            {
                this[_Count - 1] = value;
            }
        }

        // 获取此 _FixedQueue 对象的容量
        public int Capacity => _Capacity;

        // 获取此 _FixedQueue 对象的元素数目
        public int Count => _Count;

        // 重新设置此 _FixedQueue 对象的容量
        public void Resize(int capacity)
        {
            if (capacity < 0)
            {
                throw new OverflowException();
            }

            //

            if (capacity != _Capacity)
            {
                if (_Count <= 0 || capacity == 0)
                {
                    _Capacity = capacity;
                    _StartIndex = 0;
                    _Count = 0;
                    _TArray = new T[_Capacity];
                }
                else
                {
                    T[] array = new T[capacity];

                    _Count = Math.Min(capacity, _Count);

                    for (int i = 0; i < _Count; i++)
                    {
                        int _index = _StartIndex + i;

                        if (_index >= _Capacity)
                        {
                            _index -= _Capacity;
                        }

                        array[i] = _TArray[_index];
                    }

                    _Capacity = capacity;
                    _StartIndex = 0;
                    _TArray = array;
                }
            }
        }

        // 向此 _FixedQueue 对象的队尾添加一个元素
        public void Enqueue(T item)
        {
            if (_Count < _Capacity)
            {
                _Count++;
            }
            else if (_StartIndex < _Capacity)
            {
                _StartIndex++;
            }
            else
            {
                _StartIndex = 0;
            }

            this[_Count - 1] = item;
        }

        // 从此 _FixedQueue 对象的队首取出一个元素
        public T Dequeue()
        {
            T result = this[0];

            _StartIndex++;

            if (_StartIndex >= _Capacity)
            {
                _StartIndex -= _Capacity;
            }

            _Count--;

            return result;
        }

        // 删除此 _FixedQueue 对象的所有元素
        public void Clear()
        {
            _StartIndex = 0;
            _Count = 0;

            for (int i = 0; i < _Capacity; i++)
            {
                _TArray[i] = default(T);
            }
        }
    }
}