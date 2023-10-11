# Lib
Encourages a more functional approach to Unity development.
Inspired by [an optimization that skips the `Update` method](https://blog.unity.com/engine-platform/10000-update-calls)

### Installation
1. Use `https://github.com/popcron-games/com.popcron-games.lib.git` when adding as a package
2. If you have compiler errors then you're likely missing required libraries, these are available as a sample

### Retrieving a list of all instances by assignable types
```csharp
public interface IPlayer { }

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
Paths are resolved using the `IBranch` interface.
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
* `AboutToStartPlaying` - Dispatched before play mode starts, as if it was called before you pressed the play button
* `ApplicationQuitting` - Dispatched when the application is quitting, useful as cleanup code if you happen to have instances existing outside of play mode
* `EarlyUpdateEvent`, `PostLateUpdateEvent`, `PreLateUpdateEvent`, `UpdateEvent`, `FixedUpdateEvent` - Dispatched on the respective Unity events
* `PlayabilityCheck` - Dispatched before play mode starts, listeners are able to have assert like API to prevent play mode in editor
* `ValidationEvent` - Dispatched when `OnValidate` is called on instances, and before play mode starts in editor
* `UpdateEventInEditMode` - Dispatched only when in editor and not in play mode, at the same time as the `Update` event
* `GUIEvent` - Dispatched when `OnGUI` would be called, useful for static code that needs to draw GUI
* more...

# How it works
### The `Everything` class
Contains dictionaries that map the type to a collection of all of the instances registered in it. When a MonoBehaviour is automatically registered to this collection it could then be fetched statically really efficiently.

Instances must be added and removed to the `Everything` class manually, there is a sample that contains base component types that already do this (so inherit from these instead of UnityEngine.MonoBehaviour)

### `PlayabilityCheck` event
This event gets dispatched before play mode begins, components that are listeners of this event in the scene will be invoked (even if they arent known by `Everything`). Asserting any issue during this event will prevent play mode from starting.
```cs
public class Player : MonoBehaviour, IListener<PlayabilityCheck>
{
    public string? ability;

    void IListener<PlayabilityCheck>.OnEvent(PlayabilityCheck message)
    {
        message.CantIfNull(ability, "Player is missing an ability", this);
    }
}
```
### Iterating using generics
The `Everything.GetAllThatAre<T>()` generic method currently has a good decent bandaid over its design but a bandaid nonetheless. The internal collections stores a list of `object` instances but the user is expecting a `IReadOnlyCollection<T>` the collection must mean that code should somehow transform from `object[]` to `T[]`. Basically this method will cost an allocation in the same way `ArrayPool<T>.Shared` invokes allocations.

Alternatively, there is a `ForEachInAll<T>()` method that avoids this problem, or `Everything.GetAllThatAre(typeof(T))` which will return the internal collection, from where you can parse/cast yourself.

### Non Unity objects
This is also supported, the type needs to implement `ITracked` and should be added/removed to `Everything` manually.
```cs
public class Manager : ITracked
{
    
}

private Manager? manager;

public void CreateManagers()
{
    manager = new Manager();
    Everything.Add(manager);
}

public void DestroyManagers()
{
    Everything.Remove(manager);
}
```