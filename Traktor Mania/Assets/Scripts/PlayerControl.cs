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
    private List<Vector3> colliderPositions;
    private List<SphereCollider> trailColliders = new List<SphereCollider>();

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
            
            MoveForward();

            AddTrailCollider(transform.position);
        } 
    }

    private void MoveForward()
    {
        transform.position += transform.forward * Multiply * speed;
    }

    private bool IsFall()
    {
        return transform.position.y < 0;
    }

    private void AddTrailCollider(Vector3 position)
    {
        if (colliderPositions == null)
            colliderPositions = new List<Vector3>();
        
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

        trailColliders.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Trail")
            DestroyTrail();
    }
}
