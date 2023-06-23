#nullable enable
using Popcron;
using UnityEngine;

public class CustomMonoBehaviour : SealableMonoBehaviour
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

    protected sealed override void Awake()
    {
        OnAwake();
    }

    protected sealed override void Start()
    {
        OnStart();
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

    protected virtual void OnDisabled() { }
    protected virtual void OnEnabled() { }
    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }
}