#nullable enable
using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

[assembly: InternalsVisibleTo("UnityLibrary.Editor")]
namespace UnityLibrary
{
    /// <summary>
    /// Manages a singleton <see cref="VirtualMachine"/> instance and a <see cref="IProgram"/>
    /// initializated with it.
    /// </summary>
    [DefaultExecutionOrder(int.MaxValue)]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class UnityApplication
    {
        private static readonly VirtualMachine vm = new();
        private static Type? editorSystemsType;
        private static Type? runtimeSystemsType;
        private static bool? addBuiltInSystems;
        private static bool started;

        /// <summary>
        /// Singleton <see cref="VirtualMachine"/> instance that represents the <see cref="AppDomain"/> that was
        /// loaded by either the Unity editor, or the Unity player.
        /// </summary>
        public static VirtualMachine VM
        {
            get
            {
                if (!started)
                {
                    started = true;
#if UNITY_EDITOR
                    UnityApplicationSettings? settings = UnityApplicationSettings.FindSingleton();
                    if (settings == null)
                    {
                        throw new($"No {nameof(UnityApplicationSettings)} asset found in project, please create one");
                    }
#else
                    settings = UnityApplicationSettings.Singleton;
#endif

                    Start(settings);
                }

                return vm;
            }
        }

        static UnityApplication()
        {
            UnityApplicationSettings? settings = UnityApplicationSettings.FindSingleton();
            if (settings == null)
            {
                Debug.LogError($"No {nameof(UnityApplicationSettings)} asset found in project, please create one");
            }
        }

        public static bool TryStart(UnityApplicationSettings settings)
        {
            if (!started)
            {
                started = true;
                Start(settings);
                return true;
            }

            return false;
        }

        public static bool TryStop()
        {
            if (started)
            {
                started = false;
                Stop();
                return true;
            }

            return false;
        }

        private static void Start(UnityApplicationSettings settings)
        {
            // fail if program type is missing, this is needed
            runtimeSystemsType = settings.RuntimeSystemsType;
            if (runtimeSystemsType is null)
            {
                throw new InvalidOperationException($"Runtime systems type in {nameof(UnityApplicationSettings)} asset is missing");
            }

#if UNITY_EDITOR
            editorSystemsType = settings.EditorSystemsType;
#endif

            addBuiltInSystems = settings.AddBuiltInSystems;
            if (addBuiltInSystems == true)
            {
                vm.AddSystem(new UnityLibrarySystems(vm));
            }

            AddSystem(vm, editorSystemsType);
            AddSystem(vm, runtimeSystemsType);
        }

        private static void Stop()
        {
            RemoveSystem(vm, runtimeSystemsType);
            RemoveSystem(vm, editorSystemsType);
            if (addBuiltInSystems == true)
            {
                addBuiltInSystems = null;
                RemoveSystem(vm, typeof(UnityLibrarySystems));
            }

            runtimeSystemsType = null;
            editorSystemsType = null;
        }

        public static void TryReinitialize(UnityApplicationSettings settings)
        {
            TryStop();
            TryStart(settings);
        }

        private static void AddSystem(VirtualMachine vm, Type? systemType)
        {
            if (systemType is not null)
            {
                // construct with vm parameter if possible, otherwise use default constructor
                object? editorSystem = null;
                foreach (ConstructorInfo constructor in systemType.GetConstructors())
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(VirtualMachine))
                    {
                        editorSystem = Activator.CreateInstance(systemType, new object[] { vm });
                        break;
                    }
                }

                if (editorSystem is null)
                {
                    editorSystem = Activator.CreateInstance(systemType);
                }

                vm.AddSystem(editorSystem);
            }
        }

        private static void RemoveSystem(VirtualMachine vm, Type? systemType)
        {
            if (systemType is not null)
            {
                if (vm.TryRemoveSystem(systemType, out object? editorSystem))
                {
                    if (editorSystem is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}