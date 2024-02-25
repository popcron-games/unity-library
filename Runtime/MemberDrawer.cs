#nullable enable
using System;
using UnityEngine;

namespace UnityLibrary.Unity
{
    /// <summary>
    /// A base type for automatically created for drawing an inspector GUI for <typeparamref name="T"/> values, that aren't serialized by Unity.
    /// </summary>
    public abstract class MemberDrawer<T> : IMemberDrawer
    {
        Type IMemberDrawer.ValueType => typeof(T);

        public abstract T? OnGUI(Rect position, T? value, GUIContent label);

        public virtual float GetPropertyHeight(T? value, GUIContent label)
        {
            return 20;
        }

        object? IMemberDrawer.OnGUI(Rect position, object? value, GUIContent label)
        {
            if (value is not null)
            {
                if (value is T t)
                {
                    return OnGUI(position, t, label);
                }
                else
                {
                    throw new InvalidOperationException($"Expected value to be of type {typeof(T).Name} but was {value.GetType().Name}");
                }
            }
            else
            {
                return OnGUI(position, default, label) as object;
            }
        }

        float IMemberDrawer.GetPropertyHeight(object? value, GUIContent label)
        {
            if (value is not null)
            {
                if (value is T t)
                {
                    return GetPropertyHeight(t, label);
                }
                else
                {
                    throw new InvalidOperationException($"Expected value to be of type {typeof(T).Name} but was {value.GetType().Name}");
                }
            }
            else
            {
                return GetPropertyHeight(default, label);
            }
        }
    }
}
