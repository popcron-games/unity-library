#nullable enable
using System;
using UnityEditor;
using UnityEngine;

namespace Library.Unity
{
    public class EnterPlayValidationTester : IDisposable
    {
        private readonly VirtualMachine vm;

        public EnterPlayValidationTester(VirtualMachine vm)
        {
            this.vm = vm;
            EditorApplication.playModeStateChanged += Changed;
        }

        public void Dispose()
        {
            EditorApplication.playModeStateChanged -= Changed;
        }

        private void Changed(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                PlayValidationTester tester = vm.GetSystem<PlayValidationTester>();
                if (!tester.TestOpenedScenes(vm) || !tester.TestStarting(vm))
                {
                    EditorApplication.isPlaying = false;
                    Debug.LogError("Unable to enter play mode due to errors.");
                }
            }
        }
    }
}