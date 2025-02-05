#nullable enable
using UnityLibrary.Systems;

namespace UnityLibrary
{
    /// <summary>
    /// Custom implementation of a <see cref="UnityEngine.ScriptableObject"/>
    /// that is fetchable from <see cref="UnityObjects"/> and receives events when implementing <see cref="IListener{T}"/>.
    /// </summary>
    public abstract class CustomScriptableObject : UnityEngine.ScriptableObject
    {
        protected static VirtualMachine VM => UnityApplication.VM;
        protected static UnityObjects Objects => VM.GetSystem<UnityObjects>();

        protected virtual void OnEnable()
        {
            Objects.TryRegister(this);
        }

        protected virtual void OnDisable()
        {
            Objects.TryUnregister(this);
        }
    }
}