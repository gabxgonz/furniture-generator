using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FurnitureType
{
    Null,
    Chair,
    Couch,
    Table,
    Bed,
    Lamp,
    Desk
}

public enum SideSpace
{
    None,
    All,
    First,
    Last,
    Inner,
    Outer
}

public enum FacingDirection
{
    Toward,
    Away,
    Forward,
    Backward,
    Left,
    Right
}

public class Furniture : MonoBehaviour
{

    public FurnitureType type = FurnitureType.Null;
    public int xLength = 1;
    public int zLength = 1;
    public bool alignToWall = false;
    public bool alignToFurniture = false;
    public float rotation = 0f;
    public Vector3 origin = Vector3.zero;

    [Header("Parent Furniture")]
    public FacingDirection relativeLookDirection = FacingDirection.Forward;
    public List<FurnitureType> relatedFurniture;

    [Header("Available Adjacent Spaces")]
    [Tooltip("Define spaces available for dependant furniture placement in front of this furniture.")]
    public SideSpace frontAvailability = SideSpace.All;
    [Tooltip("Define spaces available for dependant furniture placement behind this furniture.")]
    public SideSpace backAvailability = SideSpace.All;
    [Tooltip("Define spaces available for dependant furniture placement on the left of this furniture.")]
    public SideSpace leftAvailability = SideSpace.All;
    [Tooltip("Define spaces available for dependant furniture placement on the right of this furniture.")]
    public SideSpace rightAvailability = SideSpace.All;

    [Header("Available Corner Spaces")]
    public bool frontLeftAvailable = true;
    public bool frontRightAvailable = true;
    public bool backLeftAvailable = true;
    public bool backRightAvailable = true;

    private List<Vector3> dependencySpaces;
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

