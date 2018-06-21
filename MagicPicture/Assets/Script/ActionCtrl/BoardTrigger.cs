﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardTrigger : MonoBehaviour {

    [SerializeField] ActionCtrl actionCtrl;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //-------------------
    // 感圧板を踏んだら
    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.name == "Player") {
            actionCtrl.Action();
        }
    }

    void OnCollisionEnter(Collision other)
    {
        actionCtrl.Action();
    }

    //---------------------
    // 感圧板から離れたら
    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.name == "Player") {
            actionCtrl.Reset();
        }
    }

    void OnCollisionExit(Collision other)
    {
        actionCtrl.Reset();
    }
}