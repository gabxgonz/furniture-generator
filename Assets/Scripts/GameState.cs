using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public List<FurnitureCount> counts = new List<FurnitureCount>();

    void Awake()
    {
        counts.Add(new FurnitureCount("Beds", FurnitureType.Bed, 1, 10));
        counts.Add(new FurnitureCount("Chairs", FurnitureType.Chair, 1, 10));
        counts.Add(new FurnitureCount("Couches", FurnitureType.Couch, 1, 10));
        counts.Add(new FurnitureCount("Doors", FurnitureType.Door, 1, 10));
        counts.Add(new FurnitureCount("Kitchen", FurnitureType.Counter, 1, 10));
        counts.Add(new FurnitureCount("Lamps", FurnitureType.Lamp, 1, 10));
        counts.Add(new FurnitureCount("Tables", FurnitureType.Table, 1, 10));
    }
}