#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityLibrary.Unity
{
    public static class MemberDrawers
    {
        private static readonly Dictionary<Type, Type> typeToDrawerType = new();
        private static readonly Dictionary<Type, IMemberDrawer> typeToDrawer = new();

        static MemberDrawers()
        {
            GetMemberDrawerTypes();
        }

        public static IMemberDrawer Get<T>()
        {
            if (TryGet(typeof(T), out IMemberDrawer? drawer))
            {
                return drawer;
            }
            else
            {
                throw new InvalidOperationException($"No member drawer found for type {typeof(T).Name}");
            }
        }

        public static IMemberDrawer Get(Type type)
        {
            if (TryGet(type, out IMemberDrawer? drawer))
            {
                return drawer;
            }
            else
            {
                throw new InvalidOperationException($"No member drawer found for type {type.Name}");
            }
        }

        public static bool TryGet(Type type, [NotNullWhen(true)] out IMemberDrawer? drawer)
        {
            return typeToDrawer.TryGetValue(type, out drawer);
        }

        public static void RemoveDueToException(Type type)
        {
            if (typeToDrawerType.ContainsKey(type))
            {
                typeToDrawerType.Remove(type);
                typeToDrawer.Remove(type);
            }
            else
            {
                throw new InvalidOperationException($"No member drawer found for type {type.Name} to remove");
            }
        }

#if UNITY_EDITOR
        private static void GetMemberDrawerTypes()
        {
            foreach (Type drawerType in TypeCache.GetTypesDerivedFrom<IMemberDrawer>())
            {
                if (!drawerType.IsPublic) continue;
                if (drawerType.IsAbstract) continue;
                if (drawerType.IsInterface) continue;

                IMemberDrawer drawer = (IMemberDrawer)Activator.CreateInstance(drawerType);
                typeToDrawerType.Add(drawer.ValueType, drawerType);
                typeToDrawer.Add(drawer.ValueType, drawer);
            }
        }
#else
        private static void GetMemberDrawerTypes()
        {
        }
#endif
    }
}
