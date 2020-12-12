public class FurnitureValue
{
    public string title;
    public int value = 1;
    public FurnitureType type;
    public int maxValue = 10;

    public FurnitureValue(string title, FurnitureType type, int value, int maxValue)
    {
        this.type = type;
        this.title = title;
        this.value = value;
        this.maxValue = maxValue;
    }

    public void SetValue(int value)
    {
        this.value = value;
    }
}
