#nullable enable
using UnityLibrary.Systems;

namespace UnityLibrary
{
    /// <summary>
    /// Custom implementation of a <see cref="UnityEngine.ScriptableObject"/>
    /// that is fetchable from <see cref="Systems.UnityObjects"/> and receives events when implementing <see cref="IListener{T}"/>.
    /// </summary>
    public abstract class CustomScriptableObject : UnityEngine.ScriptableObject
    {
        public static VirtualMachine VM => UnityApplication.VM;
        public static UnityObjects UnityObjects => VM.GetSystem<UnityObjects>();

        private UnityObjects? unityObjects = null;

        protected virtual void OnEnable()
        {
            unityObjects = UnityObjects;
            unityObjects.TryRegister(this);
        }

        protected virtual void OnDisable()
        {
            unityObjects?.TryUnregister(this);
            unityObjects = null;
        }
    }
}