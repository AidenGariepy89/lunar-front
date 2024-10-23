using System.Linq;
using Godot;

namespace Core;

public partial class Main : Node2D
{
    [Export]
    public PackedScene ServerScene;
    [Export]
    public PackedScene ClientScene;

    public override void _Ready()
    {
        if (OS.GetCmdlineArgs().Contains("--server"))
        {
            var server = ServerScene.Instantiate<Server.Server>();
            AddChild(server);
        }
        else
        {
            // client
        }
    }
}
