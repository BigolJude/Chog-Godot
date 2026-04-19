using Godot;
using System;

public partial class EscapeMenu : Control
{
	private const string ESCAPE_MENU_ACTION = "open_menu";
	private const string MAIN_MENU_SCENE = "MainMenu";

	private bool IsOpen = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed(ESCAPE_MENU_ACTION))
		{
			if(!IsOpen)
				Show();
			else
				Hide();
			IsOpen = !IsOpen;
		}
	}
	
	private void OnContinuePressed()
	{
		Hide();
	}

	private void OnSavePressed()
	{
		
	}

	private void OnExitPressed()
	{
		SceneHelper.TransitionScene(this, MAIN_MENU_SCENE);
	}
}






