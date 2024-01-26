#nullable enable
using Library.Events;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Library
{
    /// <summary>
    /// Contains systems to represent the state of a game/program.
    /// </summary>
    public class VirtualMachine : IDisposable
    {
        public event Action onBroadcast = delegate { };

        private readonly int id;
        private readonly HashSet<int> systems = new();
        private readonly Dictionary<int, object> systemToObject = new();
        private readonly Registry<object> registry = new();
        private readonly IState state;
        private bool initialized;
        private bool disposed;

        public IReadOnlyCollection<object> Systems => registry.All;

        public VirtualMachine(int id, IState state)
        {
            this.id = id;
            this.state = state;
        }

        public override int GetHashCode()
        {
            return id;
        }

        public void Initialize()
        {
            if (initialized)
            {
                throw new Exception("Virtual machine is already initialized.");
            }

            initialized = true;
            state.Initialize(this);
        }

        public void Dispose()
        {
            if (!initialized)
            {
                throw new Exception("Virtual machine is not initialized to dispose.");
            }

            if (disposed)
            {
                throw new Exception("Virtual machine is already disposed.");
            }

            disposed = true;
            state.Finalize(this);
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append("VirtualMachine ");
            builder.Append(id);
            return builder.ToString();
        }

        public bool ContainsSystem(Type type)
        {
            IReadOnlyCollection<object> systems = registry.GetAllThatAre(type);
            return systems.Count > 0;
        }

        public bool ContainsSystem<T>()
        {
            return ContainsSystem(typeof(T));
        }

        public bool ContainsSystem(int hashCode)
        {
            return systems.Contains(hashCode);
        }

        public object GetSystem(Type type)
        {
            IReadOnlyList<object> systems = registry.GetAllThatAre(type);
            if (systems.Count > 0)
            {
                return systems[0];
            }
            else
            {
                throw new InvalidOperationException($"System of type {type} not found in virtual machine");
            }
        }

        public T GetSystem<T>()
        {
            return (T)GetSystem(typeof(T));
        }

        public object GetSystem(int hashCode)
        {
            if (systems.Contains(hashCode))
            {
                return systemToObject[hashCode];
            }
            else
            {
                throw new InvalidOperationException($"System instance with hash code {hashCode} not found in virtual machine");
            }
        }

        public IReadOnlyCollection<object> GetSystemsThatAre(Type type)
        {
            return registry.GetAllThatAre(type);
        }

        public IReadOnlyCollection<object> GetSystemsThatAre<T>()
        {
            return registry.GetAllThatAre<T>();
        }

        public int AddSystem(object system)
        {
            int hash = system.GetHashCode();
            if (systems.Add(hash))
            {
                registry.Register(system);
                systemToObject.Add(hash, system);

                Type type = system.GetType();
                Broadcast(new SystemAdded(this, type));
                return hash;
            }
            else
            {
                throw new InvalidOperationException($"System instance {system} has already been added in virtual machine");
            }
        }

        public object RemoveSystem(Type type)
        {
            IReadOnlyList<object> systems = registry.GetAllThatAre(type);
            if (systems.Count > 0)
            {
                object system = systems[0];
                int hash = system.GetHashCode();
                Broadcast(new SystemRemoved(this, type));
                registry.Unregister(system);
                systemToObject.Remove(hash);
                this.systems.Remove(hash);
                return system;
            }
            else
            {
                throw new InvalidOperationException($"System of type {type} not found to remove in virtual machine");
            }
        }

        public T RemoveSystem<T>()
        {
            return (T)RemoveSystem(typeof(T));
        }

        public object RemoveSystem(int hashCode)
        {
            if (systems.Remove(hashCode))
            {
                object system = systemToObject[hashCode];
                Type type = system.GetType();
                Broadcast(new SystemRemoved(this, type));
                registry.Unregister(system);
                systemToObject.Remove(hashCode);
                return system;
            }
            else
            {
                throw new InvalidOperationException($"System instance with hash code {hashCode} not found in virtual machine");
            }
        }

        public bool TryGetSystem<T>([NotNullWhen(true)] out T? system)
        {
            IReadOnlyList<object> systems = registry.GetAllThatAre<T>();
            if (systems.Count > 0)
            {
                system = (T)systems[0];
                return true;
            }
            else
            {
                system = default;
                return false;
            }
        }

        public void Broadcast<T>(T e)
        {
            List<object> listeners = (List<object>)registry.GetAllThatAre<IListener<T>>();
            List<object> broadcastListeners = (List<object>)registry.GetAllThatAre<IBroadcastListener>();
            object[] buffer = ArrayPool<object>.Shared.Rent(listeners.Count > broadcastListeners.Count ? listeners.Count : broadcastListeners.Count);
            int length = listeners.Count;
            listeners.CopyTo(buffer);
            for (int i = 0; i < length; i++)
            {
                IListener<T> listener = (IListener<T>)buffer[i];
                listener.Receive(this, e);
            }

            length = broadcastListeners.Count;
            broadcastListeners.CopyTo(buffer);
            for (int i = 0; i < length; i++)
            {
                IBroadcastListener listener = (IBroadcastListener)buffer[i];
                listener.Receive(this, e);
            }

            ArrayPool<object>.Shared.Return(buffer);
            onBroadcast.Invoke();
        }
    }
}