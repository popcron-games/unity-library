#nullable enable
using Game.FunctionsLibrary;
using System;

namespace Game.Functions
{
    public readonly struct LoadAssetsFromAssetsDatabase : IFunctionDefinition
    {
        private readonly string searchFilter;
        private readonly Type type;

        ReadOnlySpan<char> IFunctionDefinition.Path => "unity/editor/assetsDatabase/loadAssets";
        int IFunctionDefinition.ParameterCount => 2;

        public LoadAssetsFromAssetsDatabase(Type assetType)
        {
            searchFilter = $"t:{assetType.FullName}";
            type = assetType;
        }

        public LoadAssetsFromAssetsDatabase(string searchFilter, Type assetType)
        {
            this.searchFilter = searchFilter;
            type = assetType;
        }

        void IFunctionDefinition.BuildInput(ref FunctionInput input)
        {
            input.Add(searchFilter);
            input.Add(type);
        }

        string IFunctionDefinition.GetParameterInfo(int index)
        {
            return index switch
            {
                0 => "Search Filter (string)",
                1 => "Asset Type (Type)",
                _ => "Unknown"
            };
        }
    }
}