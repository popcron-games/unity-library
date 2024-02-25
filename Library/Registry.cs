#nullable enable
using System;
using System.Collections.Generic;

namespace Game.Library
{
    /// <summary>
    /// Allows for efficient retrieval of instances by type.
    /// </summary>
    public class Registry : IRegistryView
    {
        private static readonly Dictionary<Type, HashSet<Type>> typeToAssignableTypes = new();

        public Action<object>? onRegistered;
        public Action<object>? onUnregistered;

        private readonly HashSet<object> objects = new();
        private readonly Dictionary<Type, List<object>> assignableTypeToObjects = new();

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
                if (assignableTypeToObjects.TryGetValue(assignableType, out List<object>? objects))
                {
                    objects.Add(value);
                }
                else
                {
                    objects = new List<object>
                    {
                        value
                    };
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
                    if (assignableTypeToObjects.TryGetValue(assignableType, out List<object>? objects))
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
        public IReadOnlyList<object> GetAllThatAre<A>()
        {
            return GetAllThatAre(typeof(A));
        }

        /// <summary>
        /// Returns all instances that are assignable to <typeparamref name="A"/>.
        /// They either implement it if it's an interface, or subtype if a class.
        /// </summary>
        public IReadOnlyList<object> GetAllThatAre(Type type)
        {
            if (assignableTypeToObjects.TryGetValue(type, out List<object> objects))
            {
                return objects;
            }
            else
            {
                objects = new List<object>();
                assignableTypeToObjects.Add(type, objects);
                return objects;
            }
        }

        /// <summary>
        /// Copies all instances that are assignable to <typeparamref name="A"/>.
        /// </summary>
        public int FillAllThatAre<A>(object[] buffer)
        {
            return FillAllThatAre(typeof(A), buffer);
        }

        /// <summary>
        /// Copies all instances that are assignable to <typeparamref name="A"/>.
        /// </summary>
        public int FillAllThatAre<A>(IList<object> buffer) 
        {
            return FillAllThatAre(typeof(A), buffer);
        }

        /// <summary>
        /// Copies all instances that are assignable to <typeparamref name="A"/>.
        /// </summary>
        public int FillAllThatAre(Type type, object[] buffer)
        {
            if (assignableTypeToObjects.TryGetValue(type, out List<object>? objects))
            {
                int length = objects.Count > buffer.Length ? buffer.Length : objects.Count;
                objects.CopyTo(0, buffer, 0, length);
                return length;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Copies all instances that are assignable to <typeparamref name="A"/>.
        /// </summary>
        public int FillAllThatAre(Type type, IList<object> buffer)
        {
            if (assignableTypeToObjects.TryGetValue(type, out List<object>? objects))
            {
                int length = objects.Count > buffer.Count ? buffer.Count : objects.Count;
                for (int i = 0; i < length; i++)
                {
                    buffer[i] = objects[i];
                }

                return length;
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