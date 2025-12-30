#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityLibrary.Systems
{
    /// <summary>
    /// Contains access to all components that registered themselves, and propagates
    /// events to those components when they implement <see cref="IListener{T}"/>.
    /// </summary>
    public class UnityObjects : Registry, IAnyListener
    {
        void IAnyListener.Receive<T>(VirtualMachine vm, ref T ev)
        {
            // propagate to all listeners of this event type
            IReadOnlyList<IListener<T>> list = GetAllThatAre<IListener<T>>();
            for (int i = 0; i < list.Count; i++)
            {
                IListener<T> listener = list[i];
                try
                {
                    listener.Receive(vm, ref ev);
                }
                catch (Exception ex)
                {
                    if (listener is Object context)
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
    }
}