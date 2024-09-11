using Godot;
using System;

public partial class MainMenu : Control
{
	[Export]
	public string GameStartScene { get; set; } = String.Empty;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnNewGamePressed()
	{
		SceneHelper.TransitionScene(this, GameStartScene);
	}
	
	private void OnLoadGamePressed()
	{
		// Loading of games not expected for a while now.
	}
	
	private void OnExitGamePressed()
	{
		GetTree().Quit();
	}
}
