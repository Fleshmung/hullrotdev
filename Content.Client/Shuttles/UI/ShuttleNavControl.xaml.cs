using System.Numerics;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using JetBrains.Annotations;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Collections;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Shuttles.UI;

[GenerateTypedNameReferences]
public sealed partial class ShuttleNavControl : BaseShuttleControl
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    private readonly SharedShuttleSystem _shuttles;
    private readonly SharedTransformSystem _transform;

    /// <summary>
    /// Used to transform all of the radar objects. Typically is a shuttle console parented to a grid.
    /// </summary>
    private EntityCoordinates? _coordinates;

    /// <summary>
    /// Entity of controlling console
    /// </summary>
    private EntityUid? _consoleEntity;

    private Angle? _rotation;

    private Dictionary<NetEntity, List<DockingPortState>> _docks = new();

    public bool ShowIFF { get; set; } = true;
    public bool ShowDocks { get; set; } = true;
    public bool RotateWithEntity { get; set; } = true;

    /// <summary>
    /// Raised if the user left-clicks on the radar control with the relevant entitycoordinates.
    /// </summary>
    public Action<EntityCoordinates>? OnRadarClick;

    private List<Entity<MapGridComponent>> _grids = new();

    #region Hullrot
    // These 2 handle timing updates
    private const float RadarUpdateInterval = 2f;
    private float _updateAccumulator = 0f;

    // We ask for the next update a little early so it's more responsive, but we always ignore it until it's time.
    private bool _nextUpdateReady = false;

    // private list CachedNextUpdate

    // We receive our updates in the form of a list of positions, colors, and scale.
    // private list CurrentUpdate

    // As the scan line hits them, they become active blips. They're removed from the above list and added to this list of active blips.
    // private list ActiveBlips

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        _updateAccumulator += args.DeltaSeconds;

        if (_updateAccumulator >= RadarUpdateInterval)
            _updateAccumulator = 0; // I'm not subtracting because frame updates can majorly lag in a way normal ones cannot.
    }
    #endregion Hullrot

    public ShuttleNavControl() : base(64f, 256f, 256f)
    {
        RobustXamlLoader.Load(this);
        _shuttles = EntManager.System<SharedShuttleSystem>();
        _transform = EntManager.System<SharedTransformSystem>();
    }

    public void SetMatrix(EntityCoordinates? coordinates, Angle? angle)
    {
        _coordinates = coordinates;
        _rotation = angle;
    }

    public void SetConsole(EntityUid? consoleEntity)
    {
        _consoleEntity = consoleEntity;
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (_coordinates == null || _rotation == null || args.Function != EngineKeyFunctions.UIClick ||
            OnRadarClick == null)
        {
            return;
        }

        var a = InverseScalePosition(args.RelativePosition);
        var relativeWorldPos = new Vector2(a.X, -a.Y);
        relativeWorldPos = _rotation.Value.RotateVec(relativeWorldPos);
        var coords = _coordinates.Value.Offset(relativeWorldPos);
        OnRadarClick?.Invoke(coords);
    }

    /// <summary>
    /// Gets the entitycoordinates of where the mouseposition is, relative to the control.
    /// </summary>
    [PublicAPI]
    public EntityCoordinates GetMouseCoordinates(ScreenCoordinates screen)
    {
        if (_coordinates == null || _rotation == null)
        {
            return EntityCoordinates.Invalid;
        }

        var pos = screen.Position / UIScale - GlobalPosition;

        var a = InverseScalePosition(pos);
        var relativeWorldPos = new Vector2(a.X, -a.Y);
        relativeWorldPos = _rotation.Value.RotateVec(relativeWorldPos);
        var coords = _coordinates.Value.Offset(relativeWorldPos);
        return coords;
    }

    public void UpdateState(NavInterfaceState state)
    {
        SetMatrix(EntManager.GetCoordinates(state.Coordinates), state.Angle);

        WorldMaxRange = state.MaxRange;

        if (WorldMaxRange < WorldRange)
        {
            ActualRadarRange = WorldMaxRange;
        }

        if (WorldMaxRange < WorldMinRange)
            WorldMinRange = WorldMaxRange;

        ActualRadarRange = Math.Clamp(ActualRadarRange, WorldMinRange, WorldMaxRange);

        RotateWithEntity = state.RotateWithEntity;

        _docks = state.Docks;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        DrawBacking(handle);
        DrawCircles(handle);

        // No data
        if (_coordinates == null || _rotation == null)
        {
            return;
        }

        var xformQuery = EntManager.GetEntityQuery<TransformComponent>();
        var fixturesQuery = EntManager.GetEntityQuery<FixturesComponent>();
        var bodyQuery = EntManager.GetEntityQuery<PhysicsComponent>();

        if (!xformQuery.TryGetComponent(_coordinates.Value.EntityId, out var xform)
            || xform.MapID == MapId.Nullspace)
        {
            return;
        }

        var mapPos = _transform.ToMapCoordinates(_coordinates.Value);
        var posMatrix = Matrix3Helpers.CreateTransform(_coordinates.Value.Position, _rotation.Value);
        var ourEntRot = RotateWithEntity ? _transform.GetWorldRotation(xform) : _rotation.Value;
        var ourEntMatrix = Matrix3Helpers.CreateTransform(_transform.GetWorldPosition(xform), ourEntRot);
        var shuttleToWorld = Matrix3x2.Multiply(posMatrix, ourEntMatrix);
        Matrix3x2.Invert(shuttleToWorld, out var worldToShuttle);
        var shuttleToView = Matrix3x2.CreateScale(new Vector2(MinimapScale, -MinimapScale)) * Matrix3x2.CreateTranslation(MidPointVector);

        // Draw our grid in detail
        var ourGridId = xform.GridUid;
        if (EntManager.TryGetComponent<MapGridComponent>(ourGridId, out var ourGrid) &&
            fixturesQuery.HasComponent(ourGridId.Value))
        {
            var ourGridToWorld = _transform.GetWorldMatrix(ourGridId.Value);
            var ourGridToShuttle = Matrix3x2.Multiply(ourGridToWorld, worldToShuttle);
            var ourGridToView = ourGridToShuttle * shuttleToView;
            var color = _shuttles.GetIFFColor(ourGridId.Value, self: true);

            DrawGrid(handle, ourGridToView, (ourGridId.Value, ourGrid), color);
            DrawDocks(handle, ourGridId.Value, ourGridToView);
        }

        // Draw radar position on the station
        const float radarVertRadius = 2f;
        var radarPosVerts = new Vector2[]
        {
            ScalePosition(new Vector2(0f, -radarVertRadius)),
            ScalePosition(new Vector2(radarVertRadius / 2f, 0f)),
            ScalePosition(new Vector2(0f, radarVertRadius)),
            ScalePosition(new Vector2(radarVertRadius / -2f, 0f)),
        };

        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, radarPosVerts, Color.Lime);

        var rot = ourEntRot + _rotation.Value;
        var viewBounds = new Box2Rotated(new Box2(-WorldRange, -WorldRange, WorldRange, WorldRange).Translated(mapPos.Position), rot, mapPos.Position);
        var viewAABB = viewBounds.CalcBoundingBox();

        _grids.Clear();
        _mapManager.FindGridsIntersecting(xform.MapID, new Box2(mapPos.Position - MaxRadarRangeVector, mapPos.Position + MaxRadarRangeVector), ref _grids, approx: true, includeMap: false);

        // Draw other grids... differently
        foreach (var grid in _grids)
        {
            var gUid = grid.Owner;
            if (gUid == ourGridId || !fixturesQuery.HasComponent(gUid))
                continue;

            var gridBody = bodyQuery.GetComponent(gUid);
            EntManager.TryGetComponent<IFFComponent>(gUid, out var iff);

            if (!_shuttles.CanDraw(gUid, gridBody, iff))
                continue;

            var curGridToWorld = _transform.GetWorldMatrix(gUid);
            var curGridToView = curGridToWorld * worldToShuttle * shuttleToView;

            var labelColor = _shuttles.GetIFFColor(grid, self: false, iff);
            var coordColor = new Color(labelColor.R * 0.8f, labelColor.G * 0.8f, labelColor.B * 0.8f, 0.5f);

            // Others default:
            // Color.FromHex("#FFC000FF")
            // Hostile default: Color.Firebrick
            var labelName = _shuttles.GetIFFLabel(grid, self: false, iff);

            if (ShowIFF &&
                 labelName != null)
            {
                var gridBounds = grid.Comp.LocalAABB;

                var gridCentre = Vector2.Transform(gridBody.LocalCenter, curGridToView);

                var distance = gridCentre.Length();
                var labelText = Loc.GetString("shuttle-console-iff-label", ("name", labelName),
                    ("distance", $"{distance:0.0}"));

                var mapCoords = _transform.GetWorldPosition(gUid);
                var coordsText = $"({mapCoords.X:0.0}, {mapCoords.Y:0.0})";

                // yes 1.0 scale is intended here.
                var labelDimensions = handle.GetDimensions(Font, labelText, 1f);
                var coordsDimensions = handle.GetDimensions(Font, coordsText, 0.7f);

                // y-offset the control to always render below the grid (vertically)
                var yOffset = Math.Max(gridBounds.Height, gridBounds.Width) * MinimapScale / 1.8f;

                // The actual position in the UI.
                var gridScaledPosition = gridCentre - new Vector2(0, -yOffset);

                // Normalize the grid position if it exceeds the viewport bounds
                // normalizing it instead of clamping it preserves the direction of the vector and prevents corner-hugging
                var gridOffset = gridScaledPosition / PixelSize - new Vector2(0.5f, 0.5f);
                var offsetMax = Math.Max(Math.Abs(gridOffset.X), Math.Abs(gridOffset.Y)) * 2f;
                if (offsetMax > 1)
                {
                    gridOffset = new Vector2(gridOffset.X / offsetMax, gridOffset.Y / offsetMax);

                    gridScaledPosition = (gridOffset + new Vector2(0.5f, 0.5f)) * PixelSize;
                }

                var labelUiPosition = gridScaledPosition - new Vector2(labelDimensions.X / 2f, 0);
                var coordUiPosition = gridScaledPosition - new Vector2(coordsDimensions.X / 2f, -labelDimensions.Y);

                // clamp the IFF label's UI position to within the viewport extents so it hugs the edges of the viewport
                // coord label intentionally isn't clamped so we don't get ugly clutter at the edges
                var controlExtents = PixelSize - new Vector2(labelDimensions.X, labelDimensions.Y); //new Vector2(labelDimensions.X * 2f, labelDimensions.Y);
                labelUiPosition = Vector2.Clamp(labelUiPosition, Vector2.Zero, controlExtents);

                // draw IFF label
                handle.DrawString(Font, labelUiPosition, labelText, labelColor);

                // only draw coords label if close enough
                if (offsetMax < 1)
                {
                    handle.DrawString(Font, coordUiPosition, coordsText, 0.7f, coordColor);
                }
            }

            // Detailed view
            var gridAABB = curGridToWorld.TransformBox(grid.Comp.LocalAABB);

            // Skip drawing if it's out of range.
            if (!gridAABB.Intersects(viewAABB))
                continue;

            DrawGrid(handle, curGridToView, grid, labelColor);
            DrawDocks(handle, gUid, curGridToView);
        }

        // If we've set the controlling console, and it's on a different grid
        // to the shuttle itself, then draw an additional marker to help the
        // player determine where they are relative to the shuttle.
        if (_consoleEntity != null && xformQuery.TryGetComponent(_consoleEntity, out var consoleXform))
        {
            if (consoleXform.ParentUid != _coordinates.Value.EntityId)
            {
                var consolePositionWorld = _transform.GetWorldPosition((EntityUid)_consoleEntity);
                var p = Vector2.Transform(consolePositionWorld, worldToShuttle * shuttleToView);
                handle.DrawCircle(p, 5, Color.ToSrgb(Color.Cyan), true);
            }
        }

        #region Hullrot
        // Draw radar line
        // First, figure out which angle to draw.
        Angle angle = _updateAccumulator / RadarUpdateInterval * Math.Tau;
        var origin = ScalePosition(-new Vector2(Offset.X, -Offset.Y));
        handle.DrawLine(origin, origin + angle.ToVec() * ScaledMinimapRadius * 1.42f, Color.Green.WithAlpha(0.1f));

        // Here's how the old north line worked.
        // protected void DrawNorthLine(DrawingHandleScreen handle, Angle angle)
        // {
        //     var origin = ScalePosition(-new Vector2(Offset.X, -Offset.Y));
        //     var aExtent = (angle - Math.Tau / 4).ToVec() * ScaledMinimapRadius * 1.42f;
        //     var lineColor = Color.Red.WithAlpha(0.1f);
        //     handle.DrawLine(origin, origin + aExtent, lineColor);
        // }

        // Draw blips
        #endregion
    }

    private void DrawDocks(DrawingHandleScreen handle, EntityUid uid, Matrix3x2 gridToView)
    {
        if (!ShowDocks)
            return;

        const float DockScale = 0.6f;
        var nent = EntManager.GetNetEntity(uid);

        const float sqrt2 = 1.41421356f;
        const float dockRadius = DockScale * sqrt2;
        // Worst-case bounds used to cull a dock:
        Box2 viewBounds = new Box2(-dockRadius, -dockRadius, Size.X + dockRadius, Size.Y + dockRadius);
        if (_docks.TryGetValue(nent, out var docks))
        {
            foreach (var state in docks)
            {
                var position = state.Coordinates.Position;

                var positionInView = Vector2.Transform(position, gridToView);
                if (!viewBounds.Contains(positionInView))
                {
                    continue;
                }

                var color = Color.ToSrgb(Color.Magenta);

                var verts = new[]
                {
                    Vector2.Transform(position + new Vector2(-DockScale, -DockScale), gridToView),
                    Vector2.Transform(position + new Vector2(DockScale, -DockScale), gridToView),
                    Vector2.Transform(position + new Vector2(DockScale, DockScale), gridToView),
                    Vector2.Transform(position + new Vector2(-DockScale, DockScale), gridToView),
                };

                handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, verts, color.WithAlpha(0.8f));
                handle.DrawPrimitives(DrawPrimitiveTopology.LineStrip, verts, color);
            }
        }
    }

    private Vector2 InverseScalePosition(Vector2 value)
    {
        return (value - MidPointVector) / MinimapScale;
    }
}
