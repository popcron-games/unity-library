#nullable enable
using Game.FunctionsLibrary;
using System;

namespace Game.Functions
{
    /// <summary>
    /// May return a SceneInstance or an exception.
    /// </summary>
    public readonly struct LoadScene : IFunctionDefinition
    {
        private readonly object sceneAddress;
        private readonly Action<object>? callback;

        ReadOnlySpan<char> IFunctionDefinition.Path => "unity/loadScene";
        int IFunctionDefinition.ParameterCount => 2;

        /// <summary>
        /// Creates a load scene function request.
        /// <para></para>
        /// The <paramref name="callback"/> should be used as opposed to
        /// <see cref="InvokeFunctionRequest.TryGetResult(out object?)"/> as the operation internally is asynchronous.
        /// </summary>
        public LoadScene(object sceneAddress, Action<object>? callback = null)
        {
            this.sceneAddress = sceneAddress;
            this.callback = callback;
        }

        void IFunctionDefinition.BuildInput(ref FunctionInput input)
        {
            input.Add(sceneAddress);
            input.Add(callback);
        }

        string IFunctionDefinition.GetParameterInfo(int index)
        {
            return index switch
            {
                0 => "Scene Address (string)",
                1 => "Callback (Action<object> either SceneInstance or Exception)",
                _ => "Unknown"
            };
        }
    }
}