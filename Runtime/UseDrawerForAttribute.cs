#nullable enable
using System;
using UnityLibrary.Drawers;

namespace UnityLibrary
{
    /// <summary>
    /// Makes a field or property be drawn by a <see cref="IMemberDrawer"/>
    /// specifically for the given type.
    /// </summary>
    public class UseDrawerForAttribute : Attribute
    {
        public Type type;

        public UseDrawerForAttribute(Type type)
        {
            this.type = type;
        }
    }
}
