#nullable enable
using Game.FunctionsLibrary;
using System;

namespace Game.Functions
{
    public readonly struct ReleaseAsset : IFunctionDefinition
    {
        private readonly object asset;

        ReadOnlySpan<char> IFunctionDefinition.Path => "unity/releaseAsset";
        int IFunctionDefinition.ParameterCount => 1;

        public ReleaseAsset(object asset)
        {
            this.asset = asset;
        }

        void IFunctionDefinition.BuildInput(ref FunctionInput input)
        {
            input.Add(asset);
        }

        string IFunctionDefinition.GetParameterInfo(int index)
        {
            return index switch
            {
                0 => "Asset that was Loaded (Object)",
                _ => "Unknown"
            };
        }
    }
}