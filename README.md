# Unity Library

A framework for Unity.

### Programs and systems

Programs are surface level constructs that are operated by a `VirtualMachine`.
They, along with added systems are able to receive events with the `IListener<T>` interface:
```cs
[Preserve]
public class GameProgram : IProgram
{
    void IProgram.Start(VirtualMachine vm)
    {
        vm.AddSystem(new UnityLibrarySystems(vm)); //explained further below
        vm.AddSystem(new MySystem(vm));
    }

    void IProgram.Stop(VirtualMachine vm)
    {
        vm.RemoveSystem<MySystem>().Dispose();
        vm.RemoveSystem<UnityLibrarySystems>().Dispose();
    }
}

public class MySystem : IDisposable
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
}
```

### Configuration asset

![Configuration asset](Docs/configAsset.png)

All projects will have a single configuration asset that states what type is used as the program.
As well as a reference to the initial assets. This asset is found procedurally at runtime:

### Playing from start

![Two play buttons](Docs/twoPlayButtons.png)

The original play button in the Unity editor has the behaviour of playing from the current scene.
In addition to that, there is a new play button that plays from the first scene in the build settings:

### Validation

![Manual testing](Docs/manualTesting.png)

Included is a `Validate` event, called when either of the two play buttons
are used. If any of them report an error, then entering play is disallowed.
By default, this runs for all systems:
```cs
public class MySystem : IListener<Validate>
{
    void IListener<Validate>.Receive(VirtualMachine vm, ref Validate e)
    {
        Assert.Fail(); //this system will always prevent play
    }
}
```

Validation can also be performed before attempting to play:

### The `UnityLibrarySystems` type

This included system is a collection of these:

**`UnityEventDispatcher`**:

Dispatches events from the Unity runtime to all systems:

```cs
public class MySystem : IListener<ApplicationStarted>, IListener<UpdateEvent>, IListener<ApplicationFinished>
{
    void IListener<ApplicationStarted>.Receive(VirtualMachine vm, ref ApplicationStarted ev)
    {
        Debug.Log("Playing started");
    }

    void IListener<UpdateEvent>.Receive(VirtualMachine vm, ref UpdateEvent ev)
    {
        Debug.Log(ev.delta);
    }

    void IListener<ApplicationFinished>.Receive(VirtualMachine vm, ref ApplicationFinished ev)
    {
        Debug.Log("Playing stopped");
    }
}
```

**`UnityObjects`**

Stores all `MonoBehaviour` and `ScriptableObject` that were manually registered with it.

```cs
public class Pickup : CustomMonoBehaviour
{
    //exposes a list of all pickups in the scene
    public static IReadOnlyList<Pickup> All => Registry.GetAllThatAre<Pickup>();
}
```

### The `CustomMonoBehaviour` and `CustomScriptableObject`

These two sub types register themselves with the `UnityObjects` system in `OnEnable()` 
and unregister in `OnDisable()`. They are not a requirement.

### Contributing and design

This Unity package provides the scaffolding needed to write code that may not
need to depend on Unity. While providing high level tools that help make
safer and reliable code.

Contributions to this goal are welcome.