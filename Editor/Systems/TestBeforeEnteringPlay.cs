﻿#nullable enable
using System;
using UnityEditor;
using UnityEngine;
using UnityLibrary.Systems;

namespace UnityLibrary.Editor.Systems
{
    public class TestBeforeEnteringPlay : IDisposable
    {
        private readonly VirtualMachine vm;

        public TestBeforeEnteringPlay(VirtualMachine vm)
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
                if (!tester.TestOpenedScenes(vm) && !UnityApplication.IsUnityPlayer)
                {
                    EditorApplication.isPlaying = false;
                    Debug.LogError("Unable to enter play mode due to errors.");
                }
                else if (!tester.TestStarting(vm))
                {
                    EditorApplication.isPlaying = false;
                    Debug.LogError("Unable to enter play mode due to errors.");
                }
            }
        }
    }
}