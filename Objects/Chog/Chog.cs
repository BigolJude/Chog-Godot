using Godot;
using System;
using System.Runtime.Serialization;

public partial class Chog : CharacterBody2D
{
	private const string WALK_LEFT = "walk_left";
	private const string WALK_RIGHT = "walk_right";
	private const string SPRINT_LEFT = "sprint_left";
	private const string SPRINT_RIGHT = "sprint_right";
	private const string GRAVITY_SETTING_LOCATION = "physics/2d/default_gravity";
	private const string JUMP = "jump";
	private const string ROOT_NODE = "/root/Node2D";
	private const string CHOG_DATA_NODE = "/root/ChogData";
	
	public float Gravity = ProjectSettings.GetSetting(GRAVITY_SETTING_LOCATION).AsSingle();
	
	[Export]
	public int Speed { get; set;} = 400;
	
	[Export]
	public int JumpSpeed { get; set; } = -725;
	
	[Export]
	public int JumpDuration {get; set;} = 10;
	
	[Signal]
	public delegate void HitEventHandler();

	[Signal]
	public delegate void NavigateLeftEventHandler();

	[Signal]
	public delegate void NavigateRightEventHandler();
	
	public Vector2 ScreenSize;
	
	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;
		SceneBase scene = GetNode<SceneBase>(ROOT_NODE);
		Callable onLeftNavigation = Callable.From(() => scene.OnLeftNavigation());
		Connect(SignalName.NavigateLeft, onLeftNavigation);

		Callable onRightNavigation = Callable.From(() => scene.OnRightNavigation());
		Connect(SignalName.NavigateRight, onRightNavigation);
	}

	public override void _Process(double delta)
	{
		Vector2 velocity = GetVelocity(delta);
		Velocity = velocity;
		
		GetSpriteDirection(velocity);
		MoveAndSlide();

		if(Position.X > ScreenSize.X)
		{
			EmitSignal(SignalName.NavigateRight);
		}
		else if(Position.X < 0)
		{
			EmitSignal(SignalName.NavigateLeft);
		}

		GetNode<ChogData>("/root/ChogData").PlayerInventory.AddItem(1);
	}
	
	private Vector2 GetVelocity(double delta)
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
			velocity.Y = JumpSpeed;
		}
		
		return velocity;
	}
	
	private void GetSpriteDirection(Vector2 velocity)
	{		
		AnimatedSprite2D animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		if(velocity.X != 0)
		{
			animatedSprite.FlipH = velocity.X < 0;
		}
	}

	public void AddPlayerInventory(Inventory inventory)
	{
		ChogData chogData = GetNode<ChogData>(CHOG_DATA_NODE);
		chogData.PlayerInventory = inventory;
	}
}
