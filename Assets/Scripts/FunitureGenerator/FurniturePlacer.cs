using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FurniturePlacer : MonoBehaviour
{
    [SerializeField] private List<Furniture> randomFurniture;
    [SerializeField] private List<Furniture> staticFurniture;
    [SerializeField] private List<Rug> rugs;
    [SerializeField] private List<Decoration> decorations;
    [SerializeField] private int numberOfRugs = 0;
    [SerializeField] private int xLength = 8;
    [SerializeField] private int zLength = 8;
    [SerializeField] private GameObject floor;
    private RandomSound randomSound;
    public bool debugMode = false;
    public GameState state;
    private List<Furniture> placedFurniture;
    private List<Decoration> placedDecorations;
    private List<Rug> placedRugs;
    private List<Vector3> furnitureCoordinates;
    private List<Vector3> rugCoordinates;
    private List<Vector3> tempCoordinates;
    private List<Furniture> tempFurniture;
    private Dictionary<FurnitureType, List<FurnitureType>> furnitureTypeMap;
    private System.Random random;
    private Dictionary<DecorationType, int> decorationTotals = new Dictionary<DecorationType, int>();
    private Dictionary<FurnitureType, int> furnitureTotals = new Dictionary<FurnitureType, int>();

    void Start()
    {
        BuildFurnitureTypeMap();
        randomSound = GetComponent<RandomSound>();
        placedFurniture = new List<Furniture>();
        placedDecorations = new List<Decoration>();
        placedRugs = new List<Rug>();
        tempCoordinates = new List<Vector3>();
        rugCoordinates = new List<Vector3>();
        RearrangeFurniture();
    }

    public void RearrangeFurniture()
    {
        SetDimensions();

        foreach (Furniture item in placedFurniture.ToArray())
        {
            if (!staticFurniture.Contains(item))
            {
                placedFurniture.Remove(item);
                Destroy(item.gameObject);
            }
        }
        furnitureTotals.Clear();

        random = new System.Random();
        furnitureCoordinates = BuildCoordinates();
        rugCoordinates = BuildCoordinates();
        RegisterStaticFurniture();

        foreach (FurnitureValue value in state.furnitureValues.Values)
        {
            for (int i = 0; i < value.value; i++)
            {
                Vector3 selectedPosition;
                Vector3? position = null;

                Furniture furniture = SelectRandomFurniture(value.type);

                bool hasNonWallParent = !furniture.relatedFurnitureTypes.TrueForAll((FurnitureType type) =>
                {
                    return type == FurnitureType.Wall || type == FurnitureType.WallCorner;
                });

                if (hasNonWallParent && position == null)
                {
                    position = PlaceRelativeParent(furniture);
                }

                if (furniture.relatedFurnitureTypes.Contains(FurnitureType.WallCorner) && position == null)
                {
                    position = PlaceRelativeCorner(furniture);
                }

                if (furniture.RelatedFurnitureTypes.Contains(FurnitureType.Wall) && position == null)
                {
                    position = PlaceRelativeWall(furniture);
                }

                if (position == null)
                {
                    position = PlaceRandom(furniture);
                }

                if (position != null)
                {
                    selectedPosition = (Vector3)position;
                    furniture.origin = furniture.RotatedBottomLeft(selectedPosition);

                    Furniture newFurniture = Instantiate(furniture, furniture.WorldOrigin(selectedPosition), Quaternion.Euler(0f, furniture.rotation, 0f));
                    IncrementFurniture(value.type);
                    AddToGrid(newFurniture);
                }
            }
        }

        PlaceDecorations();
        // PlaceRugs();
        PlaySound();
    }

    private void PlaceDecorations()
    {
        List<Furniture> availableParents = new List<Furniture>();
        List<Transform> availablePositions = new List<Transform>();

        foreach (Decoration decoration in placedDecorations) Destroy(decoration.gameObject);
        placedDecorations.Clear();
        decorationTotals.Clear();

        // while less than total
        for (int i = 0; i < state.decorationValue.value; i++)
        {
            // pick all decorations that are not maxed
            List<Decoration> unmaxedDecorations = new List<Decoration>();

            decorations.ForEach((Decoration decoration) =>
            {
                bool typeIsMaxed = false;
                bool decorationIsMaxed = false;

                bool typeHasMax =
                    decorationTotals.ContainsKey(decoration.type) &&
                    state.decorationMax.ContainsKey(decoration.type);

                if (typeHasMax) typeIsMaxed = decorationTotals[decoration.type] < state.decorationMax[decoration.type];

                List<Decoration> duplicates = placedDecorations.FindAll((placedDecoration) => placedDecoration.name == decoration.name + "(Clone)");
                decorationIsMaxed = decoration.max != 0 && duplicates.Count >= decoration.max;

                if (!typeHasMax && !decorationIsMaxed) unmaxedDecorations.Add(decoration);
            });

            Shuffle(unmaxedDecorations);

            foreach (Decoration decoration in unmaxedDecorations)
            {
                // find dependent furnitures that aren't decorated
                availableParents = placedFurniture.FindAll((Furniture furniture) =>
                {
                    return furniture.decorationSpaces.Count > 0 && decoration.parentFurniture.Contains(furniture.type);
                });

                if (availableParents.Count <= 0) continue;

                Furniture selectedFurniture = SelectRandom(availableParents);
                Transform decorationPosition = SelectRandom(selectedFurniture.decorationSpaces);
                selectedFurniture.decorationSpaces.Remove(decorationPosition);

                placedDecorations.Add(Instantiate(decoration, decorationPosition.position, decorationPosition.transform.rotation));
                IncrementDecoration(decoration.type);
                break;
            };
        }
    }

    private void IncrementFurniture(FurnitureType type)
    {
        if (furnitureTotals.ContainsKey(type))
        {
            furnitureTotals[type]++;
        }
        else
        {
            furnitureTotals.Add(type, 1);
        }
    }

    private void IncrementDecoration(DecorationType type)
    {
        if (decorationTotals.ContainsKey(type))
        {
            decorationTotals[type]++;
        }
        else
        {
            decorationTotals.Add(type, 1);
        }
    }

    private void PlaceRugs()
    {
        foreach (Rug rug in placedRugs)
        {
            Destroy(rug.gameObject);
        }
        placedRugs.Clear();

        for (int i = 0; i < numberOfRugs; i++)
        {
            Rug rugPrefab = SelectRandom(rugs);

            bool rugHasParents = rugPrefab.parentFurniture.Count > 0;
            bool rugFits;
            // pick random location
            // resize
            int newRugX = (int)rugPrefab.xMax;
            int newRugZ = (int)rugPrefab.zMax;

            Vector3 rugOrigin;

            if (!rugPrefab.forceMax)
            {
                newRugX = random.Next((int)rugPrefab.xMax) + 1;
                newRugZ = random.Next((int)rugPrefab.zMax) + 1;
            }


            for (int attempt = 0; attempt < 5; attempt++)
            {
                if (rugHasParents)
                {
                    // get list of placed parents
                    List<Furniture> possibleParents = placedFurniture.FindAll((Furniture furniture) =>
                    {
                        return rugPrefab.parentFurniture.Contains(furniture.type);
                    });

                    // pick one
                    Furniture parentFurniture = SelectRandom(possibleParents);
                    rugOrigin = parentFurniture.origin;
                    // resize

                    // align based on size & position

                    //if it fits
                    rugFits = RugFits(rugOrigin, newRugX, newRugZ);
                }
                else
                {
                    // select psitions that will fit dimensions
                    List<Vector3> possiblePositions = rugCoordinates.FindAll((Vector3 coord) =>
                    {
                        return coord.x + newRugX < xLength && coord.z + newRugZ < zLength;
                    });

                    rugOrigin = SelectRandom(possiblePositions);
                    rugFits = RugFits(rugOrigin, newRugX, newRugZ);
                }

                if (rugFits)
                {
                    // place at location
                    Rug newRug = Instantiate(rugPrefab, rugOrigin, Quaternion.identity);
                    Vector3 scale = new Vector3(newRugX, rugPrefab.transform.localScale.y, newRugZ);
                    newRug.transform.localScale = scale;
                    placedRugs.Add(newRug);

                    // Remove coords
                    RemoveRugCoords(rugOrigin, newRugX, newRugZ);
                    break;
                }
            }
        }
    }

    private void RegisterStaticFurniture()
    {
        foreach (Furniture furniture in staticFurniture)
        {
            Vector3 originalPosition = furniture.transform.position;
            Vector3 position;

            // align furniture to grid
            position = new Vector3(Mathf.Round(originalPosition.x), 0f, Mathf.Round(originalPosition.z));
            furniture.transform.position = position;

            // set rotation to support furniture internal calculations
            furniture.rotation = 90 * Mathf.Floor(furniture.transform.rotation.eulerAngles.y / 90);
            // set furniture origin
            furniture.origin = furniture.GridOrigin(position);

            AddToGrid(furniture);
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
        float rotation = SelectRandom(new List<float> { 0f, 90f, 180f, 270f });

        // Wall
        if (position.z == 0) rotation = 0f;
        if (position.x == 0) rotation = 90f;
        if (position.z == zLength - 1) rotation = 180f;
        if (position.x == xLength - 1) rotation = 270f;
        // Corners
        if (position.x == 0 && position.z == 0) rotation = 0f;
        if (position.x == 0 && position.z == zLength - 1) rotation = 90f;
        if (position.x == xLength - 1 && position.z == zLength - 1) rotation = 180f;
        if (position.x == xLength - 1 && position.z == 0) rotation = 270f;

        return rotation;
    }

    private void RemoveFurnitureCoords(Furniture furniture)
    {
        Vector3 bottomLeftCoord = furniture.origin;
        List<Vector3> reservedSpaces = furniture.ReservedSpaces();

        for (float x = 0; x < furniture.RotatedXLength(); x++)
        {
            for (float z = 0; z < furniture.RotatedZLength(); z++)
            {
                Vector3 furnitureCoord = new Vector3(bottomLeftCoord.x + x, 0f, bottomLeftCoord.z + z);
                furnitureCoordinates.Remove(furnitureCoord);
                if (!furniture.allowRug) rugCoordinates.Remove(furnitureCoord);
            }
        }

        furnitureCoordinates.RemoveAll((Vector3 coord) =>
        {
            return reservedSpaces.Contains(coord);
        });
    }

    private void RemoveRugCoords(Vector3 position, float rugX, float rugZ)
    {
        for (float x = 0; x < rugX; x++)
        {
            for (float z = 0; z < rugZ; z++)
            {
                Vector3 rugCoord = new Vector3(position.x + x, 0f, position.z + z);
                rugCoordinates.Remove(rugCoord);
            }
        }
    }

    private bool IsEnoughRoom(Furniture furniture, Vector3 origin)
    {
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

                if (!furnitureCoordinates.Contains(furnitureCoord))
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

    private bool FindCorner(Vector3 coord)
    {
        bool leftCorner = coord.x == 0 && (coord.z == 0 || coord.z == zLength - 1);
        bool rightCorner = coord.x == xLength - 1 && (coord.z == 0 || coord.z == zLength - 1);

        if (leftCorner || rightCorner) return true;

        return false;
    }

    private bool FindFurnitureSpaces(Vector3 coord)
    {
        if (furnitureCoordinates.Contains(coord)) return true;

        return false;
    }

    private void AddToGrid(Furniture furniture)
    {
        RemoveFurnitureCoords(furniture);
        placedFurniture.Add(furniture);
    }

    private T SelectRandom<T>(List<T> list)
    {
        int index = random.Next(list.Count);
        return list[index];
    }

    private Furniture SelectRandomFurniture(FurnitureType type)
    {
        List<Furniture> validFurniture = randomFurniture.FindAll((Furniture item) =>
        {
            List<FurnitureType> initialTypes;
            bool isCurrentType = furnitureTypeMap[type].Contains(item.type);
            if (!isCurrentType) return false;

            bool validForType = true;

            switch (type)
            {
                case FurnitureType.Kitchen:
                    initialTypes = new List<FurnitureType>{
                        FurnitureType.Sink,
                        FurnitureType.Fridge,
                        FurnitureType.Stove,
                    };

                    if (!furnitureTotals.ContainsKey(FurnitureType.Kitchen))
                    {
                        validForType = initialTypes.Contains(item.type) || item.type == FurnitureType.CornerCounter;
                    }
                    else if (furnitureTotals[FurnitureType.Kitchen] < 3)
                    {
                        validForType = initialTypes.Contains(item.type);
                    }
                    else
                    {
                        validForType = item.type != FurnitureType.CornerCounter;
                    }
                    break;
                case FurnitureType.Table:
                    initialTypes = new List<FurnitureType>{
                        FurnitureType.Desk,
                        FurnitureType.Table,
                    };

                    if (!furnitureTotals.ContainsKey(FurnitureType.Table))
                    {
                        validForType = initialTypes.Contains(item.type);
                    }
                    else if (furnitureTotals[FurnitureType.Table] < 2)
                    {
                        validForType = initialTypes.Contains(item.type);
                    }
                    else
                    {
                        validForType = item.type != FurnitureType.CornerCounter;
                    }
                    break;
                default:
                    break;
            }

            List<Furniture> duplicates = placedFurniture.FindAll((placedFurn) => placedFurn.name == item.name + "(Clone)");
            bool furnitureIsMaxed = item.max != 0 && duplicates.Count >= item.max;

            return !furnitureIsMaxed && validForType;
        });

        return SelectRandom<Furniture>(validFurniture);
    }

    private bool RugFits(Vector3 origin, int xLength, int zLength)
    {
        bool fits = true;
        for (float x = 0; x < xLength; x++)
        {
            if (!fits) break;

            for (float z = 0; z < zLength; z++)
            {
                Vector3 testPosition = new Vector3(origin.x + x, 0f, origin.z + z);
                fits = rugCoordinates.Contains(testPosition);
                if (!fits) break;
            }
        }
        return fits;
    }

    public void PlaySound()
    {
        if (randomSound != null) randomSound.PlaySound();
    }

    private void SetDimensions()
    {
        xLength = state.dimensions["length"].value;
        zLength = state.dimensions["width"].value;
        floor.transform.localScale = new Vector3(xLength, floor.transform.localScale.y, zLength);
        floor.transform.position = new Vector3((float)xLength / 2, floor.transform.position.y, (float)zLength / 2);
    }

    private void BuildFurnitureTypeMap()
    {
        furnitureTypeMap = new Dictionary<FurnitureType, List<FurnitureType>>();

        furnitureTypeMap.Add(FurnitureType.Door, new List<FurnitureType>{
            FurnitureType.Door,
        });

        furnitureTypeMap.Add(FurnitureType.Kitchen, new List<FurnitureType>{
            FurnitureType.Counter,
            FurnitureType.CornerCounter,
            FurnitureType.Fridge,
            FurnitureType.Sink,
            FurnitureType.Stove,
        });

        furnitureTypeMap.Add(FurnitureType.Bed, new List<FurnitureType>{
            FurnitureType.Bed,
        });

        furnitureTypeMap.Add(FurnitureType.Couch, new List<FurnitureType>{
            FurnitureType.Couch,
        });

        furnitureTypeMap.Add(FurnitureType.Table, new List<FurnitureType>{
            FurnitureType.Table,
            FurnitureType.Desk,
            FurnitureType.EndTable,
        });

        furnitureTypeMap.Add(FurnitureType.Chair, new List<FurnitureType>{
            FurnitureType.Chair,
        });

        furnitureTypeMap.Add(FurnitureType.Lamp, new List<FurnitureType>{
            FurnitureType.Lamp,
        });
    }

    private Vector3? PlaceRelativeParent(Furniture furniture)
    {
        Vector3? position = null;
        List<Furniture> availableParents = new List<Furniture>();
        Dictionary<Vector3, Furniture> availableCoordinates = new Dictionary<Vector3, Furniture>();

        availableParents = placedFurniture.FindAll(furniture.FindParentFurniture);

        availableParents.ForEach((Furniture parent) =>
        {
            parent.ValidSpaces(furniture).ForEach((Vector3 validSpace) =>
            {
                if (!availableCoordinates.ContainsKey(validSpace))
                {
                    availableCoordinates.Add(validSpace, parent);
                }
            });
        });

        foreach (Vector3 coordinate in availableCoordinates.Keys)
        {
            furniture.SetParentRelativeRotation(coordinate, availableCoordinates[coordinate]);
            if (IsEnoughRoom(furniture, coordinate))
            {
                position = coordinate;
                break;
            }
        }

        return position;
    }

    private Vector3? PlaceRelativeCorner(Furniture furniture)
    {
        List<Vector3> availableCoordinates = new List<Vector3>();
        Vector3? position = null;

        availableCoordinates = furnitureCoordinates.FindAll(FindCorner);
        Shuffle(availableCoordinates);

        foreach (Vector3 coordinate in availableCoordinates)
        {
            furniture.rotation = GetRotationDegrees(coordinate);
            if (IsEnoughRoom(furniture, coordinate))
            {
                position = coordinate;
                break;
            }
        }

        return position;
    }

    private Vector3? PlaceRelativeWall(Furniture furniture)
    {
        List<Vector3> availableCoordinates = new List<Vector3>();
        Vector3? position = null;

        availableCoordinates = furnitureCoordinates.FindAll(FindEdge);
        Shuffle(availableCoordinates);

        foreach (Vector3 coordinate in availableCoordinates)
        {
            furniture.rotation = GetRotationDegrees(coordinate);
            if (IsEnoughRoom(furniture, coordinate))
            {
                position = coordinate;
                break;
            }
        }

        return position;
    }

    private Vector3? PlaceRandom(Furniture furniture)
    {
        Vector3? position = null;

        Shuffle(furnitureCoordinates);

        foreach (Vector3 coordinate in furnitureCoordinates)
        {
            furniture.rotation = GetRotationDegrees(coordinate);

            if (IsEnoughRoom(furniture, coordinate))
            {
                position = coordinate;
                break;
            }
        }

        return position;
    }

    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
