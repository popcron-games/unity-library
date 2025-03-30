# Unity Library

A framework for Unity.

### Programs and systems

Programs are surface level objects that are operated by a virtual machine.
They, along with systems are able to receive events using the `IListener<T>` interface:
```cs
[Preserve]
public class MyUnityGame : IProgram
{
    void IProgram.Start(VirtualMachine vm)
    {
        vm.AddSystem(new UnityLibrarySystems(vm));
        vm.AddSystem(new MySystem(vm));
    }

    void IProgram.Stop(VirtualMachine vm)
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

### Initialization data

When virtual machines are created, they contain initial data which can then be accessed by
a program or a system. This initial data is assigned in the inspector:

### How

The logic for bootstrapping this is implemented in the `UnityApplication` class. It will initialize a
singleton `VirtualMachine` instance, and dispose of it at the latest possible moment in Unity.
It's accessible through `UnityApplication.VM`.

### Unity events

* Events like update, fixed update, late update will be broadcast as: `UpdateEvent`, `FixedUpdateEvent`, `LateUpdateEvent` etc
* `ApplicationStarted` and `ApplicationStopped` events are broadcast as callbacks when play mode has been entered and exited (constructor and dispose are still preceding events)