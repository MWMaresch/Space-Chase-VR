using UnityEngine;
using System.Collections;

public class Vehicle : MonoBehaviour {

    public const float CRASH_SPEED = 10f; // Threshold for crashing
    public int health = 3;

    private Rigidbody rb;
    // Track previous speeds to check if vehicle should crash
    private float previousSpeedX;
    private float previousSpeedZ;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        previousSpeedX = rb.velocity.x;
        previousSpeedZ = rb.velocity.z;
    }

    void OnCollisionEnter(Collision col)
    {
        if (Mathf.Abs(rb.velocity.x - previousSpeedX) < CRASH_SPEED && Mathf.Abs(rb.velocity.z - previousSpeedZ) > CRASH_SPEED)
        {
            if (col.gameObject.name == "Obstacle") // Gameobjects need to be called Obstacle for a collision to detect
            {
                health--;
                Debug.Log("You crashed.");
            }
        }
    }
}
