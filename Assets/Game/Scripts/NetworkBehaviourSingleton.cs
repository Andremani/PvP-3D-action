using UnityEngine;
using Mirror;

namespace Andremani.Pvp3DAction
{
    public abstract class NetworkBehaviourSingleton<T> : NetworkBehaviour where T : NetworkBehaviourSingleton<T>
    {
        private static T instance;

        public static T I
        {
            get
            {
//#if UNITY_EDITOR
                if (!Application.isPlaying && !Exists)
                {
                    instance = FindObjectOfType<T>();
                }
//#endif
                return instance;
            }
        }

        public static bool Exists => instance != null;

        public virtual void Awake()
        {
            if (!Exists)
            {
                instance = this as T;
            }
        }

        public void OnDestroy()
        {
            instance = null;
        }
    }
}