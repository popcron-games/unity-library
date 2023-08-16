#nullable enable
using Popcron.Lib;
using System;
using UnityEngine;
using static Popcron.Lib.PlayerLoopEventDispatcher;

namespace Popcron
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class AutoExecuteCommands
    {
        private static bool isInitialized;
        private static bool domainReloadRegistered;

        static AutoExecuteCommands()
        {
            TryToEnable();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void InitializeBeforeSplashScreen()
        {
            new RuntimeInitializeOnLoad(RuntimeInitializeLoadType.BeforeSplashScreen).Dispatch();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeAfterSceneLoad()
        {
            new RuntimeInitializeOnLoad(RuntimeInitializeLoadType.AfterSceneLoad).Dispatch();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeSubsystemRegistration()
        {
            new RuntimeInitializeOnLoad(RuntimeInitializeLoadType.SubsystemRegistration).Dispatch();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            new RuntimeInitializeOnLoad(RuntimeInitializeLoadType.BeforeSceneLoad).Dispatch();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void DoIt()
        {
            new RuntimeInitializeOnLoad(RuntimeInitializeLoadType.AfterAssembliesLoaded).Dispatch();
            Application.quitting += OnQuitting;
            TryToEnable();
        }

        private static void OnQuitting()
        {
            new ApplicationQuitting().Dispatch();
            Application.quitting -= OnQuitting;
            Disable();
        }

        private static void TryToEnable()
        {
            if (!domainReloadRegistered)
            {
                domainReloadRegistered = true;
                AppDomain.CurrentDomain.DomainUnload += (_, __) =>
                {
                    domainReloadRegistered = false;
                    if (isInitialized)
                    {
                        Disable();
                    }
                };

                AppDomain.CurrentDomain.ProcessExit += (_, __) => Disable();
            }

            Enabled();
        }

        private static void Enabled()
        {
            //static on enable like an always present mono behaviour
            GameObject go = new GameObject(nameof(GUIEventDispatcher), typeof(GUIEventDispatcher));
            go.hideFlags |= HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
        }

        private static void Disable()
        {
            //static on disable like an always present mono behaviour
        }
    }
}
