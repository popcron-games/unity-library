#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UnitySceneExtensions
{
    private static readonly Stack<Transform> stack = new();
    private static readonly List<Transform> all = new();

    /// <summary>
    /// Retrieves all game objects in the scene, not just the root.
    /// </summary>
    public static IEnumerable<GameObject> GetAllGameObjects(this Scene scene)
    {
        foreach (GameObject gameObject in scene.GetRootGameObjects())
        {
            stack.Push(gameObject.transform);
        }

        all.Clear();
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
            yield return all[i].gameObject;
        }
    }
}