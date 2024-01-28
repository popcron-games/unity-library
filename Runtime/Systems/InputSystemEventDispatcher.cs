#nullable enable
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
                vm.Broadcast(new InputDeviceRemoved(device));
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
}