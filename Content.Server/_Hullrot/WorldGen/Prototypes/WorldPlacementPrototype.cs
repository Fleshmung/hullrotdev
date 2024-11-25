using Content.Server.Worldgen.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server._Hullrot.Worldgen.Prototypes;

/// <summary>
/// While biome selection is handled by <see cref="WorldgenConfigPrototype">
/// this handles the placement of static structures, zones, and other Hullrot-specific behavior.
/// </summary>
[Prototype("worldPlacement")]
public sealed partial class WorldPlacementPrototype : IPrototype
{
    /// <inheritdoc />
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("maps", customTypeSerializer: typeof(PrototypeIdListSerializer<WorldPlacementMapPrototype>))]
    public List<string> Maps = new();
}
