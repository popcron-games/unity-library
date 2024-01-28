#nullable enable
using NUnit.Framework;

namespace Library
{
    public class RegistryTests
    {
        [Test]
        public void TestAdding()
        {
            Registry<object> registry = new();
            registry.Register(new Apple());
            Assert.AreEqual(1, registry.Count);

            registry.Register(new Beets());
            Assert.AreEqual(2, registry.Count);

            registry.Register(new Coal());
            Assert.AreEqual(3, registry.Count);
        }

        [Test]
        public void TestRemoving()
        {
            Registry<object> registry = new();
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
        }

        [Test]
        public void TestPolling()
        {
            Registry<IMaterial> registry = new();
            registry.Register(new Apple());
            registry.Register(new Beets());
            registry.Register(new Coal());

            Assert.AreEqual(3, registry.Count);
            var fruits = registry.GetAllThatAre<IFood>();
            Assert.AreEqual(2, fruits.Count);
            Assert.AreEqual(typeof(Apple), fruits[0].GetType());
            Assert.AreEqual(typeof(Beets), fruits[1].GetType());

            var materials = registry.GetAllThatAre<IMaterial>();
            Assert.AreEqual(3, materials.Count);
        }

        [Test]
        public void TestPollingAfterRemoving()
        {
            Registry<IMaterial> registry = new();
            Apple apple = new();
            registry.Register(apple);
            registry.Register(new Beets());
            registry.Register(new Coal());

            Assert.AreEqual(3, registry.Count);
            registry.Unregister(apple);
            Assert.AreEqual(2, registry.Count);

            var fruits = registry.GetAllThatAre<IFood>();
            Assert.AreEqual(1, fruits.Count);
            Assert.AreEqual(typeof(Beets), fruits[0].GetType());

            var materials = registry.GetAllThatAre<IMaterial>();
            Assert.AreEqual(2, materials.Count);
        }

        [Test]
        public void TestFilling()
        {
            Registry<IMaterial> registry = new();
            registry.Register(new Apple());
            registry.Register(new Beets());
            registry.Register(new Coal());

            Assert.AreEqual(3, registry.Count);
            IMaterial[] buffer = new IMaterial[3];
            int count = registry.FillAllThatAre<IMaterial>(buffer);
            Assert.AreEqual(3, count);
            Assert.AreEqual(typeof(Apple), buffer[0].GetType());
            Assert.AreEqual(typeof(Beets), buffer[1].GetType());
            Assert.AreEqual(typeof(Coal), buffer[2].GetType());

            buffer = new IMaterial[2];
            count = registry.FillAllThatAre<IMaterial>(buffer);
            Assert.AreEqual(2, count);
            Assert.AreEqual(typeof(Apple), buffer[0].GetType());
            Assert.AreEqual(typeof(Beets), buffer[1].GetType());

            buffer = new IMaterial[1];
            count = registry.FillAllThatAre<IMaterial>(buffer);
            Assert.AreEqual(1, count);
            Assert.AreEqual(typeof(Apple), buffer[0].GetType());
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