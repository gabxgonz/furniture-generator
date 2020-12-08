using System.Collections.Generic;
using UnityEngine;

public class FurniturePlacer : MonoBehaviour
{
    [SerializeField] private List<Furniture> furniture;
    [SerializeField] private int xLength = 1;
    [SerializeField] private int zLength = 1;
    public bool debugMode = false;
    private List<Furniture> placedFurniture;
    private List<Vector3> gridCoordinates;
    private List<Vector3> tempCoordinates;
    private List<Furniture> tempFurniture;
    private System.Random random;

    public Dictionary<FurnitureType, int> counts = new Dictionary<FurnitureType, int>(){
        {FurnitureType.Chair, 20},
        { FurnitureType.Couch, 20},
        { FurnitureType.Table, 20},
        { FurnitureType.Bed, 20},
        { FurnitureType.Lamp, 20},
        { FurnitureType.Desk, 20}
    };

    // Start is called before the first frame update
    void Start()
    {
        placedFurniture = new List<Furniture>();
        tempCoordinates = new List<Vector3>();
        RearrangeFurniture();
    }

    public void RearrangeFurniture()
    {
        foreach (Furniture item in placedFurniture)
        {
            Destroy(item.gameObject);
        }
        placedFurniture.Clear();

        random = new System.Random();
        gridCoordinates = BuildCoordinates();

        foreach (Furniture item in furniture)
        {
            for (int i = 0; i < counts[item.type]; i++)
            {

                Vector3 selectedPosition;
                Vector3 position;

                for (int placementAttempt = 0; placementAttempt < 10; placementAttempt++)
                {
                    // if should align to walls
                    if (item.alignToWall)
                    {
                        // find all available walls (use find all walls)
                        tempCoordinates = gridCoordinates.FindAll(FindEdge);
                        position = RandomPosition(tempCoordinates);
                        item.rotation = GetRotationDegrees(position);
                        // check if there's enough room when rotated according to wall
                        // place furniture
                    }
                    else if (item.alignToFurniture)
                    {
                        // find piece of furniture with free spaces
                        tempFurniture = placedFurniture.FindAll(item.FindDependantFurniture);
                        int furnitureIndex = random.Next(tempFurniture.Count);
                        tempCoordinates = tempFurniture[furnitureIndex].DependencySpaces().FindAll(FindFurnitureSpaces);
                        if (tempCoordinates.Count == 0) continue;

                        position = RandomPosition(tempCoordinates);
                        item.SetParentRelativeRotation(position, tempFurniture[furnitureIndex]);
                    }
                    else
                    {
                        position = RandomPosition(gridCoordinates);
                    }

                    bool isEnoughRoom = IsEnoughRoom(item, position);

                    if (isEnoughRoom)
                    {
                        selectedPosition = position;
                        item.origin = item.RotatedBottomLeft(selectedPosition);

                        // Set Rotation before getting rotated origin

                        Furniture newFurniture = Instantiate(item, item.RotatedOrigin(selectedPosition), Quaternion.Euler(0f, item.rotation, 0f));

                        RemoveFurnitureCoords(item, selectedPosition);
                        placedFurniture.Add(newFurniture);
                        break;
                    }

                    if (placementAttempt == 9) Debug.Log("No room for furniture: " + item);
                    continue;
                }
            }
        }
    }

    private List<Vector3> BuildCoordinates()
    {
        List<Vector3> coords = new List<Vector3>();

        for (float x = 0f; x < xLength; x++)
        {
            for (float z = 0f; z < zLength; z++)
            {
                coords.Add(new Vector3(x, 0f, z));
            }
        }

        return coords;
    }

    private Vector3 RandomPosition(List<Vector3> coords)
    {
        int randomIndex = random.Next(coords.Count);
        Vector3 position = coords[randomIndex];

        return position;
    }

    private float GetRotationDegrees(Vector3 position)
    {
        float rotation = 0f;

        if (position.x == 0) rotation = 90f;

        if (position.z == zLength - 1) rotation = 180f;

        if (position.x == xLength - 1) rotation = 270f;

        return rotation;
    }

    private void RemoveFurnitureCoords(Furniture furniture, Vector3 origin)
    {
        Vector3 bottomLeftCoord = furniture.RotatedBottomLeft(origin);
        List<Vector3> nonoSpaces = furniture.NoNoSpaces();

        // remove actual furniture spaces
        for (float x = 0; x < furniture.RotatedXLength(); x++)
        {
            for (float z = 0; z < furniture.RotatedZLength(); z++)
            {
                Vector3 furnitureCoord = new Vector3(bottomLeftCoord.x + x, 0f, bottomLeftCoord.z + z);
                gridCoordinates.Remove(furnitureCoord);
            }
        }

        // remove surrounding invalid spaces
        gridCoordinates.RemoveAll((Vector3 coord) =>
        {
            return nonoSpaces.Contains(coord);
        });
    }

    private bool IsEnoughRoom(Furniture furniture, Vector3 origin)
    {
        // update x & z with new furniture coords
        // float xRotated =
        // calculate origin relative to rotation
        Vector3 bottomLeftCoord = furniture.RotatedBottomLeft(origin);

        bool isWithinGridX = bottomLeftCoord.x + furniture.RotatedXLength() <= xLength;
        bool isWithinGridZ = bottomLeftCoord.z + furniture.RotatedZLength() <= zLength;
        bool isEnoughRoom = isWithinGridX && isWithinGridZ;

        for (float x = 0; x < furniture.RotatedXLength(); x++)
        {
            if (!isEnoughRoom) break;

            for (float z = 0; z < furniture.RotatedZLength(); z++)
            {
                Vector3 furnitureCoord = new Vector3(bottomLeftCoord.x + x, 0f, bottomLeftCoord.z + z);

                if (!gridCoordinates.Contains(furnitureCoord))
                {
                    isEnoughRoom = false;
                    break;
                }
            }
        }

        return isEnoughRoom;
    }

    private bool FindEdge(Vector3 coord)
    {
        if (coord.x == 0 || coord.x == xLength - 1 || coord.z == 0 || coord.z == zLength - 1)
        {
            return true;
        }

        return false;
    }

    private bool FindFurnitureSpaces(Vector3 coord)
    {
        if (gridCoordinates.Contains(coord)) return true;

        return false;
    }
}

