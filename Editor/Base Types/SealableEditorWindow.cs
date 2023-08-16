#nullable enable
using UnityEditor;

namespace Popcron.Sealable
{
    public abstract class SealableEditorWindow : EditorWindow
    {
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
    }
}
