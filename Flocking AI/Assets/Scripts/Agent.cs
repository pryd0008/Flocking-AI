using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Flocking AI designed to herd a group from point A to B while avoiding obstacles
/// Referenced web page only: 
/// http://gamedevelopment.tutsplus.com/tutorials/the-three-simple-rules-of-flocking-behaviors-alignment-cohesion-and-separation--gamedev-3444
/// </summary>

public class Agent : MonoBehaviour
{
    [SerializeField]
    private float VelocityWeight;
    [SerializeField]
    private float CohesionWeight;
    [SerializeField]
    private float SeparationWeight;
    [SerializeField]
    private float NavigationWeight;
    [SerializeField]
    private float RoatationSpeed;

    private uint neighbourCount;
    public Vector3 forwardDirection { get; private set; }
    private Vector3 previousVelocity;
    private List<Agent> neighbours;
    private List<GameObject> walls;
    List<Vector3> wallPositions;

    void Awake()
    {
        neighbourCount = 0;
        forwardDirection = Vector3.up;
        neighbours = new List<Agent>();
        walls = new List<GameObject>();
        wallPositions = new List<Vector3>();
    }

    void Start()
    {
        StartCoroutine(update_cr());
        StartCoroutine(updateNearestWallPoint_cr());
    }

    private IEnumerator update_cr()
    {
        while (true)
        {
            previousVelocity = forwardDirection;
            if (neighbourCount != 0)
            {
                forwardDirection += CalculateVelocity() * VelocityWeight;
                forwardDirection += CalculateCohesion() * CohesionWeight;
                forwardDirection += CalculateSeparation() * SeparationWeight;
            }
            else { forwardDirection += Vector3.up * VelocityWeight; }

            if (walls.Count > 0)
            {
                forwardDirection += CalculateCollisionAvoidance() * NavigationWeight;
            }

            this.transform.up = Vector3.Lerp(previousVelocity, forwardDirection, RoatationSpeed);
            this.transform.position += this.transform.up * VelocityWeight * Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator updateNearestWallPoint_cr()
    {
        Vector4 closestPoint;
        while (true)
        {
            foreach(GameObject wall in walls)
            {
                wallPositions.Add(wall.transform.position);
            }

            for (int i = 0; i < walls.Count; i++)
            {
                closestPoint = GetClosestTriangleAndPoint(walls[i]);
                wallPositions[i] = closestPoint;
                Debug.Log(closestPoint);
            }

            yield return null;
            wallPositions.Clear();
        }
    }

    #region Triggers
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Agent")
        {
            CalculateVelocity(other.GetComponent<Agent>());
        }
        else if (other.tag == "Wall")
        {
            walls.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Agent")
        {
            neighbours.Remove(other.GetComponent<Agent>());
        }
        else if (other.tag == "Wall")
        {
            walls.Remove(other.gameObject);
        }
    }
    #endregion

    #region Calculations
    private Vector3 CalculateVelocity(Agent other = null)
    {
        Vector3 vel = forwardDirection;
        if (other == null)
        {
            vel = Vector3.zero;
            foreach (Agent agent in neighbours)
            {
                vel += agent.forwardDirection;
            }
        }
        else { vel += other.forwardDirection; }

        vel /= neighbourCount;
        vel.Normalize();
        return vel;
    }

    private Vector3 CalculateCohesion(Agent other = null)
    {
        Vector3 vel = forwardDirection;
        if (other == null)
        {
            vel = Vector3.zero;
            foreach (Agent agent in neighbours)
            {
                vel += agent.transform.position;
            }
        }
        else { vel += other.transform.position; }

        vel /= neighbourCount;
        vel = vel - this.transform.position;
        vel.Normalize();
        return vel;
    }

    private Vector3 CalculateSeparation(Agent other = null)
    {
        Vector3 vel = forwardDirection;
        if (other == null)
        {
            vel = Vector3.zero;
            foreach (Agent agent in neighbours)
            {
                vel += agent.transform.position - this.transform.position;
            }
        }
        else { vel += other.transform.position - this.transform.position; }

        vel /= neighbourCount;
        vel *= -1;
        vel.Normalize();
        return vel;
    }

    private Vector3 CalculateCollisionAvoidance()
    {
        Vector3 vel = Vector3.zero;
        
        foreach (Vector3 wall in wallPositions)
        {
            vel += wall - this.transform.position;
        }
        vel /= walls.Count;
        vel *= -1;
        vel.Normalize();

        return vel;
    }
    #endregion

    #region Math
   /* private bool Line2LineCollision(GameObject a)
    {
        Vector2 aStart = a.GetComponent<Renderer>().bounds.size / 2 - a.transform.position;
        Vector2 aEnd = a.transform.position - a.GetComponent<Renderer>().bounds.size / 2;

        Vector2 bStart = this.transform.position;
        Vector2 bEnd = this.transform.up * this.GetComponent<SphereCollider>().radius;

        //(x1 - x2)(y3 - y4) - (y1 - y2)(x3 - x4) = 0
        float denominator = (aStart.x - aEnd.x) * (bStart.y - bEnd.y) - (aStart.y - aEnd.y) * (bStart.y - bEnd.y);
        if (denominator == 0)
        { return false; }

        float numerator1 = ((aStart.y - aEnd.y) * (bEnd.x - aEnd.x)) - ((aStart.x - aEnd.x) * (bEnd.y - aEnd.y));
        float numerator2 = ((aStart.y - aEnd.y) * (bStart.x - aStart.x)) - ((aStart.x - aEnd.x) * (bStart.y - aStart.y));

        float r = numerator1 / denominator;
        float s = numerator2 / denominator;

        return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
    }*/
    
    public Vector4 GetClosestTriangleAndPoint(GameObject obj)
    {
        Vector3 point = obj.transform.position; // Later make closest point on agent instead of center
        point = transform.InverseTransformPoint(point);
        float minDistance = float.PositiveInfinity;
        int length = (obj.GetComponent<MeshFilter>().mesh.vertices.Length / 3);
        Vector3 closestPoint = Vector3.zero;
        for (int t = 0; t < length; t++)
        {
            Vector4 result = GetTriangleInfoForPoint(point, t, obj);
            if (minDistance > result.w)
            {
                minDistance = result.w;
                closestPoint = result;
            }
        }
        Vector4 finalResult = transform.TransformPoint(closestPoint);
        finalResult.w = (closestPoint - point).sqrMagnitude;
        return finalResult;
    }

    Vector4 GetTriangleInfoForPoint(Vector3 point, int triangle, GameObject obj)
    {
        Vector4 result = Vector4.zero;
        Vector3[] vertices = obj.GetComponent<MeshFilter>().mesh.vertices;
        int[] triangles = obj.GetComponent<MeshFilter>().mesh.triangles;
        int currentTriangle = triangle;
        result.w = float.PositiveInfinity;

        if (triangle >= vertices.Length / 3)
            return result;


        //Get the vertices of the triangle
        Vector3 p1 = vertices[triangles[0 + triangle * 3]];
        Vector3 p2 = vertices[triangles[1 + triangle * 3]];
        Vector3 p3 = vertices[triangles[2 + triangle * 3]];

        Vector3 normal = Vector3.Cross((p2 - p1).normalized, (p3 - p1).normalized);

        //Project our point onto the plane
        Vector3 projected = point + Vector3.Dot((p1 - point), normal) * normal;

        //Calculate the barycentric coordinates
        float u = ((projected.x * p2.y) - (projected.x * p3.y) - (p2.x * projected.y) + (p2.x * p3.y) + (p3.x * projected.y) - (p3.x * p2.y)) /
                ((p1.x * p2.y) - (p1.x * p3.y) - (p2.x * p1.y) + (p2.x * p3.y) + (p3.x * p1.y) - (p3.x * p2.y));
        float v = ((p1.x * projected.y) - (p1.x * p3.y) - (projected.x * p1.y) + (projected.x * p3.y) + (p3.x * p1.y) - (p3.x * projected.y)) /
                ((p1.x * p2.y) - (p1.x * p3.y) - (p2.x * p1.y) + (p2.x * p3.y) + (p3.x * p1.y) - (p3.x * p2.y));
        float w = ((p1.x * p2.y) - (p1.x * projected.y) - (p2.x * p1.y) + (p2.x * projected.y) + (projected.x * p1.y) - (projected.x * p2.y)) /
                ((p1.x * p2.y) - (p1.x * p3.y) - (p2.x * p1.y) + (p2.x * p3.y) + (p3.x * p1.y) - (p3.x * p2.y));

        Vector3 centre = p1 * 0.3333f + p2 * 0.3333f + p3 * 0.3333f;

        //Find the nearest point
        Vector3 vector = (new Vector3(u, v, w)).normalized;


        //work out where that point is
        Vector3 nearest = p1 * vector.x + p2 * vector.y + p3 * vector.z;
        result = nearest;
        result.w = (nearest - point).sqrMagnitude;

        if (float.IsNaN(result.w))
        {
            result.w = float.PositiveInfinity;
        }
        return result;
    }
    #endregion
}

/* TODO:
 * 1. Replace Separation code for walls with wall detection
 */
