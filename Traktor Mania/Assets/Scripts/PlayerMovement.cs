using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 10;
    [SerializeField] private float turnSpeed = 15f;
    [SerializeField] private bool freeMoving = false;
    [SerializeField] private float swipeDeadZone = 100f;

    const float Multiply = 0.01f;

    private Touch touch;
    private Vector2 startSwipePosition;
    private Vector2 delta;
    private Vector3 direction;
    private Vector3 newDirection = Vector3.forward;
    private Quaternion targetRotation;

    private Vector3 turnPosition;


    void Update()
    {
        if (GameManager.GameState == GameStates.Runing)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                newDirection = Vector3.left;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                newDirection = Vector3.right;
            }
                
            if (Input.GetKeyDown(KeyCode.W))
            {
                newDirection = Vector3.forward;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                newDirection = Vector3.back;
            }
        }
    }
    
    void FixedUpdate()
    {
        if (GameManager.GameState == GameStates.Runing)
        {
            if (newDirection != Vector3.zero && newDirection != direction)
            {
                if (direction == Vector3.forward)
                    turnPosition = new Vector3(transform.position.x, transform.position.y, Mathf.Ceil(transform.position.z));

                if (direction == Vector3.back)
                    turnPosition = new Vector3(transform.position.x, transform.position.y, Mathf.Floor(transform.position.z));

                if (direction == Vector3.left)
                    turnPosition = new Vector3(Mathf.Floor(transform.position.x), transform.position.y, transform.position.z);

                if (direction == Vector3.right)
                    turnPosition = new Vector3(Mathf.Ceil(transform.position.x), transform.position.y, transform.position.z);
            }

            if (direction == Vector3.zero || (turnPosition - transform.position).magnitude < 0.1f)
                direction = newDirection;

            Move();
        } 
    }

    private void Move()
    {
        transform.position += direction * Multiply * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Border")
        {
            direction = Vector3.zero;
            newDirection = Vector3.zero;
        }  
    }
}
