using Content.Server.Worldgen.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server._Hullrot.Worldgen.Prototypes;

/// <summary>
/// This controls the placement of <see cref="WorldZonePrototype"/>
/// </summary>
[Prototype("worldZoneSetup")]
public sealed partial class WorldZoneSetupPrototype : IPrototype
{
    /// <inheritdoc />
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Inradius of the perfect square we're setting up.
    /// This means that the grid we're making ranges from x to -x, y to -y.
    /// </summary>
    [DataField(required: true)]
    public int Inradius;

    /// <summary>
    /// Default zone to populate the array with.
    /// </summary>
    [DataField("defaultZone", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<WorldZonePrototype>))]
    public string DefaultZone = "";

    /// <summary>
    /// Default zone for out-of-bounds.
    /// Will use default zone if not defined.
    /// </summary>
    [DataField("oobZone", customTypeSerializer: typeof(PrototypeIdSerializer<WorldZonePrototype>))]
    public string? OobZone;

    /// <summary>
    /// List of zones in this world.
    /// </summary>
    [DataField("zones", customTypeSerializer: typeof(PrototypeIdListSerializer<WorldZonePrototype>))]
    public List<string> Zones = new();
}
