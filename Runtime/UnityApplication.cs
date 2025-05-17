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
    /// Manages a singleton <see cref="VirtualMachine"/> instance and a <see cref="IProgram"/>
    /// initializated with it.
    /// </summary>
    [DefaultExecutionOrder(int.MinValue)]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class UnityApplication
    {
#if UNITY_EDITOR
        internal const string PlayFromStartKey = "playingFromStart";
        private const string UnityEditorApplication = "UnityLibrary.Editor.EditorSystems, UnityLibrary.Editor";
        private static readonly Type? unityEditorType = Type.GetType(UnityEditorApplication);
#endif
        private static readonly VirtualMachine vm = new();
        private static IProgram? program;
        private static Type? editorSystemsType;
        internal static bool started;

        /// <summary>
        /// Always <see cref="true"/> in builds.
        /// <para>
        /// When in editor, only <see cref="true"/> if playing from start with the custom play button.
        /// </para>
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
                if (!started)
                {
                    started = true;
                    Start(UnityApplicationSettings.Singleton);
                }

                return vm;
            }
        }

        static UnityApplication()
        {
            //Start();
#if UNITY_EDITOR
            AppDomain.CurrentDomain.DomainUnload += (sender, args) =>
            {
                AppDomain domain = (AppDomain)sender;
                if (domain.FriendlyName == "Unity Child Domain")
                {
                    //Stop();
                }
            };
#endif
        }

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            Application.quitting += () =>
            {
                Application.quitting -= OnQuitting;
                //Stop();
            }
        }
#endif

        internal static void Start(UnityApplicationSettings settings)
        {
#if UNITY_EDITOR
            //fail if state type is missing, this is needed
            if (settings.ProgramType is null)
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
                    errorBuilder.Append("Program type in unity application settings asset is missing. Available options are:\n");
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

                throw new(errorBuilder.ToString());
            }

            //fail if library editor systems cant be found, should never fail
            if (unityEditorType is null)
            {
                throw new($"Expected editor systems type `{UnityEditorApplication}` not found");
            }
#else
            //fail if state type is missing, this is needed
            if (settings.ProgramType is null)
            {
                throw new($"Program type in unity application settings asset is missing");
            }
#endif

#if UNITY_EDITOR
            unityEditorType.GetMethod("Start", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { vm });
#endif
            program = (IProgram)Activator.CreateInstance(settings.ProgramType);
            program.Start(vm);
            editorSystemsType = AddEditorSystems(vm, settings);
        }

        private static Type? AddEditorSystems(VirtualMachine vm, UnityApplicationSettings settings)
        {
#if UNITY_EDITOR
            Type? editorSystemsType = settings.EditorSystemsType;
            if (editorSystemsType is not null)
            {
                object? editorSystem = null;
                foreach (ConstructorInfo constructor in editorSystemsType.GetConstructors())
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(VirtualMachine))
                    {
                        editorSystem = Activator.CreateInstance(editorSystemsType, new object[] { vm });
                        break;
                    }
                }

                if (editorSystem is null)
                {
                    editorSystem = Activator.CreateInstance(editorSystemsType);
                }

                vm.AddSystem(editorSystem);
            }

            return editorSystemsType;
#else
            return null;
#endif
        }

        private static void RemoveEditorSystems(VirtualMachine vm)
        {
#if UNITY_EDITOR
            if (editorSystemsType is not null)
            {
                if (vm.TryRemoveSystem(editorSystemsType, out object? editorSystem))
                {
                    if (editorSystem is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
#endif
        }

        internal static void Reinitialize(UnityApplicationSettings settings)
        {
            if (started)
            {
                started = false;
                Stop();
            }

            Start(settings);
        }

        internal static void Stop()
        {
            RemoveEditorSystems(vm);
            program?.Finish(vm);
#if UNITY_EDITOR
            unityEditorType?.GetMethod("Stop", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { vm });
#endif
            program = null;
        }
    }
}