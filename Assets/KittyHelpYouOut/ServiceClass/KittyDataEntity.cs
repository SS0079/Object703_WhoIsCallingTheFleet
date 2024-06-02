using System;

namespace KittyHelpYouOut.ServiceClass
{
    /// <summary>
    /// 猫猫数据实体！数据激发UI解决方案
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class KittyDataEntity<T>
    {
        public KittyDataEntity( T value)
        {
            _Version = 0;
            _Value = value;
        }

        private ulong _Version;
        public ulong Version => _Version;
        private T _Value;
        public T Value => _Value;

        /// <summary>
        /// return false if new value is equal to previous value, thus set value fail
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public void Set(T order)
        {
            _Value = order;
            _Version++;
        }

        private void SyncVersion<TU>(KittyDataEntity<TU> source)
        {
            _Version = source._Version;
        }


        /// <summary>
        /// return false if request version is up to date
        /// </summary>
        /// <param name="request">request KDE</param>
        /// <returns></returns>
        public bool TryGet<TU>(KittyDataEntity<TU> request) where  TU : T
        {
            if (request.Version == this.Version) return false;
            request.SyncVersion(this);
            request._Value = (TU)this._Value;
            return true;
        }

        /// <summary>
        /// return false if request version is up to date
        /// </summary>
        /// <param name="requestVersion"></param>
        /// <param name="requestValue"></param>
        /// <typeparam name="TU"></typeparam>
        /// <returns></returns>
        public bool TryGet<TU>(ref ulong requestVersion, ref TU requestValue) where TU : T
        {
            if (requestVersion == this.Version) return false;
            requestValue = (TU)this.Value;
            requestVersion = this.Version;
            return true;
        }

    }

}