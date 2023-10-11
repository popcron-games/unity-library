#nullable enable
using Popcron.Lib;
using System;
using System.Buffers;
using System.Collections.Generic;
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
                object[] buffer = ArrayPool<object>.Shared.Rent(unityObjects.Count + objects.Count + 32);
                int length = 0;
                foreach (Object obj in unityObjects)
                {
                    buffer[length] = obj;
                    length++;
                }

                foreach (IUnityObject unityObj in objects)
                {
                    buffer[length] = unityObj;
                    length++;
                }

                ReadOnlySpan<object> span = new ReadOnlySpan<object>(buffer, 0, length);
                ArrayPool<object>.Shared.Return(buffer);
                return span;
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
            int typeIdentifier = CalculateHash(type);
            if (typeToAssignableTypes.TryGetValue(typeIdentifier, out HashSet<int>? assignableTypes))
            {
                foreach (var assignableType in assignableTypes)
                {
                    if (assignableObjects.TryGetValue(assignableType, out var objects))
                    {
                        objects.Remove(obj);
                    }
                }
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

        public static ReadOnlySpan<T> GetAllThatAre<T>()
        {
            if (assignableObjects.TryGetValue(CalculateHash<T>(), out var objects))
            {
                T[] pool = ArrayPool<T>.Shared.Rent(objects.Count);
                int length = 0;
                foreach (var obj in objects)
                {
                    if (obj is T t)
                    {
                        pool[length] = t;
                        length++;
                    }
                }

                ReadOnlySpan<T> span = new ReadOnlySpan<T>(pool, 0, length);
                ArrayPool<T>.Shared.Return(pool);
                return span;
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
        /// Returns the first instance of this type.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static object GetFirst(Type type)
        {
            return assignableObjects[CalculateHash(type)].First();
        }

        /// <summary>
        /// Returns the first instance of this type.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static T GetFirst<T>()
        {
            return (T)assignableObjects[CalculateHash<T>()].First();
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

        /// <summary>
        /// Finds the first object with this ID through the <see cref="ICanBeIdentified"/> interface.
        /// </summary>
        public static bool TryGetWithID(ReadOnlySpan<char> id, out object? value)
        {
            foreach (Object obj in unityObjects)
            {
                if (obj is IIdentifiable identified && identified.ID.SequenceEqual(id))
                {
                    value = obj;
                    return true;
                }
            }

            foreach (IUnityObject unityObj in objects)
            {
                if (unityObj is IIdentifiable identified && identified.ID.SequenceEqual(id))
                {
                    value = unityObj;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Returns the first object with this ID using the <see cref="ICanBeIdentified"/> interface.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public static object GetWithID(ReadOnlySpan<char> id)
        {
            if (TryGetWithID(id, out object? value))
            {
                return value!;
            }
            else
            {
                throw new NullReferenceException($"Object with ID {id.ToString()} not found.");
            }
        }

        /// <summary>
        /// Finds the first value at this path.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static bool TryGetAtPath(ReadOnlySpan<char> path, out object? value)
        {
            if (path.Length > 0)
            {
                value = null;
                int position = 0;
                int startIndex = 0;
                bool reachedEnd = false;
                while (position < path.Length)
                {
                    char c = path[position];
                    reachedEnd |= position == path.Length - 1;
                    if (c == '/' || reachedEnd)
                    {
                        ReadOnlySpan<char> id = path.Slice(startIndex, position);
                        startIndex = position + 1;
                        if (value is null)
                        {
                            if (!TryGetWithID(id, out value))
                            {
                                return false;
                            }
                            else
                            {
                                //keep going
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
                                else
                                {
                                    //keep going
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                    position++;
                }

                return reachedEnd;
            }
            else
            {
                throw new ArgumentException("Path is empty");
            }
        }

        public static object GetAtPath(ReadOnlySpan<char> path)
        {
            if (TryGetAtPath(path, out object? value))
            {
                return value!;
            }
            else
            {
                throw new NullReferenceException($"Object at path {path.ToString()} not found.");
            }
        }
    }
}
