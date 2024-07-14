using Godot;
using System;

public partial class SceneBase : Node2D
{
	[Export]
	public string SceneLeft { get; set; } = string.Empty;

	[Export]
	public string SceneRight { get; set; } = string.Empty;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnLeftNavigation()
	{
		if(!string.IsNullOrEmpty(SceneLeft))
		{
			SceneHelper.TransitionScene(this, SceneLeft);
		}
	}

	public void OnRightNavigation()
	{
		if(!string.IsNullOrEmpty(SceneRight))
		{
			SceneHelper.TransitionScene(this, SceneRight);
		}
	}
}
