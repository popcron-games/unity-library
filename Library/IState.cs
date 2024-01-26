#nullable enable
namespace Library
{
    /// <summary>
    /// Represents the state of a <see cref="VirtualMachine"/> instance.
    /// </summary>
    public interface IState
    {
        void Initialize(VirtualMachine vm);
        void Finalize(VirtualMachine vm);
    }
}