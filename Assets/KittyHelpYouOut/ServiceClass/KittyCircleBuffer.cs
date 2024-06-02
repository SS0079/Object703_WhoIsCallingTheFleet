using UnityEngine.Assertions;

namespace KittyHelpYouOut
{
    public interface ICircleBuffer<T>
    {
        /// <summary>
        /// 在队尾添加成员，并后移队尾
        /// </summary>
        /// <param name="item">新成员</param>
        /// <returns>返回是否成功添加</returns>
        bool AddTail(T item);
        /// <summary>
        /// 在队首前一位添加成员，并前移队首
        /// </summary>
        /// <param name="item">新成员</param>
        /// <returns>返回是否成功添加</returns>
        bool AddHead(T item);
        /// <summary>
        /// 从后向前数移除一个成员，后方成员向前缩进
        /// </summary>
        /// <param name="offset">偏移，默认为0</param>
        /// <returns>返回是否成功移除</returns>
        bool RemoveFromTail(int offset=0);
        /// <summary>
        /// 从前向后数移除一个成员，前方成员向前缩进
        /// </summary>
        /// <param name="offset">偏移，默认为0</param>
        /// <returns>返回是否成功移除</returns>
        bool RemoveFromHead(int offset=0);
        /// <summary>
        /// 从后向前数访问一个索引的值
        /// </summary>
        /// <param name="offset">偏移，默认为0</param>
        /// <returns>返回指定索引的值</returns>
        T GetFromTail(int offset = 0);
        /// <summary>
        /// 从前向后数访问一个索引的值
        /// </summary>
        /// <param name="offset">偏移，默认为0</param>
        /// <returns>返回指定索引的值</returns>
        T GetFromHead(int offset = 0);

        /// <summary>
        /// 从后向前数设置一个索引的值
        /// </summary>
        /// <param name="item">新成员值</param>
        /// <param name="offset">偏移，默认为0</param>
        /// <returns>返回是否成功设置</returns>
        bool SetFromTail(T item,int offset = 0);

        /// <summary>
        /// 从前向后数设置一个索引的值
        /// </summary>
        /// <param name="item">新成员值</param>
        /// <param name="offset">偏移，默认为0</param>
        /// <returns>返回是否成功设置</returns>
        bool SetFromHead(T item,int offset = 0);
        /// <summary>
        /// 清空缓冲并重置队首队尾
        /// </summary>
        void Clear();
    }
    
    /// <summary>
    /// 猫猫环状大缓冲！但是没有自动扩展
    /// </summary>
    /// <typeparam name="T">成员类型</typeparam>
    public class KittyCircleBuffer<T> : ICircleBuffer<T>
    {
        private KittyCircleBuffer()
        {
            
        }
        public KittyCircleBuffer(int size)
        {
            _Buffer = new T[size];
            _Head = 0;
            _Tail = -1;
            _Count = 0;
            _Size = size;
        }
        private readonly T[] _Buffer;
        private int _Size;
        private int _Head;
        /// <summary>
        /// Tail is the last element of buffer, not the first available index in buffer
        /// </summary>
        private int _Tail;
        private int _Count;
        public int Count=>_Count;
        public bool Full =>_Count>0 && _Count >= _Size;
        public bool Empty => _Count == 0;
        public bool AddTail(T item)
        {
            if (Full) return false;
            _Tail=(_Tail+1+_Size)% _Size;
            _Buffer[_Tail] = item;
            _Count++;
            return true;
        }

        public bool AddHead(T item)
        {
            if (Full) return false;
            _Head = (_Head - 1 + _Size) % _Size;
            _Buffer[_Head] = item;
            _Count++;
            return true;
        }

        public bool RemoveFromTail(int offset = 0)
        {
            //if offset is larger than buffer count, cannot remove
            if (offset >= _Count) return false;
            //invalid offset;
            if (offset < 0) return false;
            //if offset is not 0, need move all element on the right side of offset
            if (offset>0)
            {
                for (int i = 0; i < offset; i++)
                {
                    var front = (_Tail - offset + i+_Size) % _Size;
                    var next = (_Tail - offset + i + 1+_Size) % _Size;
                    _Buffer[front] = _Buffer[next];
                }
            }
            //decrease _Count and move _Tail left
            _Count--;
            _Tail = (_Tail - 1 + _Size) % _Size;
            return true;
        }

        public bool RemoveFromHead(int offset = 0)
        {
            //if offset is larger than buffer count, cannot remove
            if (offset >= _Count) return false;
            //invalid offset;
            if (offset < 0) return false;
            //if offset is not 0, need move all element on the left side of offset
            if (offset>0)
            {
                for (int i = 0; i < offset; i++)
                {
                    var front = (_Tail + offset - i + _Size) % _Size;
                    var next = (_Tail + offset - i - 1 + _Size) % _Size;
                    _Buffer[front] = _Buffer[next];
                }
            }
            //decrease _Count and move _Head right
            _Count--;
            _Head = (_Head + 1 + _Size) % _Size;
            return true;
        }

        public T GetFromTail(int offset = 0)
        {
            var target = (_Tail - offset + _Size) % _Size;
            return _Buffer[target];
        }

        public T GetFromHead(int offset = 0)
        {
            var target = (_Head + offset + _Size) % _Size;
            return _Buffer[target];
        }

        public bool SetFromTail(T item,int offset = 0)
        {
            //invalid if offset is larger than buffer count
            if (offset >= _Count) return false;
            //invalid offset;
            if (offset < 0) return false;
            var target = (_Tail - offset + _Size) % _Size;
            _Buffer[target] = item;
            return true;
        }

        public bool SetFromHead(T item,int offset = 0)
        {
            //invalid if offset is larger than buffer count
            if (offset >= _Count) return false;
            //invalid offset;
            if (offset < 0) return false;
            var target = (_Head + offset + _Size) % _Size;
            _Buffer[target] = item;
            return true;
        }

        public void Clear()
        {
            _Count = 0;
            _Head = 0;
            _Tail = 0;
        }
    }
}