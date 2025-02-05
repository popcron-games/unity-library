# Unity Library
A framework for Unity.

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
        //system added
    }

    void IListener<StartGame>.Receive(VirtualMachine vm, ref StartGame ev)
    {
        Debug.Log("Game started");
    }

    public void Dispose()
    {
        //system removed
    }
}
```

### How
Most of the logic for enabling this is implemented in `UnityApplication`, which will initialize a `VirtualMachine` instance,
and dispose of it at the latest possible moment in Unity. This virtual machine object is accessible through `UnityApplication.VM`.

### Unity events
* Events like update, fixed update, late update will be broadcast as: `UpdateEvent`, `FixedUpdateEvent`, `LateUpdateEvent` etc
* InputSystem's events will be broadcast as: `InputControlChanged`, `InputControlPressed`, `InputControlReleased`, `InputDeviceAdded`, `InputDeviceRemoved`
* `ApplicationStarted` and `ApplicationStopped` events are broadcast as callbacks when play mode has been entered and exited (constructor and dispose still preceding events)
