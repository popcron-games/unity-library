#nullable enable
using System.Collections.Generic;
using System.Reflection;

namespace System
{
    /// <summary>
    /// Stores a cache of every <see cref="Assembly"/> in the current <see cref="AppDomain"/>.
    /// </summary>
    public static class AssemblyCache
    {
        private static readonly Assembly[] all;

        public static IReadOnlyList<Assembly> All => all;

        static AssemblyCache()
        {
            List<Assembly> assemblies = new();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                assemblies.Add(assembly);
            }

            all = assemblies.ToArray();
        }
    }
}