#nullable enable
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
using Game.Events;
using Game;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityLibrary.Systems
{
    public class PlayValidationTester
    {
        /// <summary>
        /// Event for when validation has passed on an individual <see cref="TestEvent"/> listener.
        /// </summary>
        public event Validate? afterValidation;

        private readonly Dictionary<Type, bool> typesWithAssetReferences = new();

        private void CollectOpenSceneValidators(List<IListener<TestEvent>> listeners)
        {
            for (int i = 0; i < SceneManager.loadedSceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                CollectValidators(scene, listeners);
            }
        }

        private void CollectValidators(Scene scene, List<IListener<TestEvent>> listeners)
        {
            List<GameObject> all = scene.GetAllGameObjects();
            foreach (GameObject g in all)
            {
                CollectValidators(g, listeners);
            }
        }

        private void CollectValidators(GameObject gameObject, List<IListener<TestEvent>> listeners)
        {
            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component component in components)
            {
                CollectValidators(component, listeners);
            }
        }

        private void CollectValidators(VirtualMachine vm, List<IListener<TestEvent>> listeners)
        {
            foreach (object system in vm.Systems)
            {
                CollectValidators(system, listeners);
            }
        }

        private void CollectValidators(object? value, List<IListener<TestEvent>> listeners)
        {
            if (value is IListener<TestEvent> valueIsValidator)
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
            if (value is Object unityObject)
            {
#if UNITY_EDITOR
                //iterate through all referenced fields
                Type type = unityObject.GetType();
                if (TypeHasFieldsToCheck(type))
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
                CollectValidators(objects.All, listeners);
            }
        }

        private bool TypeHasFieldsToCheck(Type type)
        {
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
                    else if (typeof(IListener<TestEvent>).IsAssignableFrom(field.FieldType))
                    {
                        has = true;
                        break;
                    }
                }

                typesWithAssetReferences.Add(type, has);
            }

            return has;
        }

        private bool Test(VirtualMachine vm, IEnumerable<IListener<TestEvent>> validators)
        {
            bool passed = true;
            foreach (IListener<TestEvent> validator in validators)
            {
                try
                {
                    TestEvent ev = new();
                    validator.Receive(vm, ref ev);
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
        /// Tests the ability to start playing from current open scen.
        /// </summary>
        public bool TestOpenedScenes(VirtualMachine vm)
        {
            List<IListener<TestEvent>> listeners = new();
            CollectOpenSceneValidators(listeners);
            return Test(vm, listeners);
        }

        /// <summary>
        /// Tests the ability to start playing the game, as if this was a build.
        /// </summary>
        public bool TestStarting(VirtualMachine vm)
        {
            List<IListener<TestEvent>> listeners = new();
            listeners.Add(UnityApplicationSettings.Singleton);
            CollectValidators(vm, listeners);
            return Test(vm, listeners);
        }

        public delegate void Validate(IListener<TestEvent> target);

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