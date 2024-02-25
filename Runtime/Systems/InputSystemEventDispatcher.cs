#nullable enable
using Game;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityLibrary.Unity;

namespace UnityLibrary.Systems
{
    public class InputSystemEventDispatcher : IDisposable
    {
        private readonly VirtualMachine vm;

        public InputSystemEventDispatcher(VirtualMachine vm)
        {
            this.vm = vm;
            InputSystem.onDeviceChange += OnDeviceChange;
            InputSystem.onEvent += OnInputEvent;
            foreach (InputDevice device in InputSystem.devices)
            {
                OnDeviceChange(device, InputDeviceChange.Added);
            }
        }

        public void Dispose()
        {
            foreach (InputDevice device in InputSystem.devices)
            {
                OnDeviceChange(device, InputDeviceChange.Removed);
            }

            InputSystem.onDeviceChange -= OnDeviceChange;
            InputSystem.onEvent -= OnInputEvent;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added)
            {
                InputDeviceAdded ev = new(device.deviceId);
                vm.Broadcast(ref ev);
            }
            else if (change == InputDeviceChange.Removed)
            {
                InputDeviceRemoved ev = new(device.deviceId);
                vm.Broadcast(ref ev);
            }
        }

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            //ignore anything that isnt a state event
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;

            int deviceId = device.deviceId;
            foreach (var control in eventPtr.EnumerateChangedControls())
            {
                if (control is ButtonControl buttonControl)
                {
                    //read float value
                    bool pressed = buttonControl.ReadValueFromEvent(eventPtr) > 0.5f;
                    if (pressed)
                    {
                        InputControlPressed ev = new(deviceId, control.path);
                        vm.Broadcast(ref ev);
                    }
                    else
                    {
                        InputControlReleased ev = new(deviceId, control.path);
                        vm.Broadcast(ref ev);
                    }
                }
                else
                {
                    InputControlChanged ev = new(deviceId, control.path);
                    vm.Broadcast(ref ev);
                }
            }
        }
    }
}