#nullable enable
using Popcron;
using Popcron.Incomplete;
using UnityEngine;

public abstract class ScriptableObject : SealableScriptableObject
{
    public MonoBehaviourFlags Flags { get; private set; }

    protected override void OnEnable()
    {
        Everything.Add(this);
    }

    protected override void OnDisable()
    {
        Everything.Remove(this);
    }

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
