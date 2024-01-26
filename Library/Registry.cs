#nullable enable
using System;
using System.Collections.Generic;

namespace Library
{
    /// <summary>
    /// Access to objects and allows for efficient retrieval of them by assignable types.
    /// </summary>
    public class Registry<T>
    {
        private static readonly Dictionary<Type, HashSet<Type>> typeToAssignableTypes = new();

        public Action<T>? onRegistered;
        public Action<T>? onUnregistered;

        private readonly HashSet<T> objects = new();
        private readonly Dictionary<Type, List<T>> assignableTypeToObjects = new();

        public IReadOnlyCollection<T> All => objects;
        public int Count => objects.Count;

        public void Register(T value)
        {
            if (objects.Add(value))
            {
                Add(value);
                onRegistered?.Invoke(value);
            }
            else
            {
                throw new InvalidOperationException($"Object {value} is already registered");
            }
        }

        public void Unregister(T value)
        {
            if (objects.Remove(value))
            {
                onUnregistered?.Invoke(value);
                Remove(value);
            }
            else
            {
                throw new InvalidOperationException($"Object {value} cannot be unregistered because it hasn't been registered first");
            }
        }

        private void Add(T value)
        {
            foreach (Type assignableType in GetAssignableTypes(value.GetType()))
            {
                if (assignableTypeToObjects.TryGetValue(assignableType, out List<T>? objects))
                {
                    objects.Add(value);
                }
                else
                {
                    objects = new List<T>
                    {
                        value
                    };
                    assignableTypeToObjects.Add(assignableType, objects);
                }
            }
        }

        private void Remove(T value)
        {
            Type type = value.GetType();
            if (typeToAssignableTypes.TryGetValue(type, out _))
            {
                foreach (Type assignableType in GetAssignableTypes(type))
                {
                    if (assignableTypeToObjects.TryGetValue(assignableType, out List<T>? objects))
                    {
                        objects.Remove(value);
                    }
                }
            }
        }

        /// <summary>
        /// Returns all instances that are assignable to <typeparamref name="A"/>.
        /// They either implement it if it's an interface, or subtype if a class.
        /// </summary>
        public IReadOnlyList<T> GetAllThatAre<A>()
        {
            return GetAllThatAre(typeof(A));
        }

        /// <summary>
        /// Returns all instances that are assignable to <typeparamref name="A"/>.
        /// They either implement it if it's an interface, or subtype if a class.
        /// </summary>
        public IReadOnlyList<T> GetAllThatAre(Type type)
        {
            if (assignableTypeToObjects.TryGetValue(type, out List<T> objects))
            {
                return objects;
            }
            else
            {
                objects = new List<T>();
                assignableTypeToObjects.Add(type, objects);
                return objects;
            }
        }

        public int FillAllThatAre<A>(T[] buffer)
        {
            return FillAllThatAre(typeof(A), buffer);
        }

        public int FillAllThatAre(Type type, T[] buffer)
        {
            if (assignableTypeToObjects.TryGetValue(type, out List<T>? objects))
            {
                objects.CopyTo(buffer);
                return objects.Count;
            }
            else
            {
                return 0;
            }
        }

        private static IReadOnlyCollection<Type> GetAssignableTypes(Type type)
        {
            if (!typeToAssignableTypes.TryGetValue(type, out HashSet<Type>? assignableTypes))
            {
                assignableTypes = new();
                Type? objectType = type;
                while (objectType != null)
                {
                    Type[] implementingTypes = objectType.GetInterfaces();
                    for (int i = 0; i < implementingTypes.Length; i++)
                    {
                        Type? implementingType = implementingTypes[i];
                        while (implementingType != null)
                        {
                            assignableTypes.Add(implementingType);
                            implementingType = implementingType.BaseType;
                        }
                    }

                    assignableTypes.Add(objectType);
                    objectType = objectType.BaseType;
                }

                typeToAssignableTypes.Add(type, assignableTypes);
            }

            return assignableTypes;
        }
    }
}