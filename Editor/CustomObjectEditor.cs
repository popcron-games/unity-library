#nullable enable
using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityLibrary.Unity
{
    public abstract class CustomObjectEditor : Editor
    {
        private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly List<FieldInfo> fields = new();
        private readonly List<PropertyInfo> properties = new();
        private readonly List<MethodInfo> methods = new();
        private readonly Dictionary<string, SerializedProperty> nameToProperty = new();

        public IReadOnlyList<FieldInfo> Fields => fields;
        public IReadOnlyList<PropertyInfo> Properties => properties;
        public IReadOnlyList<MethodInfo> Methods => methods;

        protected virtual void OnEnable()
        {
            fields.AddRange(target.GetType().GetFields(flags));
            properties.AddRange(target.GetType().GetProperties(flags));
            methods.AddRange(target.GetType().GetMethods(flags));
            FindProperties();
        }

        protected virtual void OnDisable()
        {
            fields.Clear();
            properties.Clear();
            methods.Clear();
            nameToProperty.Clear();
        }

        private void FindProperties()
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                SerializedProperty property = serializedObject.FindProperty(iterator.name);
                nameToProperty.Add(property.name, property);
                enterChildren = false;
            }
        }

        public void DrawMember(MemberInfo member)
        {
            if (nameToProperty.TryGetValue(member.Name, out SerializedProperty? serializedProperty))
            {
                ManuallyDraw(serializedProperty);
            }
            else
            {
                GUI.enabled = false;
                object? value = null;
                Type? memberType = null;
                if (member is FieldInfo field)
                {
                    memberType = field.FieldType;
                    value = field.GetValue(target);
                }
                else if (member is PropertyInfo property)
                {
                    memberType = property.PropertyType;
                    value = property.GetValue(target);
                }

                if (memberType != null)
                {
                    IMemberDrawer? drawer;
                    if (value is not null)
                    {
                        MemberDrawers.TryGet(value.GetType(), out drawer);
                        if (drawer is not null)
                        {
                            memberType = value.GetType();
                        }
                    }
                    else
                    {
                        MemberDrawers.TryGet(memberType, out drawer);
                    }

                    if (drawer != null)
                    {
                        GUIContent label = new(GetDisplayName(member.Name));
                        float height = drawer.GetPropertyHeight(serializedProperty, label);
                        Rect position = EditorGUILayout.GetControlRect(true, height);
                        try
                        {
                            drawer.OnGUI(position, serializedProperty, label);
                            GC.SuppressFinalize(drawer);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                            MemberDrawers.RemoveDueToException(memberType);
                        }
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel(GetDisplayName(member.Name));
                        ManuallyDraw(target, member);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                GUI.enabled = true;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            if (target is MonoBehaviour)
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoBehaviour), false);
            }
            else if (target is ScriptableObject)
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((ScriptableObject)target), typeof(ScriptableObject), false);
            }

            EditorGUI.EndDisabledGroup();

            if (Fields.Count > 0)
            {
                HashSet<FieldInfo> fieldsToShow = new();
                foreach (FieldInfo field in Fields)
                {
                    if (ShouldIgnoreMember(field)) continue; //ignore fields declared in base classes

                    fieldsToShow.Add(field);
                }

                if (fieldsToShow.Count > 0)
                {
                    EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);
                    foreach (FieldInfo field in fieldsToShow)
                    {
                        DrawMember(field);
                    }
                }
            }

            if (Properties.Count > 0)
            {
                HashSet<PropertyInfo> propertiesToShow = new();
                foreach (PropertyInfo property in Properties)
                {
                    if (ShouldIgnoreMember(property)) continue;

                    if (property.CanRead && !property.CanWrite)
                    {
                        try
                        {
                            bool show = true;
                            object? value = property.GetValue(target);
                            foreach (FieldInfo field in Fields)
                            {
                                object? fieldValue = field.GetValue(target);
                                if (value == fieldValue)
                                {
                                    show = false; //skip properties that are read only, and their result is the same as an existing field
                                    break;
                                }
                            }

                            if (!show)
                            {
                                continue;
                            }
                        }
                        catch
                        {
                            //unfair to throw exceptions here (not the components fault)
                        }
                    }

                    propertiesToShow.Add(property);
                }

                if (propertiesToShow.Count > 0)
                {
                    EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
                    foreach (PropertyInfo property in propertiesToShow)
                    {
                        DrawMember(property);
                    }
                }
            }

            if (Methods.Count > 0)
            {
                HashSet<MethodInfo> methodsToShow = new();
                foreach (MethodInfo method in Methods)
                {
                    if (ShouldIgnoreMember(method)) continue;
                    if (!method.IsPublic) continue;
                    if (method.Name.StartsWith("get_")) continue;

                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        methodsToShow.Add(method);
                    }
                }

                //if (methodsToShow.Count > 0)
                //{
                //    EditorGUILayout.LabelField("Methods", EditorStyles.boldLabel);
                //    foreach (MethodInfo method in methodsToShow)
                //    {
                //        if (GUILayout.Button(GetDisplayName(method.Name)))
                //        {
                //            method.Invoke(target, null);
                //            EditorUtility.SetDirty(target);
                //        }
                //    }
                //}
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static bool ShouldIgnoreMember(MemberInfo member)
        {
            if (member is PropertyInfo property)
            {
                if (property.Name == "VM" && property.PropertyType == typeof(VirtualMachine))
                {
                    return true;
                }
                else
                {
                    if (member.DeclaringType == typeof(Object) || member.DeclaringType == typeof(CustomMonoBehaviour) || member.DeclaringType == typeof(CustomScriptableObject))
                    {
                        return true;
                    }
                    else if (member.DeclaringType == typeof(Component) || member.DeclaringType == typeof(MonoBehaviour) || member.DeclaringType == typeof(Behaviour) || member.DeclaringType == typeof(ScriptableObject))
                    {
                        return true;
                    }
                }
            }
            else if (member is FieldInfo field)
            {
                if (field.Name.EndsWith(">k__BackingField"))
                {
                    return true;
                }
            }

            if (member.DeclaringType == typeof(object))
            {
                return true;
            }

            return false;
        }

        private static string GetDisplayName(ReadOnlySpan<char> name)
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

        private static void ManuallyDraw(SerializedProperty property)
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

        private static object? ManuallyDraw(object target, MemberInfo member)
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

            return ManuallyDraw(type, value);
        }

        private static object? ManuallyDraw(Type type, object? value)
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
                            list[i] = ManuallyDraw(element.GetType(), element);
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
                            array.SetValue(ManuallyDraw(element.GetType(), element), i);
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
            if (value is bool boolValue)
            {
                return EditorGUILayout.Toggle(boolValue);
            }
            else if (value is int intValue)
            {
                return EditorGUILayout.IntField(intValue);
            }
            else if (value is float floatValue)
            {
                return EditorGUILayout.FloatField(floatValue);
            }
            else if (value is double doubleValue)
            {
                return EditorGUILayout.DoubleField(doubleValue);
            }
            else if (type == typeof(string))
            {
                return EditorGUILayout.TextField(value as string);
            }
            else if (value is Vector2 vector2Value)
            {
                return EditorGUILayout.Vector2Field(string.Empty, vector2Value);
            }
            else if (value is Vector3 vector3Value)
            {
                return EditorGUILayout.Vector3Field(string.Empty, vector3Value);
            }
            else if (value is Vector4 vector4Value)
            {
                return EditorGUILayout.Vector4Field(string.Empty, vector4Value);
            }
            else if (value is Quaternion quaternionValue)
            {
                return EditorGUILayout.Vector4Field(string.Empty, new Vector4(quaternionValue.x, quaternionValue.y, quaternionValue.z, quaternionValue.w));
            }
            else if (value is Color colorValue)
            {
                return EditorGUILayout.ColorField(colorValue);
            }
            else if (value is Rect rectValue)
            {
                return EditorGUILayout.RectField(rectValue);
            }
            else if (value is Bounds boundsValue)
            {
                return EditorGUILayout.BoundsField(boundsValue);
            }
            else if (type == typeof(AnimationCurve))
            {
                return EditorGUILayout.CurveField(value as AnimationCurve);
            }
            else if (type == typeof(Gradient))
            {
                return EditorGUILayout.GradientField(value as Gradient);
            }
            else if (typeof(Object).IsAssignableFrom(type))
            {
                return EditorGUILayout.ObjectField(value as Object, type, true);
            }
            else
            {
                EditorGUILayout.LabelField(value?.ToString() ?? "null");
                return value;
            }
        }
    }
}