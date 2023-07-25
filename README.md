# Lib
Encourages a more functional approach to Unity development

### Installation
1. Use `https://github.com/popcron-games/com.popcron-games.lib.git` when adding as a package
2. If you have compiler errors then you're probably missing required libraries, these are available as a sample
3. To get started, import the `Base Types` samples from the package manager window.
    This sample will import new `MonoBehaviour` and `ScriptableObject` in a global namespace, which will act as new base type with overridable base methods. The `OnUpdate` and `OnFixedUpdate` but not `Awake` and `Start` kind of methods aren't provided as virtual methods, this is because these methods have an alternative in the form of events by implementing the `IListener<UpdateEvent>` interface on your types, this is to take advantage of an [optimization that skips the `Update` method](https://blog.unity.com/engine-platform/10000-update-calls). Virtual methods are expected to have the base method called to propagate their behaviour correctly especially if it's the `OnEnable` and `OnDisable` methods, if you want to protect the inherited behaviour you can sealed the methods and call different virtual methods yourself (into OnEnable**D** and OnDisable**D**).

### Retrieving a list of all instances by assignable types
```csharp
public class Player : MonoBehaviour, IPlayer
{

}

public class SomethingElse : MonoBehaviour, IPlayer
{

}

foreach (IPlayer player in Everything.GetAllThatAre<IPlayer>())
{
    Debug.Log(player);
}
```
### Dispatching an event for all isteners
```csharp
public readonly struct PlayerHasJumped : IEvent 
{
    public readonly Player player;

    public PlayerHasJumped(Player player)
    {
        this.player = player;
    }
}

public void Jump()
{
    new PlayerHasJumped(this).Dispatch();
}
```
### Listening to events
```csharp
public class PlayerSounds : MonoBehaviour, IListener<PlayerHasJumped>
{
    void IListener<PlayerHasJumped>.OnEvent(PlayerHasJumped message)
    {
        //jumped!
    }
}
```
### Preventing play mode because of user defined reasons (validation)
```csharp
public class Player : MonoBehaviour, IListener<PlayabilityCheck>
{
    public string? ability;

    void IListener<PlayabilityCheck>.OnEvent(PlayabilityCheck message)
    {
        message.CantIfNull(ability, "Player is missing an ability", this);
    }
}
```
### Validating state before play mode (similar to OnValidate)
```csharp
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour, IListener<ValidationEvent>
{
    public Rigidbody? rb;

    void IListener<ValidationEvent>.OnEvent(ValidationEvent message)
    {
        rb = GetComponent<Rigidbody>();
    }
}
```
### Fetching instances using an identifier
```csharp
public class Item : MonoBehaviour, IIdentifier 
{
    [SerializeField]
    private string id;

    public ReadOnlySpan<char> ID => id.AsSpan();
}

if (Everything.TryGetWithID("item1", out object? item))
{

}
```
### Fetching instances using a path
```csharp
public class Inventory : MonoBehaviour, IIdentifier, IBranch
{
    [SerializeField]
    private List<Item> items = new();

    public ReadOnlySpan<char> ID => "inventory".AsSpan();

    bool IBranch.TryGetChild(ReadOnlySpan<char> id, out object? child)
    {
        foreach (Item item in items)
        {
            if (item.ID.SequenceEqual(id))
            {
                child = item;
                return true;
            }
        }

        child = null;
        return false;
    }
}

if (Everything.TryGetAtPath("inventory/item1", out object? item))
{

}
```
### Available events
There are some events already included, its not exhaustive.
* `AboutToStartPlaying` - Dispatched before play mode starts, as if it was called before you pressed the play button
* `ApplicationQuitting` - Dispatched when the application is quitting, useful as cleanup code if you happen to have instances existing outside of play mode
* `EarlyUpdateEvent`, `PostLateUpdateEvent`, `PreLateUpdateEvent`, `UpdateEvent`, `FixedUpdateEvent` - Dispatched on the respective Unity events
* `PlayabilityCheck` - Dispatched before play mode starts, listeners are able to have assert like API to prevent play mode in editor
* `ValidationEvent` - Dispatched when `OnValidate` is called on instances, and before play mode starts in editor
* `UpdateEventInEditMode` - Dispatched only when in editor and not in play mode, on the `Update` event
* `GUIEvent` - Dispatched when `OnGUI` would be called

# How it works
### The `Everything` class
Contains maps that convert the type from user input, to a collection of all of the instances in existence within the current domain. The internal collections are of `object` type, so any value or structure will be boxed.

Instances must be added and remove to the `Everything` class manually, unless you inherit from a "base" type that already ensures this for you.

### The `CustomMonoBehaviour` type and its pattern
Their only purpose is to automatically do what `Everything` expects from you if you want to use its API with your types.

This pattern is done for the `MonoBehaviour`, `ScriptableObject`, `Editor` and `EditorWindow` Unity base types so that it can be maximized.

The custom types will also dispatch a `ValidateEvent` to themselves when `OnValidate()` is meant to happen on the instance.

### Iterating using generics
The `Everything.GetAllThatAre<T>()` generic method currently has a good decent bandaid over its design but a bandaid nonetheless. The internal collections stores a list of `object` instances but the user is expecting a `IReadOnlyCollection<T>` the collection must mean that code should somehow transform from `object[]` to `T[]`. Basically this method will cost an allocation in the same way `ArrayPool<T>.Shared` invokes allocations.

Alternatively, there is a `ForEachInAll<T>()` method that avoids this problem, or `Everything.GetAllThatAre(typeof(T))` which will return the internal collection, from where you can parse/cast yourself.

### Non Unity objects
These cases also work, but the instance must be added and removed manually by you.
The type must also implement `IUnityObject`, from here it then receive events like the other cases.

### Retreiving data more efficiently than FindObjectsByType<T>
When something is added to `Everything`, the type and the ancestor types of the object, along with all implementing types and their ancestors are stored in a map.
All of these other types are "assignable from" types, and the map is a map of all types to list of instances, assuming the type is assignable from the instance type.
This means that `Everything.GetAllThatAre<T>()` returns the collection directly without extra iterations.
