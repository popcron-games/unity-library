# Unity Library

A simple framework for Unity.

### Programs and systems

Programs are surface level constructs that are operated by a `VirtualMachine`.
They, along with added systems, are able to receive events through the `IListener<T>` interface:
```cs
[Preserve]
public class GameProgram : IProgram
{
    void IProgram.Start(VirtualMachine vm)
    {
        vm.AddSystem(new GameSystem(vm));
    }

    void IProgram.Stop(VirtualMachine vm)
    {
        vm.RemoveSystem<GameSystem>().Dispose();
    }
}

public class GameSystem : SystemBase
{
    public GameSystem(VirtualMachine vm) : base(vm)
    {
        //equivalent to static constructor
    }

    public override void Dispose()
    {
        //equivalent to static destructor
    }
}
```

### Configuration asset

![Configuration asset](Docs/configAsset.png)

All projects will have a singleton configuration asset.
It states what type is used as the program, and an optional editor only system.

### Playing from start

![Two play buttons](Docs/twoPlayButtons.png)

The original play button in the Unity editor has the behaviour of playing from the current scene.
In addition to that, there is a new play button that simulates playing from a build.

### Receiving Unity engine events

Events from the Unity engine can be received by all systems, and `CustomMonoBehaviour` instances:
```cs
public class MySystem : IListener<ApplicationStarted>, IListener<ApplicationFinished>
{
    void IListener<ApplicationStarted>.Receive(VirtualMachine vm, ref ApplicationStarted e)
    {
        Debug.Log("Playing started");
    }

    void IListener<UpdateEvent>.Receive(VirtualMachine vm, ref UpdateEvent e)
    {
        Debug.Log(e.delta);
    }

    void IListener<ApplicationFinished>.Receive(VirtualMachine vm, ref ApplicationFinished e)
    {
        Debug.Log("Playing stopped");
    }
}

public class GameManager : CustomMonoBehaviour, IListener<UpdateEvent>
{
    void IListener<UpdateEvent>.Receive(VirtualMachine vm, ref UpdateEvent e)
    {
        Debug.Log(e.delta);
    }
}
```

### Enumerating through Unity components

Components that inherit from `CustomMonoBehaviour` are all available through a `UnityObjects`
system. This enables them to receive events through the `IListener<T>` interface, and be polled:
```cs
public class Pickup : CustomMonoBehaviour, IListener<Validate>, IListener<FixedUpdate>
{
    public static IReadOnlyList<Pickup> All => UnityObjects.GetAllThatAre<Pickup>();

    [SerializeField]
    private GameObject effectPrefab;

    void IListener<Validate>.Receive(VirtualMachine vm, ref Validate e)
    {
        Assert.IsNotNull(effectPrefab, "Effect prefab is expected to be assigned");
    }

    void IListener<FixedUpdate>.Receive(VirtualMachine vm, ref FixedUpdate e)
    {
        // do some logic on fixed update
    }
}
```

This works by registering and unregistering the components in their `OnEnabled()` and `OnDisabled()`
methods. And this can be done manually for your own Unity objects if it makes sense, such as scriptable objects:
```cs
public class UserManager : CustomMonoBehaviour
{
    private List<User> users = new();

    public User Create()
    {
        User newUser = ScriptableObject.CreateInstance<User>();
        users.Add(newUser);
        newUser.Initialize(users.Count);
        UnityObjects.Register(newUser); 
        VM.Broadcast(new UserCreated(newUser));
        return newUser;
    }

    public void Delete(User user)
    {
        VM.Broadcast(new UserDeleted(user));
        UnityObjects.Unregister(user);
        users.Remove(user);
        ScriptableObject.Destroy(user);
    }
}

public class User : ScriptableObject
{
    private int userId;

    internal void Initialize(int userId)
    {
        this.userId = userId;
    }
}
```

### Validation before play

![Manual testing](Docs/manualTesting.png)

Included is a `Validate` event that is dispatched before entering play mode.
And can be done so manually using the two main toolbar buttons.
If any of the handlers report an error, then entering play is aborted:
```cs
public class MySystem : IListener<Validate>
{
    void IListener<Validate>.Receive(VirtualMachine vm, ref Validate e)
    {
        Assert.Fail(); // this will prevent entering play mode
    }
}
```

Additionally, a component that modifies itself during this event will be marked
dirty in the editor. So calling `EditorUtility.SetDirty()` isn't necessary:
```cs
public class Player : CustomMonoBehaviour, IListener<Validate>
{
    [SerializeField]
    private Rigidbody2D rb;

    void IListener<Validate>.Receive(VirtualMachine vm, ref Validate e)
    {
        rb = GetComponent<Rigidbody2D>();
        Assert.IsNotNull(rb, "A Rigidbody2D component reference is missing");
    }
}
```

### The `UnityLibrarySystems` type

This is an included system that facilitates the mechanisms described above.
Specifically, dispatching events to `CustomMonoBehaviour` components, and polling them.
It's always automatically added to the virtual machine program, and can be disabled
through a setting on the configuration asset.

### Contributing and design

This package emerged from years of trying to write quality code. It is a boiled down
version of what I rewrote countless times that helped me make safer, and reliable code.

Contributions that fit this are welcome.