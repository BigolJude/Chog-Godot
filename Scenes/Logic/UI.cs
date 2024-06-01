using Godot;
using System;

public partial class UI : Control
{
	private string INTERACTION_LABEL = "InteractionHContainer/Label";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Show();
		Label interactionLabel = GetNode<Label>(INTERACTION_LABEL);
		VSplitContainer chatBoxVContainer = GetNode<VSplitContainer>("ChatBoxVContainer");
		chatBoxVContainer.Hide();
		interactionLabel.Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnInteractionEnter()
	{
		Label label = GetNode<Label>(INTERACTION_LABEL);
		label.Show();
	}
	
	private void OnInteractionLeave()
	{
		Label label = GetNode<Label>(INTERACTION_LABEL);
		label.Hide();
	}
	
	private void OnInteraction(string InteractionDescription)
	{
	}
}



