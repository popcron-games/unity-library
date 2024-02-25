#nullable enable
namespace Game.FunctionsLibrary
{
    public struct FunctionInput
    {
        public const int MaxParameterCount = 16;

        private int parameterCount;
        private readonly object?[] parameters;

        public readonly int ParameterCount => parameterCount;

        public FunctionInput(object?[] parameters)
        {
            parameterCount = 0;
            this.parameters = parameters;
        }

        public void Clear()
        {
            parameterCount = 0;
        }

        public void Add<T>(T value)
        {
            if (parameterCount >= MaxParameterCount)
            {
                throw new System.Exception("Too many parameters");
            }

            parameters[parameterCount++] = value;
        }

        public readonly object? Get(int index)
        {
            return parameters[index];
        }

        public readonly bool TryGet(int index, out object? value)
        {
            if (index >= 0 && index < parameterCount)
            {
                value = parameters[index];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
