using UnityEngine;
using System.Collections;
using System;

public class CoinCollision : MonoBehaviour {
    private double timeCnt = 0;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Move();
    }

    void OnTriggerEnter(Collider c) {
		CollectCoin(c);
    }

    protected void Move()
    {
        transform.Rotate(0, Time.deltaTime * 30, 0);
        transform.Translate(0, (float)Math.Cos(timeCnt / 90) / 900, 0);
        timeCnt++;
    }

    protected void CollectCoin(Collider c)
    {
        GameController.AddCoin();
        GameController.AddLog("Collision: " + gameObject.name + " with " + c.gameObject.name + " at " + c.gameObject.transform.position.x + ", " + c.gameObject.transform.position.y + ", " + c.gameObject.transform.position.z);
        gameObject.SetActive(false);
    }
}
