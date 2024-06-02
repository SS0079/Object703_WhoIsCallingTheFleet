using System;
using System.Collections.Generic;

namespace KittyHelpYouOut.ServiceClass
{
    public static class KittyEvent
    {
        public interface IEvent { }
        
        public class Event<T> : IEvent 
        {
            private Action<T> _Callback=e=>{};

            public IEvent Register(Action<T> callback)
            {
                _Callback += callback;
                return this;
            }

            public IEvent UnRegister(Action<T> callback)
            {
                _Callback -= callback;
                return this;
            }

            public void Trigger(T e)
            {
                _Callback.Invoke(e);
            }

        }
        
        private static readonly Dictionary<Type, IEvent> _Events=new Dictionary<Type, IEvent>();

        public static void Register<K>(Action<K> callback) 
        {
            var type = typeof(K);
            _Events.TryAdd(type, new Event<K>());
            ((Event<K>)_Events[type]).Register(callback);
        }

        public static void UnRegister<K>(Action<K> callback) 
        {
            var type = typeof(K);
            if(!_Events.ContainsKey(type)) return;
            ((Event<K>)_Events[type]).UnRegister(callback);
        }

        public static void Send<K>(K e) 
        {
            var type = typeof(K);
            if (!_Events.ContainsKey(type)) return;
            ((Event<K>)_Events[type]).Trigger(e);
        }
    }
}