#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityLibrary.Drawers
{
    public static class MemberDrawers
    {
        private static readonly List<IMemberDrawer> drawers = new();

        static MemberDrawers()
        {
            GetMemberDrawerTypes();
        }

        public static bool TryGet(Type type, [NotNullWhen(true)] out IMemberDrawer? drawer)
        {
            foreach (IMemberDrawer option in drawers)
            {
                if (option.ValueType.IsAssignableFrom(type))
                {
                    drawer = option;
                    return true;
                }
            }

            drawer = null;
            return false;
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
                drawers.Add(drawer);
            }
        }
#else
        private static void GetMemberDrawerTypes()
        {
        }
#endif
    }
}
