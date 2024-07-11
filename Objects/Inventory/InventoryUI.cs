using Godot;
using System;
using System.Collections.Generic;

public partial class InventoryUI : Control
{
	private const string INVENTORY_ACTION = "inventory";
	private const string GRID = "GridContainer";
	private bool IsOpen = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GridContainer grid = GetNode<GridContainer>(GRID);
		if(Input.IsActionJustPressed(INVENTORY_ACTION))
		{
			if(!IsOpen)
			{
				Show();
				Inventory inventory = GetNode<ChogData>("/root/ChogData").PlayerInventory;

				foreach(KeyValuePair<Item, int> item in inventory.Items)
				{
					Button button = new Button();
					button.Text = item.Key.Name;
					grid.AddChild(button);
				}
			}
			else
				Hide();

			IsOpen = !IsOpen;
		}
	}
}
