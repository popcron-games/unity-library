#nullable enable
using NUnit.Framework;
using System.Collections.Generic;

namespace UnityLibrary
{
    public class RegistryTests
    {
        [Test]
        public void TestAdding()
        {
            Registry registry = new();
            registry.Register(new Apple());
            Assert.AreEqual(1, registry.Count);

            registry.Register(new Beets());
            Assert.AreEqual(2, registry.Count);

            registry.Register(new Coal());
            Assert.AreEqual(3, registry.Count);
            Assert.AreEqual(3, registry.All.Count);
        }

        [Test]
        public void TestRemoving()
        {
            Registry registry = new();
            Apple apple = new();
            registry.Register(apple);
            Assert.AreEqual(1, registry.Count);

            registry.Register(new Beets());
            Assert.AreEqual(2, registry.Count);

            Coal coal = new();
            registry.Register(coal);
            Assert.AreEqual(3, registry.Count);

            registry.Unregister(apple);
            Assert.AreEqual(2, registry.Count);

            registry.Unregister(coal);
            Assert.AreEqual(1, registry.Count);
            Assert.AreEqual(1, registry.All.Count);
        }

        [Test]
        public void TestPolling()
        {
            Registry registry = new();
            registry.Register(new Apple());
            registry.Register(new Beets());
            registry.Register(new Coal());

            Assert.AreEqual(3, registry.Count);
            IReadOnlyList<IFood> fruits = registry.GetAllThatAre<IFood>();
            Assert.AreEqual(2, fruits.Count);
            Assert.AreEqual(typeof(Apple), fruits[0].GetType());
            Assert.AreEqual(typeof(Beets), fruits[1].GetType());

            IReadOnlyList<IMaterial> materials = registry.GetAllThatAre<IMaterial>();
            Assert.AreEqual(3, materials.Count);
            Assert.AreEqual(3, registry.All.Count);
        }

        [Test]
        public void TestPollingAfterRemoving()
        {
            Registry registry = new();
            Apple apple = new();
            registry.Register(apple);
            registry.Register(new Beets());
            registry.Register(new Coal());

            Assert.AreEqual(3, registry.Count);
            registry.Unregister(apple);
            Assert.AreEqual(2, registry.Count);

            IReadOnlyList<IFood> fruits = registry.GetAllThatAre<IFood>();
            Assert.AreEqual(1, fruits.Count);
            Assert.AreEqual(typeof(Beets), fruits[0].GetType());

            IReadOnlyList<IMaterial> materials = registry.GetAllThatAre<IMaterial>();
            Assert.AreEqual(2, materials.Count);
            Assert.AreEqual(2, registry.All.Count);
        }

        public interface IMaterial
        {

        }

        public interface IFood : IMaterial
        {

        }

        public class Apple : IFood
        {

        }

        public class Beets : IFood
        {

        }

        public class Coal : IMaterial
        {

        }
    }
}