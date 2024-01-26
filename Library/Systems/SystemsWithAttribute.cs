#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Library.Systems.Extra
{
    /// <summary>
    /// Makes sure that all type declarations with <typeparamref name="T"/> attributes are added to the <see cref="VirtualMachine"/> instance.
    /// </summary>
    public class SystemsWithAttribute<T> : IDisposable where T : Attribute
    {
        private readonly HashSet<int> instancesAdded = new();
        private readonly VirtualMachine vm;

        public SystemsWithAttribute(VirtualMachine vm)
        {
            this.vm = vm;
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
                    if (type.GetCustomAttribute<T>() is T)
                    {
                        if (type.GetConstructor(new Type[1] { typeof(VirtualMachine) }) is ConstructorInfo behaviourConstructor)
                        {
                            object system = behaviourConstructor.Invoke(new object[1] { vm });
                            int hash = vm.AddSystem(system);
                            instancesAdded.Add(hash);
                        }
                        else if (type.GetConstructor(new Type[0]) is ConstructorInfo constructor)
                        {
                            object system = constructor.Invoke(null);
                            int hash = vm.AddSystem(system);
                            instancesAdded.Add(hash);
                        }
                        else
                        {
                            throw new Exception($"System {type} does not have a constructor with no parameters or a constructor with a single parameter of type {typeof(VirtualMachine)}");
                        }
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
    }
}