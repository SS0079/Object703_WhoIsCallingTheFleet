using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace KittyHelpYouOut
{
    public class KittyMonoSingletonManual<T> : MonoBehaviour where T : Component
    {
        /// <summary>
        /// Dont destroy on load
        /// </summary>
        [Tooltip("Dont destroy on load")]
        public bool DDOL;
        // 单件子类实例
        private static T _Instance;

        /// <summary>
        ///     获得单件实例，查询场景中是否有该种类型，如果有存储静态变量，如果没有，构建一个带有这个component的gameobject
        ///     必须通过DestroyInstance自行管理单件的生命周期
        /// </summary>
        /// <returns>返回单件实例</returns>
        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    Type theType = typeof(T);

                    _Instance = (T)FindFirstObjectByType(theType);
                }
                return _Instance;
            }
        }

        /// <summary>
        ///     删除单件实例,这种继承关系的单件生命周期应该由模块显示管理
        /// </summary>
        public static void DestroyInstance()
        {
            if (_Instance != null)
            {
                Destroy(_Instance.gameObject);
            }
            _Instance = null;
        }

        public static void ClearDestroy()
        {
            DestroyInstance();
        }

        /// <summary>
        ///     Awake消息，确保单件实例的唯一性
        /// </summary>
        protected virtual void Awake()
        {
            if (_Instance != null && _Instance.gameObject != gameObject)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject); // UNITY_EDITOR
                }
            }
            else if (_Instance == null)
            {
                _Instance = GetComponent<T>();
            }
            if (DDOL)
            {
                DontDestroyOnLoad(gameObject);
            }
            Init();
        }

        /// <summary>
        ///     OnDestroy消息，确保单件的静态实例会随着GameObject销毁
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_Instance != null && _Instance.gameObject == gameObject)
            {
                _Instance = null;
            }
        }

        public virtual void DestroySelf()
        {
            _Instance = null;
            Destroy(gameObject);
        }

        public static bool HasInstance()
        {
            return _Instance != null;
        }

        protected virtual void Init()
        {

        }
    };

}
