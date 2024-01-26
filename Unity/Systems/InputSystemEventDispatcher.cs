#nullable enable
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace Library.Unity
{
    public class InputSystemEventDispatcher
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

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added)
            {
                vm.Broadcast(new InputDeviceAdded(device));
            }
            else if (change == InputDeviceChange.Removed)
            {
                vm.Broadcast(new InputDeviceRemove(device));
            }
        }

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            //ignore anything that isnt a state event
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;

            foreach (var control in eventPtr.EnumerateChangedControls())
            {
                if (control is ButtonControl buttonControl)
                {
                    //read float value
                    bool pressed = buttonControl.ReadValueFromEvent(eventPtr) > 0.5f;
                    if (pressed)
                    {
                        vm.Broadcast(new InputControlPressed(control));
                    }
                    else
                    {
                        vm.Broadcast(new InputControlReleased(control));
                    }
                }
                else
                {
                    vm.Broadcast(new InputControlChanged(control));
                }
            }
        }
    }

    public readonly struct InputDeviceAdded
    {
        public readonly InputDevice device;

        public InputDeviceAdded(InputDevice device)
        {
            this.device = device;
        }
    }

    public readonly struct InputDeviceRemove
    {
        public readonly InputDevice device;

        public InputDeviceRemove(InputDevice device)
        {
            this.device = device;
        }
    }

    public readonly struct InputControlPressed
    {
        public readonly InputControl control;

        public InputControlPressed(InputControl control)
        {
            this.control = control;
        }

        public readonly bool IsPath(ReadOnlySpan<char> controlPath)
        {
            return controlPath.Equals(control.path.AsSpan(), StringComparison.OrdinalIgnoreCase);
        }
    }

    public readonly struct InputControlReleased
    {
        public readonly InputControl control;

        public InputControlReleased(InputControl control)
        {
            this.control = control;
        }

        public readonly bool IsPath(ReadOnlySpan<char> controlPath)
        {
            return controlPath.Equals(control.path.AsSpan(), StringComparison.OrdinalIgnoreCase);
        }
    }

    public readonly struct InputControlChanged
    {
        public readonly InputControl control;

        public InputControlChanged(InputControl control)
        {
            this.control = control;
        }

        public readonly bool IsPath(ReadOnlySpan<char> controlPath)
        {
            return controlPath.Equals(control.path.AsSpan(), StringComparison.OrdinalIgnoreCase);
        }
    }
}