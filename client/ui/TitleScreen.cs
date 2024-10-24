using Godot;

namespace Client;

public partial class TitleScreen : Control
{
    public Button JoinButton;
    public LineEdit IPEdit;
    public Label ErrMsg;

    public void Initialize()
    {
        JoinButton = GetNode<Button>("JoinButton");
        IPEdit = GetNode<LineEdit>("IPAddress");
        ErrMsg = GetNode<Label>("ErrMsg");
    }
}
