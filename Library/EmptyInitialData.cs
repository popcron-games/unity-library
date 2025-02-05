#nullable enable
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnityLibrary.Runtime")]
namespace UnityLibrary
{
    internal class EmptyInitialData : IInitialData
    {
        IReadOnlyList<T> IObject.GetAllThatAre<T>()
        {
            return new List<T>();
        }
    }
}