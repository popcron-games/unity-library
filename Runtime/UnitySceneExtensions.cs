#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class UnitySceneExtensions
{
    private static readonly Stack<Transform> stack = new();
    private static readonly List<GameObject> all = new();

    /// <summary>
    /// Retrieves all game objects in the scene, not just the root.
    /// </summary>
    /// <returns>A singleton shared list of all game objects in reverse order.</returns>
    public static IReadOnlyList<GameObject> GetAllGameObjects(this Scene scene)
    {
        foreach (GameObject gameObject in scene.GetRootGameObjects())
        {
            stack.Push(gameObject.transform);
        }

        all.Clear();
        while (stack.Count > 0)
        {
            Transform transform = stack.Pop();
            all.Add(transform.gameObject);
            foreach (Transform child in transform)
            {
                stack.Push(child);
            }
        }

        return all;
    }

    /// <summary>
    /// Fills the given <paramref name="gameObjects"/> list with all game objects in
    /// the scene, not just the root.
    /// <para>
    /// The order is reverse of the hierarchy.
    /// </para>
    /// </summary>
    public static void GetAllGameObjects(this Scene scene, List<GameObject> gameObjects)
    {
        foreach (GameObject gameObject in scene.GetRootGameObjects())
        {
            stack.Push(gameObject.transform);
        }

        while (stack.Count > 0)
        {
            Transform transform = stack.Pop();
            gameObjects.Add(transform.gameObject);
            foreach (Transform child in transform)
            {
                stack.Push(child);
            }
        }
    }
}