using Godot;
using System;
using System.Diagnostics.Tracing;

public partial class player : CharacterBody3D
{
	//constants
	public const float WalkingSpeed = 3f; //walking base speed
	public const float RunningSpeed = 5f; //running base speed
	public const float JumpVerticalSpeed = 4.5f; //jumping base vertical speed

	//properties
	public float Speed { get; set; } = 3f; //the character's current speed. 
	public bool isRunning { get; set; } = false; //is the character running?
	public bool isLocked { get; set; } = false; //is the character locked? this is useful for stoping movement during animations like channels or attacks
	public Vector2 inputDir { get; set; } = new Vector2();
	public Vector3 direction { get; set; } = new Vector3();

    [Export]
	private float HorizontalSensitivity = 0.3f;
	[Export]
	private float VerticalSensitivity = 0.3f;

	Node3D CameraMount;
	Node3D Visuals;
	AnimationPlayer AnimationPlayer;


	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		CameraMount = GetNode<Node3D>("camera_mount");
		AnimationPlayer = GetNode<AnimationPlayer>("visuals/mixamo_base/AnimationPlayer");
		Visuals = GetNode<Node3D>("visuals");
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (@event is InputEventMouseMotion eventMouseMove)
		{
			RotateY(Mathf.DegToRad(eventMouseMove.Relative.X) * -1 * HorizontalSensitivity); //multiply by -1 to remove inversion	
			CameraMount.RotateX(Mathf.DegToRad(eventMouseMove.Relative.Y) * -1 * VerticalSensitivity) ;
			Visuals.RotateY(-Mathf.DegToRad(eventMouseMove.Relative.X) * -1 * HorizontalSensitivity);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!AnimationPlayer.IsPlaying()) { isLocked = false; }

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVerticalSpeed;

		// Handle Kick
		if (Input.IsActionJustPressed("primary") && !(AnimationPlayer.CurrentAnimation == "kick"))
		{
			AnimationPlayer.Stop();
            AnimationPlayer.Play("kick");
            isLocked = true;
        }
		

        //Handle Run.
        if (Input.IsActionPressed("run") && IsOnFloor())
		{ 
			isRunning = true;
			Speed = RunningSpeed;
		}
		else
		{
			isRunning = false;
			Speed = WalkingSpeed;
		}            
        
		
		// Get the input direction and handle the movement/deceleration.

        inputDir = Input.GetVector("strafe_left", "strafe_right", "forward", "backward");
		direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (!isLocked)
		{
			if (direction != Vector3.Zero)
			{

				if (isRunning)
				{
					if (AnimationPlayer.CurrentAnimation != "running")
					{
						AnimationPlayer.Play("running");
					}
				}
				else
				{
					if (AnimationPlayer.CurrentAnimation != "walking")
					{
						AnimationPlayer.Play("walking");
					}
				}

				Visuals.LookAt(Position + direction); //changes the character's look direction while moving
				velocity.X = direction.X * Speed;     //sets the character's x direction based on the inputs to the "direction" property
				velocity.Z = direction.Z * Speed;     //sets the character's y direction based on the inputs to the "direction" property
			}
			else
			{
				if (AnimationPlayer.CurrentAnimation != "idle")
				{
					AnimationPlayer.Play("idle");
				}
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
			}
		}
		else
		{
            velocity.X = Mathf.MoveToward(0, 0, Speed);
            velocity.Z = Mathf.MoveToward(0, 0, Speed);
        }

		Velocity = velocity;
		MoveAndSlide();
	}
}
