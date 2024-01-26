#nullable enable
using Library.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Library.Unity
{
    public class PlayValidationTester
    {
        /// <summary>
        /// Event for when validation has passed on an individual <see cref="PlayValidationEvent"/> listener.
        /// </summary>
        public event Validate? afterValidation;

        private readonly Dictionary<Type, bool> typesWithAssetReferences = new();

        private void CollectOpenSceneValidators(List<IListener<PlayValidationEvent>> listeners)
        {
            for (int i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                CollectValidators(scene, listeners);
            }
        }

        private void CollectValidators(Scene scene, List<IListener<PlayValidationEvent>> listeners)
        {
            List<GameObject> all = scene.GetAllGameObjects();
            foreach (GameObject g in all)
            {
                CollectValidators(g, listeners);
            }
        }

        private void CollectValidators(GameObject gameObject, List<IListener<PlayValidationEvent>> listeners)
        {
            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component component in components)
            {
                CollectValidators(component, listeners);
            }
        }

        private void CollectValidators(VirtualMachine vm, List<IListener<PlayValidationEvent>> listeners)
        {
            foreach (object system in vm.Systems)
            {
                CollectValidators(system, listeners);
            }
        }

        private void CollectValidators(object? value, List<IListener<PlayValidationEvent>> listeners)
        {
            if (value is IListener<PlayValidationEvent> valueIsValidator)
            {
                if (!listeners.Contains(valueIsValidator))
                {
                    listeners.Add(valueIsValidator);
                }
            }

            if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    CollectValidators(item, listeners);
                }
            }

            //types
            else if (value is Object unityObject)
            {
                //iterate through all referenced fields
#if UNITY_EDITOR
                Type type = unityObject.GetType();
                if (!typesWithAssetReferences.TryGetValue(type, out bool has))
                {
                    FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (FieldInfo field in fields)
                    {
                        if (field.IsPrivate)
                        {
                            if (field.GetCustomAttribute<SerializeField>() is null)
                            {
                                continue;
                            }
                        }

                        if (typeof(AssetReference).IsAssignableFrom(field.FieldType))
                        {
                            has = true;
                            break;
                        }
                        else if (typeof(Object).IsAssignableFrom(field.FieldType))
                        {
                            has = true;
                            break;
                        }
                        else if (typeof(IListener<PlayValidationEvent>).IsAssignableFrom(field.FieldType))
                        {
                            has = true;
                            break;
                        }
                    }

                    typesWithAssetReferences.Add(type, has);
                }

                if (has)
                {
                    SerializedObject serializedObject = new(unityObject);
                    SerializedProperty property = serializedObject.GetIterator();
                    while (property.NextVisible(true))
                    {
                        if (property.type.StartsWith("AssetReference"))
                        {
                            object? propertyValue = GetPropertyValue(property);
                            CollectValidators(propertyValue, listeners);
                        }
                        else if (property.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            Object? propertyValue = property.objectReferenceValue;
                            CollectValidators(propertyValue, listeners);
                        }
                        else if (property.propertyType == SerializedPropertyType.Generic)
                        {
                            object? propertyValue = GetPropertyValue(property);
                            CollectValidators(propertyValue, listeners);
                        }
                    }

                    serializedObject.Dispose();
                }
#endif
            }
            else if (value is AssetReference assetReference)
            {
#if UNITY_EDITOR
                Object? editorAsset = assetReference.editorAsset;
                if (editorAsset is GameObject g)
                {
                    CollectValidators(g, listeners);
                }
#endif
            }
            else if (value is UnityObjects objects)
            {
                HashSet<Object> currentlyAll = new(objects.All);
                foreach (Object obj in currentlyAll)
                {
                    CollectValidators(obj, listeners);
                }
            }
        }

        private bool Test(VirtualMachine vm, IEnumerable<IListener<PlayValidationEvent>> validators)
        {
            bool passed = true;
            foreach (IListener<PlayValidationEvent> validator in validators)
            {
                try
                {
                    validator.Receive(vm, new PlayValidationEvent());
                    afterValidation?.Invoke(validator);
                }
                catch (Exception ex)
                {
                    passed = false;
                    if (validator is Object context)
                    {
                        Debug.LogException(ex, context);
                    }
                    else
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            return passed;
        }

        /// <summary>
        /// Tests the ability to start playing from current opened scenes.
        /// </summary>
        public bool TestOpenedScenes(VirtualMachine vm)
        {
            List<IListener<PlayValidationEvent>> listeners = new();
            CollectOpenSceneValidators(listeners);
            return Test(vm, listeners);
        }

        /// <summary>
        /// Tests the ability to start playing the program from the beginning.
        /// </summary>
        public bool TestStarting(VirtualMachine vm)
        {
            List<IListener<PlayValidationEvent>> listeners = new();
            CollectValidators(vm, listeners);
            return Test(vm, listeners);
        }

        public delegate void Validate(IListener<PlayValidationEvent> target);

#if UNITY_EDITOR
        private static object? GetPropertyValue(SerializedProperty property)
        {
            Regex rgx = new(@"\[\d+\]", RegexOptions.Compiled);
            object? obj = property.serializedObject.targetObject;
            if (obj is null)
            {
                return null;
            }

            string path = property.propertyPath.Replace(".Array.data", "");
            string[] fieldStructure = path.Split('.');
            for (int i = 0; i < fieldStructure.Length; i++)
            {
                if (fieldStructure[i].Contains("["))
                {
                    int index = Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
                    obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
                }
                else
                {
                    obj = GetFieldValue(fieldStructure[i], obj);
                }

                if (obj is null)
                {
                    break;
                }
            }

            return obj;
        }

        private static object? GetFieldValue(string fieldName, object obj, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field is not null)
            {
                return field.GetValue(obj);
            }

            return null;
        }

        private static object? GetFieldValueWithIndex(string fieldName, object obj, int index, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);
            if (field is not null)
            {
                object val = field.GetValue(obj);
                if (val is Array array)
                {
                    return array.GetValue(index);
                }
                else if (val is IList list)
                {
                    return list[index];
                }
            }

            return null;
        }
#endif
    }
}