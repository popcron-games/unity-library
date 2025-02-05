#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityLibrary.Events;

namespace UnityLibrary
{
    /// <summary>
    /// Contains systems to represent the state of a game/program,
    /// with a provided <see cref="IProgram"/> to carry custom logic.
    /// </summary>
    public class VirtualMachine : IDisposable
    {
        private static readonly IInitialData fallbackEmptyData = new EmptyInitialData();

        private readonly Registry systemRegistry = new();
        private readonly WeakReference<IInitialData>? initialData;
        private bool disposed;

        public IReadOnlyCollection<object> Systems => systemRegistry.All;
        public bool IsDisposed => disposed;

        /// <summary>
        /// Creates an empty virtual machine.
        /// </summary>
        public VirtualMachine(IInitialData? initialData = null)
        {
            if (initialData is not null)
            {
                this.initialData = new WeakReference<IInitialData>(initialData);
            }
        }

        public IInitialData GetInitialData()
        {
            if (initialData is not null && initialData.TryGetTarget(out IInitialData? target))
            {
                return target;
            }
            else
            {
                return fallbackEmptyData;
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                throw new Exception("Virtual machine is already disposed");
            }

            disposed = true;
        }

        public bool ContainsSystem(Type type)
        {
            IReadOnlyCollection<object> systems = systemRegistry.GetAllThatAre(type);
            return systems.Count > 0;
        }

        public bool ContainsSystem<T>()
        {
            return ContainsSystem(typeof(T));
        }

        public object GetSystem(Type type)
        {
            IReadOnlyList<object> systems = systemRegistry.GetAllThatAre(type);
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

        public IReadOnlyCollection<object> GetSystemsThatAre(Type type)
        {
            return systemRegistry.GetAllThatAre(type);
        }

        public IReadOnlyList<T> GetSystemsThatAre<T>()
        {
            return systemRegistry.GetAllThatAre<T>();
        }

        public void AddSystem(object system)
        {
            if (!systemRegistry.Contains(system))
            {
                systemRegistry.Register(system);
                Type type = system.GetType();
                Broadcast(new SystemAdded(this, type));
            }
            else
            {
                throw new InvalidOperationException($"System instance {system} has already been added in virtual machine");
            }
        }

        public object RemoveSystem(Type type)
        {
            IReadOnlyList<object> systems = systemRegistry.GetAllThatAre(type);
            if (systems.Count > 0)
            {
                object system = systems[0];
                Broadcast(new SystemRemoved(this, type));
                systemRegistry.Unregister(system);
                return system;
            }
            else
            {
                throw new InvalidOperationException($"System of type {type} not found to remove in virtual machine");
            }
        }

        public T RemoveSystem<T>() where T : notnull
        {
            IReadOnlyList<T> systems = systemRegistry.GetAllThatAre<T>();
            if (systems.Count > 0)
            {
                T system = systems[0];
                Broadcast(new SystemRemoved(this, typeof(T)));
                systemRegistry.Unregister(system);
                return system;
            }
            else
            {
                throw new InvalidOperationException($"System of type {typeof(T)} not found to remove in virtual machine");
            }
        }

        public void RemoveSystem(object system)
        {
            foreach (object addedSystem in Systems)
            {
                if (addedSystem == system)
                {
                    Broadcast(new SystemRemoved(this, system.GetType()));
                    systemRegistry.Unregister(system);
                    return;
                }
            }

            throw new InvalidOperationException($"System instance {system} not found to remove in virtual machine");
        }

        public bool TryRemoveSystem<T>([NotNullWhen(true)] out T? system) where T : notnull
        {
            IReadOnlyList<T> systems = systemRegistry.GetAllThatAre<T>();
            if (systems.Count > 0)
            {
                system = systems[0];
                Broadcast(new SystemRemoved(this, typeof(T)));
                systemRegistry.Unregister(system);
                return true;
            }
            else
            {
                system = default;
                return false;
            }
        }

        public bool TryRemoveSystem<T>() where T : notnull
        {
            return TryRemoveSystem<T>(out _);
        }

        public bool TryGetSystem<T>([NotNullWhen(true)] out T? system) where T : notnull
        {
            IReadOnlyList<T> systems = systemRegistry.GetAllThatAre<T>();
            if (systems.Count > 0)
            {
                system = systems[0];
                return true;
            }
            else
            {
                system = default;
                return false;
            }
        }

        /// <summary>
        /// Broadcasts this event to all <see cref="IListener{T}"/> of it,
        /// as well as all <see cref="IAnyListener"/> instances.
        /// </summary>
        public void Broadcast<T>(ref T ev) where T : notnull
        {
            IReadOnlyList<IListener<T>> listeners = systemRegistry.GetAllThatAre<IListener<T>>();
            for (int i = 0; i < listeners.Count; i++)
            {
                IListener<T> listener = listeners[i];
                listener.Receive(this, ref ev);
            }

            IReadOnlyList<IAnyListener> broadcastListeners = systemRegistry.GetAllThatAre<IAnyListener>();
            for (int i = 0; i < broadcastListeners.Count; i++)
            {
                IAnyListener listener = broadcastListeners[i];
                listener.Receive(this, ref ev);
            }
        }

        public void Broadcast<T>(T ev) where T : notnull
        {
            Broadcast(ref ev);
        }
    }
}