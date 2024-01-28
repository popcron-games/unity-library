#nullable enable
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Library.Unity
{
    public readonly struct Destroy
    {
        public Destroy(GameObject instance)
        {
            if (!Addressables.ReleaseInstance(instance))
            {
                Object.Destroy(instance);
            }
        }
    }
}
