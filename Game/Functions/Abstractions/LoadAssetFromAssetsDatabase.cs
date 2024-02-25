#nullable enable
using Game.FunctionsLibrary;
using System;

namespace Game.Functions
{
    public readonly struct LoadAssetFromAssetsDatabase : IFunctionDefinition
    {
        private readonly string assetPath;

        ReadOnlySpan<char> IFunctionDefinition.Path => "unity/editor/assetsDatabase/loadAsset";
        int IFunctionDefinition.ParameterCount => 1;

        public LoadAssetFromAssetsDatabase(string assetPath)
        {
            this.assetPath = assetPath;
        }

        void IFunctionDefinition.BuildInput(ref FunctionInput input)
        {
            input.Add(assetPath);
        }

        string IFunctionDefinition.GetParameterInfo(int index)
        {
            return index switch
            {
                0 => "Asset Path (string)",
                _ => "Unknown"
            };
        }
    }
}