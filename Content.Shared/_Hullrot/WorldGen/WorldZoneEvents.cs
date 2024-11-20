using Content.Shared._Hullrot.Worldgen.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Hullrot.Worldgen;

[Serializable, NetSerializable]
public sealed class RequestMapZoneLayoutEvent : EntityEventArgs
{
    public readonly int MapID;

    public RequestMapZoneLayoutEvent(int mapID)
    {
        MapID = mapID;
    }
}

[Serializable, NetSerializable]
public sealed class GiveMapZoneLayoutEvent : EntityEventArgs
{
    public readonly int MapID;
    public readonly int X;
    public readonly int Y;
    //I'm joker mode
    // System.NotSupportedException: Multi-dim arrays not supported: Content.Shared._Hullrot.Worldgen.Prototypes.WorldZoneAestheticsPrototype[,]
    public readonly List<(WorldZoneAestheticsPrototype, int, int)>? Layout;
    public GiveMapZoneLayoutEvent(int mapID, int x, int y, List<(WorldZoneAestheticsPrototype, int, int)>? layout)
    {
        MapID = mapID;
        X = x;
        Y = y;
        Layout = layout;
    }
}
