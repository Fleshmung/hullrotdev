using System.Numerics;
using Content.Server.Worldgen.Prototypes;
using Content.Shared._Hullrot.Worldgen.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Hullrot.Worldgen.Prototypes;

/// <summary>
/// A "zone" is effectively a packaging of a biome and additional information that occupies a Worldgen tile.
/// In a different project, it would make sense to expand the biome prototype. Here, we instead do this to avoid conflicts.
/// </summary>
[Prototype("worldZone")]
public sealed partial class WorldZonePrototype : IPrototype
{
    /// <inheritdoc />
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("biome", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<BiomePrototype>))]
    public string Biome = default!;

    [DataField("aesthetics", customTypeSerializer: typeof(PrototypeIdSerializer<WorldZoneAestheticsPrototype>))]
    public string Aesthetics = default!;

    /// <summary>
    /// The grid tiles occupied.
    /// I'm sorry, This should be on the setup but I don't feel like dealing with serialization isuses
    /// </summary>
    [DataField("tiles")]
    public List<Vector2i> Tiles = new();
}
