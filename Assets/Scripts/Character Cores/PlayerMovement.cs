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
    public float jumpForce = 200f;

    bool rightFace = true;

    bool grounded = false;
    public Transform groundCheck;
    float groundCircleRadius = 0.1f;
    public LayerMask ground;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position,groundCircleRadius,ground); //true if touching the layer ground
        if (Speed > 0 && !rightFace)
            flip();
        if (Speed < 0 && rightFace)
            flip();
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
    public void slowDown() //applies force to slow character down
    {
        force = (rb.mass * -rb.velocity.normalized * maxSpeed) / 0.8f;
        rb.AddForce(force * Time.fixedDeltaTime);
    }
    public void jump() //to use this properly, an child gameObject of the character has to be set in the Ground Check field
                       //in PlayerMovement
    {
        if (grounded)
        {
            rb.AddForce(new Vector2(0, jumpForce));
        }
    }
    void flip() //turns character around if moving in the opposite direction from where it's facing
    {
        rightFace = !rightFace;
        Vector3 scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
    }
    
}