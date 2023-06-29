#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Popcron
{
    public static class SceneExtensions
    {
        public static void GetAllGameObjects(this Scene scene, IList<GameObject> toAdd)
        {
            Stack<GameObject> stack = new Stack<GameObject>(scene.GetRootGameObjects());
            while (stack.Count > 0)
            {
                GameObject current = stack.Pop();
                toAdd.Add(current);
                foreach (Transform child in current.transform)
                {
                    stack.Push(child.gameObject);
                }
            }
        }

        public static int GetAllGameObjects(this Scene scene, GameObject[] toFill)
        {
            int length = 0;
            Stack<GameObject> stack = new Stack<GameObject>(scene.GetRootGameObjects());
            while (stack.Count > 0)
            {
                GameObject current = stack.Pop();
                toFill[length] = current;
                length++;
                foreach (Transform child in current.transform)
                {
                    stack.Push(child.gameObject);
                }
            }

            return length;
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
