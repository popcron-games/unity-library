#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityLibrary.Drawers;
using Object = UnityEngine.Object;

namespace UnityLibrary.Editor
{
    public abstract class CustomObjectEditor : UnityEditor.Editor
    {
        private const BindingFlags MemberFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private static Container? container;
        private static SerializedObject? containerObject;

        private readonly List<(FieldInfo, SerializedProperty?)> fields = new();
        private readonly List<(PropertyInfo, SerializedProperty?)> properties = new();
        private readonly List<MethodInfo> methods = new();

        public IReadOnlyList<(FieldInfo field, SerializedProperty? serializedProperty)> Fields => fields;
        public IReadOnlyList<(PropertyInfo property, SerializedProperty? serializedProperty)> Properties => properties;
        public IReadOnlyList<MethodInfo> Methods => methods;

        protected virtual void OnEnable()
        {
            FieldInfo[] foundFields = target.GetType().GetFields(MemberFlags);
            foreach (FieldInfo field in foundFields)
            {
                SerializedProperty? serializedProperty = serializedObject.FindProperty(field.Name);
                fields.Add((field, serializedProperty));
            }

            PropertyInfo[] foundProperties = target.GetType().GetProperties(MemberFlags);
            foreach (PropertyInfo property in foundProperties)
            {
                SerializedProperty? serializedProperty = serializedObject.FindProperty(property.Name);
                properties.Add((property, serializedProperty));
            }

            methods.AddRange(target.GetType().GetMethods(MemberFlags));

            //remove members with HideInInspector attribute
            for (int i = fields.Count - 1; i >= 0; i--)
            {
                if (ShouldIgnoreMember(foundFields[i]))
                {
                    fields.RemoveAt(i);
                }
            }

            for (int i = properties.Count - 1; i >= 0; i--)
            {
                if (ShouldIgnoreMember(foundProperties[i]))
                {
                    properties.RemoveAt(i);
                }
            }

            for (int i = methods.Count - 1; i >= 0; i--)
            {
                if (ShouldIgnoreMember(methods[i]))
                {
                    methods.RemoveAt(i);
                }
            }
        }

