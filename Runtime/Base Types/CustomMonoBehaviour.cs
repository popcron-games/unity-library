#nullable enable
using UnityEngine;
using UnityLibrary;
using UnityLibrary.Systems;

public abstract class CustomMonoBehaviour : MonoBehaviour
{
    public static VirtualMachine VM => UnityApplication.VM;

    public static UnityObjects UnityObjects
    {
        get
        {
            if (VM.TryGetSystem(out UnityObjects? unityObjects))
            {
                return unityObjects;
            }
            else
            {
                if (UnityApplicationSettings.Singleton.AddBuiltInSystems)
                {
                    throw new($"The {nameof(UnityLibrary.Systems.UnityObjects)} system is missing from the virtual machine, but was expected to be present");
                }
                else
                {
                    throw new($"The {nameof(UnityLibrary.Systems.UnityObjects)} system has not been added to the virtual machine");
                }
            }
        }
    }

    private UnityObjects? unityObjects = null;

    protected virtual void OnEnable()
    {
        unityObjects = UnityObjects;
        unityObjects.Register(this);
    }

    protected virtual void OnDisable()
    {
        unityObjects?.Unregister(this);
        unityObjects = null;
    }
}