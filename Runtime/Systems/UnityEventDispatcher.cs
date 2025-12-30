#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityLibrary.Events;

namespace UnityLibrary.Systems
{
    /// <summary>
    /// Dispatches events for Unity's update, fixed update, pre update and late update events (gui too).
    /// </summary>
    [DefaultExecutionOrder(int.MinValue + 11)]
    public class UnityEventDispatcher : SystemBase
    {
        public static readonly Type[] eventTypes;

        private static UnityEventDispatcher? instance;

        private readonly List<SubsystemCallback> callbacks = new();

        static UnityEventDispatcher()
        {
            eventTypes = new Type[]
            {
                typeof(ApplicationStarted),
                typeof(ApplicationStopped),
                typeof(FixedUpdateEvent),
                typeof(LateUpdateEvent),
                typeof(PreUpdateEvent),
                typeof(UpdateEvent),
            };
        }

        public UnityEventDispatcher(VirtualMachine vm) : base(vm)
        {
            ThrowIfAlreadyInitialized();
            instance = this;
            RegisterCallbacks();
        }

        public override void Dispose()
        {
            UnregisterCallbacks();
            instance = null;
        }

        private void RegisterCallbacks()
        {
            PlayerLoopSystem playerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopSystem[] systems = playerLoopSystem.subSystemList;
            for (int i = 0; i < systems.Length; i++)
            {
                ref PlayerLoopSystem system = ref systems[i];
                if (system.type == typeof(Update))
                {
                    int index = system.AddCallback(Update);
                    callbacks.Add(new SubsystemCallback(system.type, index));
                }
                else if (system.type == typeof(FixedUpdate))
                {
                    int index = system.AddCallback(FixedUpdate);
                    callbacks.Add(new SubsystemCallback(system.type, index));
                }
                else if (system.type == typeof(PreUpdate))
                {
                    int index = system.AddCallback(PreUpdate);
                    callbacks.Add(new SubsystemCallback(system.type, index));
                }
                else if (system.type == typeof(PostLateUpdate))
                {
                    int index = system.AddCallback(PostLateUpdate);
                    callbacks.Add(new SubsystemCallback(system.type, index));
                }
            }

            playerLoopSystem.subSystemList = systems;
            PlayerLoop.SetPlayerLoop(playerLoopSystem);
        }

        private void UnregisterCallbacks()
        {
            PlayerLoopSystem playerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopSystem[] systems = playerLoopSystem.subSystemList;
            foreach (SubsystemCallback callback in callbacks)
            {
                for (int i = 0; i < systems.Length; i++)
                {
                    ref PlayerLoopSystem system = ref systems[i];
                    if (system.type == callback.systemType)
                    {
                        system.RemoveCallback(callback.index);
                        break;
                    }
                }
            }

            playerLoopSystem.subSystemList = systems;
            PlayerLoop.SetPlayerLoop(playerLoopSystem);
            callbacks.Clear();
        }

        private void Update()
        {
            vm.Broadcast(new UpdateEvent(Time.deltaTime));
        }

        private void FixedUpdate()
        {
            vm.Broadcast(new FixedUpdateEvent(Time.fixedDeltaTime));
        }

        private void PreUpdate()
        {
            vm.Broadcast(new PreUpdateEvent(Time.deltaTime));
        }

        private void PostLateUpdate()
        {
            vm.Broadcast(new LateUpdateEvent(Time.deltaTime));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ApplicationStarted()
        {
            if (instance is not null)
            {
                instance.vm.Broadcast(new ApplicationStarted());
                Application.quitting += () =>
                {
                    instance?.vm.Broadcast(new ApplicationStopped());
                };
            }
        }

        [Conditional("DEBUG")]
        private static void ThrowIfAlreadyInitialized()
        {
            if (instance is not null)
            {
                throw new InvalidOperationException($"An instance of {nameof(UnityEventDispatcher)} is already initialized");
            }
        }

        private readonly struct SubsystemCallback
        {
            public readonly Type systemType;
            public readonly int index;

            public SubsystemCallback(Type systemType, int index)
            {
                this.systemType = systemType;
                this.index = index;
            }
        }
    }
}
