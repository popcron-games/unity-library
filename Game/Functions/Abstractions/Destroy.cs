#nullable enable
using Game.FunctionsLibrary;
using System;

namespace Game.Functions
{
    public readonly struct Destroy : IFunctionDefinition
    {
        private readonly object instance;

        ReadOnlySpan<char> IFunctionDefinition.Path => "unity/destroy";
        int IFunctionDefinition.ParameterCount => 1;

        public Destroy(object instance)
        {
            this.instance = instance;
        }

        void IFunctionDefinition.BuildInput(ref FunctionInput input)
        {
            input.Add(instance);
        }

        string IFunctionDefinition.GetParameterInfo(int index)
        {
            return index switch
            {
                0 => "Instance (GameObject or Component or ScriptableObject)",
                _ => "Unknown"
            };
        }
    }
}