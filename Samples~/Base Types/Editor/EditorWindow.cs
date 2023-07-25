#nullable enable
using Popcron;
using Popcron.Incomplete;

public abstract class EditorWindow : SealableEditorWindow
{
    protected override void OnEnable()
    {
        Everything.Add(this);
    }

    protected override void OnDisable()
    {
        Everything.Remove(this);
    }
}
