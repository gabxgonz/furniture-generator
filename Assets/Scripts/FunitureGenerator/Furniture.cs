using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SideSpace
{
    None,
    All,
    First,
    Last,
    Inner,
    Outer
}

public enum Side
{
    Front,
    Back,
    Left,
    Right,
    Corner
}

public class Furniture : MonoBehaviour
{
    public FurnitureType type = FurnitureType.Null;
    public int xLength = 1;
    public int zLength = 1;
    public bool allowRug = true;
    public int max = 0;
    [HideInInspector] public float rotation = 0f;
    [HideInInspector] public Vector3 origin = Vector3.zero;

    [Header("Available Child Spaces")]
    [Tooltip("Define spaces available for dependant furniture placement in front of this furniture.")]
    public SideSpace frontAvailability = SideSpace.All;
    [Tooltip("Define spaces available for dependant furniture placement behind this furniture.")]
    public SideSpace backAvailability = SideSpace.All;
    [Tooltip("Define spaces available for dependant furniture placement on the left of this furniture.")]
    public SideSpace leftAvailability = SideSpace.All;
    [Tooltip("Define spaces available for dependant furniture placement on the right of this furniture.")]
    public SideSpace rightAvailability = SideSpace.All;

    [Tooltip("Define spaces available for dependant furniture placement on front left corner.")]
    public bool frontLeftAvailable = true;
    [Tooltip("Define spaces available for dependant furniture placement on front right corner.")]
    public bool frontRightAvailable = true;
    [Tooltip("Define spaces available for dependant furniture placement on back left corner.")]
    public bool backLeftAvailable = true;
    [Tooltip("Define spaces available for dependant furniture placement on back right corner.")]
    public bool backRightAvailable = true;

    public List<Transform> decorationSpaces;

    private List<Vector3> validSpaces;
    private Vector3 backDependencyTranslation;
    private Vector3 leftDependencyTranslation;
    private Vector3 rightDependencyTranslation;
    private Vector3 frontDependencyTranslation;
    private Vector3 sideLastDependencyTranslation;
    private Vector3 frontBackLastDependencyTranslation;

    private SideSpace relativeFrontAvailability;
    private SideSpace relativeBackAvailability;
    private SideSpace relativeLeftAvailability;
    private SideSpace relativeRightAvailability;
    private bool relativeFrontLeftAvailable;
    private bool relativeFrontRightAvailable;
    private bool relativeBackLeftAvailable;
    private bool relativeBackRightAvailable;

    [HideInInspector] public List<FurnitureType> relatedFurnitureTypes;
    private List<FurnitureRelationship> relationships = new List<FurnitureRelationship>();

    public List<FurnitureRelationship> Relationships
    {
        get
        {
            if (relationships.Count > 0) return relationships;
            return new List<FurnitureRelationship>(GetComponents<FurnitureRelationship>());
        }
    }

    void Start()
    {
        validSpaces = new List<Vector3>();
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        foreach (var item in ValidSpaces())
        {
            Gizmos.DrawSphere(item, 0.1f);
        }

        Gizmos.color = Color.red;
        foreach (var item in ReservedSpaces())
        {
            Gizmos.DrawSphere(item, 0.1f);
        }

    }

    void FixedUpdate()
    {
        DebugPlacement();
    }

    public float RotatedXLength()
    {
        float rotatedX;

        switch (rotation)
        {
            case 90f:
                rotatedX = zLength;
                break;
            case 270f:
                rotatedX = zLength;
                break;
            default:
                rotatedX = xLength;
                break;
        }

        return rotatedX;
    }

    public float RotatedZLength()
    {
        float rotatedZ;

        switch (rotation)
        {
            case 90f:
                rotatedZ = xLength;
                break;
            case 270f:
                rotatedZ = xLength;
                break;
            default:
                rotatedZ = zLength;
                break;
        }

        return rotatedZ;
    }

    public Vector3 WorldOrigin(Vector3 gridOrigin)
    {
        Vector3 worldOrigin = gridOrigin;

        switch (rotation)
        {
            case 90f:
                worldOrigin.z += 1f;
                break;
            case 180f:
                worldOrigin.x += 1f;
                worldOrigin.z += 1f;
                break;
            case 270f:
                worldOrigin.x += 1f;
                break;
            default:
                break;
        }

        return worldOrigin;
    }

    public Vector3 GridOrigin(Vector3 worldOrigin)
    {
        Vector3 gridOrigin = worldOrigin;

        switch (rotation)
        {
            case 90f:
                gridOrigin.z -= 1f;
                break;
            case 180f:
                gridOrigin.x -= 1f;
                gridOrigin.z -= 1f;
                break;
            case 270f:
                gridOrigin.x -= 1f;
                break;
            default:
                break;
        }

        return gridOrigin;
    }

