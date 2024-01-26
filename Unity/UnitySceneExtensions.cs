#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Library.Unity
{
    public static class UnitySceneExtensions
    {
        /// <summary>
        /// Retrieves all game objects in the scene, not just the root.
        /// </summary>
        public static List<GameObject> GetAllGameObjects(this Scene scene)
        {
            Stack<Transform> stack = new();
            foreach (GameObject gameObject in scene.GetRootGameObjects())
            {
                stack.Push(gameObject.transform);
            }

            List<Transform> all = new();
            while (stack.Count > 0)
            {
                Transform transform = stack.Pop();
                all.Add(transform);
                foreach (Transform child in transform)
                {
                    stack.Push(child);
                }
            }

            List<GameObject> gameObjects = new();
            for (int i = all.Count - 1; i >= 0; i--)
            {
                gameObjects.Add(all[i].gameObject);
            }

            return gameObjects;
        }
    }
}
