#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace UnityLibrary
{
    public class UnityLibraryWindow : EditorWindow
    {
        private static readonly Dictionary<ulong, GUID> typeToAssetGUID = new();
        private static readonly Dictionary<ulong, TextAsset> typeToScript = new();

        private AnimBool showSystemsBoolean;
        private Vector2 scrollPosition;

        private static bool ShowSystems
        {
            get => EditorPrefs.GetBool("UnityLibrary_ShowSystems", true);
            set => EditorPrefs.SetBool("UnityLibrary_ShowSystems", value);
        }

        private async void OnEnable()
        {
            showSystemsBoolean = new(ShowSystems);
            showSystemsBoolean.valueChanged.AddListener(Repaint);
            await FindAllScripts();
        }

        private void OnGUI()
        {
            VirtualMachine vm = UnityApplication.VM;
            Repaint();

            // show settings asset
            EditorGUILayout.ObjectField("Settings Asset", UnityApplicationSettings.FindSingleton(), typeof(UnityApplicationSettings), false);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // show all systems
            showSystemsBoolean.target = EditorGUILayout.Foldout(showSystemsBoolean.target, "Systems", true);
            if (EditorGUILayout.BeginFadeGroup(showSystemsBoolean.faded))
            {
                EditorGUI.indentLevel++;
                if (vm.Systems.Count > 0)
                {
                    foreach (object system in vm.Systems)
                    {
                        DrawSystem(system);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No systems are registered");
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            ShowSystems = showSystemsBoolean.target;
            EditorGUILayout.EndScrollView();
        }

        private void DrawSystem(object system)
        {
            string showKey = system.GetHashCode().ToString();
            bool show = EditorPrefs.GetBool(showKey, false);
            show = EditorGUILayout.Foldout(show, system.GetType().Name, true);
            if (show)
            {
                EditorGUI.indentLevel++;
                DrawScript(system);
                DrawObject(system);
                EditorGUI.indentLevel--;
            }

            EditorPrefs.SetBool(showKey, show);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static void DrawObject(object target)
        {
            FieldInfo[] fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                Type fieldType = field.FieldType;
                if (fieldType == typeof(VirtualMachine) && field.Name == "vm")
                {
                    continue;
                }

                DrawField(target, field);
            }
        }

        private static void DrawField(object target, FieldInfo field)
        {
            object? value = field.GetValue(target);
            Type fieldType = field.FieldType;
            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                EditorGUILayout.ObjectField(field.Name, value as UnityEngine.Object, typeof(UnityEngine.Object), true);
            }
            else
            {
                if (value is null)
                {
                    EditorGUILayout.LabelField(field.Name, "null");
                }
                else if (value is IDictionary dictionaryValue)
                {
                    int count = dictionaryValue.Count;
                    string showKey = value.GetHashCode().ToString();
                    bool showList = EditorPrefs.GetBool(showKey, false);
                    showList = EditorGUILayout.Foldout(showList, $"{field.Name} (Count: {count})", true);
                    if (showList)
                    {
                        EditorGUI.indentLevel++;
                        foreach (DictionaryEntry entry in dictionaryValue)
                        {
                            EditorGUILayout.BeginHorizontal();
                            DrawObject(entry.Key);
                            DrawObject(entry.Value);
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUI.indentLevel--;
                    }

                    EditorPrefs.SetBool(showKey, showList);
                }
                else if (value is IList listValue)
                {
                    int count = listValue.Count;
                    string showKey = value.GetHashCode().ToString();
                    bool showList = EditorPrefs.GetBool(showKey, false);
                    showList = EditorGUILayout.Foldout(showList, $"{field.Name} (Count: {count})", true);
                    if (showList)
                    {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < count; i++)
                        {
                            object? element = listValue[i];
                            DrawObject(element);
                        }

                        EditorGUI.indentLevel--;
                    }

                    EditorPrefs.SetBool(showKey, showList);
                }
                else
                {
                    EditorGUILayout.LabelField(field.Name, value.ToString());
                }
            }
        }

        private void DrawScript(object system)
        {
            Type systemType = system.GetType();
            ulong hash = GetLongHashCode(systemType.Name);
            string cachedGuidKey = systemType.FullName + "guid";
            if (!typeToAssetGUID.TryGetValue(hash, out GUID guid))
            {
                if (EditorPrefs.HasKey(cachedGuidKey))
                {
                    string cachedGuid = EditorPrefs.GetString(cachedGuidKey, string.Empty);
                    guid = new(cachedGuid);
                }
            }

            if (!typeToScript.TryGetValue(hash, out TextAsset? script))
            {
                script = AssetDatabase.LoadAssetByGUID<TextAsset>(guid);
                if (script != null)
                {
                    typeToScript[hash] = script;
                    EditorPrefs.SetString(cachedGuidKey, guid.ToString());
                }
            }

            EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
        }

        [MenuItem("Window/Unity Library")]
        public static void Open()
        {
            GetWindow<UnityLibraryWindow>("Unity Library");
        }

        private static async Awaitable FindAllScripts()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            typeToAssetGUID.Clear();
            int taskId = Progress.Start("Finding asset paths to all types");
            List<string> typesDeclared = new();
            GUID[] scriptGuids = AssetDatabase.FindAssetGUIDs("t:MonoScript");
            for (int i = 0; i < scriptGuids.Length; i++)
            {
                GUID guid = scriptGuids[i];
                TextAsset script = AssetDatabase.LoadAssetByGUID<TextAsset>(guid);

                double timeNow = EditorApplication.timeSinceStartup;
                bool shouldWait = timeNow - currentTime > 0.09f;
                if (shouldWait)
                {
                    currentTime = timeNow;
                    await Awaitable.WaitForSecondsAsync(0.03f);
                }

                GetDeclaredTypes(script.text.AsSpan(), typesDeclared);
                foreach (string type in typesDeclared)
                {
                    typeToAssetGUID[GetLongHashCode(type)] = guid;
                }

                typesDeclared.Clear();
                Progress.Report(taskId, (i + 1) / (float)scriptGuids.Length);
            }

            Progress.Finish(taskId);
        }

        private static void GetDeclaredTypes(ReadOnlySpan<char> content, List<string> typesDeclared)
        {
            const string Keyword = "class ";
            int index = 0;
            do
            {
                int found = content.Slice(index).IndexOf(Keyword, StringComparison.Ordinal);
                if (found == -1)
                {
                    break;
                }

                int classPos = index + found;
                int nameStart = classPos + Keyword.Length;
                int nameEnd = nameStart;
                while (nameEnd < content.Length && char.IsWhiteSpace(content[nameEnd]))
                {
                    nameEnd++;
                }

                int typeNameStart = nameEnd;
                while (nameEnd < content.Length && (char.IsLetterOrDigit(content[nameEnd]) || content[nameEnd] == '_'))
                {
                    nameEnd++;
                }

                if (typeNameStart < nameEnd)
                {
                    string typeName = content.Slice(typeNameStart, nameEnd - typeNameStart).ToString();
                    typesDeclared.Add(typeName);
                }

                index = nameEnd;
            }
            while (index < content.Length);
        }

        private static ulong GetLongHashCode(ReadOnlySpan<char> text)
        {
            const ulong FnvOffsetBasis = 14695981039346656037;
            ulong result = 0;
            for (int i = 0; i < text.Length; i++)
            {
                result ^= text[i];
                result *= FnvOffsetBasis;
            }

            return result;
        }
    }
}
