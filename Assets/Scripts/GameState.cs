using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public Dictionary<string, FurnitureValue> dimensions = new Dictionary<string, FurnitureValue>();
    public Dictionary<string, FurnitureValue> furnitureValues = new Dictionary<string, FurnitureValue>();
    public Dictionary<DecorationType, int> decorationMax = new Dictionary<DecorationType, int>();
    public FurnitureValue decorationValue;

    void Awake()
    {
        dimensions.Add("length", new FurnitureValue("Room Length", FurnitureType.Null, 8, 1, 12));
        dimensions.Add("width", new FurnitureValue("Room Width", FurnitureType.Null, 8, 1, 12));
        furnitureValues.Add("doors", new FurnitureValue("Doors", FurnitureType.Door, 1, 0, 10));
        furnitureValues.Add("kitchen", new FurnitureValue("Kitchen", FurnitureType.Counter, 5, 0, 10));
        furnitureValues.Add("beds", new FurnitureValue("Beds", FurnitureType.Bed, 1, 0, 10));
        furnitureValues.Add("couches", new FurnitureValue("Couches", FurnitureType.Couch, 1, 0, 10));
        furnitureValues.Add("tables", new FurnitureValue("Tables", FurnitureType.Table, 3, 0, 10));
        furnitureValues.Add("chairs", new FurnitureValue("Chairs", FurnitureType.Chair, 3, 0, 10));
        furnitureValues.Add("lamps", new FurnitureValue("Lamps", FurnitureType.Lamp, 2, 0, 10));
        decorationValue = new FurnitureValue("Decorations", FurnitureType.Null, 10, 0, 20);
        decorationMax.Add(DecorationType.Computer, 1);
    }
}