using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private int speed = 1;
    [SerializeField] private float turnSpeed = 15f;

    const float Multiply = 0.1f;

    private Touch touch;
    private Vector2 startSwipePosition;
    private Vector2 delta;
    private Vector3 direction;
    private Quaternion targetRotation;

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
        if (Input.touchCount > 0)
        {
            touch = Input.touches[0];
            if (touch.phase == TouchPhase.Began)
                startSwipePosition = touch.position;

            delta = touch.position - startSwipePosition;
            direction = new Vector3(delta.x, 0, delta.y);
            direction.Normalize();
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
            
            if (IsFall())
                GameManager.ChangeState(GameStates.End);
            
            Move();

            AddTrailCollider(transform.position);
        } 
    }

    private void InitializePlayer()
    {
        colliderPositions = new List<Vector3>();
        trailColliders = new List<SphereCollider>();
        areaVertices = new List<Vector3>();

        area = new GameObject();
        area.name = areaName;
        areaMeshRender = area.AddComponent<MeshRenderer>();
        areaMeshFilter = area.AddComponent<MeshFilter>();
        areaMeshCollider = area.AddComponent<MeshCollider>();
        areaMeshRender.material = material;
        areaMeshRender.material.color = color;
    }

    private void Move()
    {
        transform.position += transform.forward * Multiply * speed;
    }

    private bool IsFall()
    {
        return transform.position.y < 0;
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

        areaVertices.Clear();
        trailColliders.Clear();
    }

    private Mesh GenerateMesh(List<Vector3> vertices, string meshName)
    {
        Triangulator triangulator = new Triangulator(ToVector2(areaVertices));
        int[] meshTriangles = triangulator.Triangulate();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = meshTriangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.name = areaName + "Mesh";

        return mesh;
    }

    private void UpdateArea()
    {
        var mesh = GenerateMesh(areaVertices, name);
        areaMeshFilter.mesh = mesh;
        areaMeshCollider.sharedMesh = mesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Trail")
        {
            var targetPos = ((SphereCollider)other).center;
            GetTrailVertices();
            DeleteUnusedVertice(targetPos);
            UpdateArea();
            DestroyTrail();
        }
            
    }

    private void DeleteUnusedVertice(Vector3 collisionPoint)
    {
        var lastIndex = GetIndexOfClosestVertice(collisionPoint);
        if (areaVertices.Count > 0 && lastIndex > 0)
            areaVertices.RemoveRange(0, lastIndex - 1);
    }

    private int GetIndexOfClosestVertice(Vector3 collisionPoint)
    {
        var magnitude = float.MaxValue;
        int index = 0;
        for (int i = 0; i < areaVertices.Count; i++)
        {
            float tempMag = (areaVertices[i] - collisionPoint).magnitude;
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
            areaVertices.Add(trail.GetPosition(i));
    }

    private List<Vector2> ToVector2(List<Vector3> vertices)
    {
        var vertices2D = new List<Vector2>();
        for (int i = 0; i < vertices.Count; i++)
            vertices2D.Add(new Vector2(vertices[i].x, vertices[i].z));

        return vertices2D;
    }
}
