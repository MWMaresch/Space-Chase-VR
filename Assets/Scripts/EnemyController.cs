﻿using UnityEngine;
using System.Collections;

/*
 *  Class:              PlayerVecEnemyController
 *  Description:        Class to manage the chasing enemies
 *  Authors:            Jason Gunter
 *  Revision History:   
 *  Name:           Date:        Description:
 *  -------------------------------------------------------------------------
 *  Jason Gunter    Nov 2016     Created and finished script
 */

public class EnemyController : MonoBehaviour {
    private Transform _transform;
    private float _speed = 5.0f;

    // Use this for initialization
    void Start() {
        this._transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update() {
        var _player = GameObject.FindWithTag("Player");
        // make sure we found the player gameobject
        if (_player != null && _player.gameObject.transform.position != null) {
            this._transform.position = Vector3.MoveTowards(
                this._transform.position,
                _player.gameObject.transform.position,
                this._speed * Time.deltaTime);
        }
    }
}
