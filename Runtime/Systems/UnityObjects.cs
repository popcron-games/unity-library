#nullable enable
using Game;
using Game.Library;
using System;
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
            using RentedBuffer<object> buffer = new(Count);
            int count = FillAllThatAre<IListener<T>>(buffer);
            for (int i = 0; i < count; i++)
            {
                object asset = buffer[i];
                try
                {
                    IListener<T> listener = (IListener<T>)asset;
                    listener.Receive(vm, ref ev);
                }
                catch (Exception ex)
                {
                    if (asset is Object unityObj)
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