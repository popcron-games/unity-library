#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityLibrary.Events;
using Action = System.Action;

namespace UnityLibrary.Systems
{
    /// <summary>
    /// Dispatches events for Unity's update, fixed update, pre update and late update events (gui too).
    /// </summary>
    [DefaultExecutionOrder(int.MinValue + 11)]
    public class UnityEventDispatcher : IDisposable
    {
        public static readonly Type[] eventTypes;

        private static UnityEventDispatcher? instance;
        private static GameObject? guiObject;

        private readonly VirtualMachine vm;
        private readonly List<HashSet<Action>> callbacks = new();

        static UnityEventDispatcher()
        {
            eventTypes = new Type[]
            {
                typeof(UpdateEvent),
                typeof(FixedUpdateEvent),
                typeof(PreUpdateEvent),
                typeof(LateUpdateEvent),
                typeof(GUIEvent),
                typeof(ApplicationStarted),
                typeof(ApplicationStopped)
            };
        }

        public UnityEventDispatcher(VirtualMachine vm)
        {
            instance = this;
            this.vm = vm;
            PlayerLoopSystem playerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopSystem[] systems = playerLoopSystem.subSystemList;
            for (int i = 0; i < systems.Length; i++)
            {
                ref PlayerLoopSystem system = ref systems[i];
                if (system.type == typeof(Update))
                {
                    InsertCallback(ref system, Update);
                }
                else if (system.type == typeof(FixedUpdate))
                {
                    InsertCallback(ref system, FixedUpdate);
                }
                else if (system.type == typeof(PreUpdate))
                {
                    InsertCallback(ref system, PreUpdate);
                }
                else if (system.type == typeof(PostLateUpdate))
                {
                    InsertCallback(ref system, PostLateUpdate);
                }
            }

            playerLoopSystem.subSystemList = systems;
            PlayerLoop.SetPlayerLoop(playerLoopSystem);

            Application.quitting += () =>
            {
                vm.Broadcast(new ApplicationStopped());
                UnityEngine.Object.DestroyImmediate(guiObject);
            };
        }

        public void Dispose()
        {
            for (int i = 0; i < callbacks.Count; i++)
            {
                HashSet<Action> callback = callbacks[i];
                callback.Clear();
            }
        }

        private void InsertCallback(ref PlayerLoopSystem system, Action function)
        {
            List<PlayerLoopSystem> subsystemList = new(system.subSystemList);
            PlayerLoopSystem callbackSystem = default;
            callbackSystem.type = typeof(UnityEventDispatcher);

            HashSet<Action> callbacks = new();
            callbacks.Add(function);

            Action callback = () =>
            {
                foreach (Action callback in callbacks)
                {
                    callback.Invoke();
                }
            };

            callbackSystem.updateDelegate = new(callback);
            subsystemList.Add(callbackSystem);
            system.subSystemList = subsystemList.ToArray();
            this.callbacks.Add(callbacks);
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
            guiObject = CreateGUIObject();
            if (instance is not null)
            {
                instance.vm.Broadcast(new ApplicationStarted());
            }
        }

        private static GameObject CreateGUIObject()
        {
            GameObject obj = new(nameof(GUIEventDispatcher), typeof(GUIEventDispatcher));
            obj.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(obj);
            return obj;
        }

        public class GUIEventDispatcher : MonoBehaviour
        {
            private void OnGUI()
            {
                if (instance is not null)
                {
                    instance.vm.Broadcast(new GUIEvent());
                }
            }
        }
    }
}