    public Vector3 RotatedBottomLeft(Vector3 initialOrigin)
    {
        Vector3 rotatedOrigin = WorldOrigin(initialOrigin);

        switch (rotation)
        {
            case 90f:
                rotatedOrigin.z -= RotatedZLength();
                break;
            case 180f:
                rotatedOrigin.x -= RotatedXLength();
                rotatedOrigin.z -= RotatedZLength();
                break;
            case 270f:
                rotatedOrigin.x -= RotatedXLength();
                break;
            default:
                break;
        }

        return rotatedOrigin;
    }

    public Vector3 BottomLeft()
    {
        return origin;
    }

    public Vector3 BottomRight()
    {
        return origin + new Vector3(RotatedXLength(), 0f, 0f);
    }

    public Vector3 FrontLeft()
    {
        return origin + new Vector3(0f, 0f, RotatedZLength());
    }

    public Vector3 FrontRight()
    {
        return origin + new Vector3(RotatedXLength(), 0f, RotatedZLength());
    }

    private void DebugPlacement()
    {
        float debugHeight = 0.2f;
        if (origin != null)
        {
            Vector3 backLeft = origin + new Vector3(0f, debugHeight, 0f);
            Vector3 backRight = origin + new Vector3(RotatedXLength(), debugHeight, 0f);
            Vector3 frontLeft = origin + new Vector3(0f, debugHeight, RotatedZLength());
            Vector3 frontRight = frontLeft + new Vector3(RotatedXLength(), 0f, 0f);

            Debug.DrawLine(backLeft, backRight, Color.red);
            Debug.DrawLine(backRight, frontRight, Color.red);
            Debug.DrawLine(frontRight, frontLeft, Color.red);
            Debug.DrawLine(frontLeft, backLeft, Color.red);
        }
    }

    public bool FindParentFurniture(Furniture parent)
    {
        List<FurnitureRelationship> matchingRelationships = Relationships.FindAll(relationship =>
        {
            return relationship.parentType == parent.type;
        });

        return matchingRelationships.Count > 0;
    }

    public List<Vector3> ReservedSpaces()
    {
        List<Vector3> reservedSpaces = new List<Vector3>();
        List<Vector3> depSpaces = ValidSpaces();

        reservedSpaces.Add(FrontLeftAvailable());
        reservedSpaces.Add(FrontRightAvailable());
        reservedSpaces.Add(BackLeftAvailable());
        reservedSpaces.Add(BackRightAvailable());
        reservedSpaces.Add(LeftFirstAvailable());
        reservedSpaces.Add(LeftLastAvailable());
        reservedSpaces.AddRange(LeftInnerAvailable());
        reservedSpaces.Add(RightFirstAvailable());
        reservedSpaces.Add(RightLastAvailable());
        reservedSpaces.AddRange(RightInnerAvailable());
        reservedSpaces.Add(BackFirstAvailable());
        reservedSpaces.Add(BackLastAvailable());
        reservedSpaces.AddRange(BackInnerAvailable());
        reservedSpaces.Add(FrontFirstAvailable());
        reservedSpaces.Add(FrontLastAvailable());
        reservedSpaces.AddRange(FrontInnerAvailable());

        reservedSpaces.RemoveAll((Vector3 coord) =>
        {
            return depSpaces.Contains(coord);
        });

        return reservedSpaces;
    }

    public List<Vector3> ValidSpaces(Furniture childFurniture)
    {
        Dictionary<Side, bool> preferences = RelativePreferences(childFurniture);

        return ValidSpaces(
            preferences[Side.Front],
            preferences[Side.Back],
            preferences[Side.Left],
            preferences[Side.Right],
            preferences[Side.Corner]
            );
    }

    public List<Vector3> ValidSpaces()
    {
        return ValidSpaces(true, true, true, true, true);
    }

