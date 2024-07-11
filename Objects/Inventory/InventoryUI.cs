using Godot;
using System;

public partial class InventoryUI : Control
{
	private const string INVENTORY_ACTION = "inventory";

	private Inventory playerInventory { get; set; }
	private bool IsOpen = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed(INVENTORY_ACTION))
		{
			foreach(Item item in playerInventory.Items)
			{
				
			}
		}
	}
}
