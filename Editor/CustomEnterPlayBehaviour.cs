﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Popcron.Lib
{
    [InitializeOnLoad]
    public static class CustomEnterPlayBehaviour
    {
        private static readonly StringBuilder builder = new StringBuilder();
        private static PlayStateProcess playStateProcess;

        static CustomEnterPlayBehaviour()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanging;
            EditorApplication.delayCall += Initialize;
            EditorApplication.update += OnUpdate;
        }

        private static void Initialize()
        {
            new EditorApplicationDelayCall().Dispatch();
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
                                    component.SendMessage("OnValidate", null, SendMessageOptions.DontRequireReceiver);
                                    if (component is IListener<PlayabilityCheck> checkListener)
                                    {
                                        checkListener.OnEvent(beforePlayingCheck);
                                    }
                                }
                            }
                        });

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
                    ReadOnlySpan<object> all = Everything.All;
                    HashSet<Object> toRemove = new HashSet<Object>();
                    for (int i = 0; i < all.Length; i++)
                    {
                        if (all[i] is Object unityObject && unityObject == null)
                        {
                            toRemove.Add(unityObject);
                        }
                    }

                    foreach (var item in toRemove)
                    {
                        Everything.Remove(item);
                    }
                }
            }
        }

        private static void OnPlayModeStateChanging(PlayModeStateChange value)
        {
            new PlayModeStateChanged((int)value).Dispatch();
            if (value == PlayModeStateChange.ExitingEditMode)
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
            else if (value == PlayModeStateChange.EnteredEditMode)
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
