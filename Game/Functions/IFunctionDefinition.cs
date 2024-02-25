#nullable enable
using System;

namespace Game.FunctionsLibrary
{
    /// <summary>
    /// For defining a function signature and its input, not the implementation.
    /// <para></para>
    /// Types that implement this interface can be made required for an 
    /// implementor to exist with <see cref="Systems.FunctionSystem.RequireFunction{T}()"/>,
    /// where the <see cref="Events.TestEvent"/> may fail build/play if not implemented
    /// using a <see cref="IFunction"/>
    /// </summary>
    public interface IFunctionDefinition
    {
        int ParameterCount { get; }
        ReadOnlySpan<char> Path { get; }

        string GetParameterInfo(int index);
        void BuildInput(ref FunctionInput input);
    }
}