    void Start()
    {
        dependencySpaces = new List<Vector3>();
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

        foreach (var item in DependencySpaces())
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

    public Vector3 RotatedOrigin(Vector3 initialOrigin)
    {
        switch (rotation)
        {
            case 90f:
                initialOrigin.z += 1f;
                break;
            case 180f:
                initialOrigin.x += 1f;
                initialOrigin.z += 1f;
                break;
            case 270f:
                initialOrigin.x += 1f;
                break;
            default:
                break;
        }

        return initialOrigin;
    }

    public Vector3 RotatedBottomLeft(Vector3 initialOrigin)
    {
        Vector3 rotatedOrigin = RotatedOrigin(initialOrigin);

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

    public bool FindDependantFurniture(Furniture furniture)
    {
        if (relatedFurniture.Contains(furniture.type)) return true;
        return false;
    }

    public List<Vector3> ReservedSpaces()
    {
        List<Vector3> reservedSpaces = new List<Vector3>();
        List<Vector3> depSpaces = DependencySpaces();

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

    public List<Vector3> DependencySpaces()
    {

        if (dependencySpaces != null) dependencySpaces.Clear();
        if (dependencySpaces == null) dependencySpaces = new List<Vector3>();

        backDependencyTranslation = new Vector3(0f, 0f, -1f);
        leftDependencyTranslation = new Vector3(-1f, 0f, 0f);
        rightDependencyTranslation = new Vector3(RotatedXLength(), 0f, 0f);
        frontDependencyTranslation = new Vector3(0, 0f, RotatedZLength());
        sideLastDependencyTranslation = new Vector3(0f, 0f, RotatedZLength() - 1);
        frontBackLastDependencyTranslation = new Vector3(RotatedXLength() - 1, 0f, 0f);


        SetRelativeAvailability();

        // Add corners
        if (relativeFrontLeftAvailable) dependencySpaces.Add(FrontLeftAvailable());
        if (relativeFrontRightAvailable) dependencySpaces.Add(FrontRightAvailable());
        if (relativeBackLeftAvailable) dependencySpaces.Add(BackLeftAvailable());
        if (relativeBackRightAvailable) dependencySpaces.Add(BackRightAvailable());

        switch (relativeLeftAvailability)
        {
            case SideSpace.All:
                dependencySpaces.Add(LeftFirstAvailable());
                dependencySpaces.Add(LeftLastAvailable());
                dependencySpaces.AddRange(LeftInnerAvailable());
                break;
            case SideSpace.First:
                if (rotation > 90)
                {
                    dependencySpaces.Add(LeftLastAvailable());
                }
                else
                {
                    dependencySpaces.Add(LeftFirstAvailable());
                }
                break;
            case SideSpace.Last:
                if (rotation > 90)
                {
                    dependencySpaces.Add(LeftFirstAvailable());
                }
                else
                {
                    dependencySpaces.Add(LeftLastAvailable());
                }
                break;
            case SideSpace.Inner:
                dependencySpaces.AddRange(LeftInnerAvailable());
                break;
            case SideSpace.Outer:
                dependencySpaces.Add(LeftFirstAvailable());
                dependencySpaces.Add(LeftLastAvailable());
                break;
            default:
                break;
        }

        switch (relativeRightAvailability)
        {
            case SideSpace.All:
                dependencySpaces.Add(RightFirstAvailable());
                dependencySpaces.Add(RightLastAvailable());
                dependencySpaces.AddRange(RightInnerAvailable());
                break;
            case SideSpace.First:
                if (rotation > 90)
                {
                    dependencySpaces.Add(RightLastAvailable());
                }
                else
                {
                    dependencySpaces.Add(RightFirstAvailable());
                }
                break;
            case SideSpace.Last:
                if (rotation > 90)
                {
                    dependencySpaces.Add(RightFirstAvailable());
                }
                else
                {
                    dependencySpaces.Add(RightLastAvailable());
                }
                break;
            case SideSpace.Inner:
                dependencySpaces.AddRange(RightInnerAvailable());
                break;
            case SideSpace.Outer:
                dependencySpaces.Add(RightFirstAvailable());
                dependencySpaces.Add(RightLastAvailable());
                break;
            default:
                break;
        }

        switch (relativeBackAvailability)
        {
            case SideSpace.All:
                dependencySpaces.Add(BackFirstAvailable());
                dependencySpaces.Add(BackLastAvailable());
                dependencySpaces.AddRange(BackInnerAvailable());
                break;
            case SideSpace.First:
                if (rotation > 90)
                {
                    dependencySpaces.Add(BackLastAvailable());
                }
                else
                {
                    dependencySpaces.Add(BackFirstAvailable());
                }
                break;
            case SideSpace.Last:
                if (rotation > 90)
                {
                    dependencySpaces.Add(BackFirstAvailable());
                }
                else
                {
                    dependencySpaces.Add(BackLastAvailable());
                }
                break;
            case SideSpace.Inner:
                dependencySpaces.AddRange(BackInnerAvailable());
                break;
            case SideSpace.Outer:
                dependencySpaces.Add(BackFirstAvailable());
                dependencySpaces.Add(BackLastAvailable());
                break;
            default:
                break;
        }

        switch (relativeFrontAvailability)
        {
            case SideSpace.All:
                dependencySpaces.Add(FrontFirstAvailable());
                dependencySpaces.Add(FrontLastAvailable());
                dependencySpaces.AddRange(FrontInnerAvailable());
                break;
            case SideSpace.First:
                if (rotation > 90)
                {
                    dependencySpaces.Add(FrontLastAvailable());
                }
                else
                {
                    dependencySpaces.Add(FrontFirstAvailable());
                }
                break;
            case SideSpace.Last:
                if (rotation > 90)
                {
                    dependencySpaces.Add(FrontFirstAvailable());
                }
                else
                {
                    dependencySpaces.Add(FrontLastAvailable());
                }
                break;
            case SideSpace.Inner:
                dependencySpaces.AddRange(FrontInnerAvailable());
                break;
            case SideSpace.Outer:
                dependencySpaces.Add(FrontFirstAvailable());
                dependencySpaces.Add(FrontLastAvailable());
                break;
            default:
                break;
        }

        return dependencySpaces;
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

        switch (relativeLookDirection)
        {
            case FacingDirection.Toward:
                if (position.x >= parent.FrontRight().x) rotation = 270f;
                if (position.z >= parent.FrontRight().z) rotation = 180f;
                if (position.x < parent.FrontLeft().x) rotation = 90f;
                break;
            case FacingDirection.Away:
                if (position.x >= parent.FrontRight().x) rotation = 90f;
                if (position.z < parent.BottomLeft().z) rotation = 180f;
                if (position.x < parent.BottomLeft().x) rotation = 270f;
                break;
            case FacingDirection.Forward:
                rotation = parent.rotation;
                break;
            case FacingDirection.Right:
                rotation = parent.rotation + 90f;
                break;
            case FacingDirection.Backward:
                rotation = parent.rotation + 180f;
                break;
            case FacingDirection.Left:
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
}
