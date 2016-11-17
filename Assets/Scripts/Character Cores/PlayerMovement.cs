using UnityEngine;
using System.Collections;
using System;

public class PlayerMovement : MonoBehaviour
{
    
    public float Speed;
    public float maxSpeed = 5;
    public float Acceleration = 10f;
    public float jumpHeight = 4f;


    bool   grounded = false;
    public Transform groundCheck;
    float  groundCircleRadius = 0.2f;
    public LayerMask groundMask;


	private Rigidbody2D RB;
	private SpriteRenderer SR;
	private Vector2 force;
//	private bool rightFace = true;

    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
        SR = GetComponentInChildren<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCircleRadius, groundMask.value); //true if touching the layer ground

//        if (Speed > 0 && !rightFace)
//            flip();
//        if (Speed < 0 && rightFace)
//            flip();
    }

    public void moveRight()

    {
        Speed = RB.velocity.x;
        RB.AddForce(Vector2.right * Acceleration);

        if (Speed > maxSpeed)
        {
            Speed = maxSpeed;
        }

        //Facing the sprite to the right
        SR.flipX = false;
    }

    public void moveLeft()
    {
        Speed = RB.velocity.x;
        RB.AddForce(Vector2.left * Acceleration);
        if (Math.Abs(Speed) > maxSpeed)
        {
            Speed = -maxSpeed;
        }

		//Facing the sprite to the left
        SR.flipX = true;
    }

    public void slowDown() //applies force to slow character down
    {
        force = (RB.mass * -RB.velocity.normalized * maxSpeed) / 0.8f;
        RB.AddForce(force * Time.fixedDeltaTime);
    }

    public void jump() //to use this properly, an child gameObject of the character has to be set in the Ground Check field
                       //in PlayerMovement
    {
        if (grounded)
        {
//            rb.AddForce(new Vector2(0, jumpHeight));
			RB.velocity = new Vector2(RB.velocity.x, CalculateJumpHeight(jumpHeight));
        }
    }

//    void flip() //turns character around if moving in the opposite direction from where it's facing
//    {
//    	
//        rightFace = !rightFace;
//        Vector3 scale = transform.localScale;
//        scale.x = -scale.x;
//        transform.localScale = scale;
//    }

    float CalculateJumpHeight (float height) {
    	return Mathf.Sqrt(Mathf.Abs(2 * height * Physics2D.gravity.y));
    }
    
}