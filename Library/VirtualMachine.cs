#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityLibrary.Events;

namespace UnityLibrary
{
    /// <summary>
    /// Contains systems that can receive events through the <see cref="IListener{T}"/> interface.
    /// </summary>
    public class VirtualMachine : IDisposable
    {
        private readonly Registry systemRegistry = new();
        private bool disposed;

        /// <summary>
        /// All added systems.
        /// </summary>
        public IReadOnlyList<object> Systems => systemRegistry.All;
        public bool IsDisposed => disposed;

        public void Dispose()
        {
            ThrowIfDisposed();
            disposed = true;
        }

        /// <summary>
        /// Checks if a system with the given <paramref name="type"/> has been added.
        /// </summary>
        public bool ContainsSystem(Type type)
        {
            return systemRegistry.Contains(type);
        }

        /// <summary>
        /// Checks if a system of type <typeparamref name="T"/> has been added.
        /// </summary>
        public bool ContainsSystem<T>()
        {
            return ContainsSystem(typeof(T));
        }

        /// <summary>
        /// Retrieves the first system of the given <paramref name="systemType"/>.
        /// <para></para>
        /// In debug mode, will throw an <see cref="InvalidOperationException"/> if no such system has been added.
        /// </summary>
        public object GetFirstSystem(Type systemType)
        {
            ThrowIfSystemIsMissing(systemType);
            return systemRegistry.GetFirst(systemType);
        }

        /// <summary>
        /// Retrieves the first system of type <typeparamref name="T"/>.
        /// <para></para>
        /// In debug mode, will throw an <see cref="InvalidOperationException"/> if no such system has been added.
        /// </summary>
        public T GetFirstSystem<T>() where T : notnull
        {
            ThrowIfSystemIsMissing(typeof(T));
            return systemRegistry.GetFirst<T>();
        }

        /// <summary>
        /// Retrieves all systems of the given <paramref name="type"/>.
        /// </summary>
        public IReadOnlyList<object> GetSystemsThatAre(Type type)
        {
            return systemRegistry.GetAllThatAre(type);
        }

        /// <summary>
        /// Retrieves all systems of type <typeparamref name="T"/>.
        /// </summary>
        public IReadOnlyList<T> GetSystemsThatAre<T>()
        {
            return systemRegistry.GetAllThatAre<T>();
        }

        /// <summary>
        /// Adds the given <paramref name="system"/> to the virtual machine.
        /// </summary>
        public void AddSystem(object system)
        {
            systemRegistry.Register(system);
            Broadcast(new SystemAdded(this, system.GetType()));
        }

        /// <summary>
        /// Removes the first system of the given <paramref name="systemType"/> from the virtual machine.
        /// <para></para>
        /// Returns the removed system.
        /// <para></para>
        /// Will throw an <see cref="InvalidOperationException"/> if no such system has been added.
        /// </summary>
        public object RemoveSystem(Type systemType)
        {
            if (systemRegistry.TryGetFirst(systemType, out object? removedSystem))
            {
                Broadcast(new SystemRemoved(this, systemType));
                systemRegistry.Unregister(removedSystem);
                return removedSystem;
            }
            else
            {
                throw new InvalidOperationException($"System of type {systemType} not found to remove in virtual machine");
            }
        }

        /// <summary>
        /// Removes the first system of type <typeparamref name="T"/> from the virtual machine.
        /// <para></para>
        /// Returns the removed system.
        /// <para></para>
        /// Will throw an <see cref="InvalidOperationException"/> if no such system has been added.
        /// </summary>
        public T RemoveSystem<T>() where T : notnull
        {
            if (systemRegistry.TryGetFirst(out T? removedSystem))
            {
                Broadcast(new SystemRemoved(this, typeof(T)));
                systemRegistry.Unregister(removedSystem);
                return removedSystem;
            }
            else
            {
                throw new InvalidOperationException($"System of type {typeof(T)} not found to remove in virtual machine");
            }
        }

        /// <summary>
        /// Removes the given <paramref name="system"/> instance from the virtual machine.
        /// <para></para>
        /// Will throw an <see cref="NullReferenceException"/> if the instance is not found.
        /// </summary>
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

            throw new NullReferenceException($"System instance {system} not found to remove in virtual machine");
        }

