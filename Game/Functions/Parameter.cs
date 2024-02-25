#nullable enable
using System;

namespace UnityLibrary
{
    public readonly struct Parameter
    {
        private readonly int hash;

        private Parameter(int hash)
        {
            this.hash = hash;
        }

        public Parameter(string typeName)
        {
            hash = typeName.GetDjb2HashCode();
        }

        public Parameter(ReadOnlySpan<char> typeName)
        {
            hash = typeName.GetDjb2HashCode();
        }

        public Parameter(Type? type)
        {
            if (type != null)
            {
                hash = type.FullName.GetDjb2HashCode();
            }
            else
            {
                hash = 0;
            }
        }

        public override int GetHashCode()
        {
            return hash;
        }
    }
}
