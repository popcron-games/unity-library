#nullable enable
using System;
using UnityEngine;

namespace UnityLibrary.Drawers
{
    /// <summary>
    /// Automatically created for drawing an inspector GUI for member values that aren't serialized by Unity.
    /// </summary>
    public interface IMemberDrawer
    {
        Type ValueType { get; }

        object? OnGUI(Rect position, object? value, GUIContent label);
        float GetPropertyHeight(object? value, GUIContent label);
    }
}
