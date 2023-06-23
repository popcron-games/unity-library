#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Object = UnityEngine.Object;

namespace Popcron
{
    public static class Everything
    {
        private static readonly HashSet<Object> unityObjects = new HashSet<Object>();
        private static readonly HashSet<IUnityObject> objects = new HashSet<IUnityObject>();
        private static readonly Dictionary<int, HashSet<object>> assignableObjects = new Dictionary<int, HashSet<object>>();
        private static readonly Dictionary<int, HashSet<int>> typeToAssignableTypes = new Dictionary<int, HashSet<int>>();

        public static event Action<object>? added;
        public static event Action<object>? removed;

        public static ReadOnlySpan<object> All
        {
            get
            {
                int length = unityObjects.Count + objects.Count + 32;
                RecycledList<object> all = new RecycledList<object>(length);
                foreach (Object obj in unityObjects)
                {
                    all.AddWithoutResize(obj);
                }

                foreach (IUnityObject unityObj in objects)
                {
                    all.AddWithoutResize(unityObj);
                }

                return all.AsSpan();
            }
        }

        private static int CalculateHash(string chars)
        {
            unchecked
            {
                int hash = 0;
                for (int i = 0; i < chars.Length; i++)
                {
                    hash += chars[i] * 50801479;
                    hash *= 50801479;
                }

                return hash;
            }
        }

        private static int CalculateHash<T>() => CalculateHash(typeof(T));

        private static int CalculateHash(Type type)
        {
            unchecked
            {
                int hash = 0;
                hash += CalculateHash(type.Name) * 50801479;
                if (type.BaseType is Type baseType)
                {
                    hash += CalculateHash(baseType.Name) * 50801479;
                }

                return hash;
            }
        }

        public static void Add(Object obj)
        {
            if (unityObjects.Add(obj))
            {
                AddInternally(obj);
                added?.Invoke(obj);
            }
        }

        public static void Remove(Object obj)
        {
            if (unityObjects.Remove(obj))
            {
                RemoveInternally(obj);
                removed?.Invoke(obj);
            }
        }

        public static void Add(IUnityObject obj)
        {
            if (objects.Add(obj))
            {
                AddInternally(obj);
                added?.Invoke(obj);
            }
        }

        public static void Remove(IUnityObject obj)
        {
            if (objects.Remove(obj))
            {
                RemoveInternally(obj);
                removed?.Invoke(obj);
            }
        }

        private static void AddInternally(object obj)
        {
            Type type = obj.GetType();
            int typeIdentifier = CalculateHash(type);
            if (!typeToAssignableTypes.TryGetValue(typeIdentifier, out HashSet<int>? assignableTypes))
            {
                assignableTypes = new HashSet<int>();
                typeToAssignableTypes.Add(typeIdentifier, assignableTypes);
                while (type != null)
                {
                    assignableTypes.Add(CalculateHash(type));
                    Type[] interfaceTypes = type.GetInterfaces();
                    for (int i = 0; i < interfaceTypes.Length; i++)
                    {
                        Type interfaceType = interfaceTypes[i];
                        while (interfaceType != null)
                        {
                            assignableTypes.Add(CalculateHash(interfaceType));
                            interfaceType = interfaceType.BaseType;
                        }
                    }

                    type = type.BaseType;
                }
            }

            foreach (int assignableType in assignableTypes)
            {
                if (assignableObjects.TryGetValue(assignableType, out var objects))
                {
                    objects.Add(obj);
                }
                else
                {
                    objects = new HashSet<object> { obj };
                    assignableObjects.Add(assignableType, objects);
                }
            }
        }

        private static void RemoveInternally(object obj)
        {
            Type type = obj.GetType();
            if (assignableObjects.TryGetValue(CalculateHash(type), out var objects))
            {
                objects.Remove(obj);
            }
        }

        public static IReadOnlyCollection<object> GetAllThatAre(Type type)
        {
            if (assignableObjects.TryGetValue(CalculateHash(type), out var objects))
            {
                return objects;
            }
            else
            {
                return Array.Empty<object>();
            }
        }

        public static IReadOnlyCollection<T> GetAllThatAre<T>()
        {
            if (assignableObjects.TryGetValue(CalculateHash<T>(), out var objects))
            {
                RecycledList<T> temp = new RecycledList<T>(objects.Count);
                foreach (var obj in objects)
                {
                    if (obj is T t)
                    {
                        temp.Add(t);
                    }
                }

                return temp;
            }
            else
            {
                return Array.Empty<T>();
            }
        }

        public static bool TryGetFirst(Type type, out object obj)
        {
            if (assignableObjects.TryGetValue(CalculateHash(type), out var objects))
            {
                if (objects.Count > 0)
                {
                    obj = objects.First();
                    return true;
                }
            }

            obj = default!;
            return false;
        }

        public static bool TryGetFirst<T>(out T obj)
        {
            if (assignableObjects.TryGetValue(CalculateHash<T>(), out var objects))
            {
                if (objects.Count > 0)
                {
                    obj = (T)objects.First();
                    return true;
                }
            }

            obj = default!;
            return false;
        }

        /// <summary>
        /// Iterates through all objects in <see cref="All"/> and calls the callback.
        /// </summary>
        /// <returns>True if iteration finished early, by returning false from the callback.</returns>
        public static bool ForEachInAll(Func<object, bool> callback)
        {
            foreach (Object obj in unityObjects)
            {
                if (!callback(obj))
                {
                    return true;
                }
            }

            foreach (IUnityObject unityObj in objects)
            {
                if (!callback(unityObj))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ForEachInAll<T>(Func<T, bool> callback)
        {
            foreach (T entry in GetAllThatAre<T>())
            {
                if (!callback(entry))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetWithID(ReadOnlySpan<char> id, out object? value)
        {
            int idSpan = id.GetSpanHashCode();
            object? foundValue = null;
            bool found = ForEachInAll((obj) =>
            {
                if (obj is IIdentifiable identifiable)
                {
                    if (identifiable.ID.GetSpanHashCode() == idSpan)
                    {
                        foundValue = obj;
                        return false;
                    }
                }

                return true;
            });

            if (found)
            {
                value = foundValue!;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public static bool TryGetAtPath(ReadOnlySpan<char> path, out object? value)
        {
            if (path.Length > 0)
            {
                value = null;
                int position = 0;
                int startIndex = 0;
                while (position < path.Length)
                {
                    char c = path[position];
                    if (c == '/')
                    {
                        ReadOnlySpan<char> id = path.Slice(startIndex, position);
                        startIndex = position + 1;

                        if (value is null)
                        {
                            if (!TryGetWithID(id, out value))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (value is IBranch branch)
                            {
                                if (!branch.TryGetChild(id, out value))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        position++;
                    }
                }

                return value != null;
            }
            else
            {
                throw new Exception("Path is empty");
            }
        }
    }
}
