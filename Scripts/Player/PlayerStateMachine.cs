public sealed class PlayerStateMachine
{
    private double _movementOverrideRemaining;

    public PlayerMovementState CurrentState { get; private set; } =
        PlayerMovementState.Airborne;

    public bool AcceptsMovementInput => _movementOverrideRemaining <= 0.0;

    public void BeginMovementOverride(double duration)
    {
        _movementOverrideRemaining = duration;
        CurrentState = PlayerMovementState.CardAbility;
    }

    public void Tick(double delta)
    {
        if (_movementOverrideRemaining <= 0.0)
            return;

        _movementOverrideRemaining -= delta;

        if (_movementOverrideRemaining < 0.0)
            _movementOverrideRemaining = 0.0;
    }

    public void SyncLocomotion(bool isOnFloor)
    {
        if (!AcceptsMovementInput)
        {
            CurrentState = PlayerMovementState.CardAbility;
            return;
        }

        CurrentState = isOnFloor
            ? PlayerMovementState.Grounded
            : PlayerMovementState.Airborne;
    }
}