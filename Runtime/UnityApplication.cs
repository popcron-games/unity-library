#nullable enable
using System;
using UnityEngine;
using System.Text;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("UnityLibrary.Editor")]
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
        private const string UnityEditorApplication = "UnityLibrary.Editor.EditorSystems, UnityLibrary.Editor";
        private static readonly Type? unityEditorType = Type.GetType(UnityEditorApplication);
#endif

        private static VirtualMachine? vm;
        private static IProgram? program;
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
        public static VirtualMachine VM
        {
            get
            {
                if (vm is null)
                {
                    throw new Exception("Unable to access virtual machine because of initialization errors");
                }

                return vm;
            }
        }

        static UnityApplication()
        {
            (vm, program) = Start();
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

        private static (VirtualMachine? vm, IProgram? program) Start()
        {
            if (started)
            {
                throw new Exception("Was already asked to be started");
            }

            started = true;

            UnityApplicationSettings settings;
            settings = UnityApplicationSettings.Singleton;

#if UNITY_EDITOR
            //fail if state type is missing, this is needed
            if (settings.StateType is null)
            {
                List<Type> availableProgramTypes = new();
                foreach (Type type in TypeCache.GetTypesDerivedFrom<IProgram>())
                {
                    if (type.IsPublic)
                    {
                        availableProgramTypes.Add(type);
                    }
                }

                StringBuilder errorBuilder = new();
                if (availableProgramTypes.Count > 0)
                {
                    errorBuilder.Append("Program type in unity application settings asset is not assigned to a found type. Available options are:\n");
                    foreach (Type type in availableProgramTypes)
                    {
                        errorBuilder.Append(type.AssemblyQualifiedName);
                        errorBuilder.Append('\n');
                    }
                }
                else
                {
                    errorBuilder.Append($"No types found that implement {nameof(IProgram)}. Please create a struct type that implements it, and assign it to the unity application settings asset");
                }

                Exception exception = new(errorBuilder.ToString());
                Debug.LogException(exception, settings);
                return default;
            }

            //fail if initial data is missing
            IInitialData? initialData = settings.InitialData;
            if (initialData is null)
            {
                Debug.LogWarning("Initial data in unity application settings asset is not assigned", settings);
                initialData = new EmptyInitialData();
            }

            //fail if editor systems cant be found, (extra for editor)
            if (unityEditorType is null)
            {
                Debug.LogError($"Expected editor systems type `{UnityEditorApplication}` not found");
                return default;
            }
#endif

            VirtualMachine vm = new(initialData);
#if UNITY_EDITOR
            unityEditorType.GetMethod("Start", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { vm });
#endif
            IProgram program = (IProgram)Activator.CreateInstance(settings.StateType);
            program.Start(vm);
            return (vm, program);
        }

        public static void Reinitialize()
        {
            if (started && vm is null)
            {
                started = false;
            }

            if (!stopped)
            {
                Stop();
            }

            (vm, program) = Start();
        }

        private static void Stop()
        {
            if (stopped)
            {
                throw new Exception("Was already asked to be stopped");
            }

            stopped = true;
            if (vm is not null)
            {
                program?.Finish(vm);
#if UNITY_EDITOR
                unityEditorType?.GetMethod("Stop", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { vm });
#endif
                vm.Dispose();
                program = null;
            }
        }
    }
}
