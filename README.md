# Unity Library

A framework for Unity.

```csharp
[Preserve]
public class MyUnityGame : IProgram
{
    void IState.Start(VirtualMachine vm)
    {
        vm.AddSystem(new UnityLibrarySystems(vm));
        vm.AddSystem(new MySystem(vm));
    }

    void IState.Stop(VirtualMachine vm)
    {
        vm.RemoveSystem<MySystem>().Dispose();
        vm.RemoveSystem<UnityLibrarySystems>().Dispose();
    }
}

public class MySystem : IDisposable, IListener<ApplicationStarted>, IListener<ApplicationFinished>
{
    private readonly VirtualMachine vm;

    public MySystem(VirtualMachine vm)
    {
        this.vm = vm;
        //equivalent to static constructor
    }

    public void Dispose()
    {
        //equivalent to static destructor
    }

    void IListener<ApplicationStarted>.Receive(VirtualMachine vm, ref ApplicationStarted ev)
    {
        Debug.Log("Playing started");
    }

    void IListener<ApplicationFinished>.Receive(VirtualMachine vm, ref ApplicationFinished ev)
    {
        Debug.Log("Playing stopped");
    }
}
```

### How

Most of the logic for enabling this is implemented in `UnityApplication`, which will initialize a `VirtualMachine` instance,
and dispose of it at the latest possible moment in Unity. This virtual machine object is also accessible through `UnityApplication.VM`.

### Unity events

* Events like update, fixed update, late update will be broadcast as: `UpdateEvent`, `FixedUpdateEvent`, `LateUpdateEvent` etc
* `ApplicationStarted` and `ApplicationStopped` events are broadcast as callbacks when play mode has been entered and exited (constructor and dispose are still preceding events)
