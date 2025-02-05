#nullable enable
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityLibrary.Systems;

namespace UnityLibrary
{
    public class VirtualMachineTests
    {
        private VirtualMachine CreateVM(out TestState state)
        {
            state = new();
            VirtualMachine vm = new(state);
            return vm;
        }

        [Test]
        public void TestVMInitialization1()
        {
            using VirtualMachine vm = CreateVM(out TestState state);
            Assert.IsTrue(state.initialized);
        }

        [Test]
        public void TestDisposing1()
        {
            VirtualMachine vm = CreateVM(out TestState state);
            vm.Dispose();
            Assert.IsTrue(state.finalized);
        }

        [Test]
        public void TestDisposingTwice()
        {
            VirtualMachine vm = CreateVM(out TestState state);
            vm.Dispose();
            Assert.Throws<Exception>(() =>
            {
                vm.Dispose();
            });
        }

        [Test]
        public void TestAddingSystems1()
        {
            string value = Guid.NewGuid().ToString();
            using VirtualMachine vm = CreateVM(out TestState state);
            vm.AddSystem(new TestSystem(value));
            Assert.IsTrue(vm.ContainsSystem<TestSystem>());
        }

        [Test]
        public void TestRetreivingSystem()
        {
            string value = Guid.NewGuid().ToString();
            using VirtualMachine vm = CreateVM(out TestState state);
            int hash = vm.AddSystem(new TestSystem(value));
            Assert.IsTrue(vm.ContainsSystem(hash));

            TestSystem system = vm.GetSystem<TestSystem>();
            Assert.AreEqual(system.value, value);

            if (vm.TryGetSystem(out TestSystem? system2))
            {
                Assert.AreEqual(system2.value, value);
            }
            else
            {
                Assert.Fail("Expected system not found");
            }
        }

        [Test]
        public void TestRemovingSystems1()
        {
            string value = Guid.NewGuid().ToString();
            using VirtualMachine vm = CreateVM(out TestState state);
            int hash = vm.AddSystem(new TestSystem(value));
            Assert.IsTrue(vm.ContainsSystem(hash));

            vm.RemoveSystem(hash);
            Assert.IsFalse(vm.ContainsSystem(hash));
        }

        [Test]
        public void RestRemoveSystemByType()
        {
            using VirtualMachine vm = CreateVM(out TestState state);
            vm.AddSystem(new TestSystem(Guid.NewGuid().ToString()));
            Assert.IsTrue(vm.ContainsSystem<TestSystem>());
            vm.RemoveSystem<TestSystem>();
            Assert.IsFalse(vm.ContainsSystem<TestSystem>());
        }

        [Test]
        public void TestBroadcasting()
        {
            using VirtualMachine vm = CreateVM(out TestState state);
            TestSystem system = new(Guid.NewGuid().ToString());
            vm.AddSystem(system);

            int value = Guid.NewGuid().GetHashCode();
            vm.Broadcast(new DummyTestEvent(value));

            Assert.AreEqual(system.events.Count, 1);
            Assert.AreEqual(system.events[0].value, value);
        }

        [Test]
        public void TestSystemsThatAre()
        {
            using VirtualMachine vm = CreateVM(out TestState state);
            vm.AddSystem(new SystemsThatAre<IMachine>(vm, CreateSystem));
            Assert.IsTrue(vm.ContainsSystem<Aeroplane>());
            Assert.IsTrue(vm.ContainsSystem<Submarine>());
            Assert.IsTrue(vm.ContainsSystem<Computer>());

            Aeroplane aeroplane = vm.GetSystem<Aeroplane>();
            vm.RemoveSystem<SystemsThatAre<IMachine>>().Dispose();
            Assert.IsFalse(vm.ContainsSystem<Aeroplane>());
            Assert.IsFalse(vm.ContainsSystem<Submarine>());
            Assert.IsFalse(vm.ContainsSystem<Computer>());
            Assert.IsTrue(aeroplane.finalized);
        }

        [Test]
        public void TestSystemsThatAreWithAttribute()
        {
            using VirtualMachine vm = CreateVM(out TestState state);
            vm.AddSystem<SystemsWithAttribute<SystemAttribute>>();
            Assert.IsTrue(vm.ContainsSystem<Aeroplane>());
            Assert.IsTrue(vm.ContainsSystem<Submarine>());
            Assert.IsTrue(vm.ContainsSystem<Computer>());

            Aeroplane aeroplane = vm.GetSystem<Aeroplane>();
            Assert.AreEqual(4, vm.Systems.Count);

            vm.RemoveSystem<SystemsWithAttribute<SystemAttribute>>().Dispose();
            Assert.AreEqual(0, vm.Systems.Count);
            Debug.Log(vm.Systems.Count);
            Assert.IsFalse(vm.ContainsSystem<Aeroplane>());
            Assert.IsFalse(vm.ContainsSystem<Submarine>());
            Assert.IsFalse(vm.ContainsSystem<Computer>());
            Assert.IsTrue(aeroplane.finalized);
        }

        [Test]
        public void TestCreatingSystems()
        {
            VirtualMachine vm = CreateVM(out TestState state);
            object instance = vm.AddSystem(typeof(Aeroplane));
            int hash = instance.GetHashCode();
            Assert.IsTrue(vm.ContainsSystem(hash));
            Aeroplane aeroplane = (Aeroplane)vm.GetSystem(hash);
            Assert.AreEqual(instance, aeroplane);
            vm.Dispose();
            Assert.IsTrue(aeroplane.finalized);
        }

        [Test]
        public void TestSystemsThatAre2()
        {
            using VirtualMachine vm = CreateVM(out TestState state);
            vm.AddSystem(new SystemsThatAre<IMachine>(vm, CreateSystem));
            IReadOnlyList<IMachine> machines = vm.GetSystemsThatAre<IMachine>();
            Assert.AreEqual(3, machines.Count);
        }

        private IMachine CreateSystem(Type type, VirtualMachine vm)
        {
            return (IMachine)Activator.CreateInstance(type);
        }

        public interface IMachine
        {
        }

        public class SystemAttribute : Attribute
        {
        }

        [System]
        public class Aeroplane : IMachine, IDisposable
        {
            public bool finalized;

            public void Dispose()
            {
                finalized = true;
            }
        }

        [System]
        public class Submarine : IMachine
        {
        }

        [System]
        public class Computer : IMachine
        {
        }
    }
}