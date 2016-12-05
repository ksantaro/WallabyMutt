using UnityEngine;
using System.Collections;
using System;

public class PlayerMovement : MonoBehaviour
{
    
    public float speed;
    public float maxSpeed = 5;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float jumpHeight = 4f;


    public Transform groundCheck;
    public LayerMask groundMask;

    public AudioClip jumpSound;


	private Rigidbody2D RB;
	private SpriteRenderer SR;
	private AudioSource AS;
	private Vector2 force;
	float   groundCircleRadius = 0.2f;
	bool    grounded = false;
//	private bool rightFace = true;

    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
        SR = GetComponentInChildren<SpriteRenderer>();
        AS = GetComponentInChildren<AudioSource>();
    }

    void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundCircleRadius, groundMask.value); //true if touching the layer ground

//        if (Speed > 0 && !rightFace)
//            flip();
//        if (Speed < 0 && rightFace)
//            flip();

//		AS.clip = jumpSound;

//		print(RB.velocity.x);

		
    }

    public void moveRight()

    {
//        Speed = RB.velocity.x;
        RB.AddForce(Vector2.right * acceleration);

        if (RB.velocity.x > maxSpeed)
        {
			RB.velocity = new Vector2(maxSpeed, RB.velocity.y);
        }

        //Facing the sprite to the right
        SR.flipX = false;
    }

    public void moveLeft()
    {
//        Speed = RB.velocity.x;
        RB.AddForce(Vector2.left * acceleration);


		if (RB.velocity.x < -maxSpeed)
        {
			RB.velocity = new Vector2(-maxSpeed, RB.velocity.y);
        }

		//Facing the sprite to the left
        SR.flipX = true;


    }

    public void slowDown() //applies force to slow character down
    {
        force = new Vector2((RB.velocity.x * -deceleration), RB.velocity.y);
        RB.AddForce(force, ForceMode2D.Force);
    }

	public void jump() //to use this properly, an child gameObject of the character has to be set in the Ground Check field
                       //in PlayerMovement
    {
        if (grounded)
        {
//            rb.AddForce(new Vector2(0, jumpHeight));
			RB.velocity = new Vector2(RB.velocity.x, GetJumpVelocity(jumpHeight));

			AS.PlayOneShot(jumpSound);
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

    float GetJumpVelocity (float height) {
    	return Mathf.Sqrt(Mathf.Abs(2 * height * Physics2D.gravity.y));
    }
    
}