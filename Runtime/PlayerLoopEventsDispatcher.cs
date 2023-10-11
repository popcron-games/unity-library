#nullable enable
using Popcron.Sealable;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Popcron.Lib
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class PlayerLoopEventDispatcher
    {
        private static bool injected;

        static PlayerLoopEventDispatcher()
        {
            InjectCallbacks();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitialize()
        {
            InjectCallbacks();
        }

        private static void InjectCallbacks()
        {
            if (injected) return;

            injected = true;
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                ref PlayerLoopSystem rootSystem = ref playerLoop.subSystemList[i];
                Type systemType = rootSystem.type;

                PlayerLoopSystem newCallbackSystem = new PlayerLoopSystem()
                {
                    type = typeof(PlayerLoopEventDispatcher),
                };

                if (systemType == typeof(TimeUpdate))
                {
                    newCallbackSystem.updateDelegate = () => OnTimeUpdate();
                }
                else if (systemType == typeof(EarlyUpdate))
                {
                    newCallbackSystem.updateDelegate = () => OnEarlyUpdate();
                }
                else if (systemType == typeof(FixedUpdate))
                {
                    newCallbackSystem.updateDelegate = () => OnFixedUpdate();
                }
                else if (systemType == typeof(Update))
                {
                    newCallbackSystem.updateDelegate = () => OnUpdate();
                }
                else if (systemType == typeof(PreLateUpdate))
                {
                    newCallbackSystem.updateDelegate = () => OnPreLateUpdate();
                }
                else if (systemType == typeof(PostLateUpdate))
                {
                    newCallbackSystem.updateDelegate = () => OnPostLateUpdate();
                }

                List<PlayerLoopSystem> subSystemList = new List<PlayerLoopSystem>(rootSystem.subSystemList)
                {
                    newCallbackSystem
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