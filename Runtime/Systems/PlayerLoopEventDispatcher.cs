#nullable enable
using Game;
using Game.Events;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private static GameObject? guiObject;

        private readonly VirtualMachine vm;
        private readonly HashSet<HashSet<Action>> callbacks = new();

        public UnityEventDispatcher(VirtualMachine vm)
        {
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
                    InsertCallback(ref system, PostLateUpdateEvent);
                }
            }

            playerLoopSystem.subSystemList = systems;
            PlayerLoop.SetPlayerLoop(playerLoopSystem);

            Application.quitting += () =>
            {
                ApplicationStopped ev = new();
                vm.Broadcast(ref ev);
                UnityEngine.Object.DestroyImmediate(guiObject);
            };
        }

        public void Dispose()
        {
            for (int i = 0; i < callbacks.Count; i++)
            {
                HashSet<Action> callback = callbacks.ElementAt(i);
                callback.Clear();
            }
        }

        private void InsertCallback(ref PlayerLoopSystem system, Action function)
        {
            List<PlayerLoopSystem> subsystemList = system.subSystemList.ToList();
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
            float delta = Time.deltaTime;
            UpdateEvent ev = new(delta);
            vm.Broadcast(ref ev);
        }

        private void FixedUpdate()
        {
            float delta = Time.fixedDeltaTime;
            FixedUpdateEvent ev = new(delta);
            vm.Broadcast(ref ev);
        }

        private void PreUpdate()
        {
            float delta = Time.deltaTime;
            PreUpdateEvent ev = new(delta);
            vm.Broadcast(ref ev);
        }

        private void PostLateUpdateEvent()
        {
            float delta = Time.deltaTime;
            LateUpdateEvent ev = new(delta);
            vm.Broadcast(ref ev);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ApplicationStarted()
        {
            guiObject = CreateGUIObject();
            ApplicationStarted ev = new();
            UnityApplication.VM.Broadcast(ref ev);
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
                GUIEvent ev = new();
                UnityApplication.VM.Broadcast(ref ev);
            }
        }
    }
}
