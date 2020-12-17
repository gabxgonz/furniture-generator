using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DecorationType
{
    Appliance,
    Art,
    Chore,
    Clothes,
    Computer,
    Food,
    Hobby,
    Plant,
    Toy,
    Trash,
    TV,
}

public class Decoration : MonoBehaviour
{
    public List<FurnitureType> parentFurniture;
    public DecorationType type;
    public int max = 0;
}
