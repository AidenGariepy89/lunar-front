using Godot;
using System.Collections.Generic;
using Core;

public partial class InputCollector : Node2D
{
    List<InputAction> _actions = new List<InputAction>();
    float _time;
    bool _collecting = false;
    bool _thrustForwardOn = false;
    bool _thrustBackwardOn = false;
    bool _thrustRightwardOn = false;
    bool _thrustLeftwardOn = false;
    Vector2 _lastMouse = Vector2.Zero;

    public void StartCollection()
    {
        _time = 0.0f;
        _actions.Clear();
        _collecting = true;
    }

    public List<InputAction> FinishCollection()
    {
        _collecting = false;
        return _actions;
    }

    public void Update(float dt)
    {
        _time += dt;

        if (!_collecting)
        {
            return;
        }

        if (!GetWindow().HasFocus())
        {
            return;
        }

        var forward = Input.IsActionPressed("thrust_forward");
        var backward = Input.IsActionPressed("thrust_backward");
        var rightward = Input.IsActionPressed("thrust_right");
        var leftward = Input.IsActionPressed("thrust_left");

        if (forward != _thrustForwardOn)
        {
            var action = new InputAction();
            action.Type = InputAction.InputType.ThrustForward;
            action.Time = _time;
            action.Value = forward;
            _actions.Add(action);
            _thrustForwardOn = forward;
        }
        if (backward != _thrustBackwardOn)
        {
            var action = new InputAction();
            action.Type = InputAction.InputType.ThrustBackward;
            action.Time = _time;
            action.Value = backward;
            _actions.Add(action);
            _thrustBackwardOn = backward;
        }
        if (rightward != _thrustRightwardOn)
        {
            var action = new InputAction();
            action.Type = InputAction.InputType.ThrustRightward;
            action.Time = _time;
            action.Value = rightward;
            _actions.Add(action);
            _thrustRightwardOn = rightward;
        }
        if (leftward != _thrustLeftwardOn)
        {
            var action = new InputAction();
            action.Type = InputAction.InputType.ThrustLeftward;
            action.Time = _time;
            action.Value = leftward;
            _actions.Add(action);
            _thrustLeftwardOn = leftward;
        }
        var mousePos = GetGlobalMousePosition();
        if (Utils.VectorsDiffer(mousePos, _lastMouse, 0.5f))
        {
            var action = new InputAction();
            action.Type = InputAction.InputType.Mouse;
            action.Time = _time;
            action.Value = mousePos;
            _actions.Add(action);
            _lastMouse = mousePos;
        }
    }
}
