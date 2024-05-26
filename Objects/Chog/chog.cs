using Godot;
using System;

public partial class chog : CharacterBody2D
{
	private const string WALK_LEFT = "walk_left";
	private const string WALK_RIGHT = "walk_right";
	private const string SPRINT_LEFT = "sprint_left";
	private const string SPRINT_RIGHT = "sprint_right";
	private const string GRAVITY_SETTING_LOCATION = "physics/2d/default_gravity";
	private const string JUMP = "jump";
	
	public float Gravity = ProjectSettings.GetSetting(GRAVITY_SETTING_LOCATION).AsSingle();
	
	
	[Export]
	public int Speed { get; set;} = 400;
	
	[Export]
	public int JumpSpeed { get; set; } = -725;
	
	[Export]
	public int JumpDuration {get; set;} = 10;
	
	[Signal]
	public delegate void HitEventHandler();
	
	public Vector2 ScreenSize;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 velocity = Velocity;
		velocity.Y += Gravity * (float)delta;
		velocity.X = 0;
		
		if(Input.IsActionPressed(WALK_LEFT) || Input.IsActionPressed(SPRINT_LEFT))
		{
			velocity.X = -Speed;	
		} 
		if(Input.IsActionPressed(WALK_RIGHT) || Input.IsActionPressed(SPRINT_RIGHT))
		{
			velocity.X = Speed;	
		}
		
		if(Input.IsActionJustPressed(JUMP) && IsOnFloor())
		{
			GD.Print("IsOnFloor called");
			velocity.Y = JumpSpeed;
		}
		
		AnimatedSprite2D animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animatedSprite.FlipH = velocity.X < 0;
		
		Velocity = velocity;
		MoveAndSlide();
		
	}
	
}
