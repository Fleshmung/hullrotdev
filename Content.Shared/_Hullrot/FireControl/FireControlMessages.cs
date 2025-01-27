using Robust.Shared.Serialization;
using Robust.Shared.Map;

namespace Content.Shared._Hullrot.FireControl;

[Serializable, NetSerializable]
public sealed class FireControlConsoleUpdateEvent : EntityEventArgs
{
}

/// <summary>
/// Kind of modeled these off of how air alarms handle all this.
/// </summary>
[Serializable, NetSerializable]
public sealed class FireControlConsoleBoundInterfaceState : BoundUserInterfaceState
{
    public FireControllableEntry[] FireControllables;

    public FireControlConsoleBoundInterfaceState(FireControllableEntry[] fireControllables)
    {
        FireControllables = fireControllables;
    }
}

[Serializable, NetSerializable]
public enum FireControlConsoleUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class FireControlConsoleRefreshServerMessage : BoundUserInterfaceMessage
{

}

[Serializable, NetSerializable]
public struct FireControllableEntry
{
    /// <summary>
    /// The entity in question
    /// </summary>
    public NetEntity NetEntity;

    /// <summary>
    /// Location of the entity
    /// </summary>
    public NetCoordinates Coordinates;

    /// <summary>
    /// Display name of the entity
    /// </summary>
    public string Name;

    public FireControllableEntry(NetEntity entity, NetCoordinates coordinates, string name)
    {
        NetEntity = entity;
        Coordinates = coordinates;
        Name = name;
    }
}
