using Godot;
using Client;

namespace Core;

public partial class Minimap : Node2D
{
    [Export]
    public float ScaleRatio = 0.018f;
    [Export]
    public float MarkerRadius = 2f;
    [Export]
    public float Offset = 3f;

    [Export]
    public Color MinimapBackground = Colors.DarkSlateGray;
    [Export]
    public Color MinimapEarthTeam = Colors.Blue;
    [Export]
    public Color MinimapMarsTeam = Colors.Orange;
    [Export]
    public float Alpha = 0.8f;

    Main _mainRef;

    Timer _refresh;
    Timer _blink;
    bool _activated = false;
    bool _planetBlink = true;

    public override void _Ready()
    {
        _refresh = GetNode<Timer>("Refresh");
        _refresh.Timeout += () => QueueRedraw();

        _blink = GetNode<Timer>("Blink");
        _blink.Timeout += () => {
            _planetBlink = !_planetBlink;
        };
    }

    public override void _Draw()
    {
        if (!_activated)
        {
            return;
        }

        var viewportHalf = GetViewportRect().Size * 0.5f;
        var end = (_mainRef.Map.BottomRight - _mainRef.Map.TopLeft) * ScaleRatio;
        var start = -viewportHalf + Vector2.One * Offset;

        var bgColor = MinimapBackground;
        bgColor.A = Alpha;
        DrawRect(new Rect2(start, end), bgColor);

        if (_planetBlink)
        {
            DrawCircle(
                start + end * 0.5f + Planet.EarthPosition * ScaleRatio,
                MarkerRadius + 1f,
                MinimapEarthTeam
            );
            DrawCircle(
                start + end * 0.5f + Planet.MarsPosition * ScaleRatio,
                MarkerRadius + 1f,
                MinimapMarsTeam
            );
        }

        foreach (var child in _mainRef.Scouts.GetChildren())
        {
            var scout = child as ScoutClient;

            Color color = (scout.Data.Faction == Faction.Earth) ? MinimapEarthTeam : MinimapMarsTeam;

            if (scout.Data.MultiplayerID == Multiplayer.GetUniqueId())
            {
                DrawCircle(
                    start + end * 0.5f + scout.Position * ScaleRatio,
                    MarkerRadius + 1f,
                    Colors.White
                );
            }
            else
            {
                DrawCircle(
                    start + end * 0.5f + scout.Position * ScaleRatio,
                    MarkerRadius,
                    color
                );
            }
        }
    }

    public void Initialize(Main mainRef)
    {
        _mainRef = mainRef;

        _refresh.Start();
        _blink.Start();
        _activated = true;

        QueueRedraw();
    }
}
