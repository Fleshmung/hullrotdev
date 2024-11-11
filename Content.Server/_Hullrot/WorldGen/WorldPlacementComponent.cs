using Content.Server.Worldgen.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Hullrot.Worldgen.Prototypes;

/// <summary>
/// This is ideally added with <see cref="WorldgenConfigPrototype">
/// to make the placements defined in a <see cref="WorldPlacementPrototype"/>
/// </summary>
[RegisterComponent]
public sealed partial class WorldPlacementComponent : Component
{
    [DataField("prototype", customTypeSerializer: typeof(PrototypeIdSerializer<WorldPlacementPrototype>))]
    public string Prototype { get; private set; } = string.Empty;
}
