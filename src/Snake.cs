using Godot;
using System;
using System.Collections.Generic;

public struct SnakeNode {
    public int Id;
    public Vector2 Position;
    public float Rotation;
}

public partial class Snake : Area2D {
    [Export]
    public PackedScene SegmentScene;

    [Export]
    public float CrawlSpeed = 200f;

    [Export]
    public float SegmentTickTime = 0.2f;

    [Export]
    public Node2D SegmentParentNode = null;

    private int _length = 0;

    List<Segment> _segments = new List<Segment>();
    List<SnakeNode> _nodes = new List<SnakeNode>();

    Vector2 _lookingAt;
    Vector2 _velocity;

    Timer _savePosTimer;

    public override void _Ready() {
        _savePosTimer = GetNode<Timer>("SavePositionTimer");
        _savePosTimer.Start();
        _savePosTimer.WaitTime = SegmentTickTime;
        _savePosTimer.Timeout += SavePosition;

        _lookingAt = Position + Vector2.Down;

        CallDeferred("SetLength", 1);
    }

    public override void _Process(double delta) {
        float dt = (float)delta;

        _lookingAt = GetGlobalMousePosition();
        var direction = (_lookingAt - Position);

        if (direction != Vector2.Zero) {
            _velocity = direction.Normalized() * CrawlSpeed;
        }

        Position += _velocity * dt;
        Rotation = direction.Angle();

        if (Input.IsActionJustPressed("debug")) {
            SetLength(_length + 1);
        }
    }

    public void SetLength(int newLength) {
        if (_segments.Count == newLength) {
            return;
        }

        int count = newLength - _segments.Count;

        Node parent = SegmentParentNode;
        if (parent == null) {
            parent = GetParent();
        }

        for (int i = 0; i < count; i++) {
            var seg = SegmentScene.Instantiate<Segment>();
            seg.SegmentNumber = _length + i + 1;
            seg.GetNextPosRot = GetNextPosRot;
            seg.TickTime = SegmentTickTime;
            seg.Position = Position;
            seg.Rotation = Rotation;

            _segments.Add(seg);
            parent.AddChild(seg);

            seg.SetDelay(SegmentTickTime * (i + 1));
        }

        _length = newLength;
    }

    public int GetLength() {
        return _length;
    }

    void SavePosition() {
        if (_nodes.Count == _length * 2) {
            _nodes.RemoveRange(0, _length);
            GD.Print("Cleared!");
        }

        int latestId = 0;
        if (_nodes.Count > 0) {
            latestId = _nodes[_nodes.Count - 1].Id;
        }

        var node = new SnakeNode {
            Id = latestId + 1,
            Position = Position,
            Rotation = Rotation,
        };

        _nodes.Add(node);

        GD.Print($"{node.Id}: {node.Position} {node.Rotation}");

        SyncSegments();
    }

    void SyncSegments() {

        for (int i = 0; i < _segments.Count; i++) {
            int sync_idx = _nodes.Count - (i + 2);
            int next_idx = _nodes.Count - (i + 1);

            if (sync_idx < 0 || next_idx < 0) {
                continue;
            }

            if (sync_idx >= _nodes.Count || next_idx >= _nodes.Count) {
                continue;
            }

            var sync_node = _nodes[_nodes.Count - (i + 2)];
            var next_node = _nodes[_nodes.Count - (i + 1)];

            _segments[i].SyncPosition(sync_node, next_node);
        }
    }

    SnakeNode GetNextPosRot(int segmentNumber, int lastNodeId) {
        int nodeIdx = _nodes.FindIndex((item) => item.Id == lastNodeId);

        if (nodeIdx == -1) {
            return _nodes[_nodes.Count - segmentNumber];
        }

        return _nodes[nodeIdx + 1];
    }
}
