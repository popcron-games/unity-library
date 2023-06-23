#nullable enable
using Popcron.Editor;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Popcron
{
    [InitializeOnLoad]
    public static class CustomEnterPlayBehaviour
    {
        private static readonly StringBuilder builder = new StringBuilder();
        private static PlayStateProcess playStateProcess;

        static CustomEnterPlayBehaviour()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanging;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (playStateProcess == PlayStateProcess.PreStarting)
                {
                    float startTime = EditorPrefs.GetFloat("startTime");
                    if (EditorApplication.timeSinceStartup > startTime)
                    {
                        playStateProcess = PlayStateProcess.Starting;

                        bool ableToPlay = true;
                        PlayabilityCheck beforePlayingCheck = new PlayabilityCheck();
                        SceneManager.GetActiveScene().ForEachGameObject((go) =>
                        {
                            //report issues with fields that dont have a ? and are missing
                            foreach (Component? component in go.GetComponentsInChildren<Component>())
                            {
                                if (component != null)
                                {
                                    if (component is IListener<ValidationEvent> validationListener)
                                    {
                                        validationListener.OnEvent(new ValidationEvent(MonoBehaviourFlags.None));
                                    }

                                    if (component is IListener<PlayabilityCheck> checkListener)
                                    {
                                        checkListener.OnEvent(beforePlayingCheck);
                                    }
                                }
                            }
                        });

                        new ValidationEvent().Dispatch();
                        PlayabilityCheck globalCheck = new PlayabilityCheck().Dispatch();
                        foreach (PlayabilityCheck.Issue issue in PlayabilityCheck.GetIssues(globalCheck, beforePlayingCheck))
                        {
                            builder.Clear();
                            builder.AppendLine(issue.message);
                            builder.Append(issue.stackTrace.ToString());
                            if (issue.contextGiven)
                            {
                                Debug.LogError(builder.ToString(), issue.context);
                            }
                            else
                            {
                                Debug.LogError(builder.ToString());
                            }

                            ableToPlay = false;
                        }

                        if (ableToPlay)
                        {
                            playStateProcess = PlayStateProcess.Ready;
                            EditorApplication.isPlaying = true;
                            new AboutToStartPlaying().Dispatch();
                        }
                    }
                }
                else
                {
                    playStateProcess = PlayStateProcess.Editing;
                }
            }
        }

        private static void OnPlayModeStateChanging(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingEditMode)
            {
                if (playStateProcess != PlayStateProcess.Ready)
                {
                    EditorApplication.isPlaying = false;
                }

                if (playStateProcess == PlayStateProcess.Editing)
                {
                    playStateProcess = PlayStateProcess.PreStarting;
                    EditorPrefs.SetFloat("startTime", (float)EditorApplication.timeSinceStartup + 0.15f);
                }
            }
            else if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                if (playStateProcess == PlayStateProcess.Ready)
                {
                    playStateProcess = PlayStateProcess.Editing;
                }
            }
        }

        public enum PlayStateProcess
        {
            Editing,
            PreStarting,
            Starting,
            Ready
        }
    }
}
