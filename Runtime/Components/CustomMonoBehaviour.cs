#nullable enable
using System;
using System.Threading;
using UnityEngine;
using UnityLibrary;
using UnityLibrary.Systems;

public abstract class CustomMonoBehaviour : MonoBehaviour
{
    protected static VirtualMachine VM => UnityApplication.VM;
    protected static UnityObjects UnityObjects => VM.GetSystem<UnityObjects>();

    private CancellationTokenSource? enabledLifetime = null;

    /// <summary>
    /// Cancellation token thats raised when <see cref="OnDisable"/> is called.
    /// </summary>
    public CancellationToken DisableCancellationToken
    {
        get
        {
#if UNITY_EDITOR
            if (enabledLifetime is null)
            {
                throw new InvalidOperationException("Attempting to retrieve token representing the enabled state of the component, but it hasn't been enabled yet.");
            }
#endif

            return enabledLifetime.Token;
        }
    }

    protected virtual void OnEnable()
    {
#if UNITY_EDITOR
        if (enabledLifetime is not null)
        {
            throw new InvalidOperationException("Attempting to enable a component that's already enabled.");
        }
#endif
        enabledLifetime = new();
        UnityObjects.Register(this);
    }

    protected virtual void OnDisable()
    {
        UnityObjects.Unregister(this);
        if (enabledLifetime is not null)
        {
            enabledLifetime.Cancel();
            enabledLifetime.Dispose();
            enabledLifetime = null;
        }
        else
        {
#if UNITY_EDITOR
            throw new InvalidOperationException("Attempting to disable a component that hasn't been enabled yet.");
#endif
        }
    }
}