#nullable enable
using NUnit.Framework;
using System;
using UnityLibrary.Events;
using UnityLibrary.Systems;

namespace UnityLibrary
{
    public class FunctionTests
    {
        [Test]
        public void CallFunction()
        {
            using VirtualMachine vm = new(new TestState());
            FunctionSystem functions = new(vm);
            vm.AddSystem(functions);

            functions.ImplementFunction("checkNumber", DoCheckNumber);
            object? result = functions.Invoke("checkNumber");
            Assert.AreEqual(5, result);
        }

        [Test]
        public void RequireFunction()
        {
            using VirtualMachine vm = new(new TestState());
            FunctionSystem functions = new(vm);
            vm.AddSystem(functions);

            functions.RequireFunction<CheckNumber>();
            Assert.Throws<Exception>(() => vm.Broadcast(new Validate()));
        }

        [Test]
        public void CallFunctionDefinition()
        {
            using VirtualMachine vm = new(new TestState());
            FunctionSystem functions = new(vm);
            vm.AddSystem(functions);

            functions.ImplementFunction(new CheckNumberFunction());
            object? result = functions.Invoke(new CheckNumber());
            Assert.AreEqual(5, result);
        }

        private object? DoCheckNumber(VirtualMachine vm, FunctionInput input)
        {
            return 5;
        }

        public readonly struct CheckNumber : IFunctionDefinition
        {
            int IFunctionDefinition.ParameterCount => 0;

            ReadOnlySpan<char> IFunctionDefinition.Path => "checkNumber";

            void IFunctionDefinition.BuildInput(ref FunctionInput input)
            {
            }

            string IFunctionDefinition.GetParameterInfo(int index)
            {
                return string.Empty;
            }
        }

        public class CheckNumberFunction : IFunction<CheckNumber>
        {
            object? IFunction.Invoke(VirtualMachine vm, FunctionInput inputs)
            {
                return 5;
            }
        }
    }
}