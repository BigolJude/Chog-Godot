using Godot;
using System;

public partial class SceneBase : Node2D
{
	private const string SCENE_FOLDER = "res://Scenes/";
	private const string SCENE_SUFFIX = ".tscn";

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
			PackedScene scene = (PackedScene)ResourceLoader.Load(SCENE_FOLDER + SceneLeft + SCENE_SUFFIX);
			GetTree().ChangeSceneToPacked(scene);
		}
	}

	public void OnRightNavigation()
	{
		if(!string.IsNullOrEmpty(SceneRight))
		{
			PackedScene scene = (PackedScene)ResourceLoader.Load(SCENE_FOLDER + SceneRight + SCENE_SUFFIX);
			GetTree().ChangeSceneToPacked(scene);
		}
	}
}
