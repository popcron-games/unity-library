# Lib
Encourages a more functional approach to Unity development. My personal favourite approach if not ECS.

### Retrieving a list of all instances by assignable types
```csharp
public class Player : CustomMonoBehaviour, IPlayer
{

}

public class SomethingElse : CustomMonoBehaviour, IPlayer
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
public class PlayerSounds : CustomMonoBehaviour, IListener<PlayerHasJumped>
{
    void IListener<PlayerHasJumped>.OnEvent(PlayerHasJumped message)
    {
        //jumped!
    }
}
```
### Preventing play mode because of user defined reasons (validation)
```csharp
public class Player : CustomMonoBehaviour, IListener<PlayabilityCheck>
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
public class Player : CustomMonoBehaviour, IListener<ValidationEvent>
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
public class Item : CustomMonoBehaviour, IIdentifier 
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
public class Inventory : CustomMonoBehaviour, IIdentifier, IBranch
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
### Inputs to initialization similar to new(data) pattern
```csharp
public class Player : CustomMonoBehaviour, IInstantiateWithInput<Player.Settings>
{
    [SerializeField]
    private Settings settings;

    [Serializable]
    public struct Settings
    {
        public int characterId;

        public Settings(int characterId)
        {
            this.characterId = characterId;
        }
    }

    void IInstantiateWithInput<Player.Settings>.Pass(Player.Settings input)
    {
        settings = input;
    }

    protected override void OnEnable()
    {
        //after settings is set
    }
}

Player.Settings input = new Player.Settings(1);
Player player = CustomMonoBehaviour.InstantiateWithInput<Player, Player.Settings>(input);
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
Their only purpose is to automatically do what `Everything` expects from you. This behaviour is also guarded, so that a sub type doesn't accidentally hide it, there are sealable types first, and then the custom types under the sealable type so that it seal the virtual methods for the callbacks.

This pattern is done for the `MonoBehaviour`, `ScriptableObject`, `Editor` and `EditorWindow` Unity base types. 
If you see a set of different methods and functionalities to have that the custom ones that I wrote don't do then feel free to make your own. I'm not a fan of "base" types but working within Unity's implementation of their design there will always be "base" classes.

The custom types will also dispatch a `ValidateEvent` to themselves when `OnValidate()` is meant to happen.

### Iterating using generics
The `Everything.GetAllThatAre<T>()` generic method currently has a good decent bandaid over its design but a bandaid nonetheless. The internal collections stores a list of `object` instances but the user is expecting a `IReadOnlyCollection<T>` the collection must mean that code should somehow transform from `object[]` to `T[]`. To accomplish this, there is a `RecycledList<T>` that is reliant on `ArrayPool<T>` to rent and return buffers safely at the end of the frame before the next.

Alternatively, there is a `ForEachInAll<T>()` method that avoids this problem, or `Everything.GetAllThatAre(typeof(T))` which will return a collection of objects from where you can parse/cast yourself.

### Non Unity objects
These cases also work, but the instance must be added and removed manually by you.
The type must also implement `IUnityObject`, from here it then receive events like the other cases.

### Retreiving data more efficiently than FindObjectsByType<T>
When something is added to `Everything`, the type and the ancestor types of the object, along with all implementing types and their ancestors are stored in a map.
All of these other types are "assignable from" types, and the map is a map of all types to list of instances, assuming the type is assignable from the instance type.
This means that `Everything.GetAllThatAre<T>()` returns the collection directly without extra iterations.