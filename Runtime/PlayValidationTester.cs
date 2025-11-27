#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityLibrary.Events;

namespace UnityLibrary.Systems
{
    public static class PlayValidationTester
    {
        private static readonly List<GameObject> gameObjects = new();
        private static readonly List<Component> components = new();

        private static void CollectOpenSceneValidators(List<IListener<Validate>> listeners)
        {
            for (int i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                CollectValidators(scene, listeners);
            }
        }

        private static void CollectValidators(Scene scene, List<IListener<Validate>> listeners)
        {
            gameObjects.Clear();
            scene.GetAllGameObjects(gameObjects);
            for (int i = 0; i < gameObjects.Count; i++)
            {
                CollectValidators(gameObjects[i], listeners);
            }
        }

        private static void CollectValidators(GameObject gameObject, List<IListener<Validate>> listeners)
        {
            components.Clear();
            gameObject.GetComponents(components);
            for (int i = 0; i < components.Count; i++)
            {
                CollectValidators(components[i], listeners);
            }
        }

        private static void CollectValidators(VirtualMachine vm, List<IListener<Validate>> listeners)
        {
            IReadOnlyList<object> systems = vm.Systems;
            for (int i = 0; i < systems.Count; i++)
            {
                CollectValidators(systems[i], listeners);
            }
        }

        private static void CollectValidators(object? value, List<IListener<Validate>> listeners)
        {
            if (value is IListener<Validate> valueIsValidator)
            {
                if (!listeners.Contains(valueIsValidator))
                {
                    listeners.Add(valueIsValidator);
                }
            }

            if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    CollectValidators(item, listeners);
                }
            }
        }

        private static bool Test(VirtualMachine vm, IReadOnlyList<IListener<Validate>> validators)
        {
            // check if there are custom mono behaviours but unity objects isnt added
            if (!vm.ContainsSystem<UnityObjects>())
            {
                for (int i = 0; i < validators.Count; i++)
                {
                    if (validators[i] is CustomMonoBehaviour)
                    {
                        UnityApplicationSettings settings = UnityApplicationSettings.Singleton;
                        if (!settings.AddBuiltInSystems)
                        {
                            Debug.LogError($"There are {nameof(CustomMonoBehaviour)} components that depend on built-in systems added, but adding them is disabled in the singleton {nameof(UnityApplicationSettings)}. Enable it, or manually add {nameof(UnityObjects)} to the virtual machine", settings);
                            return false;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            // todo: check if there are systems added that depend on unity events, but unity event dispatcher isnt added
            if (!vm.ContainsSystem<UnityEventDispatcher>())
            {
                for (int i = 0; i < validators.Count; i++)
                {
                    Type validatorType = validators[i].GetType();
                    Type[] interfaces = validatorType.GetInterfaces();
                    foreach (Type interfaceType in interfaces)
                    {
                        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IListener<>))
                        {
                            Type eventType = interfaceType.GetGenericArguments()[0];
                            if (Array.IndexOf(UnityEventDispatcher.eventTypes, eventType) != -1)
                            {
                                UnityApplicationSettings settings = UnityApplicationSettings.Singleton;
                                if (!settings.AddBuiltInSystems)
                                {
                                    Debug.LogError($"There are event listeners that depend on built-in systems added, but adding them is disabled in the singleton {nameof(UnityApplicationSettings)}. Enable it, or manually add {nameof(UnityEventDispatcher)} to the virtual machine", settings);
                                    return false;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            Validate e = default;
            for (int i = 0; i < validators.Count; i++)
            {
                validators[i].Validate(vm, ref e);
            }

            return !e.failed;
        }

        /// <summary>
        /// Tests the ability to start playing from currently open scenes.
        /// </summary>
        public static bool ValidateComponents(VirtualMachine vm)
        {
            List<IListener<Validate>> listeners = new();
            CollectOpenSceneValidators(listeners);
            return Test(vm, listeners);
        }

        /// <summary>
        /// Tests the ability to start playing the game, as if this was a build.
        /// </summary>
        public static bool ValidateStarting(VirtualMachine vm)
        {
#if UNITY_EDITOR
            UnityApplicationSettings? settings = UnityApplicationSettings.FindSingleton();
            if (settings == null)
            {
                throw new($"Cannot validate starting the game because {nameof(UnityApplicationSettings)} singleton is missing, please create one.");
            }
#else
            UnityApplicationSettings? settings = UnityApplicationSettings.Singleton;
#endif

            List<IListener<Validate>> listeners = new();
            listeners.Add(settings);
            if (UnityApplication.program is IProgram program)
            {
                CollectValidators(program, listeners);
            }

            CollectValidators(vm, listeners);
            return Test(vm, listeners);
        }
    }
}