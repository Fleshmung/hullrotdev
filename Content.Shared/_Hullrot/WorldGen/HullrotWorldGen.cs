using Content.Shared._Hullrot.Worldgen.Prototypes;
using System.Diagnostics.Contracts;
using System.Numerics;
using Robust.Shared.Serialization;

namespace Content.Shared._Hullrot.Worldgen;

public static class HullrotWorldGen
{
    /// <summary>
    ///     The size of each chunk (isn't that self-explanatory.)
    ///     Be careful about how small you make this.
    /// </summary>
    public const int ChunkSize = 1000;

    /// <summary>
    ///     Converts world coordinates to chunk coordinates.
    /// </summary>
    /// <param name="inp">World coordinates</param>
    /// <returns>Chunk coordinates</returns>
    [Pure]
    public static Vector2i WorldToChunkCoords(Vector2i inp)
    {
        return (inp * new Vector2(1.0f / ChunkSize, 1.0f / ChunkSize)).Floored();
    }

    /// <summary>
    ///     Converts world coordinates to chunk coordinates.
    /// </summary>
    /// <param name="inp">World coordinates</param>
    /// <returns>Chunk coordinates</returns>
    [Pure]
    public static Vector2 WorldToChunkCoords(Vector2 inp)
    {
        return inp * new Vector2(1.0f / ChunkSize, 1.0f / ChunkSize);
    }

    /// <summary>
    ///     Converts chunk coordinates to world coordinates.
    /// </summary>
    /// <param name="inp">Chunk coordinates</param>
    /// <returns>World coordinates</returns>
    [Pure]
    public static Vector2 ChunkToWorldCoords(Vector2i inp)
    {
        return inp * ChunkSize;
    }

    /// <summary>
    ///     Converts chunk coordinates to world coordinates.
    /// </summary>
    /// <param name="inp">Chunk coordinates</param>
    /// <returns>World coordinates</returns>
    [Pure]
    public static Vector2 ChunkToWorldCoords(Vector2 inp)
    {
        return inp * ChunkSize;
    }

    /// <summary>
    ///     Converts chunk coordinates to world coordinates, getting the center of the chunk.
    /// </summary>
    /// <param name="inp">Chunk coordinates</param>
    /// <returns>World coordinates</returns>
    [Pure]
    public static Vector2 ChunkToWorldCoordsCentered(Vector2i inp)
    {
        return inp * ChunkSize + Vector2i.One * (ChunkSize / 2);
    }

    /// <summary>
    /// We're joker mode due to serialization restrictions.
    /// </summary>
    /// <returns>A format that works to send arrays to client.</returns>
    public static List<(WorldZoneAestheticsPrototype, int, int)> GetSerializableZoneList()
    {
        return new List<(WorldZoneAestheticsPrototype, int, int)>();
    }
}

[Serializable, NetSerializable]
public sealed class GiveMapZoneLayoutEvent : EntityEventArgs
{
    public readonly int X;
    public readonly int Y;
    //I'm joker mode
    // System.NotSupportedException: Multi-dim arrays not supported: Content.Shared._Hullrot.Worldgen.Prototypes.WorldZoneAestheticsPrototype[,]
    public readonly List<(WorldZoneAestheticsPrototype, int, int)>? Layout;
    public GiveMapZoneLayoutEvent(int x, int y, List<(WorldZoneAestheticsPrototype, int, int)>? layout)
    {
        X = x;
        Y = y;
        Layout = layout;
    }
}
