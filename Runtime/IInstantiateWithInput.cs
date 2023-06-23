#nullable enable

namespace Popcron
{
    /// <summary>
    /// Encourages use of <see cref="SealableMonoBehaviour.InstantiateWithInput{P, T}(P, T, UnityEngine.SceneManagement.Scene)"/>
    /// </summary>
    /// <typeparam name="T">Input for instantiation.</typeparam>
    public interface IInstantiateWithInput<T>
    {
        void Pass(T input);
    }
}
