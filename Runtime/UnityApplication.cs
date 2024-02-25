#nullable enable
using System;
using UnityEngine;
using System.Text;
using System.Runtime.CompilerServices;
using Random = System.Random;
using Game;
using System.Reflection;


#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("Popcron.UnityLibrary.Editor")]
namespace UnityLibrary
{
    /// <summary>
    /// Manages a singleton <see cref="VirtualMachine"/> instance initialized with an <see cref="VirtualMachine.IState"/>
    /// (assigned from <see cref="UnityApplicationSettings"/> in your project assets).
    /// Running in the lifetime of the existence of the executing Unity program (either editor or player).
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad, DefaultExecutionOrder(int.MinValue + 10)]
#endif
    public static class UnityApplication
    {
        internal const string PlayFromStartKey = "playingFromStart";
#if UNITY_EDITOR
        private const string AllEditorSystemsTypeName = "UnityLibrary.AllEditorSystems, Popcron.UnityLibrary.Editor";
        private static readonly Type? allEditorSystemsType = Type.GetType(AllEditorSystemsTypeName);
#endif

        private static int id = GenerateID();
        private static bool started;
        private static bool stopped;

        /// <summary>
        /// Always <see cref="true"/> in builds.
        /// In editor, only <see cref="true"/> when playing as if immitating how build would play (custom play button).
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

        /// <summary>
        /// Singleton <see cref="VirtualMachine"/> instance that represents the <see cref="AppDomain"/> that was
        /// loaded by either the Unity editor, or the Unity player.
        /// </summary>
        public static VirtualMachine VM => VirtualMachine.Get(id);

        static UnityApplication()
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
                throw new Exception("Was already asked to be started.");
            }

            started = true;

            VirtualMachine vm;
            UnityApplicationSettings settings = UnityApplicationSettings.Singleton;
#if UNITY_EDITOR
            //fail if state type is missing, this is needed
            if (settings.StateType is null)
            {
                StringBuilder errorBuilder = new();
                errorBuilder.Append("State type in unity application settings asset is not assigned to a found type. Available options are:\n");
                foreach (Type type in TypeCache.GetTypesDerivedFrom<VirtualMachine.IState>())
                {
                    if (!type.IsPublic) continue;

                    errorBuilder.AppendLine(type.AssemblyQualifiedName);
                }

                Debug.LogError(errorBuilder.ToString(), settings);
                vm = new(id, new FallbackFailureState(), settings);
                return;
            }

            //fail if initial data is missing
            if (settings.InitialData == null)
            {
                Debug.LogError("Initial data in unity application settings asset is not assigned.", settings);
                vm = new(id, new FallbackFailureState(), settings);
                return;
            }

            //fail if editor systems cant be found, (extra for editor)
            if (allEditorSystemsType is null)
            {
                Debug.LogErrorFormat("Expected editor systems type {0} not found in this AppDomain.", AllEditorSystemsTypeName);
                vm = new(id, new FallbackFailureState(), settings);
                return;
            }
#endif

            VirtualMachine.IState state = (VirtualMachine.IState)Activator.CreateInstance(settings.StateType);
#if UNITY_EDITOR
            vm = new(id, state, settings.InitialData, allEditorSystemsType);
#else
            vm = new(id, state, settings.InitialData);
#endif
        }

        public static void Reinitialize()
        {
            if (!stopped)
            {
                Stop();
            }

            id = GenerateID();
            Start();
        }

        private static void Stop()
        {
            if (stopped)
            {
                throw new Exception("Was already asked to be stopped.");
            }

            stopped = true;
            VM.Dispose();

#if UNITY_EDITOR
            //remove the editor system that was injected in Start
            if (allEditorSystemsType != null)
            {
                MethodInfo method = allEditorSystemsType.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);
                object editorSystem = VM.GetSystem(allEditorSystemsType);
                method.Invoke(editorSystem, null);
            }
#endif
        }

        private static int GenerateID()
        {
            return new Random().Next(int.MinValue, int.MaxValue);
        }

        /// <summary>
        /// Fallback state to use if there are issues when initializing, and allows <see cref="VM"/> to never be null.
        /// </summary>
        private class FallbackFailureState : VirtualMachine.IState
        {
            void VirtualMachine.IState.Initialize(VirtualMachine vm)
            {
                vm.AddSystem<UnityLibrary.UnityLibrarySystems>();
            }

            void VirtualMachine.IState.Finalize(VirtualMachine vm)
            {
                vm.RemoveSystem<UnityLibrary.UnityLibrarySystems>().Dispose();
            }
        }
    }
}
