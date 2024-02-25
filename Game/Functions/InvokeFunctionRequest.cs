#nullable enable
using Game.FunctionsLibrary;

namespace Game
{
    /// <summary>
    /// Handled by <see cref="Systems.FunctionSystem"/>. After broadcasting this
    /// as an event, the <see cref="result"/> member can be used to get the result.
    /// The <see cref="handled"/> member will be true if the function was handled
    /// (to separate the difference of null due to not being handled, and null due to being actually null yet handled).
    /// </summary>
    public struct InvokeFunctionRequest
    {
        private IFunctionDefinition function;
        private object? result;
        private bool handled;

        public bool Handled => handled;
        public IFunctionDefinition Function => function;

        public InvokeFunctionRequest(IFunctionDefinition function)
        {
            this.function = function;
            this.result = null;
            this.handled = false;
        }

        internal void Handle(object? v)
        {
            if (handled)
            {
                throw new System.Exception($"Function {function} already handled");
            }

            handled = true;
            result = v;
        }

        /// <summary>
        /// Retrieves the result of the function immediately after asking
        /// to invoke it by broadcasting (assumes function is synchronous).
        /// <para></para>
        /// If this function is required to handled, use the
        /// <see cref="Systems.FunctionSystem.RequireFunction{T}()"/> to
        /// indicate that the function must be handled.
        /// </summary>
        /// <returns>True if handled.</returns>
        public bool TryGetResult(out object? possibleValue)
        {
            possibleValue = result;
            return handled;
        }
    }
}
