using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 1;
    [SerializeField] private float turnSpeed = 15f;
    [SerializeField] private bool freeMoving = false;
    [SerializeField] private float swipeDeadZone = 100f;

    const float Multiply = 0.1f;

    private Touch touch;
    private Vector2 startSwipePosition;
    private Vector2 delta;
    private Vector3 direction;
    private Quaternion targetRotation;
    private bool isInsideArea;

    [Header("Trail")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private GameObject trailCollidersHolder;
    [SerializeField] private float minColliderDistnace = 1f;
    [SerializeField] private Collider playerCollider;
    
    [Header("Area")]
    [SerializeField] private string areaName;
    [SerializeField] private Material material;
    [SerializeField] private Color color;

    private List<Vector3> colliderPositions;
    private List<SphereCollider> trailColliders = new List<SphereCollider>();
    private GameObject area;
    private List<Vector3> trailVertices;
    private List<Vector3> areaVertices;
    private MeshRenderer areaMeshRender;
    private MeshFilter areaMeshFilter;
    private MeshCollider areaMeshCollider;

    private void Start()
    {
        InitializePlayer();
    }

    void Update()
    {
        if (GameManager.GameState == GameStates.Runing)
        {
            if (Input.GetKey(KeyCode.A))
                direction = Vector3.left;

            if (Input.GetKey(KeyCode.D))
                direction = Vector3.right;

            if (Input.GetKey(KeyCode.W))
                direction = Vector3.forward;

            if (Input.GetKey(KeyCode.S))
                direction = Vector3.back;

            if (Input.touchCount > 0)
            {
                if (freeMoving)
                    FreeMoving();
                else
                    LockMoving();
            }

            AddTrailCollider(transform.position);

            if (areaVertices != null && areaVertices.Count > 0)
            {
                isInsideArea = IsPointInPolygon(new Vector2(transform.position.x, transform.position.z), ToVector2(areaVertices));
                if (isInsideArea)
                {
                    if (trail.positionCount > 0)
                    {
                        DeformArea();
                    }

                    DestroyTrail();
                    trail.emitting = false;
                }
                else
                    trail.emitting = true;
            }
        }
    }
    
    void FixedUpdate()
    {
        if (GameManager.GameState == GameStates.Runing)
        {
            if(direction.magnitude > 0)
            {
                targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed);
            }

            Move();
        } 
    }

    private void InitializePlayer()
    {
        colliderPositions = new List<Vector3>();
        trailColliders = new List<SphereCollider>();
        trailVertices = new List<Vector3>();

        area = new GameObject();
        area.tag = "Area";
        area.name = areaName;
        areaMeshRender = area.AddComponent<MeshRenderer>();
        areaMeshFilter = area.AddComponent<MeshFilter>();
        areaMeshRender.material = material;
        areaMeshRender.material.color = color;
    }

    private void Move()
    {
        transform.position += transform.forward * Multiply * speed;
    }

    private void FreeMoving()
    {
        touch = Input.touches[0];
        if (touch.phase == TouchPhase.Began)
            startSwipePosition = touch.position;

        delta = touch.position - startSwipePosition;
        direction = new Vector3(delta.x, 0, delta.y);
        direction.Normalize();
    }

    private void LockMoving()
    {
        touch = Input.touches[0];
        if (touch.phase == TouchPhase.Began)
            startSwipePosition = touch.position;

        delta = touch.position - startSwipePosition;
        if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0 && delta.x > swipeDeadZone)
                direction = Vector3.right;
            else if(delta.x < -swipeDeadZone)
                direction = Vector3.left;
        }
        else
        {
            if (delta.y > 0 && delta.y > swipeDeadZone)
                direction = Vector3.forward;
            else if(delta.y < -swipeDeadZone)
                direction = Vector3.back;
        }
    }

    private void AddTrailCollider(Vector3 position)
    {
        if (trailColliders.Count == 0 || (colliderPositions[colliderPositions.Count - 1] - position).magnitude >= minColliderDistnace)
        {
            colliderPositions.Add(position);
            SphereCollider newCollider = trailCollidersHolder.AddComponent<SphereCollider>();
            newCollider.center = position;
            newCollider.radius = trail.startWidth / 2f;
            newCollider.isTrigger = true;
            newCollider.enabled = false;
            trailColliders.Add(newCollider);

            if (trailColliders.Count > 2)
            {
                trailColliders[trailColliders.Count - 3].enabled = true;
            }
        }
    }

    private void DestroyTrail()
    {
        trail.Clear();
        for (int i = 0; i < trailColliders.Count; i++)
        {
            Destroy(trailColliders[i]);
        }

        trailVertices.Clear();
        trailColliders.Clear();
    }

    private Mesh GenerateMesh(List<Vector3> vertices, string meshName)
    {
        areaVertices = new List<Vector3>(trailVertices);
        Triangulator triangulator = new Triangulator(ToVector2(areaVertices));
        int[] meshTriangles = triangulator.Triangulate();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = meshTriangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.name = areaName + "Mesh";

        return mesh;
    }

    private void DeformArea()
    {
        GetTrailVertices();
        int startNewAreaPoint = GetIndexOfClosestVertice(areaVertices, trailVertices[0]);
        var mag = Mathf.Abs((trailVertices[0] - areaVertices[startNewAreaPoint]).magnitude);
        if (mag < 0.4f)
        {
            int endNewAreaPoint = GetIndexOfClosestVertice(areaVertices, transform.position);

            var firstNewArea = new List<Vector3>(trailVertices);
            for (int i = endNewAreaPoint; i != startNewAreaPoint; i++)
            {
                if (i == areaVertices.Count)
                {
                    if (startNewAreaPoint == 0)
                        break;

                    i = 0;
                }
                firstNewArea.Add(areaVertices[i]);
            }

            var secondNewArea = new List<Vector3>(trailVertices);
            for (int i = startNewAreaPoint; i != endNewAreaPoint; i++)
            {
                if (i == areaVertices.Count)
                {
                    if (endNewAreaPoint == 0)
                        break;

                    i = 0;
                }
                secondNewArea.Insert(0, areaVertices[i]);
            }

            trailVertices = firstNewArea.Count > secondNewArea.Count ? firstNewArea : secondNewArea;

            UpdateArea();
        }
    }

    private void UpdateArea()
    {
        var mesh = GenerateMesh(trailVertices, name);
        areaMeshFilter.mesh = mesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Trail")
        {
            if (areaVertices == null)
            {
                var targetPos = ((SphereCollider)other).center;
                GetTrailVertices();
                DeleteUnusedVertice(trailVertices, targetPos);
                UpdateArea();
                DestroyTrail();
            }
            DestroyTrail();
            trail.emitting = false;
        }    
    }

    private void DeleteUnusedVertice(List<Vector3> vertices, Vector3 collisionPoint)
    {
        var lastIndex = GetIndexOfClosestVertice(trailVertices, collisionPoint);
        if (vertices.Count > 0 && lastIndex > 0)
            for (int i = 0; i < lastIndex; i++)
            {
                vertices.RemoveAt(0);
            }
    }

    private int GetIndexOfClosestVertice(List<Vector3> vertices, Vector3 collisionPoint)
    {
        var magnitude = float.MaxValue;
        int index = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            float tempMag = (vertices[i] - collisionPoint).magnitude;
            if (tempMag < magnitude)
            {
                magnitude = tempMag;
                index = i;
            }
        }

        return index;
    }

    private void GetTrailVertices()
    {
        for (int i = 0; i < trail.positionCount; i++)
            trailVertices.Add(trail.GetPosition(i));
    }

    public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
    {
        int polygonLength = polygon.Count, i = 0;
        bool inside = false;
        float pointX = point.x, pointY = point.y;
        float startX, startY, endX, endY;
        Vector2 endPoint = polygon[polygonLength - 1];
        endX = endPoint.x;
        endY = endPoint.y;
        while (i < polygonLength)
        {
            startX = endX; startY = endY;
            endPoint = polygon[i++];
            endX = endPoint.x; endY = endPoint.y;
            inside ^= (endY > pointY ^ startY > pointY) && ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
        }
        return inside;
    }

    private List<Vector2> ToVector2(List<Vector3> vertices)
    {
        var vertices2D = new List<Vector2>();
        for (int i = 0; i < vertices.Count; i++)
            vertices2D.Add(new Vector2(vertices[i].x, vertices[i].z));

        return vertices2D;
    }
}
