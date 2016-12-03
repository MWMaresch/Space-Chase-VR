/*
 *  Class:              PlayerVechicle
 *  Description:        Class to manage player controls/health
 *  Authors:            Michael Maresch, George Savchenko, Angelina Gutierrez
 *  Revision History:   
 *  Name:           Date:        Description:
 *  -------------------------------------------------------------------------
 *  George          10/11/2016      Merged Vechile and PlayerVechicle classes
 *  Michael         11/08/2016      Added slip stream functionality
 *  Angelina        11/09/2016      Added basic WarpGate collision check
 *  George          11/10/2016      Fixed collision detection, added health bar 
 *                                  and you lose velocity when crashing
 */
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerVehicle : NetworkBehaviour {

    [SyncVar]
    public int playerNumber = 0;
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

    private Transform _mainCamera;
    private float _cameraHeight = 0.25f;
    private float _cameraDistance = 0.15f;
    private Vector3 _cameraOffset; // How far back the camera should be

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

    private GameObject text;
    private Text t;
    private static GlobalValues playerCounter;

    //public float brakeGripLoss; //how much we slide when braking
    //grip not implemented, may not be necessary at all to implement

    // Use this for initialization
    public override void OnStartLocalPlayer () {

        _curVelocity = new Vector3(0, 0, 0);
        _curRotation = new Vector3(0, 0, 0);
        _rigidbody = GetComponent<Rigidbody>();

        _cameraOffset = new Vector3(0f, _cameraHeight, -_cameraDistance);
        _mainCamera = Camera.main.transform;
        MoveCamera();

        // Debug Shit
        text = GameObject.Find("Text");
        t = text.GetComponent<Text>();

        playerCounter = GameObject.Find("GlobalValues").GetComponent<GlobalValues>();
        playerCounter.numberOfPlayers++;

        playerNumber = playerCounter.numberOfPlayers;
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        if (!isLocalPlayer)
        {
            Destroy(this);
            return;
        }

            _previousVelocity = _rigidbody.velocity;

            UpdateControls();
            transform.position += transform.forward * _curVelocity.magnitude;

            transform.position += _curStrafe;

            healthBar = health / 100;
            Debug.Log(healthBar);

        MoveCamera();
        
        t.text = (isLocalPlayer && playerNumber > 1).ToString() + playerNumber.ToString();
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
        if (Mathf.Abs(_curVelocity.x - _previousVelocity.x) > CRASH_SPEED_THRESHOLD ||
            Mathf.Abs(_curVelocity.y - _previousVelocity.y) > CRASH_SPEED_THRESHOLD ||
            Mathf.Abs(_curVelocity.z - _previousVelocity.z) > CRASH_SPEED_THRESHOLD)
        {
            _curVelocity = new Vector3(0, 0, 0); // reset speed
            DoDamage(10); // reduce health
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

    void MoveCamera()
    {
        _mainCamera.position = transform.position;
        _mainCamera.rotation = transform.rotation;
        _mainCamera.Translate(_cameraOffset);
        _mainCamera.LookAt(transform.FindChild("Front"));
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
}
