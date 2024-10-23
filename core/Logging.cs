using Godot;

namespace Core;

public class Logger
{
    public long MultiplayerId;
    public string Name;

    public Logger(long multiplayerId, string name)
    {
        MultiplayerId = multiplayerId;
        Name = name;
    }

    public void Line(string msg)
    {
        GD.Print($"[{Name}:{MultiplayerId}] {msg}");
    }

    public void Err(string msg)
    {
        GD.PushError($"[{Name}:{MultiplayerId}] {msg}");
    }

    public void Warn(string msg)
    {
        GD.PushWarning($"[{Name}:{MultiplayerId}] {msg}");
    }

    public void StdErr(string msg)
    {
        GD.PrintErr($"[{Name}:{MultiplayerId}] {msg}");
    }
}
