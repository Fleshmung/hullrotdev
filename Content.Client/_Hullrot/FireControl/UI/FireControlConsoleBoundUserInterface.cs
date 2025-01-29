using Content.Shared._Hullrot.FireControl;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Map;
using Robust.Shared.GameObjects;
using OpenToolkit.GraphicsLibraryFramework;

namespace Content.Client._Hullrot.FireControl.UI;

[UsedImplicitly]
public sealed class FireControlConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private FireControlWindow? _window;
    private TransformSystem _xform;

    public FireControlConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _xform = EntMan.EntitySysManager.GetEntitySystem<TransformSystem>();
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<FireControlWindow>();

        _window.OnServerRefresh += OnRefreshServer;

        _window.Radar.OnRadarClick += (coords) =>
        {
            var netCoords = EntMan.GetNetCoordinates(coords);
            SendFireMessage(netCoords);
        };

        _window.Radar.DefaultCursorShape = Control.CursorShape.Crosshair;
    }

    private void OnRefreshServer()
    {
        SendMessage(new FireControlConsoleRefreshServerMessage());
    }

    private void SendFireMessage(NetCoordinates coordinates)
    {
        SendMessage(new FireControlConsoleFireMessage(coordinates));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not FireControlConsoleBoundInterfaceState castState)
            return;

        _window?.UpdateStatus(castState);
    }
}
