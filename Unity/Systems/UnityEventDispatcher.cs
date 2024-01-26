#nullable enable
using Library.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Action = System.Action;

namespace Library.Unity
{
    /// <summary>
    /// Dispatches events for Unity's update, fixed update, pre update and late update events (gui too).
    /// </summary>
    [DefaultExecutionOrder(int.MinValue + 11)]
    public class UnityEventDispatcher : IDisposable
    {
        private static GameObject? guiObject;
        private readonly HashSet<HashSet<Action>> callbacks = new();

        public UnityEventDispatcher()
        {
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
                Host.VirtualMachine.Broadcast(new ApplicationStopped());
                GameObject.DestroyImmediate(guiObject);
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
            Host.VirtualMachine.Broadcast(new UpdateEvent(delta));
        }

        private void FixedUpdate()
        {
            float delta = Time.fixedDeltaTime;
            Host.VirtualMachine.Broadcast(new FixedUpdateEvent(delta));
        }

        private void PreUpdate()
        {
            float delta = Time.deltaTime;
            Host.VirtualMachine.Broadcast(new PreUpdateEvent(delta));
        }

        private void PostLateUpdateEvent()
        {
            float delta = Time.deltaTime;
            Host.VirtualMachine.Broadcast(new LateUpdateEvent(delta));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ApplicationStarted()
        {
            guiObject = CreateGUIObject();
            Host.VirtualMachine.Broadcast(new ApplicationStarted());
        }

        private static GameObject CreateGUIObject()
        {
            GameObject obj = new GameObject("GUI", typeof(GUIEventDispatcher));
            obj.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(obj);
            return obj;
        }

        public class GUIEventDispatcher : MonoBehaviour
        {
            private void OnGUI()
            {
                Event e = Event.current;
                Host.VirtualMachine.Broadcast(new GUIEvent(e));
            }
        }
    }
}
