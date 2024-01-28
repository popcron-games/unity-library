#nullable enable
using System;
using NUnit.Framework;

namespace Library
{
    public class VirtualMachineTests
    {
        [Test]
        public void TestVMInitialization1()
        {
            TestState state = new();
            using VirtualMachine vm = new(0, state);
            vm.Initialize();
            Assert.IsTrue(state.initialized);
        }

        [Test]
        public void TestDisposing1()
        {
            TestState state = new();
            VirtualMachine vm = new(0, state);
            vm.Initialize();
            vm.Dispose();
            Assert.IsTrue(state.finalized);
        }

        [Test]
        public void TestInitializingTwice()
        {
            TestState state = new();
            using VirtualMachine vm = new(0, state);
            vm.Initialize();
            Assert.Throws<Exception>(() =>
            {
                vm.Initialize();
            });
        }

        [Test]
        public void TestDisposingWithoutInitialize()
        {
            TestState state = new();
            VirtualMachine vm = new(0, state);
            Assert.Throws<Exception>(() =>
            {
                vm.Dispose();
            });
        }

        [Test]
        public void TestInitializingAfterDispose()
        {
            TestState state = new();
            VirtualMachine vm = new(0, state);
            vm.Initialize();
            vm.Dispose();
            Assert.Throws<Exception>(() =>
            {
                vm.Initialize();
            });
        }

        [Test]
        public void TestAddingSystems1()
        {
            string value = Guid.NewGuid().ToString();
            TestState state = new();
            using VirtualMachine vm = new(0, state);
            vm.Initialize();
            vm.AddSystem(new TestSystem(value));
            Assert.IsTrue(vm.ContainsSystem<TestSystem>());
        }

        [Test]
        public void TestRetreivingSystem()
        {
            string value = Guid.NewGuid().ToString();
            TestState state = new();
            using VirtualMachine vm = new(0, state);
            vm.Initialize();
            int hash = vm.AddSystem(new TestSystem(value));
            Assert.IsTrue(vm.ContainsSystem(hash));

            TestSystem system = vm.GetSystem<TestSystem>();
            Assert.AreEqual(system.value, value);
        }

        [Test]
        public void TestRemovingSystems1()
        {
            string value = Guid.NewGuid().ToString();
            TestState state = new();
            using VirtualMachine vm = new(0, state);
            vm.Initialize();
            int hash = vm.AddSystem(new TestSystem(value));
            Assert.IsTrue(vm.ContainsSystem(hash));

            vm.RemoveSystem(hash);
            Assert.IsFalse(vm.ContainsSystem(hash));
        }

        [Test]
        public void TestBroadcasting()
        {
            using VirtualMachine vm = new(0, new TestState());
            vm.Initialize();
            TestSystem system = new(Guid.NewGuid().ToString());
            vm.AddSystem(system);

            int value = DateTime.Now.Millisecond;
            vm.Broadcast(new TestEvent(value));

            Assert.AreEqual(system.events.Count, 1);
            Assert.AreEqual(system.events[0].value, value);
        }
    }
}