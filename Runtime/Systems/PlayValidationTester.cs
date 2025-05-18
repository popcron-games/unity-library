#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityLibrary.Events;

namespace UnityLibrary.Systems
{
    public class PlayValidationTester
    {
        private static readonly List<GameObject> gameObjects = new();
        private static readonly List<Component> components = new();

        private void CollectOpenSceneValidators(List<IListener<Validate>> listeners)
        {
            for (int i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                CollectValidators(scene, listeners);
            }
        }

        private void CollectValidators(Scene scene, List<IListener<Validate>> listeners)
        {
            gameObjects.Clear();
            scene.GetAllGameObjects(gameObjects);
            for (int i = 0; i < gameObjects.Count; i++)
            {
                CollectValidators(gameObjects[i], listeners);
            }
        }

        private void CollectValidators(GameObject gameObject, List<IListener<Validate>> listeners)
        {
            components.Clear();
            gameObject.GetComponents(components);
            for (int i = 0; i < components.Count; i++)
            {
                CollectValidators(components[i], listeners);
            }
        }

        private void CollectValidators(VirtualMachine vm, List<IListener<Validate>> listeners)
        {
            IReadOnlyList<object> systems = vm.Systems;
            for (int i = 0; i < systems.Count; i++)
            {
                CollectValidators(systems[i], listeners);
            }
        }

        private void CollectValidators(object? value, List<IListener<Validate>> listeners)
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

        private bool Test(VirtualMachine vm, IReadOnlyList<IListener<Validate>> validators)
        {
            Validate e = default;
            for (int i = 0; i < validators.Count; i++)
            {
                validators[i].TryValidate(vm, ref e);
            }

            return !e.failed;
        }

        /// <summary>
        /// Tests the ability to start playing from current open scen.
        /// </summary>
        public bool TestOpenedScenes(VirtualMachine vm)
        {
            List<IListener<Validate>> listeners = new();
            CollectOpenSceneValidators(listeners);
            return Test(vm, listeners);
        }

        /// <summary>
        /// Tests the ability to start playing the game, as if this was a build.
        /// </summary>
        public bool TestStarting(VirtualMachine vm)
        {
            List<IListener<Validate>> listeners = new();
            listeners.Add(UnityApplicationSettings.Singleton);
            CollectValidators(vm, listeners);
            return Test(vm, listeners);
        }
    }
}