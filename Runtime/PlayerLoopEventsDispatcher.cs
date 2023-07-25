#nullable enable
using Popcron.Incomplete;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Popcron
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class PlayerLoopEventDispatcher
    {
        static PlayerLoopEventDispatcher()
        {
            InjectCallbacks();
        }

        private static void InjectCallbacks()
        {
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                ref PlayerLoopSystem rootSystem = ref playerLoop.subSystemList[i];
                Type systemType = rootSystem.type;

                PlayerLoopSystem callbackSystem = new PlayerLoopSystem()
                {
                    type = typeof(PlayerLoopEventDispatcher),
                };

                if (systemType == typeof(TimeUpdate))
                {
                    callbackSystem.updateDelegate = () => OnTimeUpdate();
                }
                else if (systemType == typeof(EarlyUpdate))
                {
                    callbackSystem.updateDelegate = () => OnEarlyUpdate();
                }
                else if (systemType == typeof(FixedUpdate))
                {
                    callbackSystem.updateDelegate = () => OnFixedUpdate();
                }
                else if (systemType == typeof(Update))
                {
                    callbackSystem.updateDelegate = () => OnUpdate();
                }
                else if (systemType == typeof(PreLateUpdate))
                {
                    callbackSystem.updateDelegate = () => OnPreLateUpdate();
                }
                else if (systemType == typeof(PostLateUpdate))
                {
                    callbackSystem.updateDelegate = () => OnPostLateUpdate();
                }

                List<PlayerLoopSystem> subSystemList = new List<PlayerLoopSystem>(rootSystem.subSystemList)
                {
                    callbackSystem
                };

                rootSystem.subSystemList = subSystemList.ToArray();
            }

            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void OnUpdate()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                new UpdateEventInEditMode(Time.deltaTime).Dispatch();
            }
#endif

            new UpdateEvent(Time.deltaTime).Dispatch();
        }

        private static void OnTimeUpdate()
        {
            new TimeUpdateEvent(Time.deltaTime).Dispatch();
        }

        private static void OnEarlyUpdate()
        {
            new EarlyUpdateEvent(Time.deltaTime).Dispatch();
        }

        private static void OnFixedUpdate()
        {
            new FixedUpdateEvent(Time.fixedDeltaTime).Dispatch();
        }

        private static void OnPreLateUpdate()
        {
            new PreLateUpdateEvent().Dispatch();
        }

        private static void OnPostLateUpdate()
        {
            new PostLateUpdateEvent().Dispatch();
        }

        [ExecuteAlways]
        public sealed class GUIEventDispatcher : SealableMonoBehaviour, IListener<UpdateEvent>
        {
            private static GUIEventDispatcher? instance;

            protected override sealed void OnEnable()
            {
                if (instance != null)
                {
                    Die();
                    return;
                }

                instance = this;
            }

            protected override sealed void OnDisable()
            {
                if (instance != null && instance.gameObject == gameObject)
                {
                    instance = null;
                }
            }

            private void OnGUI()
            {
                if (this == instance)
                {
                    new GUIEvent(Event.current).Dispatch();
                }
                else
                {
                    Die();
                }
            }

            private void Die()
            {
                if (this != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        DestroyImmediate(gameObject);
                    }
                }
            }

            void IListener<UpdateEvent>.OnEvent(UpdateEvent e)
            {
                if (this != instance)
                {
                    Die();
                }
            }
        }
    }
}