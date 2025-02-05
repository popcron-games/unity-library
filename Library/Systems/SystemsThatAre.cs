#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityLibrary.Systems
{
    /// <summary>
    /// Makes sure that all systems that are <typeparamref name="T"/> are added to the <see cref="VirtualMachine"/> instance
    /// using reflection.
    /// </summary>
    public class SystemsThatAre<T> : IDisposable where T : notnull
    {
        private readonly HashSet<object> systemsAdded = new();
        private readonly VirtualMachine vm;
        private readonly Type desiredType;

        public SystemsThatAre(VirtualMachine vm, Creation createCallback)
        {
            this.vm = vm;
            desiredType = typeof(T);
            foreach (Assembly assembly in AssemblyCache.All)
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
                        object system = createCallback(type, vm);
                        vm.AddSystem(system);
                        systemsAdded.Add(system);
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (object system in systemsAdded)
            {
                vm.RemoveSystem(system);
                if (system is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            systemsAdded.Clear();
        }

        public delegate T Creation(Type type, VirtualMachine vm);
    }
}