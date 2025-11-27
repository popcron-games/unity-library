#nullable enable
using System;
using UnityEngine;
using UnityLibrary;
using UnityLibrary.Events;

public static class ValidationExtensions
{
    /// <summary>
    /// Performs validation testing on the <paramref name="listener"/>.
    /// <para></para>
    /// Returns <see langword="true"/> if the <paramref name="listener"/> received the <see cref="UnityLibrary.Events.Validate"/> event (not whether it succeeded or failed).
    /// </summary>
    public static bool TryValidate(this object? listener, VirtualMachine vm, ref Validate e)
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

            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// Performs validation testing on the <paramref name="validator"/>.
    /// </summary>
    public static void Validate(this IListener<Validate>? validator, VirtualMachine vm, ref Validate e)
    {
        if (validator is not null)
        {
            try
            {
                validator.Receive(vm, ref e);
            }
            catch (Exception ex)
            {
                e.failed = true;
                if (validator is UnityEngine.Object context)
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
    /// Performs validation testing on the <paramref name="listener"/>.
    /// <para></para>
    /// Returns <see langword="true"/> if the <paramref name="listener"/> received the <see cref="UnityLibrary.Events.Validate"/> event (not whether it succeeded or failed).
    /// </summary>
    public static bool TryValidate(this object? listener, VirtualMachine vm, ref Validate e, UnityEngine.Object context)
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

            return true;
        }
        else
        {
            return false;
        }
    }
}