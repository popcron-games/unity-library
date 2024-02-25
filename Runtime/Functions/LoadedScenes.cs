#nullable enable
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UnityLibrary
{
    internal static class LoadedScenes
    {
        internal static readonly Dictionary<int, SceneInstance> map = new();
    }
}
