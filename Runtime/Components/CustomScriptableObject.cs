#nullable enable
using Library;
using Library.Systems;
using Library.Unity;

/// <summary>
/// Custom implementation of a <see cref="UnityEngine.ScriptableObject"/>
/// that is fetchable from <see cref="UnityObjects"/> and receives events when implementing <see cref="IListener{T}"/>.
/// </summary>
public abstract class CustomScriptableObject : UnityEngine.ScriptableObject
{
    protected static VirtualMachine VM => Host.VirtualMachine;
    protected static UnityObjects Objects => VM.GetSystem<UnityObjects>();

    protected virtual void OnEnable()
    {
        Objects.Register(this);
    }

    protected virtual void OnDisable()
    {
        Objects.Unregister(this);
    }
}