#nullable enable
using Game.Systems;
using System;

namespace Game.FunctionsLibrary
{
    /// <summary>
    /// Implementation for a <see cref="IFunctionDefinition"/>.
    /// </summary>
    public interface IFunction
    {
        /// <summary>
        /// The expected amount of parameters for this function.
        /// </summary>
        int ParameterCount { get; }
        ReadOnlySpan<char> Path { get; }

        object? Invoke(VirtualMachine vm, FunctionInput inputs);
    }

    public interface IFunction<T> : IFunction where T : IFunctionDefinition
    {
        int IFunction.ParameterCount => FunctionSystem.GetParameterCount<T>();
        ReadOnlySpan<char> IFunction.Path => FunctionSystem.GetPath<T>();
    }
}
