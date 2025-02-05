#nullable enable
using NUnit.Framework;
using System;

namespace UnityLibrary
{
    public class ExtensionTests
    {
        [Test]
        public void TestTextHash()
        {
            for (int i = 0; i < 100; i++)
            {
                ReadOnlySpan<char> text = Guid.NewGuid().ToString().AsSpan();
                string textStr = text.ToString();
                Assert.AreEqual(text.GetDjb2HashCode(), textStr.GetDjb2HashCode());
            }
        }

        [Test]
        public void SystemsReceiveToldEvent()
        {
            using VirtualMachine vm = new(new TestState());
            TestSystem system = new(Guid.NewGuid().ToString());
            vm.AddSystem(system);

            int value = DateTime.Now.Millisecond;
            DummyTestEvent ev = new(value);
            system.Tell(vm, ref ev);

            Assert.AreEqual(system.events.Count, 1);
            Assert.AreEqual(system.events[0].value, value);
        }

        [Test]
        public void RentedArrayGetsReturnedToPool()
        {
            using RentedArray<int> array = new(10, true);
            Assert.GreaterOrEqual(array.Length, 10);
            Assert.AreEqual(array[0], 0);
            Assert.AreEqual(array[9], 0);

            array[0] = 1;
            array[9] = 2;
            Assert.AreEqual(array[0], 1);
            Assert.AreEqual(array[9], 2);

            RentedArray<int> array2 = new(10, true);
            Assert.AreEqual(array2[0], 0);
            Assert.AreEqual(array2[9], 0);

            array2.Dispose();
        }
    }
}