using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public List<FurnitureValue> values = new List<FurnitureValue>();
    public Dictionary<string, FurnitureValue> dimensions = new Dictionary<string, FurnitureValue>();

    void Awake()
    {
        dimensions.Add("length", new FurnitureValue("Room Length", FurnitureType.Null, 8, 50));
        dimensions.Add("width", new FurnitureValue("Room Width", FurnitureType.Null, 8, 50));
        values.Add(new FurnitureValue("Beds", FurnitureType.Bed, 1, 10));
        values.Add(new FurnitureValue("Chairs", FurnitureType.Chair, 1, 10));
        values.Add(new FurnitureValue("Couches", FurnitureType.Couch, 1, 10));
        values.Add(new FurnitureValue("Doors", FurnitureType.Door, 1, 10));
        values.Add(new FurnitureValue("Kitchen", FurnitureType.Counter, 1, 10));
        values.Add(new FurnitureValue("Lamps", FurnitureType.Lamp, 1, 10));
        values.Add(new FurnitureValue("Tables", FurnitureType.Table, 1, 10));
    }
}