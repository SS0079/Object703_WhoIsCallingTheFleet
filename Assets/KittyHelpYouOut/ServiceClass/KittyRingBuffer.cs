using UnityEngine;
using UnityEngine.Assertions;

namespace KittyHelpYouOut
{
    public interface IRingBuffer<T>
    {
        /// <summary>
        /// 在队尾添加成员，并后移队尾
        /// </summary>
        /// <param name="item">新成员</param>
        /// <returns>返回是否成功添加</returns>
        int AddTail(T item);

        /// <summary>
        /// 在队尾添加成员数组，并后移队尾
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        int[] AddTail(T[] items);
        /// <summary>
        /// 在队首前一位添加成员，并前移队首
        /// </summary>
        /// <param name="item">新成员</param>
        /// <returns>返回是否成功添加</returns>
        int AddHead(T item);

        /// <summary>
        /// 在队首前添加成员数组，保持数组的顺序，并前移队首
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        int[] AddHead(T[] items);
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
    /// 猫猫环状大缓冲！而且带有自动扩展
    /// </summary>
    /// <typeparam name="T">成员类型</typeparam>
    public class KittyRingBuffer<T> : IRingBuffer<T>
    {
        private KittyRingBuffer()
        {
            
        }
        public KittyRingBuffer(int size)
        {
            buffer = new T[size];
            head = 0;
            tail = 0;
            count = 0;
            this.size = size;
        }
        private T[] buffer;
        private int size;
        private int head;
        /// <summary>
        /// Tail is the last element of buffer, not the first available index in buffer
        /// </summary>
        private int tail;
        private int count;
        public int Count=>count;
        private bool Full =>count>0 && count >= size;
        private bool Empty => count == 0;
        public int AddTail(T item)
        {
            if (Full)
            {
                Expend();
            }
            buffer[tail] = item;
            var index = tail;
            tail=(tail+1)% size;
            count++;
            return index;
        }

        public int[] AddTail(T[] items)
        {
            int[] indice = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                indice[i] = AddTail(items[i]);
            }
            return indice;
        }

        public int AddHead(T item)
        {
            if (Full)
            {
                Expend();
            }
            head = (head - 1 + size) % size;
            buffer[head] = item;
            count++;
            return head;
        }

      

        public int[] AddHead(T[] items)
        {
            int[] indice = new int[items.Length];
            for (int i = items.Length-1; i >=0; i--)
            {
                indice[i] = AddHead(items[i]);
            }
            return indice;
        }

        private void Expend()
        {
            var localBuffer = new T[size * 2];
            // if (head>=tail)//if index looped, need split raw buffer in half than copy them to the start and end of new buffer
            // {
            // }
            // else
            // {
            //     buffer.CopyTo(localBuffer,0);
            //     size = buffer.Length;
            // }
            //if head == tail ==0,than this is a normal buffer with on tail looped,set the tail to the end of available index
            if (tail==0 && head==0)
            {
                tail = count;
                //and just copy buffer to new buffer
                buffer.CopyTo(localBuffer,0);
            }
            else//or the tail is already looped and catch up the head, need split the buffer
            {
                //copy from head to end
                for (int i = head; i < size; i++)
                {
                    localBuffer[^(size - i)] = buffer[i];
                }
                //copy from start to tail
                for (int i = 0; i < tail; i++)
                {
                    localBuffer[i] = buffer[i];
                }
                //tail is correct in new buffer , head need to reset
                head = localBuffer.Length - (size - head);
            }
            
            buffer = localBuffer;
            size = buffer.Length;
        }
        
        public bool RemoveFromTail(int offset = 0)
        {
            //if offset is larger than buffer count, cannot remove
            if (offset >= count) return false;
            //invalid offset;
            if (offset < 0) return false;
            //if offset is not 0, need move all element on the right side of offset
            if (offset>0)
            {
                for (int i = 0; i < offset; i++)
                {
                    var front = (tail - offset + i+size) % size;
                    var next = (tail - offset + i + 1+size) % size;
                    buffer[front] = buffer[next];
                }
            }
            //decrease _Count and move _Tail left
            count--;
            tail = (tail - 1 + size) % size;
            return true;
        }

        public bool RemoveFromHead(int offset = 0)
        {
            //if offset is larger than buffer count, cannot remove
            if (offset >= count) return false;
            //invalid offset;
            if (offset < 0) return false;
            //if offset is not 0, need move all element on the left side of offset
            if (offset>0)
            {
                for (int i = 0; i < offset; i++)
                {
                    var front = (tail + offset - i + size) % size;
                    var next = (tail + offset - i - 1 + size) % size;
                    buffer[front] = buffer[next];
                }
            }
            //decrease _Count and move _Head right
            count--;
            head = (head + 1 + size) % size;
            return true;
        }

        public T GetFromTail(int offset = 0)
        {
            var target = (tail - offset + size-1) % size;
            return buffer[target];
        }

        public T GetFromHead(int offset = 0)
        {
            var target = (head + offset + size) % size;
            return buffer[target];
        }

        public bool SetFromTail(T item,int offset = 0)
        {
            //invalid if offset is larger than buffer count
            if (offset >= count) return false;
            //invalid offset;
            if (offset < 0) return false;
            var target = (tail - offset + size-1) % size;
            buffer[target] = item;
            return true;
        }

        public bool SetFromHead(T item,int offset = 0)
        {
            //invalid if offset is larger than buffer count
            if (offset >= count) return false;
            //invalid offset;
            if (offset < 0) return false;
            var target = (head + offset + size) % size;
            buffer[target] = item;
            return true;
        }

        public void Clear()
        {
            count = 0;
            head = 0;
            tail = 0;
        }
    }
}