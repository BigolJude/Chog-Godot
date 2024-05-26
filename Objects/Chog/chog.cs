using Godot;
using System;

public partial class chog : Node2D
{
	private const string WALK_LEFT = "walk_left";
	private const string WALK_RIGHT = "walk_right";
	private const string SPRINT_LEFT = "sprint_left";
	private const string SPRINT_RIGHT = "sprint_right";
	private const string JUMP = "jump";
	
	[Export]
	public int Speed { get; set;} = 400;
	
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
		Vector2 velocity = Vector2.Zero;
		
		if(Input.IsActionPressed(WALK_LEFT) || Input.IsActionPressed(SPRINT_LEFT))
		{
			velocity.X -= 1;	
		} 
		if(Input.IsActionPressed(WALK_RIGHT) || Input.IsActionPressed(SPRINT_RIGHT))
		{
			velocity.X += 1;	
		}
		
		AnimatedSprite2D animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		animatedSprite.FlipH = velocity.X < 0;
		
		if(velocity.Length() > 0)
		{
			velocity = velocity.Normalized() * Speed;
		}
		
		Position += velocity * (float)delta;
		Position = new Vector2(
			x: Mathf.Clamp(Position.X, 0, ScreenSize.X),
			y: Mathf.Clamp(Position.Y, 0, ScreenSize.Y)
		);
		
	}
}
