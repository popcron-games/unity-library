#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace UnityLibrary
{
    /// <summary>
    /// Allows for efficient retrieval of instances by type.
    /// </summary>
    public class Registry : IRegistry
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

        private readonly List<object> objects = new();
        private readonly Dictionary<Type, ArrayList> assignableTypeToObjects = new();

        public IReadOnlyList<object> All => objects;
        public int Count => objects.Count;

        /// <summary>
        /// Registers the given <paramref name="value"/>.
        /// <para></para>
        /// Will throw if the value has already been registered.
        /// </summary>
        public void Register(object value)
        {
            ThrowIfRegistered(value);
            Add(value);
        }

        /// <summary>
        /// Unregisters the given <paramref name="value"/>.
        /// <para></para>
        /// Will throw if the value has not been registered.
        /// </summary>
        public void Unregister(object value)
        {
            ThrowIfUnregistered(value);
            Remove(value);
        }

        /// <summary>
        /// Tries to register the given <paramref name="value"/>.
        /// <para></para>
        /// Returns <see langword="true"/> if the value was not already registered.
        /// </summary>
        public bool TryRegister(object value)
        {
            if (!objects.Contains(value))
            {
                Add(value);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to unregister the given <paramref name="value"/>.
        /// <para></para>
        /// Returns <see langword="true"/> if the value was registered.
        /// </summary>
        public bool TryUnregister(object value)
        {
            if (objects.Contains(value))
            {
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
            objects.Add(value);
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

            OnRegistered(value);
        }

        private void Remove(object value)
        {
            OnUnregistered(value);
            Type type = value.GetType();
            if (typeToAssignableTypes.TryGetValue(type, out _))
            {
                foreach (Type assignableType in GetAssignableTypes(type))
                {
                    if (assignableTypeToObjects.TryGetValue(assignableType, out ArrayList? objects))
                    {
                        objects.Remove(value);
                        if (objects.Count == 0)
                        {
                            assignableTypeToObjects.Remove(assignableType);
                        }
                    }
                }
            }

            objects.Remove(value);
        }

        protected virtual void OnRegistered(object value)
        {
            onRegistered?.Invoke(value);
        }

        protected virtual void OnUnregistered(object value)
        {
            onUnregistered?.Invoke(value);
        }

        /// <summary>
        /// Checks if the given <paramref name="value"/> is registered.
        /// </summary>
        public bool Contains(object value)
        {
            return objects.Contains(value);
        }

        /// <summary>
        /// Checks if there is at least one instance with the given <paramref name="type"/> registered.
        /// </summary>
        public bool Contains(Type type)
        {
            return assignableTypeToObjects.ContainsKey(type);
        }

        /// <summary>
        /// Checks if there is at least one instance of type <typeparamref name="T"/> registered.
        /// </summary>
        public bool Contains<T>()
        {
            return assignableTypeToObjects.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Returns all instances that are assignable to <typeparamref name="T"/>.
        /// <para></para>
        /// They either implement it if it's an interface, or are a subtype if a class.
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
                return Array.Empty<T>();
            }
        }

        /// <summary>
        /// Returns all instances that are assignable to the given <paramref name="type"/>.
        /// <para></para>
        /// They either implement it if it's an interface, or are a subtype if a class.
        /// </summary>
        public IReadOnlyList<object> GetAllThatAre(Type type)
        {
            if (assignableTypeToObjects.TryGetValue(type, out ArrayList objects))
            {
                return objects;
            }
            else
            {
                return Array.Empty<object>();
            }
        }

        /// <summary>
        /// Tries to retrieve the first instance of type <typeparamref name="T"/>.
        /// </summary>
        public bool TryGetFirst<T>([NotNullWhen(true)] out T? foundValue) where T : notnull
        {
            if (assignableTypeToObjects.TryGetValue(typeof(T), out ArrayList? objects))
            {
                foundValue = (T)objects[0];
                return true;
            }

            foundValue = default;
            return false;
        }

        /// <summary>
        /// Tries to retrieve the first instance of the given <paramref name="type"/>.
        /// </summary>
        public bool TryGetFirst(Type type, [NotNullWhen(true)] out object? foundValue)
        {
            if (assignableTypeToObjects.TryGetValue(type, out ArrayList? objects))
            {
                foundValue = objects[0];
                return true;
            }

            foundValue = default;
            return false;
        }

        /// <summary>
        /// Retrieves the first instance of type <typeparamref name="T"/>.
        /// <para></para>
        /// Will throw if no such instance has been registered.
        /// </summary>
        public T GetFirst<T>() where T : notnull
        {
            ThrowIfNone(typeof(T));
            ArrayList? objects = assignableTypeToObjects[typeof(T)];
            return (T)objects[0];
        }

        /// <summary>
        /// Retrieves the first instance of the given <paramref name="type"/>.
        /// <para></para>
        /// Will throw if no such instance has been registered.
        /// </summary>
        public object GetFirst(Type type)
        {
            ThrowIfNone(type);
            ArrayList? objects = assignableTypeToObjects[type];
            return objects[0];
        }

        [Conditional("DEBUG")]
        private void ThrowIfNone(Type type)
        {
            if (!assignableTypeToObjects.ContainsKey(type))
            {
                throw new($"No objects of type {type} have been registered");
            }
        }

        [Conditional("DEBUG")]
        private void ThrowIfRegistered(object value)
        {
            if (objects.Contains(value))
            {
                throw new($"Object {value} ({value.GetType()}) has already been registered");
            }
        }

        [Conditional("DEBUG")]
        private void ThrowIfUnregistered(object value)
        {
            if (!objects.Contains(value))
            {
                throw new($"Object {value} ({value.GetType()}) has not been registered");
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