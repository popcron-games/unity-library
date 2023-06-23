#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Popcron
{
    public static class SceneExtensions
    {
        public static IReadOnlyList<GameObject> GetAllGameObjects(this Scene scene)
        {
            Stack<GameObject> stack = new Stack<GameObject>(scene.GetRootGameObjects());
            RecycledList<GameObject> all = new RecycledList<GameObject>(stack.Count);
            while (stack.Count > 0)
            {
                GameObject current = stack.Pop();
                all.Add(current);
                foreach (Transform child in current.transform)
                {
                    stack.Push(child.gameObject);
                }
            }

            return all;
        }

        /// <summary>
        /// Empties the scene of all objects.
        /// </summary>
        public static void Clear(this Scene scene)
        {
            IReadOnlyList<GameObject> all = GetAllGameObjects(scene);
            for (int i = all.Count - 1; i >= 0; i--)
            {
                GameObject item = all[i];
                GameObject.DestroyImmediate(item);
            }
        }

        public static void ForEachGameObject(this Scene scene, Action<GameObject> action)
        {
            Stack<GameObject> stack = new Stack<GameObject>(scene.GetRootGameObjects());
            while (stack.Count > 0)
            {
                GameObject current = stack.Pop();
                action.Invoke(current);
                foreach (Transform child in current.transform)
                {
                    stack.Push(child.gameObject);
                }
            }
        }
    }
}
