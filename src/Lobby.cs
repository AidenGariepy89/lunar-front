using Godot;

public partial class Lobby : Control
{
    Main _main;

    Button _joinButton;

    public override void _Ready()
    {
        _joinButton = GetNode<Button>("Join");
        _joinButton.Pressed += PressedJoin;
    }

    public void Instantiate(Main main)
    {
        _main = main;
    }

    void PressedJoin()
    {
        _main.StartClient();
    }
}
