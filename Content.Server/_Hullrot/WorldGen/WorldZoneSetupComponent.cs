using Content.Server.Worldgen.Components;
using Content.Server._Hullrot.Worldgen.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Hullrot.Worldgen;

/// <summary>
/// Added to an entity with <see cref="WorldControllerComponent"> to
/// place zones. For information on zones, see <see cref="WorldZoneSetupPrototype"/>
/// </summary>
[RegisterComponent]
public sealed partial class WorldZoneSetupComponent : Component
{
    /// <summary>
    /// Trigger warning: 2d array
    /// </summary>
    public WorldZonePrototype[,]? ZoneArray;

    [DataField("prototype", customTypeSerializer: typeof(PrototypeIdSerializer<WorldZoneSetupPrototype>))]
    public string Prototype { get; private set; } = string.Empty;
}
