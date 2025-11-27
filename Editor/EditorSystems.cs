#nullable enable
using System.Runtime.CompilerServices;
using UnityLibrary.Editor.Systems;

[assembly: InternalsVisibleTo("UnityLibrary.Runtime")]
namespace UnityLibrary.Editor
{
    /// <summary>
    /// Added by <see cref="UnityApplication"/> before its virtual machine initializes.
    /// </summary>
    internal static class EditorSystems
    {
        public static void Start(VirtualMachine vm)
        {
            vm.AddSystem(new TestBeforeEnteringPlay(vm));
        }

        public static void Stop(VirtualMachine vm)
        {
            vm.RemoveSystem<TestBeforeEnteringPlay>().Dispose();
        }
    }
}