        protected virtual void OnDisable()
        {
            fields.Clear();
            properties.Clear();
            methods.Clear();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //show what script is being inspected
            EditorGUI.BeginDisabledGroup(true);
            if (target is MonoBehaviour monoBehaviour)
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(monoBehaviour), typeof(MonoBehaviour), false);
            }
            else if (target is ScriptableObject scriptableObject)
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(scriptableObject), typeof(ScriptableObject), false);
            }

            EditorGUI.EndDisabledGroup();

            //show fields
            EditorGUI.BeginChangeCheck();
            foreach ((FieldInfo field, SerializedProperty? serializedProperty) in Fields)
            {
                if (!TryDrawMember(field) && serializedProperty is not null)
                {
                    EditorGUILayout.PropertyField(serializedProperty, new GUIContent(serializedProperty.displayName), true);
                }
            }

            //show properties
            if (Properties.Count > 0)
            {
                foreach ((PropertyInfo property, SerializedProperty? serializedProperty) in Properties)
                {
                    try
                    {
                        bool show = true;

                        //skip properties that have the same value as a field (likely backing field)
                        object? value = property.GetValue(target);
                        foreach ((FieldInfo field, _) in Fields)
                        {
                            object? fieldValue = field.GetValue(target);
                            if (value == fieldValue)
                            {
                                show = false;
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

                    if (!TryDrawMember(property) && serializedProperty is not null)
                    {
                        EditorGUILayout.PropertyField(serializedProperty, new GUIContent(serializedProperty.displayName), true);
                    }
                }
            }

            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                EditorUtility.SetDirty(serializedObject.targetObject);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public bool TryDrawMember(MemberInfo member)
        {
            bool readOnly = false;
            object? memberValue;
            Type memberType;
            FieldInfo? field = member as FieldInfo;
            PropertyInfo? property = member as PropertyInfo;
            if (field is not null)
            {
                memberType = field.FieldType;
                memberValue = field.GetValue(target);

                //dont show private non serialized fields in edit mode
                bool serialized = field.GetCustomAttribute<SerializeField>() is not null;
                if (!serialized && !EditorApplication.isPlaying && !field.IsPublic)
                {
                    return true;
                }

                readOnly = !serialized;
            }
            else if (property is not null)
            {
                memberType = property.PropertyType;
                try
                {
                    memberValue = property.GetValue(target);
                }
                catch (Exception ex)
                {
                    memberValue = ex;
                }

                //dont show properties when in edit mode
                if (!EditorApplication.isPlaying)
                {
                    return true;
                }

                readOnly = !property.CanWrite;
            }
            else
            {
                throw new NotImplementedException($"Member type {member.GetType()} is not supported");
            }

            GUI.enabled = !readOnly;
            GUIContent label = new(GetDisplayName(member));
            if (memberValue is Exception exception)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);
                EditorGUILayout.HelpBox(exception.Message, MessageType.Error);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (TryManuallyDraw(label, memberType, ref memberValue))
                {
                    if (field is not null)
                    {
                        field.SetValue(target, memberValue);
                    }
                    else if (property is not null)
                    {
                        if (property.CanWrite)
                        {
                            property.SetValue(target, memberValue);
                        }
                    }
                }
                else
                {
                    GUI.enabled = true;
                    return false;
                }
            }

            GUI.enabled = true;
            return true;
        }

        private static bool ShouldIgnoreMember(MemberInfo member)
        {
            if (member.GetCustomAttribute<HideInInspector>() is not null)
            {
                return true;
            }

            if (member is PropertyInfo property)
            {
                if (!property.CanRead)
                {
                    return false;
                }

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

        private static string GetDisplayName(MemberInfo member)
        {
            if (member.GetCustomAttribute<InspectorNameAttribute>() is InspectorNameAttribute customName)
            {
                return customName.displayName;
            }

            ReadOnlySpan<char> name = member.Name;
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

        private static bool TryManuallyDraw(GUIContent label, Type type, ref object? value)
        {
            if (container == null)
            {
                container = CreateInstance<Container>();
            }

            if (containerObject is null)
            {
                containerObject = new SerializedObject(container);
            }

            if (MemberDrawers.TryGet(type, out IMemberDrawer? drawer))
            {
                float height = drawer.GetPropertyHeight(value, label);
                Rect position = EditorGUILayout.GetControlRect(false, height);
                try
                {
                    value = drawer.OnGUI(position, value, label);
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            if (typeof(IList).IsAssignableFrom(type))
            {
                IList list;
                if (value is null)
                {
                    EditorGUILayout.LabelField(label, "null");
                    return true;
                }
                else
                {
                    list = (IList)value;
                }

                Array? array = list as Array;
                EditorGUILayout.BeginVertical();
                //EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.Width(Screen.width - 90));
                EditorGUI.indentLevel++;
                int newLength = EditorGUILayout.IntField("Length", list.Count);
                if (newLength < 0)
                {
                    newLength = 0;
                }

                Type elementType;
                if (array is not null)
                {
                    elementType = type.GetElementType();
                }
                else
                {
                    elementType = type.GetGenericArguments()[0];
                }

                //EditorGUILayout.EndHorizontal();
                if (newLength != list.Count)
                {
                    //handle resizing for arrays and lists
                    if (array is not null)
                    {
                        Array newArray = Array.CreateInstance(elementType, newLength);
                        int count = Math.Min(newLength, list.Count);
                        for (int i = 0; i < count; i++)
                        {
                            newArray.SetValue(list[i], i);
                        }

                        //assign new values to defaults
                        if (elementType.IsValueType)
                        {
                            object defaultValue = Activator.CreateInstance(elementType);
                            for (int i = count; i < newLength; i++)
                            {
                                newArray.SetValue(defaultValue, i);
                            }
                        }
                        else
                        {
                            for (int i = count; i < newLength; i++)
                            {
                                newArray.SetValue(null, i);
                            }
                        }

                        list = newArray;
                    }
                    else
                    {
                        if (newLength > list.Count)
                        {
                            while (list.Count < newLength)
                            {
                                list.Add(null);
                            }
                        }
                        else
                        {
                            while (list.Count > newLength)
                            {
                                list.RemoveAt(list.Count - 1);
                            }
                        }
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    object? element = list[i];
                    GUIContent elementContent = new($"Element {i}");
                    TryManuallyDraw(elementContent, elementType, ref element);
                    list[i] = element;
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                value = list;
                return true;
            }
            else if (value is UnityEventBase unityEventBase)
            {
                //container.unityEventValue = unityEventBase;
                //SerializedProperty property = containerObject.FindProperty("unityEventValue");
                //EditorGUILayout.PropertyField(property);
                //return container.unityEventValue;
                return false;
            }
            else if (value is Enum enumValue)
            {
                value = EditorGUILayout.EnumPopup(label, enumValue);
                return true;
            }
            else if (value is bool boolValue)
            {
                container.boolValue = boolValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("boolValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.boolValue;
                return true;
            }
            else if (value is int intValue)
            {
                container.intValue = intValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("intValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.intValue;
                return true;
            }
            else if (value is long longValue)
            {
                container.longValue = longValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("longValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.longValue;
                return true;
            }
            else if (value is float floatValue)
            {
                container.floatValue = floatValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("floatValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.floatValue;
                return true;
            }
            else if (value is double doubleValue)
            {
                container.doubleValue = doubleValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("doubleValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.doubleValue;
                return true;
            }
            else if (type == typeof(string))
            {
                container.stringValue = value as string ?? string.Empty;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("stringValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.stringValue;
                return true;
            }
            else if (value is Vector2 vector2Value)
            {
                container.vector2Value = vector2Value;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("vector2Value");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.vector2Value;
                return true;
            }
            else if (value is Vector3 vector3Value)
            {
                container.vector3Value = vector3Value;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("vector3Value");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.vector3Value;
                return true;
            }
            else if (value is Vector4 vector4Value)
            {
                container.vector4Value = vector4Value;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("vector4Value");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.vector4Value;
                return true;
            }
            else if (value is Quaternion quaternionValue)
            {
                container.quaternionValue = quaternionValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("quaternionValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.quaternionValue;
                return true;
            }
            else if (value is Color colorValue)
            {
                container.colorValue = colorValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("colorValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.colorValue;
                return true;
            }
            else if (value is Rect rectValue)
            {
                container.rectValue = rectValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("rectValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.rectValue;
                return true;
            }
            else if (value is Bounds boundsValue)
            {
                container.boundsValue = boundsValue;
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("boundsValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.boundsValue;
                return true;
            }
            else if (type == typeof(AnimationCurve))
            {
                container.animationCurveValue = value as AnimationCurve ?? new();
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("animationCurveValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.animationCurveValue;
                return true;
            }
            else if (type == typeof(Gradient))
            {
                container.gradientValue = value as Gradient ?? new();
                containerObject.Update();
                SerializedProperty property = containerObject.FindProperty("gradientValue");
                EditorGUILayout.PropertyField(property, label);
                containerObject.ApplyModifiedProperties();
                value = container.gradientValue;
                return true;
            }
            else if (typeof(Object).IsAssignableFrom(type))
            {
                //todo: check if this field/property is not meant to be null, and if it is, highlight it red
                value = EditorGUILayout.ObjectField(label, value as Object, type, true);
                return true;
            }
            else
            {
                return false;
            }
        }

        public sealed class Container : ScriptableObject
        {
            public bool boolValue;
            public float floatValue;
            public int intValue;
            public long longValue;
            public double doubleValue;
            public string stringValue;
            public Vector2 vector2Value;
            public Vector3 vector3Value;
            public Vector4 vector4Value;
            public Quaternion quaternionValue;
            public Color colorValue;
            public Rect rectValue;
            public Bounds boundsValue;
            public AnimationCurve animationCurveValue;
            public Gradient gradientValue;
            public UnityEventBase unityEventValue;
        }

        public sealed class UnityEventContainer<T1> : ScriptableObject
        {
            public UnityEvent<T1> value;
        }

        public sealed class UnityEventContainer<T1, T2> : ScriptableObject
        {
            public UnityEvent<T1, T2> value;
        }

        public sealed class UnityEventContainer<T1, T2, T3> : ScriptableObject
        {
            public UnityEvent<T1, T2, T3> value;
        }

        public sealed class UnityEventContainer<T1, T2, T3, T4> : ScriptableObject
        {
            public UnityEvent<T1, T2, T3, T4> value;
        }
    }
}