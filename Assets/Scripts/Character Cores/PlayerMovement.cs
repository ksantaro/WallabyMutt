using UnityEngine;
using System.Collections;
using System;

public class PlayerMovement : MonoBehaviour
{

    public static float Speed;
    public static float maxSpeed = 5;
    public static float Acceleration = 10f;
    public static Vector2 force;

    public static void moveRight(Rigidbody2D rb)
    {
        Speed = rb.velocity.x;
        rb.AddForce(Vector2.right * Acceleration);
        if (Speed > maxSpeed)
        {
            Speed = maxSpeed;
        }
    }
    public static void moveLeft(Rigidbody2D rb)
    {
        Speed = rb.velocity.x;
        rb.AddForce(Vector2.left * Acceleration);
        if (Math.Abs(Speed) > maxSpeed)
        {
            Speed = -maxSpeed;
        }
    }
    public static void slowDown(Rigidbody2D rb)
    {
        force = (rb.mass * -rb.velocity.normalized * maxSpeed) / 0.8f;
        rb.AddForce(force * Time.fixedDeltaTime);
    }
}