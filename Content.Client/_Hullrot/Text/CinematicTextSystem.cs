using Robust.Client.Graphics;

namespace Content.Client._Hullrot.Text;

public sealed class CinematicTextSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overMan = default!;
    private CinematicTextOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new();
        _overMan.AddOverlay(_overlay);
    }

    public void DrawText(string text)
    {
        _overlay.Text = text;
        _overlay.CharInterval = TimeSpan.FromSeconds(2f / text.Length);
    }
}

