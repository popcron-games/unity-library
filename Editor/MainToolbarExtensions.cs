using System;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityLibrary.Events;
using UnityLibrary.Systems;

namespace UnityLibrary.Editor
{
    [InitializeOnLoad]
    public static class MainToolbarExtensions
    {
        private const string ScenesBeforePlayKey = "com.popcron-games.unity-library.ScenesBeforePlay";
        private const string ValidateComponentsMenuItem = "Unity Library/Validate Components";
        private const string ValidateSystemsMenuItem = "Unity Library/Validate Systems";
        private const string SimulateBuildMenuItem = "Unity Library/Simulate Build";

        private static VirtualMachine VM => UnityApplication.VM;

        static MainToolbarExtensions()
        {
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        [MainToolbarElement(SimulateBuildMenuItem, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = -1)]
        public static MainToolbarElement CreatePlayButton()
        {
            string iconAddress = EditorGUIUtility.isProSkin ? "d_PlayButton" : "PlayButton";
            Texture2D? icon = EditorGUIUtility.IconContent(iconAddress).image as Texture2D;
            MainToolbarContent content = new("Play", icon, "Simulate build by playing from start scene");
            return new MainToolbarButton(content, SimulateBuild);
        }

        [MainToolbarElement(ValidateComponentsMenuItem, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = -2)]
        public static MainToolbarElement CreateValidateComponentsButton()
        {
            string iconAddress = EditorGUIUtility.isProSkin ? "d_Toggle Icon" : "Toggle Icon";
            Texture2D? icon = EditorGUIUtility.IconContent(iconAddress).image as Texture2D;
            MainToolbarContent content = new("Validate Components", icon, $"Dispatches a {nameof(Validate)} event to all components in opened scenes");
            return new MainToolbarButton(content, ValidateComponents);
        }

        [MainToolbarElement(ValidateSystemsMenuItem, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = -3)]
        public static MainToolbarElement CreateValidateSystemsButton()
        {
            string iconAddress = EditorGUIUtility.isProSkin ? "d_ToggleGroup Icon" : "ToggleGroup Icon";
            Texture2D? icon = EditorGUIUtility.IconContent(iconAddress).image as Texture2D;
            MainToolbarContent content = new("Validate Program", icon, $"Dispatches a {nameof(Validate)} event to the program and all systems in the virtual machine");
            return new MainToolbarButton(content, ValidateSystems);
        }

        private static void PlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                if (EditorPrefs.GetBool(UnityApplication.PlayFromStartKey))
                {
                    // create temporary empty scene
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                    // unload first scene if different from opened scenes
                    if (GetFirstScenePathFromBuildSettings() is string firstScenePath)
                    {
                        for (int i = 0; i < SceneManager.loadedSceneCount; i++)
                        {
                            Scene loadedScene = SceneManager.GetSceneAt(i);
                            if (loadedScene.path == firstScenePath)
                            {
                                SceneManager.UnloadSceneAsync(loadedScene);
                            }
                        }
                    }
                }

                RestoreOpenedScenes();
                UnloadEmptyScenes();
                EditorPrefs.SetBool(UnityApplication.PlayFromStartKey, false);
            }
        }

        private static void UnloadEmptyScenes()
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

        private static void ValidateComponents()
        {
            if (PlayValidationTester.ValidateComponents(VM))
            {
                Debug.Log("Play validation checks passed");
            }
        }

        private static void ValidateSystems()
        {
            if (PlayValidationTester.ValidateStarting(VM))
            {
                Debug.Log("Play validation checks passed");
            }
        }

        private static void SimulateBuild()
        {
            if (PlayValidationTester.ValidateStarting(VM))
            {
                EditorPrefs.SetBool(UnityApplication.PlayFromStartKey, true);
                BackupOpenedScenes();
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                // switch to first scene
                if (GetFirstScenePathFromBuildSettings() is string firstScenePath)
                {
                    EditorSceneManager.OpenScene(firstScenePath, OpenSceneMode.Single);
                }
                else
                {
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                }

                EditorApplication.isPlaying = true;
            }
            else
            {
                Debug.LogError("Play validation checks failed");
            }
        }

        private static void RestoreOpenedScenes()
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
                else if (position == length - 1)
                {
                    ReadOnlySpan<char> activeScenePath = scenesBeforePlay[start..];
                    for (int i = 0; i < SceneManager.loadedSceneCount; i++)
                    {
                        Scene scene = SceneManager.GetSceneAt(i);
                        if (scene.path.AsSpan().SequenceEqual(activeScenePath))
                        {
                            SceneManager.SetActiveScene(scene);
                        }
                    }
                }

                position++;
            }

            EditorPrefs.DeleteKey(ScenesBeforePlayKey);
        }

        private static void BackupOpenedScenes()
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

            builder.Append(SceneManager.GetActiveScene().path);
            EditorPrefs.SetString(ScenesBeforePlayKey, builder.ToString());
        }

        private static string? GetFirstScenePathFromBuildSettings()
        {
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            for (int i = 0; i < buildScenes.Length; i++)
            {
                EditorBuildSettingsScene buildScene = buildScenes[i];
                if (buildScene.enabled)
                {
                    return buildScene.path;
                }
            }

            return null;
        }
    }
}
