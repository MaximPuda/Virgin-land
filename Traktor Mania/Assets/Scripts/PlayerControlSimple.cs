using System.Collections.Generic;
using UnityEngine;

public class PlayerControlSimple : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private int speed = 10;
    [SerializeField] private float turnSpeed = 15f;
    [SerializeField] private float swipeDeadZone = 100f;

    const float Multiply = 0.01f;

    private Touch touch;
    private Vector2 startSwipePosition;
    private Vector2 delta;
    private Vector3 direction;
    private Quaternion targetRotation;

    [Header("Trail")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private GameObject trailCollidersHolder;
    [SerializeField] private float minColliderDistnace = 1f;
    [SerializeField] private float pointOffset = 10f;
    [SerializeField] private Collider playerCollider;

    [Header("Area")]
    [SerializeField] private string areaName;
    [SerializeField] private Material material;
    [SerializeField] private LineRenderer outlineTrail;

    private GameObject area;
    private bool isInsideArea;
    private List<Vector3> colliderPositions;
    private List<SphereCollider> trailColliders = new List<SphereCollider>();
    private List<Vector3> trailVertices;
    private List<Vector3> meshVertices;
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
            if (trailVertices.Count == 0)
                trailVertices.Add(trail.transform.position);

            if (Input.GetKeyDown(KeyCode.A))
            {
                
                if (direction != Vector3.left)
                {
                    direction = Vector3.left;
                    trailVertices.Add(trail.transform.position);
                }
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                if (direction != Vector3.right)
                {
                    direction = Vector3.right;
                    trailVertices.Add(trail.transform.position);
                }
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (direction != Vector3.forward)
                {
                    direction = Vector3.forward;
                    trailVertices.Add(trail.transform.position);
                }
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                if (direction != Vector3.back)
                {
                    direction = Vector3.back;
                    trailVertices.Add(trail.transform.position);
                }
            }

            if (Input.touchCount > 0)
            {
                    TouchControl();
            }

            AddTrailCollider(transform.position);

            if (meshVertices != null && meshVertices.Count > 0)
            {
                isInsideArea = IsPointInPolygon(new Vector2(transform.position.x, transform.position.z), ToVector2(meshVertices));
                if (isInsideArea)
                {
                    if (trail.positionCount > 0)
                    {
                        //DeformArea();
                        UpdateArea();
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
            if (direction.magnitude > 0)
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
        areaMeshCollider = area.AddComponent<MeshCollider>();
    }

    private void Move()
    {
        transform.position += transform.forward * Multiply * speed;
    }

    private void TouchControl()
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
        meshVertices = new List<Vector3>(vertices);
        Triangulator triangulator = new Triangulator(ToVector2(meshVertices));
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

    private void UpdateArea()
    {
        var mesh = GenerateMesh(trailVertices, name);
        areaMeshFilter.mesh = mesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Trail")
        {
            DestroyTrail();
            trail.emitting = false;
        }    
    }

    private void AlignPoints(List<Vector3> points)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            var deltaX = Mathf.Abs(points[i + 1].x - points[i].x);
            var deltaZ = Mathf.Abs(points[i + 1].z - points[i].z);

            if (deltaX > 0 && deltaX < 1f)
            {
                points[i + 1] = new Vector3(points[i].x, trail.transform.position.y, points[i + 1].z);
                Debug.Log(i + 1 + "  Align X");
            }

            if (deltaZ > 0 && deltaZ < 1f)
            { 
                points[i + 1] = new Vector3(points[i + 1].x, trail.transform.position.y, points[i].z);
                Debug.Log(i + 1 + "  Align Z");
            }
        }
    }

    private void DrawOutline(List<Vector3> points)
    {
    }

    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
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
