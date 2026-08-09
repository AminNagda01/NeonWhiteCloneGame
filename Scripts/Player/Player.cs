using Godot;

public partial class Player : CharacterBody3D
{
	[ExportCategory("Movement")]
	[Export] public float MoveSpeed { get; private set; } = 8.0f;
	[Export] public float JumpVelocity { get; private set; } = 5.5f;
	[Export] public float PlayerGravitySpeed { get; private set; } = -10f;
	[Export] public float GroundAcceleration { get; private set; } = 60f;
	[Export] public float GroundDeceleration { get; private set; } = 80f;
	[Export] public float AirDeceleration { get; private set; } = 25f;
	[Export] public float CoyoteTime { get; private set; } = 0.12f;
	[Export] public float JumpBufferTime { get; private set; } = 0.12f;

	[ExportCategory("Look")]
	[Export] public Node3D CameraPivot { get; private set; }
	[Export] public Camera3D PlayerCamera { get; private set; }
	
	[ExportCategory("Camera Feel")]
	[Export] public float SpeedFovBonus { get; private set; } = 10.0f; //how much to add onto fov at speed, so that fov can change 
	[Export] public float FovChangeSpeed { get; private set; } = 8.0f;
	
	private float MaxFov => PlayerSettings.Instance.BaseFov + SpeedFovBonus;
	
	private float MinPitchDegrees = -89.0f;
	private float MaxPitchDegrees = 89.0f;
	private float _minPitchRadians; //if we for some reason needed to change the max and min pitch in real time,
	private float _maxPitchRadians; //all 4 lines here would need to be public, and also not instantiated in Ready() 
	
	private double _timeSinceLeftGround;
	private double _timeSinceJumpPressed;
	
	private Vector3 _playerGravityVector; 

	private float _cameraPitch;

	private float effectiveSens; 

	//currently sense is 0-1, so this will scale it properly so that max is 0.05f 
	private float sensMulti = 0.01f;

	public override void _Ready()
	{
		_minPitchRadians = Mathf.DegToRad(MinPitchDegrees);
		_maxPitchRadians = Mathf.DegToRad(MaxPitchDegrees);
		_playerGravityVector = new Vector3(0, PlayerGravitySpeed, 0); 
		
		_timeSinceLeftGround = CoyoteTime + 1.0;
		_timeSinceJumpPressed = JumpBufferTime + 1.0;

		effectiveSens = PlayerSettings.Instance.MouseSensitivity * sensMulti; 
		
		if (CameraPivot == null)
		{
			GD.PushError($"{nameof(Player)} needs a CameraPivot assigned.");
			SetPhysicsProcess(false);
			SetProcessUnhandledInput(false);
			return;
		}
		
		if (PlayerCamera == null)
		{
			GD.PushError($"{nameof(Player)} needs a PlayerCamera assigned.");
			SetPhysicsProcess(false);
			SetProcessUnhandledInput(false);
			return;
		}

		PlayerCamera.Fov = PlayerSettings.Instance.BaseFov;

		Input.MouseMode = Input.MouseModeEnum.Captured;

		if (PlayerSettings.Instance == null)
		{
			GD.PushError("PlayerSettings.Instance is null!");
		}
		else
		{
			PlayerSettings.Instance.playerSettingsChanged += UpdatePlayerSettings; 
		}
	}

	private void UpdatePlayerSettings()
	{
		PlayerCamera.Fov = PlayerSettings.Instance.BaseFov;
		effectiveSens = PlayerSettings.Instance.MouseSensitivity * sensMulti;
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (Input.MouseMode != Input.MouseModeEnum.Captured)
		{
			return;
		}

		if (inputEvent is InputEventMouseMotion mouseMotion)
		{
			RotateLook(mouseMotion.Relative);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		UpdateJumpTimers(delta);
		Vector3 velocity = Velocity;

		ApplyGravity(ref velocity, delta);
		ApplyJump(ref velocity);
		ApplyMovement(ref velocity, delta);

		Velocity = velocity;
		MoveAndSlide();
		
		UpdateCameraFov(delta);
	}
	
	private void UpdateJumpTimers(double delta)
	{
		if (IsOnFloor())
		{
			_timeSinceLeftGround = 0.0;
		}
		else
		{
			_timeSinceLeftGround += delta;
		}

		if (Input.IsActionJustPressed("jump"))
		{
			_timeSinceJumpPressed = 0.0;
		}
		else
		{
			_timeSinceJumpPressed += delta;
		}
	}

	private void ApplyGravity(ref Vector3 velocity, double delta)
	{
		if (IsOnFloor())
		{
			return;
		}

		velocity += _playerGravityVector * (float)delta;
	}

	private void ApplyJump(ref Vector3 velocity)
	{
		bool canUseCoyoteJump = _timeSinceLeftGround <= CoyoteTime;
		bool hasBufferedJump = _timeSinceJumpPressed <= JumpBufferTime;
		
		if (!canUseCoyoteJump || !hasBufferedJump)
		{
			return;
		}

		velocity.Y = JumpVelocity;
		
		_timeSinceLeftGround = CoyoteTime + 1.0;
		_timeSinceJumpPressed = JumpBufferTime + 1.0;
	}

	private void ApplyMovement(ref Vector3 velocity, double delta)
	{
		Vector2 inputDirection = Input.GetVector(
			"move_left",
			"move_right",
			"move_forward",
            "move_backward"
		);

		Vector3 moveDirection = Transform.Basis * new Vector3(
			inputDirection.X,
			0.0f,
			inputDirection.Y
		);

		moveDirection = moveDirection.Normalized();
		
		Vector3 horizontalVelocity = new Vector3(
			velocity.X,
			0.0f,
			velocity.Z
		);

		if (moveDirection == Vector3.Zero) {
			float deceleration = IsOnFloor() ? GroundDeceleration : AirDeceleration;

			horizontalVelocity = horizontalVelocity.MoveToward(Vector3.Zero,deceleration * (float)delta);
		}
		else {
			Vector3 targetVelocity = moveDirection * MoveSpeed;
			float acceleration = GroundAcceleration;

			horizontalVelocity = horizontalVelocity.MoveToward(
				targetVelocity,
				acceleration * (float)delta
			);
		}

		velocity.X = horizontalVelocity.X;
		velocity.Z = horizontalVelocity.Z; 
	}
	
	private void UpdateCameraFov(double delta)
	{
		Vector3 horizontalVelocity = new Vector3(
			Velocity.X,
			0.0f,
			Velocity.Z
		);

		float horizontalSpeed = horizontalVelocity.Length();

		float speedPercent = Mathf.Clamp(
			horizontalSpeed / MoveSpeed,
			0.0f,
			1.0f
		);

		float targetFov = Mathf.Lerp(
			PlayerSettings.Instance.BaseFov,
			MaxFov,
			speedPercent
		);

		PlayerCamera.Fov = Mathf.Lerp(
			PlayerCamera.Fov,
			targetFov,
			FovChangeSpeed * (float)delta
		);
	}

	private void RotateLook(Vector2 mouseDelta)
	{
		RotateY(-mouseDelta.X * effectiveSens); //here, RotateY means rotate the component (camera) around the y axis

		_cameraPitch -= mouseDelta.Y * effectiveSens;

		_cameraPitch = Mathf.Clamp(_cameraPitch, _minPitchRadians, _maxPitchRadians);

		CameraPivot.Rotation = new Vector3(_cameraPitch, 0.0f, 0.0f);
	}
}
