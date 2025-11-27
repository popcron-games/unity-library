#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityLibrary.Events;
using Object = UnityEngine.Object;

namespace UnityLibrary.Systems
{
    /// <summary>
    /// Contains access to all components that registered themselves, and propagates
    /// events to those components when they implement <see cref="IListener{T}"/>.
    /// </summary>
    public class UnityObjects : Registry, IAnyListener, IListener<Validate>
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

            //propagate to scriptable objects 
            foreach (ScriptableObject scriptableObject in SingletonScriptableObjects.scriptableObjects)
            {
                if (scriptableObject is IListener<T> listener)
                {
                    try
                    {
                        listener.Receive(vm, ref ev);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex, scriptableObject);
                    }
                }
            }
        }

        void IListener<Validate>.Receive(VirtualMachine vm, ref Validate e)
        {
            IReadOnlyList<IListener<Validate>> list = GetAllThatAre<IListener<Validate>>();
            for (int i = 0; i < list.Count; i++)
            {
                IListener<Validate> listener = list[i];
                listener.Validate(vm, ref e);
            }

            //propagate to scriptable objects
            foreach (ScriptableObject scriptableObject in SingletonScriptableObjects.scriptableObjects)
            {
                if (scriptableObject is IListener<Validate> listener)
                {
                    listener.Validate(vm, ref e);
                }
            }
        }
    }
}