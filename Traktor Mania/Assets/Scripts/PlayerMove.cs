using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Rigidbody playerRB;
    [SerializeField] private int speed = 3;
    const float Multiply = 0.1f;

    //public Direction {set; }
    
    void FixedUpdate()
    {
        MoveForward();

        if (Input.GetKey(KeyCode.S))
            MoveDown();
        if (Input.GetKey(KeyCode.A))
            MoveLeft();
        if (Input.GetKey(KeyCode.D))
            MoveRight();
    }

    public void MoveForward ()
    {
        transform.position += transform.forward * Multiply * speed;
    }
    public void MoveDown()
    {
        transform.position -= transform.forward * Multiply * speed;
    }
    public void MoveLeft()
    {
        transform.Rotate(0, -2 * speed, 0);
    }
    public void MoveRight()
    {
        transform.Rotate(0, 2 * speed, 0);
    }
}
