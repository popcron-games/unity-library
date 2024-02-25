#nullable enable
using System;
using Game;
using Game.Systems;
using NUnit.Framework;

namespace UnityLibrary
{
    public class VirtualMachineTests
    {
        [Test]
        public void TestVMInitialization1()
        {
            TestState state = new();
            using VirtualMachine vm = new(0, state);
            Assert.IsTrue(state.initialized);
        }

        [Test]
        public void TestDisposing1()
        {
            TestState state = new();
            VirtualMachine vm = new(0, state);
            vm.Dispose();
            Assert.IsTrue(state.finalized);
        }

        [Test]
        public void TestDisposingTwice()
        {
            TestState state = new();
            VirtualMachine vm = new(0, state);
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
            TestState state = new();
            using VirtualMachine vm = new(0, state);
            vm.AddSystem(new TestSystem(value));
            Assert.IsTrue(vm.ContainsSystem<TestSystem>());
        }

        [Test]
        public void TestRetreivingSystem()
        {
            string value = Guid.NewGuid().ToString();
            TestState state = new();
            using VirtualMachine vm = new(0, state);
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
            TestState state = new();
            using VirtualMachine vm = new(0, state);
            int hash = vm.AddSystem(new TestSystem(value));
            Assert.IsTrue(vm.ContainsSystem(hash));

            vm.RemoveSystem(hash);
            Assert.IsFalse(vm.ContainsSystem(hash));
        }

        [Test]
        public void TestBroadcasting()
        {
            using VirtualMachine vm = new(0, new TestState());
            TestSystem system = new(Guid.NewGuid().ToString());
            vm.AddSystem(system);

            int value = Guid.NewGuid().GetHashCode();
            var ev = new TestEvent(value);
            vm.Broadcast(ref ev);

            Assert.AreEqual(system.events.Count, 1);
            Assert.AreEqual(system.events[0].value, value);
        }

        [Test]
        public void TestSystemsThatAre()
        {
            using VirtualMachine vm = new(0, new TestState());
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
            using VirtualMachine vm = new(0, new TestState());
            vm.AddSystem<SystemsWithAttribute<SystemAttribute>>();
            Assert.IsTrue(vm.ContainsSystem<Aeroplane>());
            Assert.IsTrue(vm.ContainsSystem<Submarine>());
            Assert.IsTrue(vm.ContainsSystem<Computer>());

            Aeroplane aeroplane = vm.GetSystem<Aeroplane>();
            vm.RemoveSystem<SystemsWithAttribute<SystemAttribute>>().Dispose();
            Assert.IsFalse(vm.ContainsSystem<Aeroplane>());
            Assert.IsFalse(vm.ContainsSystem<Submarine>());
            Assert.IsFalse(vm.ContainsSystem<Computer>());
            Assert.IsTrue(aeroplane.finalized);
        }

        [Test]
        public void TestCreatingSystems()
        {
            VirtualMachine vm = new(0, new TestState());
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
            using VirtualMachine vm = new(0, new TestState());
            vm.AddSystem(new SystemsThatAre<IMachine>(vm, CreateSystem));
            var machines = vm.GetSystemsThatAre<IMachine>();
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