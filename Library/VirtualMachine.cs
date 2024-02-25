#nullable enable
using Game.Events;
using Game.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace Game
{
    /// <summary>
    /// Contains systems to represent the state of a game/program,
    /// with a provided <see cref="IState"/> to carry custom logic.
    /// </summary>
    public class VirtualMachine : IDisposable
    {
        private static readonly Dictionary<int, VirtualMachine> all = new();
        private static readonly IInitialData fallbackEmptyData = new EmptyInitialData();

        private readonly int id;
        private readonly HashSet<int> systems = new();
        private readonly HashSet<int> createdSystems = new();
        private readonly Dictionary<int, object> systemToObject = new();
        private readonly Registry systemRegistry = new();
        private readonly IState state;
        private readonly WeakReference<IInitialData>? initialData;
        private bool disposed;

        public IReadOnlyCollection<object> Systems => systemRegistry.All;

        /// <summary>
        /// Creates an empty virtual machine with the given <see cref="IState"/> and initializes it.
        /// </summary>
        public VirtualMachine(int id, IState state, IInitialData? initialData = null)
        {
            if (all.ContainsKey(id))
            {
                throw new InvalidOperationException($"Virtual machine with ID {id} already exists.");
            }

            this.id = id;
            this.state = state;
            if (initialData is not null)
            {
                this.initialData = new WeakReference<IInitialData>(initialData);
            }

            all.Add(id, this);
            state.Initialize(this);
        }

        /// <summary>
        /// Creates an empty virtual machine with the given state, and creates the given systems before initializing <see cref="IState"/>.
        /// </summary>
        public VirtualMachine(int id, IState state, IInitialData initialData, params Type[] initialSystemTypes)
        {
            if (all.ContainsKey(id))
            {
                throw new InvalidOperationException($"Virtual machine with ID {id} already exists.");
            }

            this.state = state;
            if (initialData is not null)
            {
                this.initialData = new WeakReference<IInitialData>(initialData);
            }

            all.Add(id, this);
            foreach (Type systemType in initialSystemTypes)
            {
                AddSystem(systemType);
            }

            state.Initialize(this);
        }

        public override int GetHashCode()
        {
            return id * 397;
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
                throw new Exception("Virtual machine is already disposed.");
            }

            disposed = true;
            foreach (int hashCode in createdSystems)
            {
                object createdSystem = RemoveSystem(hashCode);
                if (createdSystem is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            state.Finalize(this);
            all.Remove(id);
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
            IReadOnlyCollection<object> systems = systemRegistry.GetAllThatAre(type);
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
            return systemRegistry.GetAllThatAre(type);
        }

        public IReadOnlyCollection<object> GetSystemsThatAre<T>()
        {
            return systemRegistry.GetAllThatAre<T>();
        }

        public int AddSystem(object system)
        {
            int hash = system.GetHashCode();
            if (systems.Add(hash))
            {
                systemRegistry.Register(system);
                systemToObject.Add(hash, system);

                Type type = system.GetType();
                SystemAdded ev = new(this, type);
                Broadcast(ref ev);
                return hash;
            }
            else
            {
                throw new InvalidOperationException($"System instance {system} has already been added in virtual machine");
            }
        }

        /// <summary>
        /// Creates and adds a new system of type <typeparamref name="T"/>.
        /// Constructor can be either default or a single <see cref="VirtualMachine"/> input parameter.
        /// </summary>
        public T AddSystem<T>()
        {
            return (T)AddSystem(typeof(T));
        }

        /// <summary>
        /// Creates and adds a new system of type <typeparamref name="T"/>.
        /// Constructor can be either default or a single <see cref="VirtualMachine"/> input parameter.
        /// </summary>
        public object AddSystem(Type systemType)
        {
            if (systemType.GetConstructor(new Type[] { typeof(VirtualMachine) }) is ConstructorInfo constructorWithVm)
            {
                object system = constructorWithVm.Invoke(new object[] { this });
                int hash = AddSystem(system);
                createdSystems.Add(hash);
                return system;
            }
            else if (systemType.GetConstructor(Type.EmptyTypes) is ConstructorInfo defaultConstructor)
            {
                object system = defaultConstructor.Invoke(Array.Empty<object>());
                int hash = AddSystem(system);
                createdSystems.Add(hash);
                return system;
            }
            else
            {
                throw new InvalidOperationException($"System type {systemType} does not have a constructor that takes a {nameof(VirtualMachine)} or no-argument constructor");
            }
        }

        public object RemoveSystem(Type type)
        {
            IReadOnlyList<object> systems = systemRegistry.GetAllThatAre(type);
            if (systems.Count > 0)
            {
                object system = systems[0];
                int hash = system.GetHashCode();
                SystemRemoved ev = new(this, type);
                Broadcast(ref ev);
                systemRegistry.Unregister(system);
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
                SystemRemoved ev = new(this, type);
                Broadcast(ref ev);
                systemRegistry.Unregister(system);
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
            IReadOnlyList<object> systems = systemRegistry.GetAllThatAre<T>();
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

        /// <summary>
        /// Broadcasts this event to all <see cref="IListener{T}"/> of it,
        /// as well as all <see cref="IAnyListener"/> instances.
        /// </summary>
        public void Broadcast<T>(ref T ev) where T : notnull
        {
            List<object> listeners = (List<object>)systemRegistry.GetAllThatAre<IListener<T>>();
            List<object> broadcastListeners = (List<object>)systemRegistry.GetAllThatAre<IAnyListener>();
            int bufferLength = listeners.Count > broadcastListeners.Count ? listeners.Count : broadcastListeners.Count;
            using RentedBuffer<object> buffer = new(bufferLength);
            int length = listeners.Count;
            buffer.CopyFrom(listeners);
            for (int i = 0; i < length; i++)
            {
                IListener<T> listener = (IListener<T>)buffer[i];
                listener.Receive(this, ref ev);
            }

            length = broadcastListeners.Count;
            buffer.CopyFrom(broadcastListeners);
            for (int i = 0; i < length; i++)
            {
                IAnyListener listener = (IAnyListener)buffer[i];
                listener.Receive(this, ref ev);
            }
        }

        public static VirtualMachine Get(int id)
        {
            return all[id];
        }

        /// <summary>
        /// Represents the constructor and <see cref="IDisposable.Dispose"/> events of a <see cref="VirtualMachine"/>.
        /// </summary>
        public interface IState
        {
            /// <summary>
            /// Invoked when a <see cref="VirtualMachine"/> has been created.
            /// </summary>
            void Initialize(VirtualMachine vm);

            /// <summary>
            /// Invoked when a <see cref="VirtualMachine"/> is being disposed.
            /// </summary>
            void Finalize(VirtualMachine vm);
        }

        public interface IInitialData : IRegistryView
        {
        }

        internal class EmptyInitialData : IInitialData
        {
            private readonly List<object> empty = new();

            IReadOnlyList<object> IRegistryView.GetAllThatAre<T>()
            {
                return empty;
            }
        }
    }
}