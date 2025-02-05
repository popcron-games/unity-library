#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityLibrary.Editor
{
    public static class OptionalDefinitions
    {
        private static readonly Dictionary<int, bool> isOptional = new();

        static OptionalDefinitions()
        {
            Task.Run(async () => await Analyze());
        }

        private static async Task Analyze()
        {
            List<string> filePaths = new();
            string projectFolder = Directory.GetParent(Application.dataPath).FullName;
            filePaths.AddRange(Directory.GetFiles(Path.Combine(projectFolder, "Assets"), "*.cs", SearchOption.AllDirectories));
            filePaths.AddRange(Directory.GetFiles(Path.Combine(projectFolder, "Packages"), "*.cs", SearchOption.AllDirectories));
            string[] fileSources = new string[filePaths.Count];
            for (int i = 0; i < filePaths.Count; i++)
            {
                string filePath = filePaths[i];
                string source = await File.ReadAllTextAsync(filePath);
                List<int> memberPaths = FindOptionalMembers(source);
                foreach (int path in memberPaths)
                {
                    isOptional[path] = true;
                }

                fileSources[i] = source;
            }
        }

        private static List<int> FindOptionalMembers(ReadOnlySpan<char> source)
        {
            int i = 0;
            int startIndex = 0;
            List<int> result = new();
            while (i < source.Length)
            {
                if (source[i] == '\n')
                {
                    ReadOnlySpan<char> line = source[startIndex..i].TrimStart(' ');
                    startIndex = i + 1;
                    int optionalIndex = line.IndexOf("?", StringComparison.Ordinal);
                    if (optionalIndex != -1)
                    {
                        ReadOnlySpan<char> leftToken = ReadOnlySpan<char>.Empty;
                        int leftIndex = optionalIndex - 1;
                        while (leftIndex >= 0)
                        {
                            if (line[leftIndex] == ' ')
                            {
                                leftToken = line[(leftIndex + 1)..optionalIndex];
                                break;
                            }

                            leftIndex--;
                        }

                        ReadOnlySpan<char> rightToken = ReadOnlySpan<char>.Empty;
                        int rightIndex = optionalIndex + 2;
                        bool notMember = false;
                        while (rightIndex < line.Length)
                        {
                            char c = line[rightIndex];
                            if (c == ')' || c == ' ')
                            {
                                notMember = true;
                                break;
                            }
                            else if (c == ';')
                            {
                                rightToken = line[(optionalIndex + 2)..rightIndex].TrimEnd('\r');
                                break;
                            }

                            rightIndex++;
                        }

                        if (notMember)
                        {
                            i++;
                            continue;
                        }

                        result.Add(GetHashCode(rightToken));
                    }
                }

                i++;
            }

            return result;
        }

        public static bool? IsOptional(SerializedProperty property)
        {
            int hash = GetHashCode(property.propertyPath);
            if (isOptional.TryGetValue(hash, out bool result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        private static int GetHashCode(ReadOnlySpan<char> text)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < text.Length; i++)
                {
                    hash = hash * 31 + text[i];
                }

                return hash;
            }
        }
    }
}