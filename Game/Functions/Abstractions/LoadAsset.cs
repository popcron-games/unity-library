#nullable enable
using Game.FunctionsLibrary;
using System;

namespace Game.Functions
{
    public readonly struct LoadAsset : IFunctionDefinition
    {
        private readonly object assetAddress;
        private readonly Action<object?> callback;

        ReadOnlySpan<char> IFunctionDefinition.Path => "unity/loadAsset";
        int IFunctionDefinition.ParameterCount => 2;

        public LoadAsset(object assetAddress, Action<object?> callback)
        {
            this.assetAddress = assetAddress;
            this.callback = callback;
        }

        void IFunctionDefinition.BuildInput(ref FunctionInput input)
        {
            input.Add(assetAddress);
            input.Add(callback);
        }

        string IFunctionDefinition.GetParameterInfo(int index)
        {
            return index switch
            {
                0 => "Asset Address (string)",
                1 => "Callback (Action<Object>)",
                _ => "Unknown"
            };
        }
    }
}