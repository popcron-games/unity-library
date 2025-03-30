using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityLibrary.Systems
{
    /// <summary>
    /// Contains access to all components that inherit from <see cref="CustomMonoBehaviour"/>,
    /// and dispatches events to those components when they implement <see cref="IListener{T}"/>
    /// </summary>
    public class UnityObjects : Registry, IAnyListener
    {
        void IAnyListener.Receive<T>(VirtualMachine vm, ref T ev)
        {
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
                    if (listener is Object unityObj)
                    {
                        Debug.LogException(ex, unityObj);
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