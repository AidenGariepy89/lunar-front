using Godot;

namespace Client;

public partial class Hud : Control
{
    Label _earthScore;
    Label _marsScore;

    public void Initialize(int earth, int mars)
    {
        _earthScore = GetNode<Label>("%EarthScore");
        _marsScore = GetNode<Label>("%MarsScore");

        _earthScore.Text = earth.ToString();
        _marsScore.Text = mars.ToString();
    }

    public void Update(int earth, int mars)
    {
        _earthScore.Text = earth.ToString();
        _marsScore.Text = mars.ToString();
    }
}
