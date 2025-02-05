#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityLibrary.Systems
{
    /// <summary>
    /// Makes sure that all type declarations with <typeparamref name="T"/> attributes are added to the <see cref="VirtualMachine"/> instance
    /// using reflection.
    /// </summary>
    public class SystemsWithAttribute<T> : IDisposable where T : Attribute
    {
        private readonly HashSet<object> systemsAdded = new();
        private readonly VirtualMachine vm;

        public SystemsWithAttribute(VirtualMachine vm, Creation createCallback)
        {
            this.vm = vm;
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
                    if (type.GetCustomAttribute<T>() is T)
                    {
                        T system = createCallback(type, vm);
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