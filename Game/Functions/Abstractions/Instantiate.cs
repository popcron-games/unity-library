#nullable enable
using Game.FunctionsLibrary;
using System;

namespace Game.Functions
{
    /// <summary>
    /// Asynchronously instantiates from a prefab address.
    /// </summary>
    public readonly struct Instantiate : IFunctionDefinition
    {
        private readonly object prefabAddress;
        private readonly object? parent;
        private readonly Action<object?> callback;

        ReadOnlySpan<char> IFunctionDefinition.Path => "unity/instantiate";
        int IFunctionDefinition.ParameterCount => 3;

        /// <summary>
        /// Instantiates an object asynchronously parented under an optional object.
        /// <para></para>
        /// This function does not produce a synchronous result. Use the
        /// <paramref name="callback"/> to retrieve the instantiated object.
        /// </summary>
        public Instantiate(object prefabAddress, object? parent, Action<object?> callback)
        {
            this.prefabAddress = prefabAddress;
            this.parent = parent;
            this.callback = callback;
        }

        /// <summary>
        /// Instantiates an object asynchronously parented under an optional object.
        /// <para></para>
        /// This function does not produce a synchronous result. Use the
        /// <paramref name="callback"/> to retrieve the instantiated object.
        /// </summary>
        public Instantiate(object prefabAddress, Action<object?> callback)
        {
            this.prefabAddress = prefabAddress;
            this.parent = null;
            this.callback = callback;
        }

        void IFunctionDefinition.BuildInput(ref FunctionInput input)
        {
            input.Add(prefabAddress);
            input.Add(parent);
            input.Add(callback);
        }

        string IFunctionDefinition.GetParameterInfo(int index)
        {
            return index switch
            {
                0 => "Prefab Address (Addressable)",
                1 => "Parent (Transform or null)",
                2 => "Action<object?> (null or Exception or good result)",
                _ => "Unknown"
            };
        }
    }
}