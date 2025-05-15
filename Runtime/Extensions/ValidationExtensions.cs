#nullable enable
using System;
using UnityEngine;
using UnityLibrary;
using UnityLibrary.Events;

public static class ValidationExtensions
{
    /// <summary>
    /// Performs validation testing on the listener.
    /// </summary>
    public static void TryValidate(this object? listener, VirtualMachine vm, ref Validate e)
    {
        if (listener is IListener<Validate> validator)
        {
            try
            {
                validator.Receive(vm, ref e);
            }
            catch (Exception ex)
            {
                e.failed = true;
                if (listener is UnityEngine.Object context)
                {
                    Debug.LogException(ex, context);
                }
                else
                {
                    Debug.LogException(ex);
                }
            }
        }
    }

    /// <summary>
    /// Performs validation testing on the listener.
    /// </summary>
    public static void TryValidate(this object? listener, VirtualMachine vm, ref Validate e, UnityEngine.Object context)
    {
        if (listener is IListener<Validate> validator)
        {
            try
            {
                validator.Receive(vm, ref e);
            }
            catch (Exception ex)
            {
                e.failed = true;
                Debug.LogException(ex, context);
            }
        }
    }
}