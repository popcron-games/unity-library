#nullable enable
using Popcron;
using Popcron.Sealable;

public abstract class MonoBehaviour : SealableMonoBehaviour
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