        /// <summary>
        /// Tries to remove the first system of type <typeparamref name="T"/> from the virtual machine,
        /// and fills the <paramref name="removedSystem"/> out parameter.
        /// <para></para>
        /// Returns <see langword="true"/> if a system was found and removed, <see langword="false"/> otherwise.
        /// </summary>
        public bool TryRemoveSystem<T>([NotNullWhen(true)] out T? removedSystem) where T : notnull
        {
            if (systemRegistry.TryGetFirst(out removedSystem))
            {
                Broadcast(new SystemRemoved(this, typeof(T)));
                systemRegistry.Unregister(removedSystem);
                return true;
            }
            else
            {
                removedSystem = default;
                return false;
            }
        }

        /// <summary>
        /// Tries to remove the first system of type <typeparamref name="T"/> from the virtual machine.
        /// <para></para>
        /// Returns <see langword="true"/> if a system was found and removed, <see langword="false"/> otherwise.
        /// </summary>
        public bool TryRemoveSystem<T>() where T : notnull
        {
            return TryRemoveSystem<T>(out _);
        }

        /// <summary>
        /// Tries to remove the first system of the given <paramref name="systemType"/> from the virtual machine,
        /// and fills the <paramref name="removedSystem"/> out parameter.
        /// <para></para>
        /// Returns <see langword="true"/> if a system was found and removed, <see langword="false"/> otherwise.
        /// </summary>
        public bool TryRemoveSystem(Type systemType, [NotNullWhen(true)] out object? removedSystem)
        {
            if (systemRegistry.TryGetFirst(systemType, out removedSystem))
            {
                Broadcast(new SystemRemoved(this, systemType));
                systemRegistry.Unregister(removedSystem);
                return true;
            }
            else
            {
                removedSystem = default;
                return false;
            }
        }

        /// <summary>
        /// Tries to get the first system of the given <paramref name="systemType"/>.
        /// <para></para>
        /// Returns <see langword="true"/> if a system was found, <see langword="false"/> otherwise.
        /// </summary>
        public bool TryGetSystem(Type systemType, [NotNullWhen(true)] out object? foundSystem)
        {
            return systemRegistry.TryGetFirst(systemType, out foundSystem);
        }

        /// <summary>
        /// Tries to get the first system of type <typeparamref name="T"/>.
        /// <para></para>
        /// Returns <see langword="true"/> if a system was found, <see langword="false"/> otherwise.
        /// </summary>
        public bool TryGetSystem<T>([NotNullWhen(true)] out T? foundSystem) where T : notnull
        {
            return systemRegistry.TryGetFirst(out foundSystem);
        }

        /// <summary>
        /// Broadcasts the given <paramref name="e"/> event as a reference to
        /// all <see cref="IListener{T}"/> systems, as well as all <see cref="IAnyListener"/> instances.
        /// </summary>
        public void Broadcast<T>(ref T e) where T : notnull
        {
            IReadOnlyList<IListener<T>> listeners = systemRegistry.GetAllThatAre<IListener<T>>();
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].Receive(this, ref e);
            }

            if (e is not Validate)
            {
                IReadOnlyList<IAnyListener> broadcastListeners = systemRegistry.GetAllThatAre<IAnyListener>();
                for (int i = 0; i < broadcastListeners.Count; i++)
                {
                    broadcastListeners[i].Receive(this, ref e);
                }
            }
        }

        /// <summary>
        /// Broadcasts the given <paramref name="e"/> event to all <see cref="IListener{T}"/> systems,
        /// as well as all <see cref="IAnyListener"/> instances.
        /// </summary>
        public void Broadcast<T>(T e) where T : notnull
        {
            IReadOnlyList<IListener<T>> listeners = systemRegistry.GetAllThatAre<IListener<T>>();
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].Receive(this, ref e);
            }

            if (e is not Validate)
            {
                IReadOnlyList<IAnyListener> broadcastListeners = systemRegistry.GetAllThatAre<IAnyListener>();
                for (int i = 0; i < broadcastListeners.Count; i++)
                {
                    broadcastListeners[i].Receive(this, ref e);
                }
            }
        }

        [Conditional("DEBUG")]
        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Virtual machine is disposed");
            }
        }

        [Conditional("DEBUG")]
        private void ThrowIfSystemIsMissing(Type systemType)
        {
            if (!ContainsSystem(systemType))
            {
                throw new InvalidOperationException($"System of type `{systemType}` has not been added to the virtual machine");
            }
        }
    }
}