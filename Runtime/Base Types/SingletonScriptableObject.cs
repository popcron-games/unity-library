#nullable enable
using UnityEngine;

namespace UnityLibrary
{
    [ExecuteAlways]
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private static T? singleton;

        public static T Singleton
        {
            get
            {
                if (singleton == null)
                {
                    throw new($"Singleton of type {typeof(T)} not found in preloaded objects");
                }

                return singleton;
            }
        }

        protected virtual void OnEnable()
        {
            if (singleton is null)
            {
                singleton = this as T;
                SingletonScriptableObjects.scriptableObjects.Add(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (singleton is not null)
            {
                SingletonScriptableObjects.scriptableObjects.Remove(this);
                singleton = null;
            }
        }
    }
}