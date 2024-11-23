using Content.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Utility;

namespace Content.Shared._Hullrot.Worldgen.Prototypes;

/// <summary>
/// This sets the music, name, and parallax for zones.
/// </summary>
[Prototype("worldZoneAesthetics"), NetSerializable, Serializable]
public sealed partial class WorldZoneAestheticsPrototype : IPrototype
{
    /// <inheritdoc />
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("ambientMusic", customTypeSerializer: typeof(PrototypeIdSerializer<AmbientMusicPrototype>))]
    public string? AmbientMusic;

    // cannot be checked for serialization unfortunately - par for the course for client prototypes
    [DataField]
    public string? Parallax;

    [DataField]
    public string? Name;

    /// <summary>
    ///     Icon representing this action in the zone map.
    /// </summary>
    [DataField]
    public SpriteSpecifier Icon = new SpriteSpecifier.Texture(new("Tiles/cropped_parallax.png"));

}
