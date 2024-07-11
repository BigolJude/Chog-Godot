using System;

public class Item
{   
    public int Id { get; set; }
    public string Name { get; set; }
    public Item(int mId, string mName)
    {
        this.Id = mId;
        this.Name = mName;
    }
}