/*
 *  Class:              PlayerVechicle
 *  Description:        Class to manage player controls/health
 *  Authors:            Michael Maresch, George Savchenko, Angelina Gutierrez, Jason Gunter
 *  Revision History:   
 *  Name:           Date:        Description:
 *  -------------------------------------------------------------------------
 *  George          10/11/2016      Merged Vechile and PlayerVechicle classes
 *  Michael         11/08/2016      Added slip stream functionality
 *  Angelina        11/09/2016      Added basic WarpGate collision check
 *  George          11/10/2016      Fixed collision detection, added health bar 
 *                                  and you lose velocity when crashing
 *  Jason           12/10/2016      Added 'Trigger' collision with warp gate, 
 *                                  moved warp gate to new positions.
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
    public const float CRASH_SPEED_THRESHOLD = 0.6f; // Threshold for crashing
    public float health = 100; // Player health

    //Jason's HackJob
    public Transform WarpGate;
    public Transform Checkpoint1;
    public Transform Checkpoint2;
    public Transform Checkpoint3;
    public Transform Checkpoint4;
    public Transform Checkpoint5;
    private int warpGatePos = 0;
    //END OF HAckJob

    private Rigidbody _rigidbody;
    private Vector3 _curVelocity;
    private Vector3 _curRotation;
    private Vector3 _curStrafe;
    private Vector3 _previousVelocity; // Track previous speeds to check if vehicle should crash

    private float healthBar = 0;
    private Vector2 pos = new Vector2(20, 40);
    private Vector2 size = new Vector2(60, 20);
    private Texture2D progressBarEmpty;
    private Texture2D progressBarFull;

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

        MoveWarpGate();

        _previousVelocity = _rigidbody.velocity;

        UpdateControls();
        transform.position += transform.forward * _curVelocity.magnitude;

        transform.position += _curStrafe;

        healthBar = health/100;
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

    private void DoDamage(int damageAmount)
    {
        if (health > 0)
        {
            health -= damageAmount;
            healthBar -= damageAmount / 100;
        }
        else
        {
            health = 0;
            SceneManager.LoadScene("scene_goal"); // change to scene lose
        }
    }

    // Collision detection
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "WarpGate") {
            if (warpGatePos == 4) {
                Debug.Log("Nice");
                SceneManager.LoadScene("scene_goal");
            } else {
                warpGatePos++;
            }          
        } else {
            if (Mathf.Abs(_curVelocity.x - _previousVelocity.x) > CRASH_SPEED_THRESHOLD ||
            Mathf.Abs(_curVelocity.y - _previousVelocity.y) > CRASH_SPEED_THRESHOLD ||
            Mathf.Abs(_curVelocity.z - _previousVelocity.z) > CRASH_SPEED_THRESHOLD) {
                _curVelocity = new Vector3(0, 0, 0); // reset speed
                DoDamage(10); // reduce health
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "SlipStream")
        {
            curAcceleration = slipStreamAcceleration;
            Debug.Log("gofast");
        }

        if (other.gameObject.tag == "Obstacle")
        {
            health--;
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

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "X: " + _curVelocity.x + " Y: " + _curVelocity.y + " Z :" + _curVelocity.z + "\n Health : " + health);

        // draw the background:
        GUI.BeginGroup(new Rect(pos.x, pos.y, size.x, size.y));
        GUI.Box(new Rect(0, 0, size.x, size.y), progressBarEmpty);

        // draw the filled-in part:
        GUI.BeginGroup(new Rect(0, 0, size.x * healthBar, size.y));
        GUI.Box(new Rect(0, 0, size.x, size.y), progressBarFull);
        GUI.EndGroup();

        GUI.EndGroup();
    }

    private void MoveWarpGate() {
        switch(warpGatePos) {
            case 0:
                WarpGate.position = Checkpoint1.position;
                WarpGate.rotation = Checkpoint1.rotation;
                break;
            case 1:
                WarpGate.position = Checkpoint2.position;
                WarpGate.rotation = Checkpoint2.rotation;
                break;
            case 2:
                WarpGate.position = Checkpoint3.position;
                WarpGate.rotation = Checkpoint3.rotation;
                break;
            case 3:
                WarpGate.position = Checkpoint4.position;
                WarpGate.rotation = Checkpoint4.rotation;
                break;
            case 4:
                WarpGate.position = Checkpoint5.position;
                WarpGate.rotation = Checkpoint5.rotation;
                break;
            default:
                break;
        }
    }
}
