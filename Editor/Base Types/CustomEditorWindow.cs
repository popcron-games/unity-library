#nullable enable
using Popcron;

public class CustomEditorWindow : SealableEditorWindow
{
    protected sealed override void OnEnable()
    {
        Everything.Add(this);
        OnEnabled();
    }

    protected sealed override void OnDisable()
    {
        OnDisabled();
        Everything.Remove(this);
    }

    protected virtual void OnEnabled() { }
    protected virtual void OnDisabled() { }
}
