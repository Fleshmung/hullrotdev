using Robust.Client.UserInterface;
using Content.Shared._Hullrot.Worldgen.Prototypes;
using Robust.Client.Graphics;
using System.Numerics;

namespace Content.Client._Hullrot.WorldGen.UI;

/// <summary>
/// Draws a map of world zones.
/// </summary>
public sealed class WorldZoneMapControl : Control
{
    private const int ICON_SIZE = 32;
    private WorldZoneAestheticsPrototype[,] _zoneMap;
    public WorldZoneMapControl(WorldZoneAestheticsPrototype[,] zoneMap)
    {
        IoCManager.InjectDependencies(this);

        _zoneMap = zoneMap;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var x_length = _zoneMap.GetLength(0);
        var y_length = _zoneMap.GetLength(1);

        var box = new UIBox2i(new Vector2i(0, 0), new Vector2i(x_length * ICON_SIZE, y_length * ICON_SIZE));
        handle.DrawRect(box, Color.FromHex("#424245"));

        // draw grid
        int x = 1;
        while (x < x_length * ICON_SIZE + 1)
        {
            handle.DrawLine(new Vector2(x, 1), new Vector2(x, y_length * ICON_SIZE + 1), color: Color.White);
            x += 32;
        }

        int y = 1;
        while (y < y_length * ICON_SIZE + 1)
        {
            handle.DrawLine(new Vector2(1, y), new Vector2(x_length * ICON_SIZE + 1, y), color: Color.White);
            y += 32;
        }
    }
}
