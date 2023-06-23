#nullable enable
namespace Popcron.Editor
{
    public class CustomEditor : SealableEditor
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
}