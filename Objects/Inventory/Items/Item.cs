using System;
using Godot;

public class Item
{   
	public int Id { get; set; }
	public string Name { get; set; }

	public Texture2D Icon { get; set; } 
	public Item(int mId, string mName, Texture2D mIcon)
	{
		this.Id = mId;
		this.Name = mName;
		this.Icon = mIcon;
	}
	
	public Item(int mId, string mName, string iconName)
	{
		this.Id = mId;
		this.Name = mName;
		this.Icon = SceneHelper.LoadTexture2D(iconName);
	}
}
