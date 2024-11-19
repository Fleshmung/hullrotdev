using Content.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Hullrot.Worldgen.Prototypes;

/// <summary>
/// This sets the music, name, and parallax for zones.
/// </summary>
[Prototype("worldZoneAesthetics")]
public sealed partial class WorldZoneAestheticsPrototype : IPrototype
{
    /// <inheritdoc />
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("ambientMusic", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<AmbientMusicPrototype>))]
    public string AmbientMusic = default!;

    /// This needs to be tested in integration tests since parallax prototypes are not visible here
    [DataField("parallax", required: true)]
    public string Parallax = default!;
}
