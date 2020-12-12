using System.Collections;
using System.Collections.Generic;

public class FurnitureCount
{
    public string title;
    public int count = 1;
    public FurnitureType type;
    public int maxValue = 10;

    public FurnitureCount(string title, FurnitureType type, int count, int maxValue)
    {
        this.type = type;
        this.title = title;
        this.count = count;
        this.maxValue = maxValue;
    }

    public void SetCount(int count)
    {
        this.count = count;
    }
}
