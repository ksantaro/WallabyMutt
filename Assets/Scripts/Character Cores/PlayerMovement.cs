using UnityEngine;
using System.Collections;
using System;

public class PlayerMovement : MonoBehaviour
{
    
    public float Speed;
    public float maxSpeed = 5;
    public float Acceleration = 10f;
    public Vector2 force;
    public Rigidbody2D rb;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    public void moveRight()

    {
        Speed = rb.velocity.x;
        rb.AddForce(Vector2.right * Acceleration);

        if (Speed > maxSpeed)
        {
            Speed = maxSpeed;
        }
    }
    public void moveLeft()
    {
        Speed = rb.velocity.x;
        rb.AddForce(Vector2.left * Acceleration);
        if (Math.Abs(Speed) > maxSpeed)
        {
            Speed = -maxSpeed;
        }
    }
    public void slowDown()
    {
        force = (rb.mass * -rb.velocity.normalized * maxSpeed) / 0.8f;
        rb.AddForce(force * Time.fixedDeltaTime);
    }
}