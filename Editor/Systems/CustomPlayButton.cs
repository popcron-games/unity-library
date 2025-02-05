#nullable enable
using System;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityLibrary.Systems;
using UnityToolbarExtender;

namespace UnityLibrary.Editor.Systems
{
    public class CustomPlayButton : IDisposable
    {
        private const string ScenesBeforePlayKey = "ScenesBeforePlay";

        private readonly PlayValidationTester tester;
        private readonly VirtualMachine vm;

        public CustomPlayButton(VirtualMachine vm)
        {
            this.vm = vm;
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
                EditorPrefs.SetBool(UnityApplication.PlayFromStartKey, false);
                RestoreOpenedScenes();
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
            const float ButtonHeight = 21f;
            GUILayout.FlexibleSpace();
            GUI.enabled = !EditorApplication.isPlaying;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Test:");
            if (GUILayout.Button(new GUIContent("Scenes"), GUILayout.Height(ButtonHeight)))
            {
                if (tester.TestOpenedScenes(vm))
                {
                    Debug.Log("Play validation checks passed.");
                }
            }

            if (GUILayout.Button(new GUIContent("Playing"), GUILayout.Height(ButtonHeight)))
            {
                if (tester.TestOpenedScenes(vm) && tester.TestStarting(vm))
                {
                    Debug.Log("Play validation checks passed.");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(100);
            if (GUILayout.Button(new GUIContent("Play"), GUILayout.Height(ButtonHeight)))
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
            EditorPrefs.SetBool(UnityApplication.PlayFromStartKey, true);
            BackupOpenedScenes();
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

        private void RestoreOpenedScenes()
        {
            ReadOnlySpan<char> scenesBeforePlay = EditorPrefs.GetString(ScenesBeforePlayKey);
            int start = 0;
            int length = scenesBeforePlay.Length;
            int position = 0;
            while (position < length)
            {
                char c = scenesBeforePlay[position];
                if (c == ';')
                {
                    ReadOnlySpan<char> scenePath = scenesBeforePlay[start..position];
                    if (scenePath.Length > 0)
                    {
                        EditorSceneManager.OpenScene(scenePath.ToString(), OpenSceneMode.Additive);
                    }

                    start = position + 1;
                }

                position++;
            }

            EditorPrefs.DeleteKey(ScenesBeforePlayKey);
        }

        private void BackupOpenedScenes()
        {
            StringBuilder builder = new();
            for (int i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!string.IsNullOrEmpty(scene.path))
                {
                    builder.Append(scene.path);
                    builder.Append(';');
                }
            }

            EditorPrefs.SetString(ScenesBeforePlayKey, builder.ToString());
        }
    }
}