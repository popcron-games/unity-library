#nullable enable
using System;
using System.Collections.Generic;

namespace UnityLibrary
{
    /// <summary>
    /// Allows for efficient retrieval of instances by type.
    /// </summary>
    public class Registry : IObject
    {
        private static readonly Dictionary<Type, HashSet<Type>> typeToAssignableTypes = new();

        /// <summary>
        /// Called when an object is registered, happens during <c>OnEnable</c>.
        /// </summary>
        public Action<object>? onRegistered;

        /// <summary>
        /// Called when an object is unregistered, happens during <c>OnDisable</c>.
        /// </summary>
        public Action<object>? onUnregistered;

        private readonly HashSet<object> objects = new();
        private readonly Dictionary<Type, ArrayList> assignableTypeToObjects = new();

        public IReadOnlyCollection<object> All => objects;
        public int Count => objects.Count;

        public void Register(object value)
        {
            if (TryRegister(value))
            {
                //ok
            }
            else
            {
                throw new InvalidOperationException($"Object {value} ({value.GetType()}) cannot be registered because it has already been registered");
            }
        }

        public void Unregister(object value)
        {
            if (TryUnregister(value))
            {
                //ok
            }
            else
            {
                throw new InvalidOperationException($"Object {value} ({value.GetType()}) cannot be unregistered because it has not been registered");
            }
        }

        public bool TryRegister(object value)
        {
            if (objects.Add(value))
            {
                Add(value);
                onRegistered?.Invoke(value);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryUnregister(object value)
        {
            if (objects.Remove(value))
            {
                onUnregistered?.Invoke(value);
                Remove(value);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Add(object value)
        {
            foreach (Type assignableType in GetAssignableTypes(value.GetType()))
            {
                if (assignableTypeToObjects.TryGetValue(assignableType, out ArrayList? objects))
                {
                    objects.Add(value);
                }
                else
                {
                    objects = new ArrayList();
                    objects.Add(value);
                    assignableTypeToObjects.Add(assignableType, objects);
                }
            }
        }

        private void Remove(object value)
        {
            Type type = value.GetType();
            if (typeToAssignableTypes.TryGetValue(type, out _))
            {
                foreach (Type assignableType in GetAssignableTypes(type))
                {
                    if (assignableTypeToObjects.TryGetValue(assignableType, out ArrayList? objects))
                    {
                        objects.Remove(value);
                    }
                }
            }
        }

        public bool Contains(object value)
        {
            return objects.Contains(value);
        }

        /// <summary>
        /// Returns all instances that are assignable to <typeparamref name="T"/>.
        /// They either implement it if it's an interface, or subtype if a class.
        /// </summary>
        public IReadOnlyList<T> GetAllThatAre<T>()
        {
            Type type = typeof(T);
            if (assignableTypeToObjects.TryGetValue(type, out ArrayList objects))
            {
                return objects.AsReadOnlyList<T>();
            }
            else
            {
                objects = new();
                assignableTypeToObjects.Add(type, objects);
                return objects.AsReadOnlyList<T>();
            }
        }

        /// <summary>
        /// Returns all instances that are assignable to <typeparamref name="A"/>.
        /// They either implement it if it's an interface, or subtype if a class.
        /// </summary>
        public IReadOnlyList<object> GetAllThatAre(Type type)
        {
            if (assignableTypeToObjects.TryGetValue(type, out ArrayList objects))
            {
                return objects;
            }
            else
            {
                objects = new ArrayList();
                assignableTypeToObjects.Add(type, objects);
                return objects;
            }
        }

        private static IReadOnlyCollection<Type> GetAssignableTypes(Type type)
        {
            if (!typeToAssignableTypes.TryGetValue(type, out HashSet<Type>? assignableTypes))
            {
                assignableTypes = new();
                Type? objectType = type;
                while (objectType is not null)
                {
                    Type[] implementingTypes = objectType.GetInterfaces();
                    for (int i = 0; i < implementingTypes.Length; i++)
                    {
                        Type? implementingType = implementingTypes[i];
                        while (implementingType is not null)
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