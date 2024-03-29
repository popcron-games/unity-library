# Unity Library
A package for implementing logic into Unity applications safely, where
the overall logic is backed by user written code.

```csharp
[Preserve]
public class MyUnityGame : VirtualMachine.IState
{
    void IState.Initialize(VirtualMachine vm)
    {
        vm.AddSystem(new UnityLibrarySystems(vm));
        vm.AddSystem(new MySystem(vm));

        vm.Broadcast(new StartGame());
    }

    void IState.Finalize(VirtualMachine vm)
    {
        vm.Broadcast(new FinishGame());

        vm.RemoveSystem<MySystem>().Dispose();
        vm.RemoveSystem<UnityLibrarySystems>().Dispose();
    }
}

public class MySystem : IDisposable, IListener<StartGame>
{
    private readonly VirtualMachine vm;

    public MySystem(VirtualMachine vm)
    {
        this.vm = vm;
        // Set up
    }

    void IListener<StartGame>.Receive(VirtualMachine vm, ref StartGame ev)
    {
        Console.WriteLine("Game started");
    }

    public void Dispose()
    {
        // Clean up
    }
}
```

### How
Most of the logic for enabling this is implemented in `UnityApplication`, which will initialize a `VirtualMachine` instance, and dispose of it at the latest possible moment inside of Unity. This is so that its `VM` property is never null, and can be accessed from anywhere in the project by nature that the Unity application is currently executing to be able to call `UnityApplication.VM`.

How `UnityApplication` knows what `IState` to create, is through a scriptable object that will always be present in the project as a requirement. This allows the package to contain the logic for the container of the virtual machine, while allowing the user to define the start and ends of the virtual machine's lifecycle. If the user doesn't assign the state to any valid type, it will default to `DefaultState` which is a state that does nothing (to keep the `VM` value valid always).

The efficiency of the broadcast event depends on how many unique systems are added to virtual machines. Whenever an object is added or removed from a `Registry` instance, it will also keep track of the assignable types that it is. So that a dictionary of type, to the list of objects that are that type are able to kept in an event based way. This avoids the need to iterate through all the objects in a loop just to check if the type `is`, in the case of `Registry` its an immediate lookup.

### Unity events
* Events like update, fixed update, late update will be broadcast as: `UpdateEvent`, `FixedUpdateEvent`, `LateUpdateEvent` etc
* InputSystem's events will be broadcast as: `InputControlChanged`, `InputControlPressed`, `InputControlReleased`, `InputDeviceAdded`, `InputDeviceRemoved`
* `ApplicationStarted` and `ApplicationStopped` events are broadcast as callbacks when play mode has been entered and exited (constructor and dispose still preceding events)
