#nullable enable
using Game;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Game.Systems
{
    /// <summary>
    /// Makes sure that all systems that are <typeparamref name="T"/> are added to the <see cref="VirtualMachine"/> instance.
    /// </summary>
    public class SystemsThatAre<T> : IDisposable where T : notnull
    {
        private readonly HashSet<int> instancesAdded = new();
        private readonly VirtualMachine vm;
        private readonly Type desiredType;

        public SystemsThatAre(VirtualMachine vm, Handle handleCallback)
        {
            this.vm = vm;
            desiredType = typeof(T);
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetExportedTypes();
                }
                catch
                {
                    continue;
                }

                foreach (Type type in types)
                {
                    if (type.IsAbstract) continue;
                    if (type.IsInterface) continue;
                    if (desiredType.IsAssignableFrom(type))
                    {
                        object system = handleCallback(type, vm);
                        int hash = vm.AddSystem(system);
                        instancesAdded.Add(hash);
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (int hash in instancesAdded)
            {
                if (vm.RemoveSystem(hash) is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            instancesAdded.Clear();
        }

        public delegate T Handle(Type type, VirtualMachine vm);
    }
}