    public List<Vector3> ValidSpaces(bool front, bool back, bool left, bool right, bool corner)
    {
        if (validSpaces != null) validSpaces.Clear();
        if (validSpaces == null) validSpaces = new List<Vector3>();

        backDependencyTranslation = new Vector3(0f, 0f, -1f);
        leftDependencyTranslation = new Vector3(-1f, 0f, 0f);
        rightDependencyTranslation = new Vector3(RotatedXLength(), 0f, 0f);
        frontDependencyTranslation = new Vector3(0, 0f, RotatedZLength());
        sideLastDependencyTranslation = new Vector3(0f, 0f, RotatedZLength() - 1);
        frontBackLastDependencyTranslation = new Vector3(RotatedXLength() - 1, 0f, 0f);


        SetRelativeAvailability();

        if (corner)
        {
            if (relativeFrontLeftAvailable) validSpaces.Add(FrontLeftAvailable());
            if (relativeFrontRightAvailable) validSpaces.Add(FrontRightAvailable());
            if (relativeBackLeftAvailable) validSpaces.Add(BackLeftAvailable());
            if (relativeBackRightAvailable) validSpaces.Add(BackRightAvailable());
        }

        if (left)
        {
            switch (relativeLeftAvailability)
            {
                case SideSpace.All:
                    validSpaces.Add(LeftFirstAvailable());
                    validSpaces.Add(LeftLastAvailable());
                    validSpaces.AddRange(LeftInnerAvailable());
                    break;
                case SideSpace.First:
                    if (rotation > 90)
                    {
                        validSpaces.Add(LeftLastAvailable());
                    }
                    else
                    {
                        validSpaces.Add(LeftFirstAvailable());
                    }
                    break;
                case SideSpace.Last:
                    if (rotation > 90)
                    {
                        validSpaces.Add(LeftFirstAvailable());
                    }
                    else
                    {
                        validSpaces.Add(LeftLastAvailable());
                    }
                    break;
                case SideSpace.Inner:
                    validSpaces.AddRange(LeftInnerAvailable());
                    break;
                case SideSpace.Outer:
                    validSpaces.Add(LeftFirstAvailable());
                    validSpaces.Add(LeftLastAvailable());
                    break;
                default:
                    break;
            }
        }

        if (right)
        {
            switch (relativeRightAvailability)
            {
                case SideSpace.All:
                    validSpaces.Add(RightFirstAvailable());
                    validSpaces.Add(RightLastAvailable());
                    validSpaces.AddRange(RightInnerAvailable());
                    break;
                case SideSpace.First:
                    if (rotation > 90)
                    {
                        validSpaces.Add(RightLastAvailable());
                    }
                    else
                    {
                        validSpaces.Add(RightFirstAvailable());
                    }
                    break;
                case SideSpace.Last:
                    if (rotation > 90)
                    {
                        validSpaces.Add(RightFirstAvailable());
                    }
                    else
                    {
                        validSpaces.Add(RightLastAvailable());
                    }
                    break;
                case SideSpace.Inner:
                    validSpaces.AddRange(RightInnerAvailable());
                    break;
                case SideSpace.Outer:
                    validSpaces.Add(RightFirstAvailable());
                    validSpaces.Add(RightLastAvailable());
                    break;
                default:
                    break;
            }
        }

        if (back)
        {
            switch (relativeBackAvailability)
            {
                case SideSpace.All:
                    validSpaces.Add(BackFirstAvailable());
                    validSpaces.Add(BackLastAvailable());
                    validSpaces.AddRange(BackInnerAvailable());
                    break;
                case SideSpace.First:
                    if (rotation > 90)
                    {
                        validSpaces.Add(BackLastAvailable());
                    }
                    else
                    {
                        validSpaces.Add(BackFirstAvailable());
                    }
                    break;
                case SideSpace.Last:
                    if (rotation > 90)
                    {
                        validSpaces.Add(BackFirstAvailable());
                    }
                    else
                    {
                        validSpaces.Add(BackLastAvailable());
                    }
                    break;
                case SideSpace.Inner:
                    validSpaces.AddRange(BackInnerAvailable());
                    break;
                case SideSpace.Outer:
                    validSpaces.Add(BackFirstAvailable());
                    validSpaces.Add(BackLastAvailable());
                    break;
                default:
                    break;
            }
        }

        if (front)
        {
            switch (relativeFrontAvailability)
            {
                case SideSpace.All:
                    validSpaces.Add(FrontFirstAvailable());
                    validSpaces.Add(FrontLastAvailable());
                    validSpaces.AddRange(FrontInnerAvailable());
                    break;
                case SideSpace.First:
                    if (rotation > 90)
                    {
                        validSpaces.Add(FrontLastAvailable());
                    }
                    else
                    {
                        validSpaces.Add(FrontFirstAvailable());
                    }
                    break;
                case SideSpace.Last:
                    if (rotation > 90)
                    {
                        validSpaces.Add(FrontFirstAvailable());
                    }
                    else
                    {
                        validSpaces.Add(FrontLastAvailable());
                    }
                    break;
                case SideSpace.Inner:
                    validSpaces.AddRange(FrontInnerAvailable());
                    break;
                case SideSpace.Outer:
                    validSpaces.Add(FrontFirstAvailable());
                    validSpaces.Add(FrontLastAvailable());
                    break;
                default:
                    break;
            }
        }

        return validSpaces;
    }

