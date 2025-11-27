#nullable enable
using UnityEditor;
using UnityEngine;
using UnityLibrary.Systems;

namespace UnityLibrary.Editor.Systems
{
    public class TestBeforeEnteringPlay : SystemBase
    {
        public TestBeforeEnteringPlay(VirtualMachine vm) : base(vm)
        {
            EditorApplication.playModeStateChanged += Changed;
        }

        public override void Dispose()
        {
            EditorApplication.playModeStateChanged -= Changed;
        }

        private void Changed(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                if (!PlayValidationTester.ValidateComponents(vm) && !UnityApplication.IsUnityPlayer)
                {
                    EditorApplication.ExitPlaymode();
                    EditorApplication.delayCall += () => RepaintAllViews();
                    Debug.LogError("Unable to enter play mode due to errors");
                }
                else if (!PlayValidationTester.ValidateStarting(vm))
                {
                    EditorApplication.ExitPlaymode();
                    EditorApplication.delayCall += () => RepaintAllViews();
                    Debug.LogError("Unable to enter play mode due to errors");
                }
            }
        }

        private static void RepaintAllViews()
        {
            EditorApplication.delayCall += () =>
            {
                foreach (EditorWindow window in Resources.FindObjectsOfTypeAll<EditorWindow>())
                {
                    window.Repaint();
                }

                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            };
        }
    }
}