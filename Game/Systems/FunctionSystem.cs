#nullable enable
using Game.Events;
using Game.FunctionsLibrary;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Game.Systems
{
    /// <summary>
    /// System for building and invoking functions.
    /// <para></para>
    /// Added by <see cref="GameSystems"/>.
    /// </summary>
    public class FunctionSystem : IDisposable, IListener<TestEvent>, IListener<InvokeFunctionRequest>
    {
        private static readonly Dictionary<Type, string> functionDefinitionTypeToPath = new();
        private static readonly Dictionary<Type, IFunctionDefinition> functionDefinitionTypeToDefault = new();

        private readonly List<RequiredFunction> requiredFunctions = new();
        private readonly Dictionary<int, Func<VirtualMachine, FunctionInput, object?>> functionToCallbacks = new();
        private readonly Dictionary<int, int> functionToParameterCount = new();
        private readonly HashSet<int> functions = new();
        private readonly Dictionary<int, string> functionToPaths = new();
        private readonly Dictionary<int, IFunction> functionImplementors = new();
        private readonly VirtualMachine vm;

        public IReadOnlyList<RequiredFunction> RequiredFunctions => requiredFunctions;
        public int Count => functions.Count;

        /// <summary>
        /// All implemented functions.
        /// </summary>
        public IEnumerable<IFunction> All
        {
            get
            {
                foreach (int hash in functions)
                {
                    yield return functionImplementors[hash];
                }
            }
        }

        public FunctionSystem(VirtualMachine vm)
        {
            this.vm = vm;
        }

        void IListener<TestEvent>.Receive(VirtualMachine vm, ref TestEvent e)
        {
            foreach (RequiredFunction required in RequiredFunctions)
            {
                if (!HasFunction(required.path, required.declaration.parameterCount))
                {
                    throw new Exception($"Function with path {required.path} is required but isn't implemented.");
                }

                int functionHash = required.declaration.GetHashCode();
                IFunction function = functionImplementors[functionHash];
                using RentedBuffer<object> inputBuffer = new(FunctionInput.MaxParameterCount);
                FunctionInput testInput = new(inputBuffer);
                IFunctionDefinition definition = (IFunctionDefinition)FormatterServices.GetUninitializedObject(required.definitionType);
                definition.BuildInput(ref testInput);
                int builtParamCount = testInput.ParameterCount;
                GC.SuppressFinalize(definition);
                if (function.ParameterCount != builtParamCount)
                {
                    throw new Exception($"Function with path {required.path} has {function.ParameterCount} parameters but {builtParamCount} were built.");
                }
            }
        }

        void IListener<InvokeFunctionRequest>.Receive(VirtualMachine vm, ref InvokeFunctionRequest e)
        {
            if (!e.Handled)
            {
                e.Handle(Invoke(e.Function));
            }
        }

        public void Dispose()
        {
            requiredFunctions.Clear();
        }

        public object? Invoke(IFunctionDefinition function)
        {
            ReadOnlySpan<char> path = function.Path;
            int parameterCount = GetParameterCount(function.GetType());
            int functionHash = new FunctionDeclaration(path, parameterCount).GetHashCode();
            if (functionToCallbacks.TryGetValue(functionHash, out var f))
            {
                using RentedBuffer<object> inputBuffer = new(parameterCount);
                FunctionInput input = new(inputBuffer);
                function.BuildInput(ref input);
                return f.Invoke(vm, input);
            }
            else
            {
                throw new Exception($"Function with path {path.ToString()} does not exist");
            }
        }

        public object? Invoke(ReadOnlySpan<char> path)
        {
            FunctionDeclaration dec = new(path, 0);
            int functionHash = dec.GetHashCode();
            if (functionToCallbacks.TryGetValue(functionHash, out var f))
            {
                return f.Invoke(vm, new FunctionInput());
            }
            else
            {
                throw new Exception($"Function with path {path.ToString()} does not exist");
            }
        }

        public object? Invoke(ReadOnlySpan<char> path, object? p1)
        {
            FunctionDeclaration dec = new(path, 1);
            int functionHash = dec.GetHashCode();
            if (functionToCallbacks.TryGetValue(functionHash, out var f))
            {
                using RentedBuffer<object?> buffer = new(1);
                buffer[0] = p1;
                return f.Invoke(vm, new FunctionInput(buffer));
            }
            else
            {
                throw new Exception($"Function with path {path.ToString()} and parameter {p1} does not exist");
            }
        }

        public object? Invoke(ReadOnlySpan<char> path, object? p1, object? p2)
        {
            FunctionDeclaration dec = new(path, 2);
            int functionHash = dec.GetHashCode();
            if (functionToCallbacks.TryGetValue(functionHash, out var f))
            {
                using RentedBuffer<object?> buffer = new(2);
                buffer[0] = p1;
                buffer[1] = p2;
                return f.Invoke(vm, new FunctionInput(buffer));
            }
            else
            {
                throw new Exception($"Function with path {path.ToString()} and parameters {p1}, {p2} does not exist");
            }
        }

        public object? Invoke(ReadOnlySpan<char> path, object? p1, object? p2, object? p3)
        {
            FunctionDeclaration dec = new(path, 3);
            int functionHash = dec.GetHashCode();
            if (functionToCallbacks.TryGetValue(functionHash, out var f))
            {
                using RentedBuffer<object?> buffer = new(3);
                buffer[0] = p1;
                buffer[1] = p2;
                buffer[2] = p3;
                return f.Invoke(vm, new FunctionInput(buffer));
            }
            else
            {
                throw new Exception($"Function with path {path.ToString()} and parameters {p1}, {p2}, {p3} does not exist");
            }
        }

        public object? Invoke(ReadOnlySpan<char> path, object? p1, object? p2, object? p3, object? p4)
        {
            FunctionDeclaration dec = new(path, 4);
            int functionHash = dec.GetHashCode();
            if (functionToCallbacks.TryGetValue(functionHash, out var f))
            {
                using RentedBuffer<object?> buffer = new(4);
                buffer[0] = p1;
                buffer[1] = p2;
                buffer[2] = p3;
                buffer[3] = p4;
                return f.Invoke(vm, new FunctionInput(buffer));
            }
            else
            {
                throw new Exception($"Function with path {path.ToString()} and parameters {p1}, {p2}, {p3}, {p4} does not exist");
            }
        }

        private void RequireFunction(RequiredFunction required)
        {
            if (requiredFunctions.Contains(required))
            {
                throw new Exception($"Function with path {required.path} is already required");
            }
            else
            {
                requiredFunctions.Add(required);
            }
        }

        public void RequireFunction<T>() where T : struct, IFunctionDefinition
        {
            ReadOnlySpan<char> path = GetPath<T>();
            int parameterCount = GetParameterCount<T>();
            RequireFunction(new RequiredFunction(path.ToString(), new(path, parameterCount), typeof(T)));
        }

        public void ImplementFunction<T>(T function) where T : IFunction
        {
            ReadOnlySpan<char> path = function.Path;
            FunctionDeclaration dec = new(path, function.ParameterCount);
            int functionHash = dec.GetHashCode();
            if (functions.Contains(functionHash))
            {
                throw new Exception($"Function with path {path.ToString()} is already registered");
            }
            else
            {
                functionToCallbacks.Add(functionHash, (vm, inputs) => function.Invoke(vm, inputs));
                functions.Add(functionHash);
                functionToParameterCount.Add(path.GetDjb2HashCode(), function.ParameterCount);
                functionToPaths.Add(functionHash, path.ToString());
                functionImplementors.Add(functionHash, function);

                //remove from required functions
                for (int i = requiredFunctions.Count - 1; i >= 0; i--)
                {
                    RequiredFunction required = requiredFunctions[i];
                    if (required.path.AsSpan().SequenceEqual(path))
                    {
                        requiredFunctions.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public bool HasFunction(ReadOnlySpan<char> path, int paramaterCount = 0)
        {
            FunctionDeclaration dec = new(path, paramaterCount);
            int functionHash = dec.GetHashCode();
            return functions.Contains(functionHash);
        }

        /// <summary>
        /// Retreives the path of the function.
        /// </summary>
        public static ReadOnlySpan<char> GetPath<T>() where T : IFunctionDefinition
        {
            if (functionDefinitionTypeToPath.TryGetValue(typeof(T), out string? path))
            {
                return path.AsSpan();
            }
            else
            {
                T tempInstance = (T)FormatterServices.GetUninitializedObject(typeof(T));
                path = tempInstance.Path.ToString();
                functionDefinitionTypeToPath.Add(typeof(T), path);
                GC.SuppressFinalize(tempInstance);
                return path.AsSpan();
            }
        }

        public static int GetParameterCount<T>() where T : IFunctionDefinition
        {
            return GetParameterCount(typeof(T));
        }

        public static int GetParameterCount(Type type)
        {
            if (!functionDefinitionTypeToDefault.TryGetValue(type, out IFunctionDefinition? value))
            {
                value = (IFunctionDefinition)FormatterServices.GetUninitializedObject(type);
                functionDefinitionTypeToDefault.Add(type, value);
            }

            return value.ParameterCount;
        }

        public static string? GetParameterInfo(Type type, int parameterIndex)
        {
            if (!functionDefinitionTypeToDefault.TryGetValue(type, out IFunctionDefinition? value))
            {
                value = (IFunctionDefinition)FormatterServices.GetUninitializedObject(type);
                functionDefinitionTypeToDefault.Add(type, value);
            }

            return value.GetParameterInfo(parameterIndex);
        }

        public readonly struct RequiredFunction
        {
            public readonly string path;
            public readonly FunctionDeclaration declaration;
            public readonly Type definitionType;

            public RequiredFunction(string path, FunctionDeclaration declaration, Type definitionType)
            {
                this.path = path;
                this.declaration = declaration;
                this.definitionType = definitionType;
            }
        }
    }
}