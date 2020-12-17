public class FurnitureValue
{
    public string title;
    public int value = 1;
    public FurnitureType type;
    public int minValue = 0;
    public int maxValue = 10;

    public FurnitureValue(string title, FurnitureType type, int value, int minValue, int maxValue)
    {
        this.type = type;
        this.title = title;
        this.value = value;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

    public void SetValue(int value)
    {
        this.value = value;
    }
}
