using System;
using Godot;

public partial class Segment : Area2D {

    public delegate SnakeNode SegmentCallback(int segmentNumber, int lastNodeId);

    public int SegmentNumber;
    public SegmentCallback GetNextPosRot;
    public float TickTime;

    Timer _animationTimer;
    Timer _delayTimer;
    Sprite2D _sprite;

    SnakeNode _start;
    SnakeNode _target;
    float _progress = 0.0f;

    bool _shouldMove = false;

    public override void _Ready() {
        _sprite = GetNode<Sprite2D>("Sprite2D");

        _animationTimer = GetNode<Timer>("AnimationTimer");
        _animationTimer.Timeout += AnimationTrigger;

        _delayTimer = GetNode<Timer>("DelayTimer");
        _delayTimer.Timeout += DelayTimeout;
    }

    public override void _Process(double delta) {
        if (!_shouldMove) {
            return;
        }
        if (_progress == 1.0f) {
            return;
        }

        _progress += 1.0f / (TickTime / (float)delta);
        Mathf.Clamp(_progress, 0.0f, 1.0f);

        Position = _start.Position.Lerp(_target.Position, _progress);
        Rotation = Mathf.Lerp(_start.Rotation, _target.Rotation, _progress);
    }

    public void SetDelay(double delay) {
        _delayTimer.WaitTime = delay;
        _delayTimer.Start();
        _shouldMove = false;
    }

    public void SyncPosition(SnakeNode node, SnakeNode next) {
        Position = node.Position;
        Rotation = node.Rotation;

        _progress = 0.0f;
        _start = node;
        _target = next;
    }

    void AnimationTrigger() {
        _sprite.FlipV = !_sprite.FlipV;
    }

    void DelayTimeout() {
        _shouldMove = true;
    }
}
