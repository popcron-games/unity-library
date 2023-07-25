#nullable enable
using UnityEditor;

namespace Popcron.Incomplete
{
    public abstract class SealableEditorWindow : EditorWindow
    {
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
    }
}