    private Vector3 LeftFirstAvailable()
    {
        Vector3 leftFirst = origin + leftDependencyTranslation;
        return leftFirst;
    }

    private Vector3 LeftLastAvailable()
    {
        Vector3 leftLast = origin + leftDependencyTranslation + sideLastDependencyTranslation;
        return leftLast;
    }
    private List<Vector3> LeftInnerAvailable()
    {
        List<Vector3> vertices = new List<Vector3>();

        for (float i = 1; i < sideLastDependencyTranslation.z; i++)
        {
            vertices.Add(origin + leftDependencyTranslation + new Vector3(0f, 0f, i));
        }

        return vertices;
    }

    private Vector3 RightFirstAvailable()
    {
        Vector3 rightFirst = origin + rightDependencyTranslation;
        return rightFirst;
    }

    private Vector3 RightLastAvailable()
    {
        Vector3 rightLast = origin + rightDependencyTranslation + sideLastDependencyTranslation;
        return rightLast;
    }

    private List<Vector3> RightInnerAvailable()
    {
        List<Vector3> vertices = new List<Vector3>();

        for (float i = 1; i < sideLastDependencyTranslation.z; i++)
        {
            vertices.Add((origin + rightDependencyTranslation) + new Vector3(0f, 0f, i));
        }

        return vertices;
    }

    private Vector3 BackFirstAvailable()
    {
        Vector3 backFirst = origin + backDependencyTranslation;
        return backFirst;
    }

    private Vector3 BackLastAvailable()
    {
        Vector3 backLast = origin + backDependencyTranslation + frontBackLastDependencyTranslation;
        return backLast;
    }

    private List<Vector3> BackInnerAvailable()
    {
        List<Vector3> vertices = new List<Vector3>();
        for (float i = 1; i < frontBackLastDependencyTranslation.x; i++)
        {
            vertices.Add((origin + backDependencyTranslation) + new Vector3(i, 0f, 0f));
        }

        return vertices;
    }

    private Vector3 FrontFirstAvailable()
    {
        Vector3 frontFirst = origin + frontDependencyTranslation;
        return frontFirst;
    }

    private Vector3 FrontLastAvailable()
    {
        Vector3 frontLast = origin + frontDependencyTranslation + frontBackLastDependencyTranslation;
        return frontLast;
    }

    private List<Vector3> FrontInnerAvailable()
    {
        List<Vector3> vertices = new List<Vector3>();

        for (float i = 1; i < frontBackLastDependencyTranslation.x; i++)
        {
            vertices.Add((origin + frontDependencyTranslation) + new Vector3(i, 0f, 0f));
        }

        return vertices;
    }

    private Vector3 FrontLeftAvailable()
    {
        Vector3 frontLeft = origin + frontDependencyTranslation + leftDependencyTranslation;
        return frontLeft;
    }

    private Vector3 FrontRightAvailable()
    {
        Vector3 frontRight = origin + frontDependencyTranslation + rightDependencyTranslation;
        return frontRight;
    }

    private Vector3 BackLeftAvailable()
    {
        Vector3 backLeft = origin + backDependencyTranslation + leftDependencyTranslation;
        return backLeft;
    }

    private Vector3 BackRightAvailable()
    {
        Vector3 backRight = origin + backDependencyTranslation + rightDependencyTranslation;
        return backRight;
    }

    private void SetRelativeAvailability()
    {
        switch (rotation)
        {
            case 90f:
                relativeFrontAvailability = leftAvailability;
                relativeBackAvailability = rightAvailability;
                relativeLeftAvailability = backAvailability;
                relativeRightAvailability = frontAvailability;
                relativeFrontLeftAvailable = backLeftAvailable;
                relativeFrontRightAvailable = frontLeftAvailable;
                relativeBackLeftAvailable = backRightAvailable;
                relativeBackRightAvailable = frontRightAvailable;
                break;
            case 180f:
                relativeFrontAvailability = backAvailability;
                relativeBackAvailability = frontAvailability;
                relativeLeftAvailability = rightAvailability;
                relativeRightAvailability = leftAvailability;
                relativeFrontLeftAvailable = backRightAvailable;
                relativeFrontRightAvailable = backLeftAvailable;
                relativeBackLeftAvailable = frontRightAvailable;
                relativeBackRightAvailable = frontLeftAvailable;
                break;
            case 270f:
                relativeFrontAvailability = rightAvailability;
                relativeBackAvailability = leftAvailability;
                relativeLeftAvailability = frontAvailability;
                relativeRightAvailability = backAvailability;
                relativeFrontLeftAvailable = frontRightAvailable;
                relativeFrontRightAvailable = backRightAvailable;
                relativeBackLeftAvailable = frontLeftAvailable;
                relativeBackRightAvailable = backLeftAvailable;
                break;
            default:
                relativeFrontAvailability = frontAvailability;
                relativeBackAvailability = backAvailability;
                relativeLeftAvailability = leftAvailability;
                relativeRightAvailability = rightAvailability;
                relativeFrontLeftAvailable = frontLeftAvailable;
                relativeFrontRightAvailable = frontRightAvailable;
                relativeBackLeftAvailable = backLeftAvailable;
                relativeBackRightAvailable = backRightAvailable;
                break;
        }
    }

