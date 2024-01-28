#nullable enable
using Library.Functions;
using Library.Systems;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace Library.Unity
{
    public class CustomPlayButton : IDisposable
    {
        private readonly PlayValidationTester tester;

        public CustomPlayButton(VirtualMachine vm)
        {
            tester = vm.GetSystem<PlayValidationTester>();
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        public void Dispose()
        {
            ToolbarExtender.LeftToolbarGUI.Remove(OnToolbarGUI);
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        }

        private void PlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                EditorPrefs.SetBool(Host.PlayFromStartKey, false);
                RestoreOpenedScenes.Invoke();
                UnloadEmptyScenes();
            }
        }

        private void UnloadEmptyScenes()
        {
            for (int i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (string.IsNullOrEmpty(scene.path) && scene.rootCount == 0)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            VirtualMachine vm = Host.VirtualMachine;
            GUI.enabled = !EditorApplication.isPlaying;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Test:");
            if (GUILayout.Button(new GUIContent("Scenes"), GUILayout.Height(20)))
            {
                if (tester.TestOpenedScenes(vm))
                {
                    Debug.Log("Play validation checks passed.");
                }
            }

            if (GUILayout.Button(new GUIContent("Playing"), GUILayout.Height(20)))
            {
                if (tester.TestOpenedScenes(vm) && tester.TestStarting(vm))
                {
                    Debug.Log("Play validation checks passed.");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(100);
            if (GUILayout.Button(new GUIContent("Play"), GUILayout.Height(20)))
            {
                if (tester.TestStarting(vm))
                {
                    EnterPlay();
                }
                else
                {
                    Debug.LogError("Unable to enter play mode due to errors.");
                }
            }
        }

        private void EnterPlay()
        {
            EditorPrefs.SetBool(Host.PlayFromStartKey, true);
            BackupOpenedScenes.Invoke();
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            bool loadedFirstScene = false;
            for (int i = 0; i < buildScenes.Length; i++)
            {
                EditorBuildSettingsScene buildScene = buildScenes[i];
                if (buildScene.enabled)
                {
                    EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
                    loadedFirstScene = true;
                    break;
                }
            }

            if (!loadedFirstScene)
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }

            EditorApplication.isPlaying = true;
        }
    }
}