using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureRelationship : MonoBehaviour
{
    [Tooltip("Furniture to place relative to")]
    public FurnitureType parentType;

    [Tooltip("Direction to face relative to parent")]
    public RelativeDirection lookDirection;

    [Header("Parent Side Preferences")]
    public bool preferFront = true;
    public bool preferBack = true;
    public bool preferLeft = true;
    public bool preferRight = true;
    public bool preferCorner = true;
}
