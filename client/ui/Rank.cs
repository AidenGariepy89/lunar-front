using Godot;

namespace Client;

public partial class Rank : Node2D
{
    Client _client;
    ScoutClient _scout;

    public void Initialize(Client client, ScoutClient scout)
    {
        _client = client;
        _scout = scout;
    }

    public override void _Process(double delta)
    {
        Position = _scout.Position;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_client.ShowDebug)
        {
            DrawString(
                ThemeDB.FallbackFont,
                new Vector2(0, 30),
                $"#{_scout.Rank}",
                HorizontalAlignment.Center,
                modulate: Colors.White
            );
            DrawString(
                ThemeDB.FallbackFont,
                new Vector2(0, 50),
                $"{_scout.Data.KillCount} Kills",
                HorizontalAlignment.Center,
                modulate: Colors.White
            );
        }
    }
}
