using System.Numerics;
using Content.Server.Worldgen.Prototypes;
using Content.Server.Maps;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Hullrot.Worldgen;

/// <summary>
/// While biome selection is handled by <see cref="WorldgenConfigPrototype">
/// this handles the placement of static structures, zones, and other Hullrot-specific behavior.
/// </summary>
[Prototype("worldPlacementMap")]
public sealed partial class WorldPlacementMapPrototype : IPrototype
{
    /// <inheritdoc />
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("mapPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<GameMapPrototype>))]
    public string Map = "";

    [DataField]
    public Vector2 Pos = Vector2.Zero;
}
