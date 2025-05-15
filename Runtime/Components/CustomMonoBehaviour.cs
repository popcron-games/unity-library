#nullable enable
using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityLibrary;
using UnityLibrary.Systems;

public abstract class CustomMonoBehaviour : MonoBehaviour
{
    public static VirtualMachine VM => UnityApplication.VM;
    public static UnityObjects UnityObjects => VM.GetSystem<UnityObjects>();

    private CancellationTokenSource? enabledLifetime = null;
    private UnityObjects? unityObjects = null;

    /// <summary>
    /// Cancellation token thats raised when <see cref="OnDisable"/> is called.
    /// </summary>
    public CancellationToken disableCancellationToken
    {
        get
        {
            ThrowIfNotEnabledYet();
            return enabledLifetime!.Token;
        }
    }

    protected virtual void OnEnable()
    {
        ThrowIfAlreadyEnabled();
        enabledLifetime = new();
        unityObjects = UnityObjects;
        unityObjects.Register(this);
    }

    protected virtual void OnDisable()
    {
        ThrowIfNotEnabledYet();
        unityObjects?.Unregister(this);
        unityObjects = null;
        enabledLifetime!.Cancel();
        enabledLifetime!.Dispose();
        enabledLifetime = null;
    }

    [Conditional("DEBUG")]
    private void ThrowIfNotEnabledYet()
    {
        if (enabledLifetime is null)
        {
            throw new InvalidOperationException("Attempting to access a component that hasn't been enabled yet");
        }
    }

    [Conditional("DEBUG")]
    private void ThrowIfAlreadyEnabled()
    {
        if (enabledLifetime is not null)
        {
            throw new InvalidOperationException("Attempting to enable a component that's already enabled");
        }
    }
}