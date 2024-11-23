using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Console;

namespace Content.Client._Hullrot.WorldGen.UI;

[UsedImplicitly]
public sealed class WorldZoneMapUIController : UIController
{
    [Dependency] private readonly IConsoleHost _con = default!;

    public override void Initialize()
    {
        _con.RegisterCommand("worldzonemap", "Draw a map of the current world", "No arguments", WorldZoneMapCommand);
    }

    private void WorldZoneMapCommand(IConsoleShell shell, string argStr, string[] args)
    {
        ToggleWindow();
    }

    private WorldZoneMapWindow _zoneWindow = default!;

    private void EnsureWindow()
    {
        if (_zoneWindow is { Disposed: false })
            return;

        _zoneWindow = UIManager.CreateWindow<WorldZoneMapWindow>();
    }

    public void OpenWindow()
    {
        EnsureWindow();

        _zoneWindow.OpenCentered();
        _zoneWindow.MoveToFront();
    }

    public void ToggleWindow()
    {
        EnsureWindow();

        if (_zoneWindow.IsOpen)
        {
            _zoneWindow.Close();
        }
        else
        {
            OpenWindow();
        }
    }
}
