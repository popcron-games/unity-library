#nullable enable
using System;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Library.Unity
{
    public static class CustomEditorFunctions
    {
        public static string GetDisplayName(ReadOnlySpan<char> name)
        {
            StringBuilder builder = new();
            bool lastWasUpper = false;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (i == 0)
                {
                    builder.Append(char.ToUpper(c));
                }
                else if (char.IsUpper(c))
                {
                    if (!lastWasUpper)
                    {
                        builder.Append(' ');
                    }

                    builder.Append(c);
                }
                else
                {
                    builder.Append(c);
                }

                lastWasUpper = char.IsUpper(c);
            }

            return builder.ToString();
        }

        public static void Draw(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue == null)
                {
                    GUI.color = Color.yellow; //display as yellow if null as a warning
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(EditorGUIUtility.IconContent("Warning"), GUILayout.Width(18), GUILayout.Height(18));
                    EditorGUILayout.PropertyField(property, true);
                    EditorGUILayout.EndHorizontal();
                    GUI.color = Color.white;
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }

        public static object? Draw(object target, MemberInfo member)
        {
            Type type;
            object? value;
            if (member is FieldInfo field)
            {
                type = field.FieldType;
                value = field.GetValue(target);
            }
            else if (member is PropertyInfo property)
            {
                type = property.PropertyType;
                try
                {
                    value = property.GetValue(target);
                }
                catch
                {
                    EditorGUILayout.LabelField("?");
                    return null;
                }
            }
            else
            {
                throw new NotImplementedException($"Member type {member.GetType()} is not supported");
            }

            return Draw(type, value);
        }

        public static object? Draw(Type type, object? value)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                IList? list = value as IList;
                if (list is null)
                {
                    EditorGUILayout.LabelField("null");
                    return null;
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Size", GUILayout.Width(50));
                    int size = EditorGUILayout.IntField(list.Count);
                    EditorGUILayout.EndHorizontal();
                    if (size != list.Count)
                    {
                        if (size > list.Count)
                        {
                            while (list.Count < size)
                            {
                                list.Add(null);
                            }
                        }
                        else
                        {
                            while (list.Count > size)
                            {
                                list.RemoveAt(list.Count - 1);
                            }
                        }
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Element {i}", GUILayout.Width(100));
                        object? element = list[i];
                        if (element is not null)
                        {
                            list[i] = Draw(element.GetType(), element);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("null");
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                    return list;
                }
            }
            else if (type.IsArray)
            {
                Array? array = value as Array;
                if (array is null)
                {
                    EditorGUILayout.LabelField("null");
                    return null;
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Size", GUILayout.Width(50));
                    int size = EditorGUILayout.IntField(array.Length);
                    EditorGUILayout.EndHorizontal();
                    if (size != array.Length)
                    {
                        if (size > array.Length)
                        {
                            Array newArray = Array.CreateInstance(type.GetElementType()!, size);
                            Array.Copy(array, newArray, array.Length);
                            array = newArray;
                        }
                        else
                        {
                            Array newArray = Array.CreateInstance(type.GetElementType()!, size);
                            Array.Copy(array, newArray, size);
                            array = newArray;
                        }
                    }

                    for (int i = 0; i < array.Length; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Element {i}", GUILayout.Width(100));
                        object? element = array.GetValue(i);
                        if (element is not null)
                        {
                            array.SetValue(Draw(element.GetType(), element), i);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("null");
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                    return array;
                }
            }
            if (type == typeof(bool))
            {
                return EditorGUILayout.Toggle((bool)value);
            }
            else if (type == typeof(int))
            {
                return EditorGUILayout.IntField((int)value);
            }
            else if (type == typeof(float))
            {
                return EditorGUILayout.FloatField((float)value);
            }
            else if (type == typeof(string))
            {
                return EditorGUILayout.TextField((string)value);
            }
            else if (type == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(string.Empty, (Vector2)value);
            }
            else if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(string.Empty, (Vector3)value);
            }
            else if (type == typeof(Vector4))
            {
                return EditorGUILayout.Vector4Field(string.Empty, (Vector4)value);
            }
            else if (type == typeof(Color))
            {
                return EditorGUILayout.ColorField((Color)value);
            }
            else if (type == typeof(AnimationCurve))
            {
                return EditorGUILayout.CurveField((AnimationCurve)value);
            }
            else if (type == typeof(Bounds))
            {
                return EditorGUILayout.BoundsField((Bounds)value);
            }
            else if (type == typeof(Rect))
            {
                return EditorGUILayout.RectField((Rect)value);
            }
            else if (type == typeof(Quaternion))
            {
                return Quaternion.Euler(EditorGUILayout.Vector3Field(string.Empty, ((Quaternion)value).eulerAngles));
            }
            else if (type == typeof(LayerMask))
            {
                return EditorGUILayout.LayerField((LayerMask)value);
            }
            else if (type == typeof(Guid))
            {
                return Guid.Parse(EditorGUILayout.TextField(((Guid)value).ToString()));
            }
            else if (type == typeof(Gradient))
            {
                return EditorGUILayout.GradientField((Gradient)value);
            }
            else if (typeof(Object).IsAssignableFrom(type))
            {
                return EditorGUILayout.ObjectField((Object)value, type, true);
            }
            else
            {
                EditorGUILayout.LabelField(value?.ToString() ?? "null");
                return value;
            }
        }
    }
}