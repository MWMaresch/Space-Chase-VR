/*
 *  Class:              PlayerVechicle
 *  Description:        Class to manage player controls/health
 *  Authors:            Michael Maresch and George Savchenko
 *  Revision History:   
 *  Name:           Date:        Description:
 *  -------------------------------------------------------------------------
 *  George          10/11/2016      Merged Vechile and PlayerVechicle classes
 *  Michael         11/08/2016      Added slip stream functionality
 *  Angelina        11/09/2016      Added basic WarpGate collision check
 */
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class PlayerVehicle : MonoBehaviour {

    public float maxSpeed = 1; //when accelerating, we cannot go past this speed
    public float slipStreamAcceleration = 0.015f; //how much speed increases when we accelerate
    public float curAcceleration = 0.005f; //how much speed increases when we accelerate
    public float slowAcceleration = 0.005f; //how much speed increases when we accelerate
    public float deceleration = 1.01f; //rate at which vehicle slows down when not accelerating
    public float turnStrength = 0.1f; //the higher this is, the tighter the turn
    public float turnFriction = 0.07f; //how much we slow down when turning
    public float turnDeceleration = 0.9f; //how much the turn slows down
    public float maxTurnSpeed = 0; //we cannot turn faster than this
    public float strafeStrength = 0.01f; //how much we move to the side when holding a strafe button
    public float brakeStrength = 1.01f; //how much we slow down when braking
    public const float CRASH_SPEED_THRESHOLD = 10f; // Threshold for crashing
    public int health = 3; // Player health

    private Rigidbody _rigidbody;
    private Vector3 _curVelocity;
    private Vector3 _curRotation;
    private Vector3 _curStrafe;
    private float _previousSpeedX, _previousSpeedZ; // Track previous speeds to check if vehicle should crash

    //public float brakeGripLoss; //how much we slide when braking
    //grip not implemented, may not be necessary at all to implement

    // Use this for initialization
    void Start () {
        _curVelocity = new Vector3(0, 0, 0);
        _curRotation = new Vector3(0, 0, 0);
        _rigidbody = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        _previousSpeedX = _rigidbody.velocity.x;
        _previousSpeedZ = _rigidbody.velocity.z;

        UpdateControls();
        transform.position += transform.forward * _curVelocity.magnitude;

        transform.position += _curStrafe;
    }

    private void UpdateControls()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //turn
        _curVelocity /= (Mathf.Abs(h * turnFriction * (turnStrength/10)) + 1);
        _curVelocity /= (Mathf.Abs(v * turnFriction * (turnStrength / 10)) + 1);
        //h is between -1 and 1, so we make it between 1 and 2,
        //where 1 represents 0 and 2 represents both 1 and -1
        //to avoid division by 0 while still being able to use these variables
        
        _curRotation += new Vector3(v * turnStrength, h * turnStrength, 0);
        _curRotation *= turnDeceleration;
        
        transform.localEulerAngles += _curRotation;

        //accelerate
        if (Input.GetButton("Accelerate"))
        {
            //instead of refusing to add more speed when we're at the max,
            //we clamp it after adding more to allow velocity regarding turning to exist
            _curVelocity += (transform.forward * curAcceleration);
            _curVelocity = Vector3.ClampMagnitude(_curVelocity, maxSpeed);
        }
        else
        {
            _curVelocity /= deceleration;
        }

        //brake
        if (Input.GetButton("Brake"))
        {
            _curVelocity /= brakeStrength;
        }

        //strafe
        if (Input.GetButton("StrafeLeft"))
        {
            _curStrafe += (-transform.right * strafeStrength);
        }
        if (Input.GetButton("StrafeRight"))
        {
            _curStrafe += (transform.right * strafeStrength);
        }
        _curStrafe /= 1.5f;
        //insert code to make the vehicle lean left/right when strafing
    }

    // Collision detection
    void OnCollisionEnter(Collision col)
    {
        if (Mathf.Abs(_rigidbody.velocity.x - _previousSpeedX) < CRASH_SPEED_THRESHOLD && Mathf.Abs(_rigidbody.velocity.z - _previousSpeedZ) > CRASH_SPEED_THRESHOLD)
        {
            if (col.gameObject.name == "Obstacle") // Gameobjects need to be called Obstacle for a collision to detect
            {
                health--;
                Debug.Log("You crashed.");
            }
        }

        if (col.gameObject.tag == "WarpGate")
        {
            Debug.Log("Nice");
            SceneManager.LoadScene("scene_goal");

        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "SlipStream")
        {
            curAcceleration = slipStreamAcceleration;
            Debug.Log("gofast");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "SlipStream")
        {
            curAcceleration = slowAcceleration;
            Debug.Log("stop going fast");
        }
    }
}
