#nullable enable
using System;

namespace Game.FunctionsLibrary
{
    public readonly struct FunctionDeclaration
    {
        public readonly int parameterCount;
        public readonly int pathHash;

        public FunctionDeclaration(ReadOnlySpan<char> path, int parameterCount)
        {
            pathHash = path.GetDjb2HashCode();
            this.parameterCount = parameterCount;
        }

        public FunctionDeclaration(int pathHash, int parameterCount)
        {
            this.pathHash = pathHash;
            this.parameterCount = parameterCount;
        }

        public override int GetHashCode()
        {
            return pathHash * (parameterCount + 1);
        }
    }
}
