#nullable enable
using Popcron;
using UnityEngine;

public class CustomScriptableObject : SealableScriptableObject
{
    public MonoBehaviourFlags Flags { get; private set; }

    protected sealed override void OnEnable()
    {
        OnEnabled();
        Everything.Add(this);
    }

    protected sealed override void OnDisable()
    {
        Everything.Remove(this);
        OnDisabled();
    }

    protected virtual void OnEnabled() { }
    protected virtual void OnDisabled() { }

    protected sealed override void OnValidate()
    {
        Flags |= MonoBehaviourFlags.InOnValidate;
        if (this is IListener<ValidationEvent> validationListener)
        {
            validationListener.OnEvent(new ValidationEvent(Flags));
        }

        Flags &= ~MonoBehaviourFlags.InOnValidate;
    }

    protected sealed override void Reset()
    {
        Flags |= MonoBehaviourFlags.InReset;
        if (this is IListener<ValidationEvent> validationListener)
        {
            validationListener.OnEvent(new ValidationEvent(Flags));
        }

        Flags &= ~MonoBehaviourFlags.InReset;
    }
}
