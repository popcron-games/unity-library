#nullable enable
using System;

namespace UnityLibrary
{
    /// <summary>
    /// Represents the constructor and <see cref="IDisposable.Dispose"/> events of a <see cref="VirtualMachine"/>.
    /// </summary>
    public interface IProgram
    {
        /// <summary>
        /// Invoked when a <see cref="VirtualMachine"/> has been created.
        /// </summary>
        void Start(VirtualMachine vm);

        /// <summary>
        /// Invoked when a <see cref="VirtualMachine"/> is being disposed.
        /// </summary>
        void Finish(VirtualMachine vm);
    }
}