#nullable enable
using Popcron;
using Popcron.Sealable;

public abstract class Editor : SealableEditor
{
    protected override void OnEnable()
    {
        base.OnEnable();
        Everything.Add(this);
    }

    protected override void OnDisable()
    {
        Everything.Remove(this);
        base.OnDisable();
    }
}