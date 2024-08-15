using Godot;
using System.Collections.Generic;

public partial class InventoryUI : Control
{
	private const string INVENTORY_ACTION = "inventory";
	private const string GRID = "GridContainer";
	private const string ITEM_TEXT_FORMAT = "{0} ({1})";

	private bool IsOpen = false;

	public override void _Ready()
	{
		Hide();
	}

	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed(INVENTORY_ACTION))
		{
			GridContainer grid = GetNode<GridContainer>(GRID);
			if(!IsOpen)
			{
				Inventory inventory = GetNode<ChogData>(GlobalStrings.ChogDataLocation).PlayerInventory;
				foreach(KeyValuePair<Item, int> item in inventory.Items)
				{
					Button button = GenerateInventoryButton(item);
					grid.AddChild(button);
				}
				Show();
			}
			else
			{
				Hide();
				foreach(Node child in grid.GetChildren())
				{
					grid.RemoveChild(child);
				}
			}
			IsOpen = !IsOpen;
		}
	}
  
	private Button GenerateInventoryButton(KeyValuePair<Item, int> inventoryItem)
	{
		return new Button(){
			Icon = inventoryItem.Key.Icon,
			Text = string.Format(ITEM_TEXT_FORMAT, inventoryItem.Key.Name, inventoryItem.Value),
			IconAlignment = HorizontalAlignment.Center,
			VerticalIconAlignment = VerticalAlignment.Top,
			ExpandIcon = true,
			CustomMinimumSize = new Vector2(100, 100)
		};
	}
}
