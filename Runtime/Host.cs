#nullable enable
using System;
using UnityEngine;
using System.Text;
using System.Runtime.CompilerServices;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("Library.Editor")]
namespace Library.Unity
{
    /// <summary>
    /// Acts as a host for a <see cref="VirtualMachine"/> running under the lifecycle of the <see cref="AppDomain.CurrentDomain"/>.
    /// Representing either the Unity Editor or the Unity Player.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad, DefaultExecutionOrder(int.MinValue + 10)]
#endif
    public static class Host
    {
        internal const string PlayFromStartKey = "playingFromStart";

        private static bool started;
        private static bool stopped;
        private static bool starting;
        private static VirtualMachine? vm;

        /// <summary>
        /// Always true in builds, and only true in editor when playing using the custom play button.
        /// </summary>
        public static bool IsUnityPlayer
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetBool(PlayFromStartKey);
#else
                return true;
#endif
            }
        }

        public static VirtualMachine VirtualMachine
        {
            get
            {
                if (vm is null)
                {
#if UNITY_EDITOR
                    if (starting)
                    {
                        throw new Exception("Virtual machine is not initialized yet.");
                    }
                    else
                    {
                        throw new Exception("Virtual machine has been disposed.");
                    }
#endif
                }

                return vm;
            }
        }

        static Host()
        {
            Start();
#if UNITY_EDITOR
            AppDomain.CurrentDomain.DomainUnload += (sender, args) =>
            {
                AppDomain domain = (AppDomain)sender;
                if (domain.FriendlyName == "Unity Child Domain")
                {
                    Stop();
                }
            };
#endif
        }

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            Application.quitting += OnQuitting;
        }

        private static void OnQuitting()
        {
            Application.quitting -= OnQuitting;
            Stop();
        }
#endif

        private static void Start()
        {
            if (started)
            {
                throw new Exception("Host is already started.");
            }

            started = true;
            starting = true;

            int id = GenerateID();
            HostSettings settings = HostSettings.Singleton;
#if UNITY_EDITOR
            if (settings.StateType is null)
            {
                StringBuilder errorBuilder = new();
                errorBuilder.Append("State type is not set in HostSettings asset. Available options are:\n");
                foreach (Type type in TypeCache.GetTypesDerivedFrom<IState>())
                {
                    errorBuilder.AppendLine(type.AssemblyQualifiedName);
                }

                Debug.LogError(errorBuilder.ToString(), settings);
                vm = new(id, new DefaultState());
                vm.Initialize();
                return;
            }
#endif

            starting = false;

#if UNITY_EDITOR
            Type? editorSystemsType = Type.GetType("Library.EditorSystems, Library.Editor");
            if (editorSystemsType is null)
            {
                Debug.LogErrorFormat("Editor systems type not found in AppDomain.");
                vm = new(id, new DefaultState());
                vm.Initialize();
                return;
            }
#endif

            IState state = (IState)Activator.CreateInstance(settings.StateType);
            vm = new VirtualMachine(id, state);
#if UNITY_EDITOR
            vm.AddSystem(Activator.CreateInstance(editorSystemsType, vm));
#endif
            vm.Initialize();
        }

        private static void Stop()
        {
            if (stopped)
            {
                throw new Exception("Host is already stopped.");
            }

            stopped = true;
            if (vm is null)
            {
                throw new Exception("Virtual machine is not initialized to dispose.");
            }

            try
            {
                vm.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            vm = null;
        }

        private static int GenerateID()
        {
            return Random.Range(int.MinValue, int.MaxValue);
        }

        public class DefaultState : IState
        {
            void IState.Initialize(VirtualMachine vm)
            {
                vm.AddSystem(new UnitySystems(vm));
            }

            void IState.Finalize(VirtualMachine vm)
            {
                vm.RemoveSystem<UnitySystems>().Dispose();
            }
        }
    }
}
