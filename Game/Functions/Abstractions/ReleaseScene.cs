#nullable enable
using Game.FunctionsLibrary;
using System;

namespace Game.Functions
{
    public readonly struct ReleaseScene : IFunctionDefinition
    {
        private readonly object firstArg;
        private readonly Action? callback;

        ReadOnlySpan<char> IFunctionDefinition.Path => "unity/releaseScene";
        int IFunctionDefinition.ParameterCount => 2;

        public ReleaseScene(string sceneNameOrPath, Action? callback = null)
        {
            firstArg = sceneNameOrPath;
            this.callback = callback;
        }

        public ReleaseScene(object loadAddress, Action? callback = null)
        {
            firstArg = loadAddress;
            this.callback = callback;
        }

        void IFunctionDefinition.BuildInput(ref FunctionInput input)
        {
            input.Add(firstArg);
            input.Add(callback);
        }

        string IFunctionDefinition.GetParameterInfo(int index)
        {
            return index switch
            {
                0 => "Scene Name or Path (string)",
                1 => "Callback (Action for when finished)",
                _ => "Unknown"
            };
        }
    }
}