    public float SetParentRelativeRotation(Vector3 position, Furniture parent)
    {
        Vector3 parentCenter = parent.Center();
        rotation = 0f;

        FurnitureRelationship parentRelationship = Relationships.Find(relationship =>
        {
            return relationship.parentType == parent.type;
        });

        switch (parentRelationship.lookDirection)
        {
            case RelativeDirection.Toward:
                if (position.x >= parent.FrontRight().x) rotation = 270f;
                if (position.z >= parent.FrontRight().z) rotation = 180f;
                if (position.x < parent.FrontLeft().x) rotation = 90f;
                break;
            case RelativeDirection.Away:
                if (position.x >= parent.FrontRight().x) rotation = 90f;
                if (position.z < parent.BottomLeft().z) rotation = 180f;
                if (position.x < parent.BottomLeft().x) rotation = 270f;
                break;
            case RelativeDirection.Forward:
                rotation = parent.rotation;
                break;
            case RelativeDirection.Right:
                rotation = parent.rotation + 90f;
                break;
            case RelativeDirection.Backward:
                rotation = parent.rotation + 180f;
                break;
            case RelativeDirection.Left:
                rotation = parent.rotation + 270f;
                break;
            default:
                break;
        }

        if (rotation >= 360) rotation -= 360f;

        return rotation;
    }

    public Vector3 Center()
    {
        float centerX = (origin.x + RotatedXLength() - 1) / 2;
        float centerZ = (origin.z + RotatedZLength() - 1) / 2;

        return new Vector3(centerX, 0f, centerZ);
    }

    private Dictionary<Side, bool> RelativePreferences(Furniture childFurniture)
    {
        Dictionary<Side, bool> preferences;

        // Find parent relationship in child
        FurnitureRelationship childRelationship = childFurniture.Relationships.Find(relationship =>
        {
            return relationship.parentType == type;
        });

        switch (rotation)
        {
            case 90f:
                preferences = new Dictionary<Side, bool>(){
                    { Side.Front, childRelationship.preferRight},
                    { Side.Back, childRelationship.preferLeft},
                    { Side.Left, childRelationship.preferBack},
                    { Side.Right, childRelationship.preferFront},
                    { Side.Corner, childRelationship.preferCorner},
                };
                break;
            case 180f:
                preferences = new Dictionary<Side, bool>(){
                    { Side.Front, childRelationship.preferBack},
                    { Side.Back, childRelationship.preferFront},
                    { Side.Left, childRelationship.preferRight},
                    { Side.Right, childRelationship.preferLeft},
                    { Side.Corner, childRelationship.preferCorner},
                };
                break;
            case 270f:
                preferences = new Dictionary<Side, bool>(){
                    { Side.Front, childRelationship.preferLeft},
                    { Side.Back, childRelationship.preferRight},
                    { Side.Left, childRelationship.preferFront},
                    { Side.Right, childRelationship.preferBack},
                    { Side.Corner, childRelationship.preferCorner},
                };
                break;
            default:
                preferences = new Dictionary<Side, bool>(){
                { Side.Front, childRelationship.preferFront},
                { Side.Back, childRelationship.preferBack},
                { Side.Left, childRelationship.preferLeft},
                { Side.Right, childRelationship.preferRight},
                { Side.Corner, childRelationship.preferCorner},
            };
                break;
        }

        return preferences;
    }

    public IList<FurnitureType> RelatedFurnitureTypes
    {
        get
        {
            if (relatedFurnitureTypes.Count > 0) return relatedFurnitureTypes.AsReadOnly();

            foreach (FurnitureRelationship relationship in Relationships)
            {
                if (!relatedFurnitureTypes.Contains(relationship.parentType))
                {
                    relatedFurnitureTypes.Add(relationship.parentType);
                }
            }

            return relatedFurnitureTypes.AsReadOnly();
        }
    }
}
