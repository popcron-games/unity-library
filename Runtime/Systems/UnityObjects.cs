﻿#nullable enable
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Library.Systems
{
    /// <summary>
    /// Contains access to all components that inherit from <see cref="CustomMonoBehaviour"/>,
    /// and dispatches events to those components when they implement <see cref="IListener{T}"/>
    /// </summary>
    public class UnityObjects : Registry<Object>, IBroadcastListener
    {
        void IBroadcastListener.Receive<T>(VirtualMachine vm, T e)
        {
            using RentedArray<Object> buffer = new(Count);
            int count = FillAllThatAre<IListener<T>>(buffer);
            for (int i = 0; i < count; i++)
            {
                Object asset = buffer[i];
                try
                {
                    IListener<T> listener = (IListener<T>)asset;
                    listener.Receive(vm, e);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex, asset);
                }
            }
        }
    }
}